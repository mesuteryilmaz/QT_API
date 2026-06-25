using QT.Core.Primitives;
using QT.Market.Snapshots;

namespace QT.Market.OrderBook;

public readonly record struct MboMutation(
    NormalizedBookAction Action,
    MboOrderSnapshot? Before,
    MboOrderSnapshot? After,
    bool IsReprice,
    bool Accepted,
    string? RejectionReason);

public sealed class MboOrderBook
{
    private sealed class Order
    {
        public required string Id;
        public required BookSide Side;
        public required long PriceTicks;
        public required long Quantity;
        public required long Priority;
        public required DateTime FirstSeenUtc;
        public required DateTime LastUpdateUtc;
        public required bool HasRealId;
    }

    private readonly double tickSize;
    private readonly Dictionary<string, Order> orders = new(StringComparer.Ordinal);
    private readonly SortedDictionary<long, long> bidQty = new(Comparer<long>.Create((a, b) => b.CompareTo(a)));
    private readonly SortedDictionary<long, int> bidCount = new(Comparer<long>.Create((a, b) => b.CompareTo(a)));
    private readonly SortedDictionary<long, long> askQty = new();
    private readonly SortedDictionary<long, int> askCount = new();

    private long realIdEvents;
    private long syntheticIdEvents;

    public MboOrderBook(double tickSize)
    {
        this.tickSize = tickSize > 0 ? tickSize : 1.0;
    }

    public int OrderCount => orders.Count;
    public int BidLevels => bidQty.Count;
    public int AskLevels => askQty.Count;
    public long RealIdEvents => realIdEvents;
    public long SyntheticIdEvents => syntheticIdEvents;
    public double RealIdCoverage => realIdEvents + syntheticIdEvents == 0
        ? 0.0
        : realIdEvents / (double)(realIdEvents + syntheticIdEvents);

    public void ResetCoverageCounters()
    {
        realIdEvents = 0;
        syntheticIdEvents = 0;
    }

    public void Clear()
    {
        orders.Clear();
        bidQty.Clear();
        bidCount.Clear();
        askQty.Clear();
        askCount.Clear();
        realIdEvents = 0;
        syntheticIdEvents = 0;
    }

    public MboMutation Apply(DateTime eventTimeUtc, string? rawId, BookSide side, long priceTicks, long quantity, long priority, bool closed, bool isSnapshotSeed)
    {
        if (side is not (BookSide.Bid or BookSide.Ask))
            return new MboMutation(NormalizedBookAction.None, null, null, false, false, "unknown side");

        bool hasRealId = !string.IsNullOrWhiteSpace(rawId);
        if (!isSnapshotSeed)
        {
            if (hasRealId) realIdEvents++;
            else syntheticIdEvents++;
        }

        string id = hasRealId ? rawId! : SyntheticKey(side, priceTicks);
        if (closed || quantity <= 0)
            return Remove(eventTimeUtc, id);

        if (priceTicks <= 0)
            return new MboMutation(NormalizedBookAction.None, null, null, false, false, "non-positive price");

        if (orders.TryGetValue(id, out var existing))
        {
            var before = ToSnapshot(existing);
            bool reprice = existing.PriceTicks != priceTicks || existing.Side != side;
            Dec(existing.Side, existing.PriceTicks, existing.Quantity);
            existing.Side = side;
            existing.PriceTicks = priceTicks;
            existing.Quantity = quantity;
            existing.Priority = priority;
            existing.LastUpdateUtc = eventTimeUtc;
            existing.HasRealId = hasRealId;
            Inc(existing.Side, existing.PriceTicks, existing.Quantity);
            return new MboMutation(NormalizedBookAction.Update, before, ToSnapshot(existing), reprice, true, null);
        }

        var order = new Order
        {
            Id = id,
            Side = side,
            PriceTicks = priceTicks,
            Quantity = quantity,
            Priority = priority,
            FirstSeenUtc = eventTimeUtc,
            LastUpdateUtc = eventTimeUtc,
            HasRealId = hasRealId
        };
        orders[id] = order;
        Inc(side, priceTicks, quantity);
        return new MboMutation(isSnapshotSeed ? NormalizedBookAction.Snapshot : NormalizedBookAction.Add,
            null, ToSnapshot(order), false, true, null);
    }

    public MboMutation Remove(DateTime eventTimeUtc, string? rawId)
    {
        if (string.IsNullOrWhiteSpace(rawId))
            return new MboMutation(NormalizedBookAction.Remove, null, null, false, false, "missing order id");

        if (!orders.TryGetValue(rawId, out var existing))
            return new MboMutation(NormalizedBookAction.Remove, null, null, false, false, "order id not found");

        var before = ToSnapshot(existing);
        Dec(existing.Side, existing.PriceTicks, existing.Quantity);
        orders.Remove(rawId);
        var after = before with { Quantity = 0, LastUpdateUtc = eventTimeUtc };
        return new MboMutation(NormalizedBookAction.Remove, before, after, false, true, null);
    }

    public bool TryBestBid(out long ticks, out long quantity) => TryBest(bidQty, out ticks, out quantity);
    public bool TryBestAsk(out long ticks, out long quantity) => TryBest(askQty, out ticks, out quantity);

    public IReadOnlyList<BookLevelSnapshot> Top(BookSide side, int depth)
    {
        var qty = side == BookSide.Bid ? bidQty : askQty;
        var cnt = side == BookSide.Bid ? bidCount : askCount;
        var rows = new List<BookLevelSnapshot>(Math.Min(depth, qty.Count));
        foreach (var (ticks, quantity) in qty)
        {
            if (rows.Count >= depth) break;
            cnt.TryGetValue(ticks, out int orderCount);
            rows.Add(new BookLevelSnapshot(side, ticks, ticks * tickSize, quantity, orderCount));
        }
        return rows;
    }

    public IReadOnlyList<MboOrderSnapshot> Orders()
    {
        var rows = new List<MboOrderSnapshot>(orders.Count);
        foreach (var order in orders.Values)
            rows.Add(ToSnapshot(order));

        rows.Sort(static (a, b) =>
        {
            int side = a.Side.CompareTo(b.Side);
            if (side != 0) return side;
            int price = a.Side == BookSide.Bid
                ? b.PriceTicks.CompareTo(a.PriceTicks)
                : a.PriceTicks.CompareTo(b.PriceTicks);
            return price != 0 ? price : string.CompareOrdinal(a.OrderId, b.OrderId);
        });
        return rows;
    }

    public long QueueAheadSize(string orderId)
    {
        if (!orders.TryGetValue(orderId, out var target))
            return 0;

        long ahead = 0;
        foreach (var order in orders.Values)
        {
            if (order.Side == target.Side && order.PriceTicks == target.PriceTicks && order.Priority < target.Priority)
                ahead += order.Quantity;
        }
        return ahead;
    }

    private string SyntheticKey(BookSide side, long priceTicks)
        => $"{(side == BookSide.Bid ? "B" : "A")}:{priceTicks}";

    private static bool TryBest(SortedDictionary<long, long> side, out long ticks, out long quantity)
    {
        if (side.Count == 0)
        {
            ticks = 0;
            quantity = 0;
            return false;
        }

        var first = side.First();
        ticks = first.Key;
        quantity = first.Value;
        return true;
    }

    private void Inc(BookSide side, long ticks, long quantity)
    {
        var qty = side == BookSide.Bid ? bidQty : askQty;
        var cnt = side == BookSide.Bid ? bidCount : askCount;
        qty[ticks] = (qty.TryGetValue(ticks, out var q) ? q : 0) + quantity;
        cnt[ticks] = (cnt.TryGetValue(ticks, out var c) ? c : 0) + 1;
    }

    private void Dec(BookSide side, long ticks, long quantity)
    {
        var qty = side == BookSide.Bid ? bidQty : askQty;
        var cnt = side == BookSide.Bid ? bidCount : askCount;
        if (qty.TryGetValue(ticks, out var q))
        {
            long next = q - quantity;
            if (next <= 0) qty.Remove(ticks);
            else qty[ticks] = next;
        }

        if (cnt.TryGetValue(ticks, out var c))
        {
            int next = c - 1;
            if (next <= 0) cnt.Remove(ticks);
            else cnt[ticks] = next;
        }
    }

    private MboOrderSnapshot ToSnapshot(Order order)
        => new(order.Id, order.Side, order.PriceTicks, order.PriceTicks * tickSize, order.Quantity,
            order.Priority, order.FirstSeenUtc, order.LastUpdateUtc, order.HasRealId,
            false);
}
