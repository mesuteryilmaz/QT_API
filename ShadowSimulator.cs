using System;
using TradingPlatform.BusinessLayer;

namespace MBO_Market_Data_Analytics
{
    /// <summary>
    /// How the strategy routes signals to execution.
    /// </summary>
    public enum ExecutionMode
    {
        ShadowThenPromote, // start paper; promote to live IB only after the shadow record clears the gate
        ShadowOnly,        // never route to IB (pure forward paper test)
        LiveOnly           // route to IB immediately (legacy behavior, no envelope)
    }

    /// <summary>
    /// Running performance statistics over a stream of closed-trade net PnL values. Thread-safe.
    /// Provides a one-sided lower confidence bound used as a luck-resistant promotion/demotion gate:
    /// requiring LCB &gt; 0 demands more trades when per-trade variance is high.
    /// </summary>
    public sealed class TradePerformanceTracker
    {
        private readonly object sync = new object();
        private int count, wins, losses;
        private double sum, sumSq;
        private double cum, peak, maxDD;

        public void Reset()
        {
            lock (sync)
            {
                count = wins = losses = 0;
                sum = sumSq = 0;
                cum = peak = maxDD = 0;
            }
        }

        public void Add(double netPnL)
        {
            lock (sync)
            {
                count++;
                sum += netPnL;
                sumSq += netPnL * netPnL;
                if (netPnL > 0) wins++; else if (netPnL < 0) losses++;
                cum += netPnL;
                if (cum > peak) peak = cum;
                double dd = peak - cum;
                if (dd > maxDD) maxDD = dd;
            }
        }

        public int Count { get { lock (sync) return count; } }
        public double NetPnL { get { lock (sync) return cum; } }
        public double Mean { get { lock (sync) return count > 0 ? sum / count : 0; } }
        public double WinRate { get { lock (sync) return count > 0 ? (double)wins / count : 0; } }
        public double MaxDrawdown { get { lock (sync) return maxDD; } }

        /// <summary>Sample standard deviation of per-trade net PnL (0 if fewer than 2 trades).</summary>
        public double StdDev
        {
            get
            {
                lock (sync) { return StdDevInternal(); }
            }
        }

        private double StdDevInternal()
        {
            if (count < 2) return 0;
            double variance = (sumSq - (sum * sum) / count) / (count - 1);
            return Math.Sqrt(Math.Max(0, variance));
        }

        /// <summary>
        /// One-sided lower confidence bound on mean per-trade PnL: mean − z·(std/√n).
        /// Returns −∞ with fewer than 2 trades.
        /// </summary>
        public double LowerConfidenceBound(double z)
        {
            lock (sync)
            {
                if (count < 2) return double.NegativeInfinity;
                double mean = sum / count;
                double sem = StdDevInternal() / Math.Sqrt(count);
                return mean - z * sem;
            }
        }

        /// <summary>True when count ≥ minTrades, mean ≥ minExpectancy, and the LCB is positive.</summary>
        public bool MeetsBar(int minTrades, double z, double minExpectancy)
        {
            lock (sync)
            {
                if (count < Math.Max(2, minTrades)) return false;
                double mean = sum / count;
                if (mean < minExpectancy) return false;
                double sem = StdDevInternal() / Math.Sqrt(count);
                return (mean - z * sem) > 0;
            }
        }
    }

    /// <summary>
    /// Paper-trades the strategy's signals against the live tape without touching the broker. Models a
    /// single bracketed position at a time. Entries fill aggressively at the far touch (pay the spread)
    /// — pessimistic and reproducible without an MBO queue model; stop exits fill at the stop price
    /// (+ slippage), take-profit at the limit price. Net-of-cost PnL accrues to <see cref="Performance"/>.
    ///
    /// Single-threaded by contract: only ever driven from the strategy's worker thread.
    /// </summary>
    public sealed class ShadowSimulator
    {
        public sealed class SimConfig
        {
            public double TickSize = 0.25;
            public double PointValue = 20.0;
            public double OrderQty = 1.0;
            public double CommissionPerContract = 0.0; // per side
            public double SlippageTicks = 0.0;
            public int CooldownSeconds = 5;
        }

        private readonly SimConfig cfg;
        private readonly TradePerformanceTracker perf = new TradePerformanceTracker();

        private int pos; // 0 flat, +1 long, -1 short
        private double entry, tp, sl;
        private DateTime lastExit = DateTime.MinValue;

        public ShadowSimulator(SimConfig config)
        {
            this.cfg = config ?? throw new ArgumentNullException(nameof(config));
        }

        public TradePerformanceTracker Performance => perf;
        public bool InPosition => pos != 0;

        public void Reset()
        {
            pos = 0;
            entry = tp = sl = 0;
            lastExit = DateTime.MinValue;
            perf.Reset();
        }

        public bool CanEnter(DateTime now)
            => pos == 0 && (now - lastExit).TotalSeconds >= cfg.CooldownSeconds;

        /// <summary>
        /// Enters a paper position. When bestOrderCount ≤ 2 (thin queue), fills at mid rather than far touch.
        /// Returns false if not flat.
        /// </summary>
        public bool Enter(Side side, double bid, double ask, int tpTicks, int slTicks, DateTime now, int bestOrderCount = 0)
        {
            if (pos != 0) return false;

            double ts = cfg.TickSize > 0 ? cfg.TickSize : 1.0;
            double slip = cfg.SlippageTicks * ts;
            bool thinQueue = bestOrderCount > 0 && bestOrderCount <= 2;
            double mid = (bid + ask) / 2.0;

            if (side == Side.Buy)
            {
                double farTouch = ask > 0 ? ask : bid;
                if (farTouch <= 0) return false;
                double fill = thinQueue ? mid : farTouch;
                pos = +1;
                entry = fill + slip;
                tp = tpTicks > 0 ? entry + tpTicks * ts : double.PositiveInfinity;
                sl = slTicks > 0 ? entry - slTicks * ts : double.NegativeInfinity;
            }
            else
            {
                double farTouch = bid > 0 ? bid : ask;
                if (farTouch <= 0) return false;
                double fill = thinQueue ? mid : farTouch;
                pos = -1;
                entry = fill - slip;
                tp = tpTicks > 0 ? entry - tpTicks * ts : double.NegativeInfinity;
                sl = slTicks > 0 ? entry + slTicks * ts : double.PositiveInfinity;
            }
            return true;
        }

        /// <summary>Processes bracket exits against the latest trade price. Returns true if a trade closed.</summary>
        public bool OnMarket(double lastPrice, DateTime now)
        {
            if (pos == 0 || lastPrice <= 0) return false;

            double ts = cfg.TickSize > 0 ? cfg.TickSize : 1.0;
            double slip = cfg.SlippageTicks * ts;

            if (pos > 0)
            {
                if (lastPrice <= sl) { Close(sl - slip, now); return true; } // stop, adverse
                if (lastPrice >= tp) { Close(tp, now); return true; }        // limit
            }
            else
            {
                if (lastPrice >= sl) { Close(sl + slip, now); return true; }
                if (lastPrice <= tp) { Close(tp, now); return true; }
            }
            return false;
        }

        /// <summary>Closes any open paper position at the supplied price (e.g. on promotion handoff).</summary>
        public void ForceCloseAt(double price, DateTime now)
        {
            if (pos != 0 && price > 0) Close(price, now);
        }

        private void Close(double exitPrice, DateTime now)
        {
            double gross = (exitPrice - entry) * pos * cfg.PointValue * cfg.OrderQty;
            double commission = cfg.CommissionPerContract * cfg.OrderQty * 2.0; // entry + exit
            perf.Add(gross - commission);
            pos = 0;
            lastExit = now;
        }
    }
}
