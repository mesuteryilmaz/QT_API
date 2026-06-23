using System;
using System.Collections.Generic;

namespace MBO_Market_Data_Analytics
{
    /// <summary>Normalized order-book action resolved from the raw feed. For <see cref="MboAction.Trade"/>
    /// the <c>IsBid</c> field carries the aggressor side (true = buy-aggressor).</summary>
    public enum MboAction : byte { Add, Update, Remove, Snapshot, Trade }

    /// <summary>
    /// One normalized Market-By-Order event — the canonical record/replay unit. Captures exactly what
    /// the live engine sees, so a recorded stream replays identically (train == serve).
    /// </summary>
    public readonly struct MboEvent
    {
        public readonly long Seq;
        public readonly DateTime Time;
        public readonly MboAction Action;
        public readonly bool IsBid;
        public readonly double Price;
        public readonly double Size;
        public readonly long Priority;
        public readonly int NumberOrders;
        public readonly string OrderId;

        public MboEvent(long seq, DateTime time, MboAction action, bool isBid, double price, double size, long priority, int numberOrders, string orderId)
        {
            Seq = seq;
            Time = time;
            Action = action;
            IsBid = isBid;
            Price = price;
            Size = size;
            Priority = priority;
            NumberOrders = numberOrders;
            OrderId = orderId;
        }
    }

    /// <summary>
    /// Reconstructs an order-keyed (MBO) book from the live Quantower Level2 stream and resolves each
    /// raw quote into a normalized <see cref="MboEvent"/>. Maintains per-price size/order-count
    /// aggregates incrementally and can answer queue-ahead size for a given order.
    ///
    /// Not thread-safe; callers serialize access (the recorder indicator locks around it).
    /// </summary>
    public sealed class MboOrderBook
    {
        private sealed class Order
        {
            public long PriceTicks;
            public double Size;
            public bool IsBid;
            public long Priority;
        }

        private readonly double tickSize;
        private readonly Dictionary<string, Order> orders = new Dictionary<string, Order>();
        private readonly Dictionary<long, double> bidSizeByTick = new Dictionary<long, double>();
        private readonly Dictionary<long, int> bidCountByTick = new Dictionary<long, int>();
        private readonly Dictionary<long, double> askSizeByTick = new Dictionary<long, double>();
        private readonly Dictionary<long, int> askCountByTick = new Dictionary<long, int>();

        private long seq;

        public MboOrderBook(double tickSize)
        {
            this.tickSize = tickSize > 0 ? tickSize : 1.0;
        }

        public int OrderCount => orders.Count;
        public int BidLevels => bidSizeByTick.Count;
        public int AskLevels => askSizeByTick.Count;

        public void Clear()
        {
            orders.Clear();
            bidSizeByTick.Clear();
            bidCountByTick.Clear();
            askSizeByTick.Clear();
            askCountByTick.Clear();
        }

        /// <summary>Applies a live quote and returns the resolved normalized event.</summary>
        public MboEvent Apply(DateTime time, string? id, bool isBid, double price, double size, long priority, int numberOrders, bool closed)
        {
            long s = ++seq;
            string key = !string.IsNullOrEmpty(id) ? id! : SyntheticKey(isBid, price);

            MboAction action;
            if (closed || size <= 0)
            {
                action = MboAction.Remove;
                RemoveOrder(key);
            }
            else if (orders.ContainsKey(key))
            {
                action = MboAction.Update;
                Set(key, price, size, isBid, priority);
            }
            else
            {
                action = MboAction.Add;
                Set(key, price, size, isBid, priority);
            }

            return new MboEvent(s, time, action, isBid, price, size, priority, numberOrders, key);
        }

        /// <summary>Applies an initial snapshot item.</summary>
        public MboEvent ApplySnapshot(DateTime time, string? id, bool isBid, double price, double size, long priority, int numberOrders)
        {
            long s = ++seq;
            string key = !string.IsNullOrEmpty(id) ? id! : SyntheticKey(isBid, price);
            if (size > 0) Set(key, price, size, isBid, priority);
            return new MboEvent(s, time, MboAction.Snapshot, isBid, price, size, priority, numberOrders, key);
        }

        /// <summary>Emits a Trade event (does not modify the resting book). IsBid = buy-aggressor.</summary>
        public MboEvent ApplyTrade(DateTime time, bool isBuyAggressor, double price, double size)
        {
            long s = ++seq;
            return new MboEvent(s, time, MboAction.Trade, isBuyAggressor, price, size, 0, 0, "");
        }

        private string SyntheticKey(bool isBid, double price)
            => (isBid ? "B:" : "A:") + ((long)Math.Round(price / tickSize)).ToString();

        private void Set(string key, double price, double size, bool isBid, long priority)
        {
            long pt = (long)Math.Round(price / tickSize);
            if (orders.TryGetValue(key, out var o))
            {
                DecLevel(o.IsBid, o.PriceTicks, o.Size);
                o.PriceTicks = pt; o.Size = size; o.IsBid = isBid; o.Priority = priority;
            }
            else
            {
                o = new Order { PriceTicks = pt, Size = size, IsBid = isBid, Priority = priority };
                orders[key] = o;
            }
            IncLevel(isBid, pt, size);
        }

        private void RemoveOrder(string key)
        {
            if (orders.TryGetValue(key, out var o))
            {
                DecLevel(o.IsBid, o.PriceTicks, o.Size);
                orders.Remove(key);
            }
        }

        private void IncLevel(bool isBid, long pt, double size)
        {
            var sizeMap = isBid ? bidSizeByTick : askSizeByTick;
            var countMap = isBid ? bidCountByTick : askCountByTick;
            sizeMap[pt] = (sizeMap.TryGetValue(pt, out var s) ? s : 0) + size;
            countMap[pt] = (countMap.TryGetValue(pt, out var c) ? c : 0) + 1;
        }

        private void DecLevel(bool isBid, long pt, double size)
        {
            var sizeMap = isBid ? bidSizeByTick : askSizeByTick;
            var countMap = isBid ? bidCountByTick : askCountByTick;
            if (sizeMap.TryGetValue(pt, out var s))
            {
                double ns = s - size;
                if (ns <= 1e-9) sizeMap.Remove(pt); else sizeMap[pt] = ns;
            }
            if (countMap.TryGetValue(pt, out var c))
            {
                int nc = c - 1;
                if (nc <= 0) countMap.Remove(pt); else countMap[pt] = nc;
            }
        }

        public bool TryGetBestBid(out double price, out double size) => TryBest(bidSizeByTick, true, out price, out size);
        public bool TryGetBestAsk(out double price, out double size) => TryBest(askSizeByTick, false, out price, out size);

        private bool TryBest(Dictionary<long, double> map, bool isBid, out double price, out double size)
        {
            price = 0; size = 0;
            bool found = false;
            long bestTick = isBid ? long.MinValue : long.MaxValue;
            foreach (var kv in map)
            {
                if ((isBid && kv.Key > bestTick) || (!isBid && kv.Key < bestTick))
                {
                    bestTick = kv.Key; size = kv.Value; found = true;
                }
            }
            if (found) price = bestTick * tickSize;
            return found;
        }

        /// <summary>
        /// Best-effort queue-ahead size for an order: total resting size at the same price+side with a
        /// "better" (lower) priority value. Order-level feature substrate; O(orders), diagnostic use.
        /// </summary>
        public double QueueAheadSize(string orderId)
        {
            if (!orders.TryGetValue(orderId, out var target)) return 0;
            double ahead = 0;
            foreach (var o in orders.Values)
            {
                if (o.IsBid == target.IsBid && o.PriceTicks == target.PriceTicks && o.Priority < target.Priority)
                    ahead += o.Size;
            }
            return ahead;
        }
    }
}
