using System;
using System.Collections.Generic;
using System.Linq;
using TradingPlatform.BusinessLayer;
using TradingPlatform.BusinessLayer.Integration;

namespace MBO_Market_Data_Analytics
{
    [Flags]
    public enum FeedCapabilities
    {
        None                    = 0,
        Trades                  = 1 << 0,
        TradeAggressor          = 1 << 1,
        TradeId                 = 1 << 2,
        TopOfBook               = 1 << 3,
        MarketByPrice           = 1 << 4,
        NonAggregatedDepth      = 1 << 5,
        OrderId                 = 1 << 6,
        OrderAction             = 1 << 7,
        ExchangeTimestamp       = 1 << 8,
        SequenceNumber          = 1 << 9,
        HistoricalTrades        = 1 << 10,
        HistoricalBars          = 1 << 11
    }

    public enum MetricQuality : byte
    {
        Unavailable = 0,
        Heuristic   = 1,
        Derived     = 2,
        Exact       = 3
    }

    public enum AggressorSide
    {
        Buy,
        Sell,
        Unknown
    }

    public enum BookSide
    {
        Bid,
        Ask
    }

    public readonly struct MetricValue
    {
        public readonly double Value;
        public readonly MetricQuality Quality;
        public readonly long EventTimeNs;
        public readonly long ReceiveTicks;
        public readonly bool IsWarm;

        public MetricValue(
            double value,
            MetricQuality quality,
            long eventTimeNs,
            long receiveTicks,
            bool isWarm)
        {
            Value = value;
            Quality = quality;
            EventTimeNs = eventTimeNs;
            ReceiveTicks = receiveTicks;
            IsWarm = isWarm;
        }

        public override string ToString()
        {
            return Quality == MetricQuality.Unavailable ? "-" : $"{Value:F2} ({Quality})";
        }
    }

    /// <summary>
    /// Advanced real-time market microstructure analytics engine for CME Futures.
    /// Transforms Trades, Level 2 Order Book, Depth, and Events into actionable analytics.
    ///
    /// Time windows are driven by event timestamps (not wall-clock), which makes the engine
    /// usable for historical replay/backtesting and immune to clock skew. Sliding-window
    /// aggregates are maintained incrementally (O(1) amortized per event) instead of being
    /// recomputed via LINQ on every read, so both the per-event hot path and the periodic
    /// snapshot publish are cheap and allocation-free.
    /// </summary>
    public class DataAnalyticsCalculator
    {
        private readonly Symbol symbol;
        private readonly object locker = new object();

        public FeedCapabilities Capabilities { get; private set; }

        /// <summary>Adds capability flags discovered post-construction (e.g. live aggressor sniffing on mapped symbols).</summary>
        public void GrantCapability(FeedCapabilities cap) { Capabilities |= cap; }

        // Dynamic and Manual Configuration
        public double TradeVolumeWindowShort { get; set; } = 1000;
        public double TradeVolumeWindowLong { get; set; } = 5000;

        public int OrderCountWindowShort { get; set; } = 2000;
        public int OrderCountWindowLong { get; set; } = 10000;

        // Absorption: minimum traded volume within the 5s window for the level to count as absorption.
        // Set to > 0 before Start() to pin it; 0 means auto-calibrate from MTR at calibration time.
        public double AbsorptionVolumeThreshold { get; set; } = 0;

        // Calibration State Machine (Method A)
        public bool IsCalibrated { get; set; } = false;
        public bool IsManualMode { get; set; } = false;
        public int CalibrationTradesThreshold { get; set; } = 1000;
        private int calibrationTradesProcessed = 0;
        private double calibrationTradedVolume = 0;
        private int calibrationL2MessagesCount = 0;
        private const double FALLBACK_MTR = 5.0;

        // Saturation value returned for ratios whose denominator is zero but numerator is positive
        // (e.g. strong one-sided buying with zero sell volume). Prevents a degenerate "0" from being
        // misread as the opposite extreme by downstream signal logic.
        // M-21: must equal AdaptiveParameterConfig.RatioClampMax (10.0) so saturated ratios are
        // clamped to the same ceiling used by adaptive history samples.
        private const double RATIO_SATURATION = 10.0;

        // Latest event time the engine has advanced to (event-time clock).
        private long currentTicks;

        // Session Cumulative Accumulators (Section 4.4)
        private double totalBuyerTradeCount;
        private double totalSellerTradeCount;
        private double totalBuyerTradeQty;
        private double totalSellerTradeQty;
        private double totalTradesCount;

        private double totalArrivedOrderCount;
        private double totalArrivedOrderQty;
        private double totalCancelledOrderCount;
        private double totalCancelledOrderQty;

        private double totalCancelledBuyOrderCount;
        private double totalCancelledSellOrderCount;
        private double totalCancelledBuyOrderQty;
        private double totalCancelledSellOrderQty;

        // Session VWAP Accumulators (Section 6.2 & 6.3)
        private double sessionVwapSumPriceQty;
        private double sessionVwapQty;
        private double sessionPriceSum;
        private double sessionPriceSumSq;
        private int sessionPriceCount;

        private double sessionBuyVwapPriceQtySum;
        private double sessionBuyVwapQty;
        private double sessionSellVwapPriceQtySum;
        private double sessionSellVwapQty;

        // Cancel VWAP Accumulators
        private double sessionCancelVwapSumPriceQty;
        private double sessionCancelVwapQty;
        private double sessionCancelBuyVwapSumPriceQty;
        private double sessionCancelBuyVwapQty;
        private double sessionCancelSellVwapSumPriceQty;
        private double sessionCancelSellVwapQty;

        // Dynamic Aggressor Flag Quality Metrics
        private double totalTradesWithAggressor = 0;

        // H-13: last processed trade price from the event stream (for VWAP distance).
        private double lastTradePrice = 0;

        // H-05: reprice events (order moved price/side) tracked separately from cancel+arrival.
        private double totalRepriceCount = 0;
        private double totalRepriceQty = 0;

        // H-07: first-event timestamps for window warm-up tracking.
        // A window is only "warm" once its full duration has elapsed since the first event.
        private DateTime? firstTradeTime = null;
        private DateTime? firstL2Time = null;

        // Incremental sliding-window aggregators (replace the old LINQ-scanned queues)
        private readonly TradeWindowAggregator tradeAgg = new TradeWindowAggregator();
        private readonly L2WindowAggregator l2Agg = new L2WindowAggregator();

        // Level 2 Price Book Levels (price-tick -> total size). In MBP mode these are updated directly;
        // in MBO mode they are derived from the order-keyed book.
        // SortedDictionary keeps levels in price order so CalculateImbalance / GetBookPressure can
        // traverse the top-N levels with an O(log n + k) iterator instead of a full O(m log m) sort.
        // bidBook: reverse comparer → highest price first. askBook: default ascending → lowest price first.
        private readonly SortedDictionary<long, double> bidBook = new SortedDictionary<long, double>(Comparer<long>.Create((a, b) => b.CompareTo(a)));
        private readonly SortedDictionary<long, double> askBook = new SortedDictionary<long, double>();

        // -------- MBO (order-level) ingestion, gated by MboMode --------
        // When true, ProcessLevel2 interprets each (id, price, size) as a single order keyed by id,
        // maintaining an order-keyed book and keeping bidBook/askBook (and best) in sync. This makes the
        // arrival/cancel/spoof metrics order-accurate. Default false preserves the price-aggregated path.
        public bool MboMode { get; set; } = false;

        /// <summary>True once at least one bid and one ask have been processed — minimum two-sided BBO validity check.</summary>
        public bool HasTwoSidedBook => hasBestBid && hasBestAsk;

        private sealed class MboLevelRef { public long Ticks; public double Size; public bool IsBid; }
        private readonly Dictionary<string, MboLevelRef> mboOrders = new Dictionary<string, MboLevelRef>();

        // Fast lookup for spoof detection in MBO mode: orderId → the L2AdditionTracker recorded when
        // the order was added. Enables exact add→cancel matching without a price+time heuristic.
        private readonly Dictionary<string, L2AdditionTracker> mboAddByOrderId = new Dictionary<string, L2AdditionTracker>();

        // Symmetric lattice (grid/ladder) detection. Operates on price LEVELS (bidBook/askBook), so it
        // is available in both MBO and MBP mode. A grid maker rests similar-size orders at regular tick
        // spacing on both sides; we look for a regularly-spaced run of similar-size levels per side,
        // require bid/ask symmetry, and gate the reported confidence on temporal PERSISTENCE — a real
        // ladder sits across consecutive scans, transient clusters do not. NOTE: without participant
        // IDs this is a structural "lattice score", not proof of a single market maker (audit H-14).
        private struct LatticeResult
        {
            public long RungSize;     // representative resting size per rung
            public int BidRungs;
            public int AskRungs;
            public int SpacingTicks;
            public double Symmetry;
            public double RawScore;   // pre-persistence structural score in [0,1]
        }
        private LatticeResult lattice;        // last structural scan
        private double latticeScore;          // persistence-weighted confidence in [0,1]
        private int latticePersist;           // consecutive scans matching the same (size, spacing)
        private long latticeLastSize, latticeLastSpacing;
        private long latticeLastScanTicks;
        private const long LATTICE_SCAN_INTERVAL_TICKS = 250 * TimeSpan.TicksPerMillisecond;

        // Per-price order counts (MBO mode only). Maintained in parallel with bidBook/askBook so that
        // the number of distinct orders at the best bid/ask can be read in O(1).
        private readonly Dictionary<long, int> bidCountBook = new Dictionary<long, int>();
        private readonly Dictionary<long, int> askCountBook = new Dictionary<long, int>();
        private int bestBidOrderCount;
        private int bestAskOrderCount;

        // Incremental best bid/ask (maintained in both modes; used by microprice + OFI).
        private bool hasBestBid, hasBestAsk;
        private long bestBidTicks, bestAskTicks;
        private double bestBidSize, bestAskSize;

        // Order Flow Imbalance (Cont–Kukanov–Stoikov), best level, summed over a short window.
        private const double OFI_WINDOW_SECONDS = 5.0;
        private readonly Queue<(long ticks, double v)> ofiWindow = new Queue<(long, double)>();
        private double ofiSum;

        // Cache of recent unmatched trades for passive fill matching (heuristics)
        private readonly List<TradeFillMatch> unmatchedTrades = new List<TradeFillMatch>();

        // Microstructure signal tracking collections
        private readonly List<L2AdditionTracker> recentAdditions = new List<L2AdditionTracker>();
        private readonly List<RecentExecutionTracker> recentExecutions = new List<RecentExecutionTracker>();
        private readonly Queue<IcebergEvent> icebergEvents = new Queue<IcebergEvent>();
        private readonly Queue<ReplenishmentEvent> replenishmentEvents = new Queue<ReplenishmentEvent>();
        private readonly Queue<SpoofEvent> spoofEvents = new Queue<SpoofEvent>();

        // Running bid/ask sums for the 60s microstructure event windows (incremental; updated at
        // enqueue and decremented at dequeue in PruneMicrostructureCollections — O(1) reads).
        private double _icebergBidSum, _icebergAskSum;
        private double _replenishBidSum, _replenishAskSum;
        private double _spoofBidSum, _spoofAskSum;

        public DataAnalyticsCalculator(Symbol symbol, FeedCapabilities capabilities = FeedCapabilities.None)
        {
            this.symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            this.Capabilities = capabilities;
            ApplyFallbackMtr();
        }

        public DataAnalyticsCalculator(Symbol symbol, double volShort, double volLong, int calibrationTrades, FeedCapabilities capabilities = FeedCapabilities.None)
            : this(symbol, capabilities)
        {
            this.TradeVolumeWindowShort = volShort;
            this.TradeVolumeWindowLong = volLong;
            this.CalibrationTradesThreshold = calibrationTrades;
        }

        #region Event Receivers

        public void ProcessTrade(Last last)
        {
            if (last == null) return;
            ProcessTrade(Core.Instance.TimeUtils.DateTimeUtcNow, last.Price, last.Size, last.AggressorFlag);
        }

        public void ProcessTrade(DateTime time, double price, double size, AggressorFlag aggressor)
        {
            if (price <= 0 || size <= 0)
                return;

            lock (locker)
            {
                long ticks = AdvanceTo(time.Ticks);

                // 1. Live calibration accumulation
                if (!IsCalibrated)
                {
                    calibrationTradesProcessed++;
                    calibrationTradedVolume += size;

                    if (calibrationTradesProcessed >= CalibrationTradesThreshold)
                        CompleteCalibration();
                }

                // Track aggressor coverage quality
                totalTradesCount++;
                firstTradeTime ??= time;
                bool isBuy = aggressor == AggressorFlag.Buy;
                bool isSell = aggressor == AggressorFlag.Sell;
                if (isBuy || isSell)
                    totalTradesWithAggressor++;

                // 2. Feed the incremental rolling aggregator
                tradeAgg.Add(ticks, price, size, isBuy, isSell);

                // Cache for matching passive executions
                unmatchedTrades.Add(new TradeFillMatch(time, price, size, aggressor));

                // Record recent executions for replenishment tracking
                recentExecutions.Add(new RecentExecutionTracker(time, price, isSell));

                // Mark recent additions at this price as filled (prevent spoof trigger)
                foreach (var add in recentAdditions)
                {
                    if (Math.Abs(add.Price - price) < 1e-9)
                        add.HadTrades = true;
                }

                // Iceberg heuristic check
                long priceTicks = (long)Math.Round(price / symbol.TickSize);
                double displayedQty = 0;
                if (isBuy)
                    askBook.TryGetValue(priceTicks, out displayedQty);
                else if (isSell)
                    bidBook.TryGetValue(priceTicks, out displayedQty);

                if (displayedQty > 0 && size > displayedQty * 1.5)
                {
                    double icebergVolume = size - displayedQty;
                    icebergEvents.Enqueue(new IcebergEvent(time, icebergVolume, isSell));
                    if (isSell) _icebergBidSum += icebergVolume; else _icebergAskSum += icebergVolume;
                }

                // 3. Update Session Cumulative Buyer/Seller trade metrics
                if (isBuy)
                {
                    totalBuyerTradeCount++;
                    totalBuyerTradeQty += size;

                    sessionBuyVwapPriceQtySum += (price * size);
                    sessionBuyVwapQty += size;
                }
                else if (isSell)
                {
                    totalSellerTradeCount++;
                    totalSellerTradeQty += size;

                    sessionSellVwapPriceQtySum += (price * size);
                    sessionSellVwapQty += size;
                }

                // 4. Update Session VWAP Metrics
                sessionVwapSumPriceQty += (price * size);
                sessionVwapQty += size;

                sessionPriceSum += price;
                sessionPriceSumSq += (price * price);
                sessionPriceCount++;
                lastTradePrice = price;  // H-13: track last event-stream price for VWAP distance

                PruneUnmatchedTrades(time);
                PruneMicrostructureCollections(time);
            }
        }

        public void ProcessLevel2(Level2Quote quote)
        {
            if (quote == null) return;
            bool isBid = quote.PriceType == QuotePriceType.Bid;
            ProcessLevel2(Core.Instance.TimeUtils.DateTimeUtcNow, quote.Id, quote.Price, quote.Size, isBid, quote.Closed);
        }

        public void ProcessLevel2(DateTime time, string? id, double price, double size, bool isBid, bool closed)
        {
            // MBP path only. In MBO mode the host routes events through ProcessMboEvent instead.
            // C-04: a feed reset (FLUSH) commonly carries zero price/size, so detect it BEFORE the
            // price guard — otherwise the reset marker is silently dropped and stale orders persist.
            if (string.Equals(id, "FLUSH", StringComparison.OrdinalIgnoreCase))
            {
                lock (locker) { Reset(); }
                return;
            }
            if (price <= 0) return;

            lock (locker)
            {
                AdvanceTo(time.Ticks);

                if (!IsCalibrated)
                    calibrationL2MessagesCount++;

                bool pHasBid = hasBestBid, pHasAsk = hasBestAsk;
                long pBidT = bestBidTicks, pAskT = bestAskTicks;
                double pBidSz = bestBidSize, pAskSz = bestAskSize;

                long priceTicks = (long)Math.Round(price / symbol.TickSize);
                var book = isBid ? bidBook : askBook;
                double newQty = closed ? 0 : size;

                if (book.TryGetValue(priceTicks, out double previousQty))
                {
                    if (newQty <= 0)
                    {
                        book.Remove(priceTicks);
                        if (previousQty > 0) HandleL2Reduction(time, price, previousQty, isBid);
                    }
                    else if (newQty < previousQty)
                    {
                        book[priceTicks] = newQty;
                        HandleL2Reduction(time, price, previousQty - newQty, isBid);
                    }
                    else if (newQty > previousQty)
                    {
                        book[priceTicks] = newQty;
                        RecordL2Addition(time, price, newQty - previousQty, isBid);
                    }
                }
                else if (newQty > 0)
                {
                    book[priceTicks] = newQty;
                    RecordL2Addition(time, price, newQty, isBid);
                }

                UpdateBest(isBid, priceTicks, book.TryGetValue(priceTicks, out var ft) ? ft : 0);
                AccumulateOfi(pHasBid, pBidT, pBidSz, pHasAsk, pAskT, pAskSz);
                PruneUnmatchedTrades(time);
                PruneMicrostructureCollections(time);
            }
        }

        /// <summary>
        /// Processes a normalized MBO event produced by the host's MboOrderBook. This is the primary
        /// L2 entry point when MboMode is active. Unlike ProcessLevel2, it receives a pre-classified
        /// action (Add/Update/Remove) and the stable order ID, enabling exact per-order spoof detection
        /// and accurate arrival/cancel counting without price+time heuristics.
        /// </summary>
        public void ProcessMboEvent(MboEvent evt)
        {
            // C-04: detect FLUSH reset markers before the price guard — they commonly carry zero
            // price/size, so the previous price<=0 early-return dropped them and left a stale book.
            if (string.Equals(evt.OrderId, "FLUSH", StringComparison.OrdinalIgnoreCase))
            {
                lock (locker) { Reset(); }
                return;
            }
            if (evt.Price <= 0) return;
            if (evt.Action == MboAction.Trade || evt.Action == MboAction.Snapshot) return;

            lock (locker)
            {
                AdvanceTo(evt.Time.Ticks);

                if (!IsCalibrated)
                    calibrationL2MessagesCount++;

                bool pHasBid = hasBestBid, pHasAsk = hasBestAsk;
                long pBidT = bestBidTicks, pAskT = bestAskTicks;
                double pBidSz = bestBidSize, pAskSz = bestAskSize;

                double ts = symbol.TickSize > 0 ? symbol.TickSize : 1.0;
                long tick = (long)Math.Round(evt.Price / ts);
                bool isBid = evt.IsBid;
                string orderId = evt.OrderId;

                switch (evt.Action)
                {
                    case MboAction.Add:
                    {
                        // H-04: idempotent upsert — purge stale state if this order ID already exists
                        // (duplicate Add from the feed) to prevent double-counting size and order count.
                        bool isDuplicate = mboOrders.TryGetValue(orderId, out var dup);
                        if (isDuplicate && dup != null)
                        {
                            double purged = ApplyPriceLevelDelta(dup.IsBid, dup.Ticks, -dup.Size);
                            UpdateBest(dup.IsBid, dup.Ticks, purged);
                            CountBookDec(dup.IsBid, dup.Ticks);
                        }
                        double nt = ApplyPriceLevelDelta(isBid, tick, evt.Size);
                        mboOrders[orderId] = new MboLevelRef { Ticks = tick, Size = evt.Size, IsBid = isBid };
                        UpdateBest(isBid, tick, nt);
                        CountBookInc(isBid, tick);
                        if (!isDuplicate) RecordL2Addition(evt.Time, evt.Price, evt.Size, isBid, orderId);
                        break;
                    }
                    case MboAction.Update:
                    {
                        if (mboOrders.TryGetValue(orderId, out var existing))
                        {
                            if (existing.Ticks == tick && existing.IsBid == isBid)
                            {
                                double delta = evt.Size - existing.Size;
                                if (delta != 0)
                                {
                                    double nt = ApplyPriceLevelDelta(isBid, tick, delta);
                                    existing.Size = evt.Size;
                                    UpdateBest(isBid, tick, nt);
                                    if (delta > 0) RecordL2Addition(evt.Time, evt.Price, delta, isBid, orderId);
                                    else HandleL2Reduction(evt.Time, evt.Price, -delta, isBid, orderId);
                                }
                            }
                            else
                            {
                                // H-05: order repriced or changed side — update book state but do NOT
                                // route through HandleL2Reduction+RecordL2Addition, which would inflate
                                // cancel and arrival counts. Track as a distinct reprice event instead.
                                double oldTotal = ApplyPriceLevelDelta(existing.IsBid, existing.Ticks, -existing.Size);
                                UpdateBest(existing.IsBid, existing.Ticks, oldTotal);
                                CountBookDec(existing.IsBid, existing.Ticks);
                                double newTotal = ApplyPriceLevelDelta(isBid, tick, evt.Size);
                                existing.Ticks = tick; existing.Size = evt.Size; existing.IsBid = isBid;
                                UpdateBest(isBid, tick, newTotal);
                                CountBookInc(isBid, tick);
                                totalRepriceCount++;
                                totalRepriceQty += evt.Size;
                            }
                        }
                        else
                        {
                            // Unseen order: treat as Add
                            double nt = ApplyPriceLevelDelta(isBid, tick, evt.Size);
                            mboOrders[orderId] = new MboLevelRef { Ticks = tick, Size = evt.Size, IsBid = isBid };
                            UpdateBest(isBid, tick, nt);
                            CountBookInc(isBid, tick);
                            RecordL2Addition(evt.Time, evt.Price, evt.Size, isBid, orderId);
                        }
                        break;
                    }
                    case MboAction.Remove:
                    {
                        if (mboOrders.TryGetValue(orderId, out var o))
                        {
                            double nt = ApplyPriceLevelDelta(o.IsBid, o.Ticks, -o.Size);
                            mboOrders.Remove(orderId);
                            UpdateBest(o.IsBid, o.Ticks, nt);
                            CountBookDec(o.IsBid, o.Ticks);
                            HandleL2Reduction(evt.Time, o.Ticks * ts, o.Size, o.IsBid, orderId);
                        }
                        break;
                    }
                }

                RefreshBestCounts();
                AccumulateOfi(pHasBid, pBidT, pBidSz, pHasAsk, pAskT, pAskSz);
                PruneUnmatchedTrades(evt.Time);
                PruneMicrostructureCollections(evt.Time);
            }
        }

        public void ProcessLevel2Item(Level2Item item, bool isBid)
        {
            if (item == null) return;
            ProcessLevel2Item(item.Price, item.Size, isBid);
        }

        public void ProcessLevel2Item(double price, double size, bool isBid)
        {
            if (price <= 0 || size <= 0)
                return;

            lock (locker)
            {
                long priceTicks = (long)Math.Round(price / symbol.TickSize);
                var book = isBid ? bidBook : askBook;
                book[priceTicks] = size;
                UpdateBest(isBid, priceTicks, size);
            }
        }

        /// <summary>
        /// Seeds a single per-order entry into the MBO book during startup.
        /// Unlike ProcessLevel2, this skips calibration counters and pruning — call it
        /// only before the worker thread starts (i.e. from SeedBookSnapshot / RecoverFromOverflow).
        /// </summary>
        public void ProcessLevel2ItemMbo(string id, double price, double size, bool isBid)
        {
            if (string.IsNullOrEmpty(id) || price <= 0 || size <= 0) return;
            lock (locker)
            {
                double ts = symbol.TickSize > 0 ? symbol.TickSize : 1.0;
                long tick = (long)Math.Round(price / ts);
                double nt = ApplyPriceLevelDelta(isBid, tick, size);
                mboOrders[id] = new MboLevelRef { Ticks = tick, Size = size, IsBid = isBid };
                UpdateBest(isBid, tick, nt);
                var cBook = isBid ? bidCountBook : askCountBook;
                cBook[tick] = (cBook.TryGetValue(tick, out int cx) ? cx : 0) + 1;
                RefreshBestCounts();
            }
        }

        /// <summary>
        /// Advances the event-time clock (and decays all sliding windows) to the supplied time
        /// without ingesting an event. Call this from the snapshot publisher so that, during quiet
        /// periods, rolling windows still decay toward zero. In live mode pass the platform UTC now;
        /// in replay mode pass the timestamp of the last processed event.
        /// </summary>
        public void AdvanceTime(DateTime now)
        {
            lock (locker)
            {
                AdvanceTo(now.Ticks);
            }
        }

        private long AdvanceTo(long ticks)
        {
            // Clamp to monotonic non-decreasing time so the windowed aggregators stay consistent
            // even if trade and L2 timestamps interleave slightly out of order.
            if (ticks < currentTicks)
                ticks = currentTicks;
            else
                currentTicks = ticks;

            tradeAgg.Advance(currentTicks);
            l2Agg.Advance(currentTicks);

            long ofiCutoff = currentTicks - (long)(OFI_WINDOW_SECONDS * TimeSpan.TicksPerSecond);
            while (ofiWindow.Count > 0 && ofiWindow.Peek().ticks < ofiCutoff)
                ofiSum -= ofiWindow.Dequeue().v;

            // Throttled symmetric-lattice scan (~4/s of event time, MBO or MBP).
            if (currentTicks - latticeLastScanTicks >= LATTICE_SCAN_INTERVAL_TICKS)
            {
                latticeLastScanTicks = currentTicks;
                UpdateLatticeDetector();
            }

            return ticks;
        }

        public void Reset()
        {
            lock (locker)
            {
                tradeAgg.Clear();
                l2Agg.Clear();
                bidBook.Clear();
                askBook.Clear();
                ClearBookDerivedState();
                unmatchedTrades.Clear();

                recentAdditions.Clear();
                recentExecutions.Clear();
                icebergEvents.Clear();
                replenishmentEvents.Clear();
                spoofEvents.Clear();
                _icebergBidSum = _icebergAskSum = 0;
                _replenishBidSum = _replenishAskSum = 0;
                _spoofBidSum = _spoofAskSum = 0;

                totalTradesCount = 0;
                totalTradesWithAggressor = 0;

                if (IsManualMode)
                {
                    IsCalibrated = true;
                }
                else
                {
                    IsCalibrated = false;
                    calibrationTradesProcessed = 0;
                    calibrationTradedVolume = 0;
                    calibrationL2MessagesCount = 0;
                    ApplyFallbackMtr();
                }

                ClearSessionAccumulators();
            }
        }

        public void ResetBookStateOnly()
        {
            lock (locker)
            {
                bidBook.Clear();
                askBook.Clear();
                ClearBookDerivedState();
                l2Agg.Clear();
                recentAdditions.Clear();
                recentExecutions.Clear();
                // M-04: pattern queues are derived from book state; stale entries produce ghost signals after reconnect
                icebergEvents.Clear();
                replenishmentEvents.Clear();
                spoofEvents.Clear();
                _icebergBidSum = _icebergAskSum = 0;
                _replenishBidSum = _replenishAskSum = 0;
                _spoofBidSum = _spoofAskSum = 0;
            }
        }

        public void ResetSessionState()
        {
            lock (locker)
            {
                ClearSessionAccumulators();

                tradeAgg.Clear();
                l2Agg.Clear();
                unmatchedTrades.Clear();

                recentAdditions.Clear();
                recentExecutions.Clear();
                icebergEvents.Clear();
                replenishmentEvents.Clear();
                spoofEvents.Clear();
                _icebergBidSum = _icebergAskSum = 0;
                _replenishBidSum = _replenishAskSum = 0;
                _spoofBidSum = _spoofAskSum = 0;
            }
        }

        private void ClearBookDerivedState()
        {
            mboOrders.Clear();
            mboAddByOrderId.Clear();
            lattice = default;
            latticeScore = 0.0;
            latticePersist = 0;
            latticeLastSize = latticeLastSpacing = 0;
            latticeLastScanTicks = 0; // M-04: force a fresh scan on the next event after a reseed
            bidCountBook.Clear();
            askCountBook.Clear();
            bestBidOrderCount = 0;
            bestAskOrderCount = 0;
            hasBestBid = hasBestAsk = false;
            bestBidTicks = bestAskTicks = 0;
            bestBidSize = bestAskSize = 0;
            ofiWindow.Clear();
            ofiSum = 0;
        }

        private const long LATTICE_WINDOW_TICKS = 100; // look this far from best for ladder rungs
        private const int LATTICE_MIN_RUNGS = 3;       // minimum regularly-spaced levels per side
        private const int LATTICE_FULL_RUNGS = 5;      // depth bonus saturates here
        private const int LATTICE_MIN_PERSIST = 4;     // scans (~1s) before confidence reaches RawScore
        private const int LATTICE_MAX_PERSIST = 8;

        // Runs the structural scan and folds it into a persistence-weighted confidence. Called on a
        // throttled cadence from AdvanceTo, under locker.
        private void UpdateLatticeDetector()
        {
            lattice = ScanSymmetricLattice();

            // Persistence: a real ladder keeps the same (rung size, spacing) across consecutive scans.
            bool sameAsLast = lattice.RawScore > 0 &&
                              lattice.RungSize == latticeLastSize &&
                              lattice.SpacingTicks == latticeLastSpacing;
            if (sameAsLast)
                latticePersist = Math.Min(LATTICE_MAX_PERSIST, latticePersist + 1);
            else
                latticePersist = lattice.RawScore > 0 ? 1 : 0;

            latticeLastSize = lattice.RungSize;
            latticeLastSpacing = lattice.SpacingTicks;

            double persistFrac = Math.Min(1.0, latticePersist / (double)LATTICE_MIN_PERSIST);
            latticeScore = lattice.RawScore * persistFrac;
        }

        private LatticeResult ScanSymmetricLattice()
        {
            if (!hasBestBid || !hasBestAsk) return default;

            var bid = FindLadder(bidBook, isBid: true, bestBidTicks);
            var ask = FindLadder(askBook, isBid: false, bestAskTicks);

            if (bid.rungs < LATTICE_MIN_RUNGS || ask.rungs < LATTICE_MIN_RUNGS) return default;
            if (bid.spacing < 1 || Math.Abs(bid.spacing - ask.spacing) > 1) return default; // symmetric spacing

            // Symmetric rung size (the two sides quote a similar size).
            double sizeSym = (double)Math.Min(bid.size, ask.size) / Math.Max(bid.size, ask.size);
            if (sizeSym < 0.5) return default;

            double rungSym = (double)Math.Min(bid.rungs, ask.rungs) / Math.Max(bid.rungs, ask.rungs);
            double reg = (bid.regularity + ask.regularity) / 2.0;
            double depthBonus = Math.Min(1.0, Math.Min(bid.rungs, ask.rungs) / (double)LATTICE_FULL_RUNGS);
            double raw = reg * Math.Sqrt(rungSym) * sizeSym * depthBonus;

            return new LatticeResult
            {
                RungSize = (bid.size + ask.size) / 2,
                BidRungs = bid.rungs,
                AskRungs = ask.rungs,
                SpacingTicks = (bid.spacing + ask.spacing) / 2,
                Symmetry = rungSym,
                RawScore = raw
            };
        }

        // Finds the strongest regularly-spaced run of similar-size price LEVELS on one side, within a
        // window of the best. Groups levels by rounded resting size, then for each size band finds the
        // longest contiguous run of levels spaced at the modal gap. Returns (#rungs, spacing, size, reg).
        private (int rungs, int spacing, long size, double regularity) FindLadder(
            SortedDictionary<long, double> book, bool isBid, long bestTick)
        {
            var bySize = new Dictionary<long, List<long>>();
            foreach (var kv in book)
            {
                if (kv.Value <= 0) continue;
                long dist = isBid ? bestTick - kv.Key : kv.Key - bestTick;
                if (dist < 0 || dist > LATTICE_WINDOW_TICKS) continue;
                long sz = (long)Math.Round(kv.Value);
                if (sz <= 0) continue;
                if (!bySize.TryGetValue(sz, out var lst)) bySize[sz] = lst = new List<long>();
                lst.Add(kv.Key);
            }

            int bestRungs = 0, bestSpacing = 0; long bestSize = 0; double bestReg = 0.0;
            foreach (var kv in bySize)
            {
                var ticks = kv.Value;
                if (ticks.Count < LATTICE_MIN_RUNGS) continue;
                ticks.Sort();

                int g = ModalGap(ticks);
                if (g <= 0) continue;

                int onGrid = 0, gaps = ticks.Count - 1, longest = 1, cur = 1;
                for (int i = 1; i < ticks.Count; i++)
                {
                    if (ticks[i] - ticks[i - 1] == g) { onGrid++; cur++; if (cur > longest) longest = cur; }
                    else cur = 1;
                }
                if (longest < LATTICE_MIN_RUNGS) continue;
                double reg = gaps > 0 ? onGrid / (double)gaps : 0.0;

                if (longest > bestRungs || (longest == bestRungs && reg > bestReg))
                {
                    bestRungs = longest; bestSpacing = g; bestSize = kv.Key; bestReg = reg;
                }
            }
            return (bestRungs, bestSpacing, bestSize, bestReg);
        }

        private static int ModalGap(List<long> sortedTicks)
        {
            var counts = new Dictionary<long, int>();
            for (int i = 1; i < sortedTicks.Count; i++)
            {
                long g = sortedTicks[i] - sortedTicks[i - 1];
                if (g <= 0) continue;
                counts[g] = (counts.TryGetValue(g, out int c) ? c : 0) + 1;
            }
            int bestC = 0; long bestG = 0;
            foreach (var kv in counts)
                if (kv.Value > bestC) { bestC = kv.Value; bestG = kv.Key; }
            return (int)bestG;
        }

        private void CountBookInc(bool isBid, long tick)
        {
            var book = isBid ? bidCountBook : askCountBook;
            book[tick] = (book.TryGetValue(tick, out int c) ? c : 0) + 1;
        }

        private void CountBookDec(bool isBid, long tick)
        {
            var book = isBid ? bidCountBook : askCountBook;
            if (book.TryGetValue(tick, out int c))
            {
                if (c <= 1) book.Remove(tick);
                else book[tick] = c - 1;
            }
        }

        private void RefreshBestCounts()
        {
            bestBidOrderCount = hasBestBid && bidCountBook.TryGetValue(bestBidTicks, out var bc) ? bc : 0;
            bestAskOrderCount = hasBestAsk && askCountBook.TryGetValue(bestAskTicks, out var ac) ? ac : 0;
        }

        private void ClearSessionAccumulators()
        {
            totalBuyerTradeCount = 0;
            totalSellerTradeCount = 0;
            totalBuyerTradeQty = 0;
            totalSellerTradeQty = 0;
            totalTradesCount = 0;
            totalTradesWithAggressor = 0;  // H-10: missing here caused coverage to exceed 100% after rollover
            firstTradeTime = null;          // H-07: reset warm-up clock on session rollover
            firstL2Time = null;             // H-07
            lastTradePrice = 0;             // H-13
            totalRepriceCount = 0;          // H-05
            totalRepriceQty = 0;            // H-05

            totalArrivedOrderCount = 0;
            totalArrivedOrderQty = 0;
            totalCancelledOrderCount = 0;
            totalCancelledOrderQty = 0;

            totalCancelledBuyOrderCount = 0;
            totalCancelledSellOrderCount = 0;
            totalCancelledBuyOrderQty = 0;
            totalCancelledSellOrderQty = 0;

            sessionVwapSumPriceQty = 0;
            sessionVwapQty = 0;
            sessionPriceSum = 0;
            sessionPriceSumSq = 0;
            sessionPriceCount = 0;

            sessionBuyVwapPriceQtySum = 0;
            sessionBuyVwapQty = 0;
            sessionSellVwapPriceQtySum = 0;
            sessionSellVwapQty = 0;

            sessionCancelVwapSumPriceQty = 0;
            sessionCancelVwapQty = 0;

            sessionCancelBuyVwapSumPriceQty = 0;
            sessionCancelBuyVwapQty = 0;

            sessionCancelSellVwapSumPriceQty = 0;
            sessionCancelSellVwapQty = 0;
        }

        #endregion

        #region Internal Pipeline Processing & Pruning

        private void ApplyFallbackMtr()
        {
            OrderCountWindowShort = (int)(TradeVolumeWindowShort * FALLBACK_MTR);
            OrderCountWindowLong = (int)(TradeVolumeWindowLong * FALLBACK_MTR);
        }

        private void CompleteCalibration()
        {
            if (calibrationTradedVolume > 0)
            {
                double mtr = calibrationL2MessagesCount / calibrationTradedVolume;

                OrderCountWindowShort = (int)Math.Max(100, TradeVolumeWindowShort * mtr);
                OrderCountWindowLong = (int)Math.Max(500, TradeVolumeWindowLong * mtr);

                // Auto-calibrate absorption threshold only when no manual override was set.
                // 2× the typical 5s trading rate: fires only when volume is unusually concentrated
                // at a single price level within the 5s window.
                if (AbsorptionVolumeThreshold <= 0)
                {
                    double typical5sVol = TradeVolumeWindowShort / 12.0;
                    AbsorptionVolumeThreshold = Math.Max(10, typical5sVol * 2.0);
                }

                IsCalibrated = true;

                Core.Instance.Loggers.Log(
                    $"Calibration complete for {symbol.Name}. MTR: {mtr:F2} (L2 messages: {calibrationL2MessagesCount}, Trades Volume: {calibrationTradedVolume}). Calibrated Windows -> Short: {OrderCountWindowShort} updates, Long: {OrderCountWindowLong} updates. Absorption threshold: {AbsorptionVolumeThreshold:F0} contracts/5s.",
                    LoggingLevel.System
                );
            }
            else
            {
                ApplyFallbackMtr();
            }
        }

        private double ApplyPriceLevelDelta(bool isBid, long tick, double delta)
        {
            var book = isBid ? bidBook : askBook;
            double cur = book.TryGetValue(tick, out var v) ? v : 0;
            double nt = cur + delta;
            if (nt <= 1e-9) { book.Remove(tick); return 0; }
            book[tick] = nt;
            return nt;
        }

        // Incremental best-quote maintenance (both modes). Rescans only when the best level empties.
        private void UpdateBest(bool isBid, long tick, double newTotal)
        {
            if (isBid)
            {
                if (newTotal > 0)
                {
                    if (!hasBestBid || tick > bestBidTicks) { hasBestBid = true; bestBidTicks = tick; bestBidSize = newTotal; }
                    else if (tick == bestBidTicks) bestBidSize = newTotal;
                }
                else if (hasBestBid && tick == bestBidTicks)
                {
                    RescanBest(true);
                }
            }
            else
            {
                if (newTotal > 0)
                {
                    if (!hasBestAsk || tick < bestAskTicks) { hasBestAsk = true; bestAskTicks = tick; bestAskSize = newTotal; }
                    else if (tick == bestAskTicks) bestAskSize = newTotal;
                }
                else if (hasBestAsk && tick == bestAskTicks)
                {
                    RescanBest(false);
                }
            }
        }

        private void RescanBest(bool isBid)
        {
            // SortedDictionary iteration starts at the logical "first" entry (highest bid / lowest ask),
            // so the best quote is always the first element — no full scan needed.
            if (isBid)
            {
                hasBestBid = bidBook.Count > 0;
                bestBidSize = 0;
                if (hasBestBid) { var e = bidBook.First(); bestBidTicks = e.Key; bestBidSize = e.Value; }
            }
            else
            {
                hasBestAsk = askBook.Count > 0;
                bestAskSize = 0;
                if (hasBestAsk) { var e = askBook.First(); bestAskTicks = e.Key; bestAskSize = e.Value; }
            }
        }

        // Cont–Kukanov–Stoikov best-level OFI contribution from the pre/post best quotes.
        private void AccumulateOfi(bool pHasBid, long pBidT, double pBidSz, bool pHasAsk, long pAskT, double pAskSz)
        {
            if (!pHasBid || !pHasAsk || !hasBestBid || !hasBestAsk)
                return; // OFI needs a two-sided book before and after the update

            double eb = bestBidTicks > pBidT ? bestBidSize
                      : bestBidTicks == pBidT ? bestBidSize - pBidSz
                      : -pBidSz;

            double ea = bestAskTicks < pAskT ? bestAskSize
                      : bestAskTicks == pAskT ? bestAskSize - pAskSz
                      : -pAskSz;

            double ofi = eb - ea;
            if (ofi != 0)
            {
                ofiWindow.Enqueue((currentTicks, ofi));
                ofiSum += ofi;
            }
        }

        private void RecordL2Addition(DateTime time, double price, double size, bool isBid, string? orderId = null)
        {
            l2Agg.AddAddition(time.Ticks, size, isBid);
            firstL2Time ??= time;
            totalArrivedOrderCount++;
            totalArrivedOrderQty += size;

            var tracker = new L2AdditionTracker(time, price, size, isBid, orderId);
            recentAdditions.Add(tracker);

            // In MBO mode register by order ID for O(1) spoof lookup in HandleL2Reduction.
            if (MboMode && !string.IsNullOrEmpty(orderId))
                mboAddByOrderId[orderId] = tracker;

            // Replenishment: new order added at a price where an execution happened recently.
            var matchedExec = recentExecutions.FirstOrDefault(x =>
                Math.Abs(x.Price - price) < 1e-9 &&
                x.IsBid == isBid &&
                (time - x.Time).TotalMilliseconds <= 1000.0);

            if (matchedExec != null)
            {
                replenishmentEvents.Enqueue(new ReplenishmentEvent(time, size, isBid));
                if (isBid) _replenishBidSum += size; else _replenishAskSum += size;
            }
        }

        private void HandleL2Reduction(DateTime time, double price, double reductionSize, bool isBid, string? orderId = null)
        {
            double matchTradeQty = 0;
            AggressorFlag expectedAggressor = isBid ? AggressorFlag.Sell : AggressorFlag.Buy;

            for (int i = 0; i < unmatchedTrades.Count; i++)
            {
                var trade = unmatchedTrades[i];
                if (Math.Abs(trade.Price - price) < 1e-9 && trade.Side == expectedAggressor && trade.RemainingQty > 0)
                {
                    double needed = reductionSize - matchTradeQty;
                    double matched = Math.Min(needed, trade.RemainingQty);
                    trade.RemainingQty -= matched;
                    matchTradeQty += matched;
                    if (matchTradeQty >= reductionSize) break;
                }
            }

            unmatchedTrades.RemoveAll(t => t.RemainingQty <= 0);

            double cancelledQty = Math.Max(0, reductionSize - matchTradeQty);

            if (cancelledQty > 0)
            {
                l2Agg.AddCancellation(time.Ticks, cancelledQty, isBid);
                totalCancelledOrderCount++;
                totalCancelledOrderQty += cancelledQty;
                if (isBid)
                {
                    totalCancelledBuyOrderCount++;
                    totalCancelledBuyOrderQty += cancelledQty;
                    sessionCancelBuyVwapSumPriceQty += (price * cancelledQty);
                    sessionCancelBuyVwapQty += cancelledQty;
                }
                else
                {
                    totalCancelledSellOrderCount++;
                    totalCancelledSellOrderQty += cancelledQty;
                    sessionCancelSellVwapSumPriceQty += (price * cancelledQty);
                    sessionCancelSellVwapQty += cancelledQty;
                }
                sessionCancelVwapSumPriceQty += (price * cancelledQty);
                sessionCancelVwapQty += cancelledQty;

                // Spoof detection.
                // MBO mode: look up the exact order that was added — no price+time ambiguity.
                // MBP mode: fall back to the price+time heuristic.
                L2AdditionTracker? matchAdd;
                if (MboMode && !string.IsNullOrEmpty(orderId))
                {
                    if (mboAddByOrderId.TryGetValue(orderId, out var addRecord) &&
                        !addRecord.HadTrades &&
                        (time - addRecord.Time).TotalMilliseconds <= 1000.0)
                    {
                        matchAdd = addRecord;
                        mboAddByOrderId.Remove(orderId);
                        recentAdditions.Remove(matchAdd);
                    }
                    else
                    {
                        matchAdd = null;
                    }
                }
                else
                {
                    matchAdd = recentAdditions.FirstOrDefault(a =>
                        Math.Abs(a.Price - price) < 1e-9 &&
                        a.IsBid == isBid &&
                        (time - a.Time).TotalMilliseconds <= 1000.0 &&
                        !a.HadTrades);
                    if (matchAdd != null) recentAdditions.Remove(matchAdd);
                }

                if (matchAdd != null)
                {
                    spoofEvents.Enqueue(new SpoofEvent(time, cancelledQty, isBid));
                    if (isBid) _spoofBidSum += cancelledQty; else _spoofAskSum += cancelledQty;
                }
            }
        }

        private void PruneUnmatchedTrades(DateTime time)
        {
            unmatchedTrades.RemoveAll(t => (time - t.Time).TotalMilliseconds > 200.0);
        }

        private void PruneMicrostructureCollections(DateTime time)
        {
            recentAdditions.RemoveAll(x =>
            {
                bool expired = (time - x.Time).TotalMilliseconds > 5000.0;
                if (expired && x.OrderId != null) mboAddByOrderId.Remove(x.OrderId);
                return expired;
            });
            recentExecutions.RemoveAll(x => (time - x.Time).TotalMilliseconds > 5000.0);

            while (icebergEvents.Count > 0 && (time - icebergEvents.Peek().Time).TotalSeconds > 60)
            {
                var e = icebergEvents.Dequeue();
                if (e.IsBid) _icebergBidSum -= e.Qty; else _icebergAskSum -= e.Qty;
            }

            while (replenishmentEvents.Count > 0 && (time - replenishmentEvents.Peek().Time).TotalSeconds > 60)
            {
                var e = replenishmentEvents.Dequeue();
                if (e.IsBid) _replenishBidSum -= e.Qty; else _replenishAskSum -= e.Qty;
            }

            while (spoofEvents.Count > 0 && (time - spoofEvents.Peek().Time).TotalSeconds > 60)
            {
                var e = spoofEvents.Dequeue();
                if (e.IsBid) _spoofBidSum -= e.Qty; else _spoofAskSum -= e.Qty;
            }
        }

        #endregion

        #region Dynamic Metrics Retrieval

        private MetricValue CreateMetric(double value, MetricQuality quality, bool isWarm)
        {
            long receiveTicks = System.Diagnostics.Stopwatch.GetTimestamp();
            // currentTicks is DateTime.Ticks (100-ns intervals since year 1).
            // Subtracting UnixEpoch before multiplying avoids signed-64 overflow on 2026+ dates.
            long eventTimeNs = (currentTicks - DateTime.UnixEpoch.Ticks) * 100L;
            return new MetricValue(value, quality, eventTimeNs, receiveTicks, isWarm);
        }

        private bool IsTradeWarm(int seconds)
        {
            if (!IsCalibrated || !firstTradeTime.HasValue) return false;
            if (tradeAgg.TotalCount(WindowForSeconds(seconds)) == 0) return false;
            return (new DateTime(currentTicks, DateTimeKind.Utc) - firstTradeTime.Value).TotalSeconds >= seconds;
        }

        private bool IsL2Warm()
        {
            if (!IsCalibrated || !firstL2Time.HasValue) return false;
            if (l2Agg.AddCount(TradeWindows.L2Short) + l2Agg.CancelCount(TradeWindows.L2Short) == 0) return false;
            return (new DateTime(currentTicks, DateTimeKind.Utc) - firstL2Time.Value).TotalSeconds >= 60;
        }

        private MetricQuality GetTradeQuality()
        {
            if (!Capabilities.HasFlag(FeedCapabilities.TradeAggressor))
                return MetricQuality.Heuristic;

            if (totalTradesCount > 0)
            {
                double coverage = totalTradesWithAggressor / totalTradesCount;
                if (coverage > 0.95) return MetricQuality.Exact;
                if (coverage > 0.50) return MetricQuality.Derived;
            }

            return MetricQuality.Heuristic;
        }

        private static int WindowForSeconds(int seconds)
        {
            switch (seconds)
            {
                case 1: return TradeWindows.S1;
                case 2: return TradeWindows.S2;
                case 5: return TradeWindows.S5;
                case 30: return TradeWindows.S30;
                case 60: return TradeWindows.S60;
                case 300: return TradeWindows.M5;
                case 900: return TradeWindows.M15;
                default: return TradeWindows.S60;
            }
        }

        // --- 4. Buyer/Seller Rolling 60s Metrics ---

        public MetricValue GetBuyTradeCount60() => CreateMetric(tradeAgg.BuyCount(TradeWindows.S60), GetTradeQuality(), IsTradeWarm(60));
        public MetricValue GetSellTradeCount60() => CreateMetric(tradeAgg.SellCount(TradeWindows.S60), GetTradeQuality(), IsTradeWarm(60));
        public MetricValue GetBuyVolume60() => CreateMetric(tradeAgg.BuyQty(TradeWindows.S60), GetTradeQuality(), IsTradeWarm(60));
        public MetricValue GetSellVolume60() => CreateMetric(tradeAgg.SellQty(TradeWindows.S60), GetTradeQuality(), IsTradeWarm(60));

        public MetricValue GetBuySellCountRatio60()
            => MakeRatio(tradeAgg.BuyCount(TradeWindows.S60), tradeAgg.SellCount(TradeWindows.S60), GetTradeQuality(), IsTradeWarm(60));

        public MetricValue GetBuySellVolumeRatio60()
            => MakeRatio(tradeAgg.BuyQty(TradeWindows.S60), tradeAgg.SellQty(TradeWindows.S60), GetTradeQuality(), IsTradeWarm(60));

        // --- 4.4 Session Cumulative Metrics ---

        public MetricValue GetCumBuyVolume() => CreateMetric(totalBuyerTradeQty, GetTradeQuality(), IsCalibrated);
        public MetricValue GetCumSellVolume() => CreateMetric(totalSellerTradeQty, GetTradeQuality(), IsCalibrated);
        public MetricValue GetCumDelta() => CreateMetric(totalBuyerTradeQty - totalSellerTradeQty, GetTradeQuality(), IsCalibrated);
        public MetricValue GetCumTradeCount() => CreateMetric(totalTradesCount, GetTradeQuality(), IsCalibrated);

        // --- 5. Delta Analytics ---

        public MetricValue GetDelta1s() => CreateMetric(tradeAgg.Delta(TradeWindows.S1), GetTradeQuality(), IsTradeWarm(1));
        public MetricValue GetDelta5s() => CreateMetric(tradeAgg.Delta(TradeWindows.S5), GetTradeQuality(), IsTradeWarm(5));
        public MetricValue GetDelta30s() => CreateMetric(tradeAgg.Delta(TradeWindows.S30), GetTradeQuality(), IsTradeWarm(30));
        public MetricValue GetDelta60s() => CreateMetric(tradeAgg.Delta(TradeWindows.S60), GetTradeQuality(), IsTradeWarm(60));

        public MetricValue GetDeltaVelocity()
        {
            // DeltaVelocity = Delta over [now-1s, now] minus Delta over [now-2s, now-1s].
            // With nested windows: prev = Delta(2s) - Delta(1s), so velocity = 2*Delta(1s) - Delta(2s).
            double d1 = tradeAgg.Delta(TradeWindows.S1);
            double d2 = tradeAgg.Delta(TradeWindows.S2);
            return CreateMetric((2.0 * d1) - d2, GetTradeQuality(), IsTradeWarm(2));
        }

        // --- 6. Order Flow Imbalance ---

        public MetricValue GetOfi()
            => CreateMetric(ofiSum, IsL2Warm() ? MetricQuality.Derived : MetricQuality.Unavailable, IsCalibrated);

        // --- 6b. Symmetric Lattice (grid/ladder) Detector ---
        // Price-level based, so available in both MBO and MBP mode. Score is a persistence-gated
        // structural confidence in [0,1], not proof of a single market maker (no participant IDs).

        public MetricValue GetLatticeScore()
            => CreateMetric(latticeScore, IsL2Warm() ? MetricQuality.Derived : MetricQuality.Unavailable, IsCalibrated);

        public MetricValue GetLatticeBidRungs()
            => CreateMetric(lattice.BidRungs, IsL2Warm() ? MetricQuality.Derived : MetricQuality.Unavailable, IsCalibrated);

        public MetricValue GetLatticeAskRungs()
            => CreateMetric(lattice.AskRungs, IsL2Warm() ? MetricQuality.Derived : MetricQuality.Unavailable, IsCalibrated);

        public MetricValue GetLatticeSpacingTicks()
            => CreateMetric(lattice.SpacingTicks, IsL2Warm() ? MetricQuality.Derived : MetricQuality.Unavailable, IsCalibrated);

        public MetricValue GetLatticeRungSize()
            => CreateMetric(lattice.RungSize, IsL2Warm() ? MetricQuality.Derived : MetricQuality.Unavailable, IsCalibrated);

        // --- 6. VWAP Analytics ---

        public MetricValue GetRollingVwap1m() => CreateMetric(tradeAgg.Vwap(TradeWindows.S60), MetricQuality.Exact, IsTradeWarm(60));
        public MetricValue GetRollingVwap5m() => CreateMetric(tradeAgg.Vwap(TradeWindows.M5), MetricQuality.Exact, IsTradeWarm(300));
        public MetricValue GetRollingVwap15m() => CreateMetric(tradeAgg.Vwap(TradeWindows.M15), MetricQuality.Exact, IsTradeWarm(900));

        public MetricValue GetSessionVwap()
        {
            double val = sessionVwapQty == 0 ? 0 : sessionVwapSumPriceQty / sessionVwapQty;
            return CreateMetric(val, MetricQuality.Exact, IsCalibrated);
        }

        public MetricValue GetVwapDistance()
        {
            // H-13: use event-stream price instead of live symbol.Last which can be ahead of the
            // calculator queue and differs in replay.
            double lastPrice = lastTradePrice;
            if (lastPrice <= 0) return CreateMetric(0.0, MetricQuality.Exact, IsCalibrated);
            double vwap = GetSessionVwap().Value;
            if (vwap == 0) return CreateMetric(0.0, MetricQuality.Exact, IsCalibrated);

            double tickSize = symbol.TickSize > 0 ? symbol.TickSize : 1.0;
            double val = (lastPrice - vwap) / tickSize;
            return CreateMetric(val, MetricQuality.Exact, IsCalibrated);
        }

        // H-12: renamed from GetVwapDeviation — this is unweighted sample std dev of trade prices,
        // NOT volume-weighted deviation around session VWAP. Name corrected to avoid misleading callers.
        public MetricValue GetTradePriceStdDev()
        {
            if (sessionPriceCount < 2) return CreateMetric(0.0, MetricQuality.Exact, IsCalibrated);
            double variance = (sessionPriceSumSq - (sessionPriceSum * sessionPriceSum) / sessionPriceCount) / (sessionPriceCount - 1);
            double val = Math.Sqrt(Math.Max(0.0, variance));
            return CreateMetric(val, MetricQuality.Exact, IsCalibrated);
        }

        public MetricValue GetBuyVwap()
        {
            double val = sessionBuyVwapQty == 0 ? 0 : sessionBuyVwapPriceQtySum / sessionBuyVwapQty;
            return CreateMetric(val, GetTradeQuality(), IsCalibrated);
        }

        public MetricValue GetSellVwap()
        {
            double val = sessionSellVwapQty == 0 ? 0 : sessionSellVwapPriceQtySum / sessionSellVwapQty;
            return CreateMetric(val, GetTradeQuality(), IsCalibrated);
        }

        // --- 7. Order Arrivals (60s Window) ---
        // In MBO mode each RecordL2Addition call = 1 real order event (exact per-order tracking).
        // In MBP mode it is a price-level delta that may represent multiple orders (derived).

        public MetricValue GetNewOrderCount60() => CreateMetric(l2Agg.AddCount(TradeWindows.L2Short), MboMode ? MetricQuality.Exact : MetricQuality.Derived, IsL2Warm());
        public MetricValue GetNewBidCount60() => CreateMetric(l2Agg.AddCount(TradeWindows.L2Short, BookSide.Bid), MboMode ? MetricQuality.Exact : MetricQuality.Derived, IsL2Warm());
        public MetricValue GetNewAskCount60() => CreateMetric(l2Agg.AddCount(TradeWindows.L2Short, BookSide.Ask), MboMode ? MetricQuality.Exact : MetricQuality.Derived, IsL2Warm());
        public MetricValue GetNewBidVolume60() => CreateMetric(l2Agg.AddQty(TradeWindows.L2Short, BookSide.Bid), MboMode ? MetricQuality.Exact : MetricQuality.Derived, IsL2Warm());
        public MetricValue GetNewAskVolume60() => CreateMetric(l2Agg.AddQty(TradeWindows.L2Short, BookSide.Ask), MboMode ? MetricQuality.Exact : MetricQuality.Derived, IsL2Warm());

        // --- 8. Order Cancellations (60s Window) ---
        // MBO count: EXACT — each HandleL2Reduction call from closed=true is one real cancel.
        // MBO volume: DERIVED — HandleL2Reduction still does trade-matching for partial-fill
        //   disambiguation on size-decrease events; result is accurate but formula-derived.
        // MBP: both DERIVED/HEURISTIC because price-level deltas can't distinguish fills from cancels.

        public MetricValue GetCancelCount60() => CreateMetric(l2Agg.CancelCount(TradeWindows.L2Short), MboMode ? MetricQuality.Exact : MetricQuality.Derived, IsL2Warm());
        public MetricValue GetCancelBidCount60() => CreateMetric(l2Agg.CancelCount(TradeWindows.L2Short, BookSide.Bid), MboMode ? MetricQuality.Exact : MetricQuality.Derived, IsL2Warm());
        public MetricValue GetCancelAskCount60() => CreateMetric(l2Agg.CancelCount(TradeWindows.L2Short, BookSide.Ask), MboMode ? MetricQuality.Exact : MetricQuality.Derived, IsL2Warm());
        public MetricValue GetCancelBidVolume60() => CreateMetric(l2Agg.CancelQty(TradeWindows.L2Short, BookSide.Bid), MboMode ? MetricQuality.Derived : MetricQuality.Heuristic, IsL2Warm());
        public MetricValue GetCancelAskVolume60() => CreateMetric(l2Agg.CancelQty(TradeWindows.L2Short, BookSide.Ask), MboMode ? MetricQuality.Derived : MetricQuality.Heuristic, IsL2Warm());

        public MetricValue GetCancelRatio60()
            => MakeRatio(l2Agg.CancelCount(TradeWindows.L2Short), l2Agg.AddCount(TradeWindows.L2Short), MboMode ? MetricQuality.Exact : MetricQuality.Derived, IsL2Warm());

        public MetricValue GetCancelVolumeRatio60()
        {
            double addedVol = l2Agg.AddQty(TradeWindows.L2Short, BookSide.Bid) + l2Agg.AddQty(TradeWindows.L2Short, BookSide.Ask);
            double cancelledVol = l2Agg.CancelQty(TradeWindows.L2Short, BookSide.Bid) + l2Agg.CancelQty(TradeWindows.L2Short, BookSide.Ask);
            return MakeRatio(cancelledVol, addedVol, MboMode ? MetricQuality.Derived : MetricQuality.Heuristic, IsL2Warm());
        }

        // --- 9. DOM Imbalance ---

        public MetricValue GetDOMImbalance3() => CalculateImbalance(3);
        public MetricValue GetDOMImbalance5() => CalculateImbalance(5);
        public MetricValue GetDOMImbalance10() => CalculateImbalance(10);
        public MetricValue GetQueueImbalance() => CalculateImbalance(1);

private MetricValue CalculateImbalance(int levels)
        {
            lock (locker)
            {
                double bidVol = 0, askVol = 0;
                if (levels == 1)
                {
                    // Best-quote imbalance: already maintained incrementally — no book traversal needed.
                    bidVol = hasBestBid ? bestBidSize : 0;
                    askVol = hasBestAsk ? bestAskSize : 0;
                }
                else
                {
                    // bidBook/askBook are SortedDictionary ordered by price (bid: descending, ask: ascending),
                    // so iterating the first `levels` entries gives the top-of-book levels in O(log n + k).
                    int n = 0;
                    foreach (var kv in bidBook) { bidVol += kv.Value; if (++n >= levels) break; }
                    n = 0;
                    foreach (var kv in askBook) { askVol += kv.Value; if (++n >= levels) break; }
                }
                double sum = bidVol + askVol;
                double val = sum == 0 ? 0 : (bidVol - askVol) / sum;
                return CreateMetric(val, MetricQuality.Derived, bidBook.Count > 0 || askBook.Count > 0);
            }
        }

        // --- 11. Order Book Pressure ---

        public MetricValue GetBookPressure()
        {
            lock (locker)
            {
                if (!hasBestBid || !hasBestAsk)
                    return CreateMetric(0.0, MetricQuality.Derived, false);

                double midTicks = (bestBidTicks + bestAskTicks) / 2.0;
                double weightedBidVol = 0, weightedAskVol = 0;

                int n = 0;
                foreach (var kv in bidBook)
                {
                    double dist = Math.Abs(kv.Key - midTicks);
                    if (dist > 0) weightedBidVol += kv.Value / dist;
                    if (++n >= 10) break;
                }
                n = 0;
                foreach (var kv in askBook)
                {
                    double dist = Math.Abs(kv.Key - midTicks);
                    if (dist > 0) weightedAskVol += kv.Value / dist;
                    if (++n >= 10) break;
                }

                return CreateMetric(weightedBidVol - weightedAskVol, MetricQuality.Derived, IsCalibrated);
            }
        }

        // --- 12. Absorption Analytics ---

        public MetricValue GetAbsorptionBuy() => CalculateAbsorption(true);
        public MetricValue GetAbsorptionSell() => CalculateAbsorption(false);

        public MetricValue GetBestBidOrderCount() =>
            CreateMetric(bestBidOrderCount, MboMode ? MetricQuality.Exact : MetricQuality.Unavailable, hasBestBid);
        public MetricValue GetBestAskOrderCount() =>
            CreateMetric(bestAskOrderCount, MboMode ? MetricQuality.Exact : MetricQuality.Unavailable, hasBestAsk);
        public MetricValue GetBestBidAvgOrderSize()
        {
            double avg = bestBidOrderCount > 0 ? bestBidSize / bestBidOrderCount : 0;
            return CreateMetric(avg, MboMode ? MetricQuality.Exact : MetricQuality.Unavailable, hasBestBid && bestBidOrderCount > 0);
        }
        public MetricValue GetBestAskAvgOrderSize()
        {
            double avg = bestAskOrderCount > 0 ? bestAskSize / bestAskOrderCount : 0;
            return CreateMetric(avg, MboMode ? MetricQuality.Exact : MetricQuality.Unavailable, hasBestAsk && bestAskOrderCount > 0);
        }

        private MetricValue CalculateAbsorption(bool isBuy)
        {
            var (totalVol, range, buyVol, sellVol) = tradeAgg.GetAbsorption5s();
            double val = 0;

            if (totalVol >= AbsorptionVolumeThreshold && range <= symbol.TickSize + 1e-9)
            {
                val = isBuy ? buyVol : sellVol;
            }
            return CreateMetric(val, MetricQuality.Exact, IsTradeWarm(5));
        }

        // --- 13. Iceberg Detection ---

        public MetricValue GetIcebergScoreBid() => CreateMetric(SumIcebergEvents(true), MboMode ? MetricQuality.Derived : MetricQuality.Heuristic, IsCalibrated);
        public MetricValue GetIcebergScoreAsk() => CreateMetric(SumIcebergEvents(false), MboMode ? MetricQuality.Derived : MetricQuality.Heuristic, IsCalibrated);

        private double SumIcebergEvents(bool isBid) => isBid ? _icebergBidSum : _icebergAskSum;

        // --- 14. Replenishment Detection ---

        public MetricValue GetReplenishmentBid() => CreateMetric(SumReplenishmentEvents(true), MboMode ? MetricQuality.Derived : MetricQuality.Heuristic, IsCalibrated);
        public MetricValue GetReplenishmentAsk() => CreateMetric(SumReplenishmentEvents(false), MboMode ? MetricQuality.Derived : MetricQuality.Heuristic, IsCalibrated);

        private double SumReplenishmentEvents(bool isBid) => isBid ? _replenishBidSum : _replenishAskSum;

        // --- 15. Spoofing Heuristics ---

        public MetricValue GetSpoofScoreBid() => CreateMetric(SumSpoofEvents(true), MboMode ? MetricQuality.Derived : MetricQuality.Heuristic, IsCalibrated);
        public MetricValue GetSpoofScoreAsk() => CreateMetric(SumSpoofEvents(false), MboMode ? MetricQuality.Derived : MetricQuality.Heuristic, IsCalibrated);

        private double SumSpoofEvents(bool isBid) => isBid ? _spoofBidSum : _spoofAskSum;

        #endregion

        #region Feature Store Exporter (wide CSV format)

        // Header order MUST stay in lockstep with BuildFeatureRow.
        private static readonly string[] FeatureColumns =
        {
            "Timestamp","Symbol",
            "BuyTradeCount60","SellTradeCount60","BuyVolume60","SellVolume60","BuySellCountRatio60","BuySellVolumeRatio60",
            "CumBuyVolume","CumSellVolume","CumDelta","CumTradeCount",
            "Delta1s","Delta5s","Delta30s","Delta60s","DeltaVelocity",
            "RollingVWAP1m","RollingVWAP5m","RollingVWAP15m","SessionVWAP","VWAPDistance","TradePriceStdDev","BuyVWAP","SellVWAP",
            "NewOrderCount","NewBidCount","NewAskCount","NewBidVolume","NewAskVolume",
            "CancelCount","CancelBidCount","CancelAskCount","CancelBidVolume","CancelAskVolume","CancelRatio","CancelVolumeRatio",
            "DOMImbalance3","DOMImbalance5","DOMImbalance10","QueueImbalance","BookPressure",
            "AbsorptionBuy","AbsorptionSell","IcebergScoreBid","IcebergScoreAsk","ReplenishmentBid","ReplenishmentAsk","SpoofScoreBid","SpoofScoreAsk"
        };

        /// <summary>
        /// Appends one wide-format row (one column per metric) to the CSV feature store.
        /// Writes the header automatically when the file does not yet exist.
        /// </summary>
        public void AppendToCsvFeatureStore(string filePath)
        {
            lock (locker)
            {
                try
                {
                    bool writeHeader = !System.IO.File.Exists(filePath);
                    using (var writer = new System.IO.StreamWriter(filePath, true))
                    {
                        if (writeHeader)
                            writer.WriteLine(string.Join(",", FeatureColumns));

                        writer.WriteLine(BuildFeatureRow());
                    }
                }
                catch (Exception ex)
                {
                    Core.Instance.Loggers.Log($"Failed to append feature store row: {ex.Message}", LoggingLevel.Error);
                }
            }
        }

        private string BuildFeatureRow()
        {
            string F(double v) => v.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);

            var cells = new string[]
            {
                DateTime.UtcNow.ToString("o"),
                symbol.Name,
                F(GetBuyTradeCount60().Value), F(GetSellTradeCount60().Value), F(GetBuyVolume60().Value), F(GetSellVolume60().Value),
                F(GetBuySellCountRatio60().Value), F(GetBuySellVolumeRatio60().Value),
                F(GetCumBuyVolume().Value), F(GetCumSellVolume().Value), F(GetCumDelta().Value), F(GetCumTradeCount().Value),
                F(GetDelta1s().Value), F(GetDelta5s().Value), F(GetDelta30s().Value), F(GetDelta60s().Value), F(GetDeltaVelocity().Value),
                F(GetRollingVwap1m().Value), F(GetRollingVwap5m().Value), F(GetRollingVwap15m().Value), F(GetSessionVwap().Value),
                F(GetVwapDistance().Value), F(GetTradePriceStdDev().Value), F(GetBuyVwap().Value), F(GetSellVwap().Value),
                F(GetNewOrderCount60().Value), F(GetNewBidCount60().Value), F(GetNewAskCount60().Value), F(GetNewBidVolume60().Value), F(GetNewAskVolume60().Value),
                F(GetCancelCount60().Value), F(GetCancelBidCount60().Value), F(GetCancelAskCount60().Value), F(GetCancelBidVolume60().Value), F(GetCancelAskVolume60().Value),
                F(GetCancelRatio60().Value), F(GetCancelVolumeRatio60().Value),
                F(GetDOMImbalance3().Value), F(GetDOMImbalance5().Value), F(GetDOMImbalance10().Value), F(GetQueueImbalance().Value), F(GetBookPressure().Value),
                F(GetAbsorptionBuy().Value), F(GetAbsorptionSell().Value), F(GetIcebergScoreBid().Value), F(GetIcebergScoreAsk().Value),
                F(GetReplenishmentBid().Value), F(GetReplenishmentAsk().Value), F(GetSpoofScoreBid().Value), F(GetSpoofScoreAsk().Value)
            };
            return string.Join(",", cells);
        }

        #endregion

        #region Backward Compatibility Wrappers (Keeps Strategy & Old Views Working)

        public MetricValue GetBuyerInitiatedTradeCountShort() => GetBuyTradeCount60();
        public MetricValue GetSellerInitiatedTradeCountShort() => GetSellTradeCount60();
        public MetricValue GetBuyerInitiatedTradeQtyShort() => GetBuyVolume60();
        public MetricValue GetSellerInitiatedTradeQtyShort() => GetSellVolume60();
        public MetricValue GetBuyerSellerTradeCountRatioShort() => GetBuySellCountRatio60();
        public MetricValue GetBuyerSellerQtyRatioShort() => GetBuySellVolumeRatio60();

        public MetricValue GetCumulativeBuyerSellerTradeCountRatio()
            => MakeRatio(totalBuyerTradeCount, totalSellerTradeCount, GetTradeQuality(), IsCalibrated);

        public MetricValue GetCumulativeBuyerSellerQtyRatio()
            => MakeRatio(totalBuyerTradeQty, totalSellerTradeQty, GetTradeQuality(), IsCalibrated);

        public MetricValue GetTradesVwapLong() => GetRollingVwap15m();
        public MetricValue GetCumulativeTradesVwap() => GetSessionVwap();

        public MetricValue GetBuyerTradesVwapLong()
            => CreateMetric(tradeAgg.BuyVwap(TradeWindows.M15), GetTradeQuality(), IsTradeWarm(900));

        public MetricValue GetSellerTradesVwapLong()
            => CreateMetric(tradeAgg.SellVwap(TradeWindows.M15), GetTradeQuality(), IsTradeWarm(900));

        public MetricValue GetL2AddEventCountShort() => GetNewOrderCount60();
        public MetricValue GetCumulativeL2AddEventCount() => CreateMetric(totalArrivedOrderCount, MetricQuality.Derived, IsCalibrated);
        public MetricValue GetL2AddedVisibleQtyShort() => CreateMetric(GetNewBidVolume60().Value + GetNewAskVolume60().Value, MetricQuality.Derived, IsL2Warm());
        public MetricValue GetCumulativeL2AddedVisibleQty() => CreateMetric(totalArrivedOrderQty, MetricQuality.Derived, IsCalibrated);

        public MetricValue GetBidL2AddEventCountShort() => GetNewBidCount60();
        public MetricValue GetAskL2AddEventCountShort() => GetNewAskCount60();
        public MetricValue GetBidL2AddedVisibleQtyShort() => GetNewBidVolume60();
        public MetricValue GetAskL2AddedVisibleQtyShort() => GetNewAskVolume60();

        public MetricValue GetMeanBidAddedVisibleQtyLong()
            => CreateMetric(l2Agg.AddMean(TradeWindows.L2Long, BookSide.Bid), MetricQuality.Derived, IsL2Warm());

        public MetricValue GetMeanAskAddedVisibleQtyLong()
            => CreateMetric(l2Agg.AddMean(TradeWindows.L2Long, BookSide.Ask), MetricQuality.Derived, IsL2Warm());

        public MetricValue GetStdDevBidAddedVisibleQtyLong()
            => CreateMetric(l2Agg.AddStdDev(TradeWindows.L2Long, BookSide.Bid), MetricQuality.Derived, IsL2Warm());

        public MetricValue GetStdDevAskAddedVisibleQtyLong()
            => CreateMetric(l2Agg.AddStdDev(TradeWindows.L2Long, BookSide.Ask), MetricQuality.Derived, IsL2Warm());

        public MetricValue GetL2RemoveEventCountShort() => GetCancelCount60();
        public MetricValue GetEstimatedCancelQtyShort() => CreateMetric(GetCancelBidVolume60().Value + GetCancelAskVolume60().Value, MetricQuality.Heuristic, IsL2Warm());

        public MetricValue GetBidL2RemoveEventCountShort() => GetCancelBidCount60();
        public MetricValue GetAskL2RemoveEventCountShort() => GetCancelAskCount60();

        public MetricValue GetEstimatedCancelBuyQtyShort() => GetCancelBidVolume60();
        public MetricValue GetEstimatedCancelSellQtyShort() => GetCancelAskVolume60();

        // H-05: reprice events are order-ID modifications that move price/side — distinct from cancel+arrival.
        public MetricValue GetRepriceCount() => CreateMetric(totalRepriceCount, MboMode ? MetricQuality.Exact : MetricQuality.Unavailable, IsCalibrated);
        public MetricValue GetRepriceQty() => CreateMetric(totalRepriceQty, MboMode ? MetricQuality.Exact : MetricQuality.Unavailable, IsCalibrated);

        public MetricValue GetCumulativeL2RemoveEventCount() => CreateMetric(totalCancelledOrderCount, MetricQuality.Derived, IsCalibrated);
        public MetricValue GetCumulativeEstimatedCancelVwap() => CreateMetric(sessionCancelVwapQty == 0 ? 0 : sessionCancelVwapSumPriceQty / sessionCancelVwapQty, MetricQuality.Heuristic, IsCalibrated);
        public MetricValue GetCumulativeEstimatedCancelBuyVwap() => CreateMetric(sessionCancelBuyVwapQty == 0 ? 0 : sessionCancelBuyVwapSumPriceQty / sessionCancelBuyVwapQty, MetricQuality.Heuristic, IsCalibrated);
        public MetricValue GetCumulativeEstimatedCancelSellVwap() => CreateMetric(sessionCancelSellVwapQty == 0 ? 0 : sessionCancelSellVwapSumPriceQty / sessionCancelSellVwapQty, MetricQuality.Heuristic, IsCalibrated);

        public MetricValue GetRemovedToAddedVisibleRatioCountShort() => GetCancelRatio60();
        public MetricValue GetRemovedToAddedVisibleRatioQtyShort() => GetCancelVolumeRatio60();

        public MetricValue GetCumulativeRemovedToAddedVisibleRatioCount() => MakeRatio(totalCancelledOrderCount, totalArrivedOrderCount, MetricQuality.Derived, IsCalibrated);
        public MetricValue GetCumulativeRemovedToAddedVisibleRatioQty() => MakeRatio(totalCancelledOrderQty, totalArrivedOrderQty, MetricQuality.Heuristic, IsCalibrated);

        #endregion

        #region Helpers

        /// <summary>
        /// Builds a ratio metric with safe handling of the zero-denominator case.
        /// - numerator == 0 and denominator == 0  => Unavailable (no data; do not act on it)
        /// - denominator == 0 and numerator > 0   => RATIO_SATURATION (one-sided extreme, not 0)
        /// - otherwise                            => numerator / denominator
        /// </summary>
        private MetricValue MakeRatio(double numerator, double denominator, MetricQuality quality, bool isWarm)
        {
            if (denominator == 0)
            {
                if (numerator == 0)
                    return CreateMetric(0.0, MetricQuality.Unavailable, isWarm);
                return CreateMetric(RATIO_SATURATION, quality, isWarm);
            }
            return CreateMetric(numerator / denominator, quality, isWarm);
        }

        #endregion

        #region Sliding-Window Aggregators

        // Window index constants. Trade windows and L2 windows share an index space; only the
        // indices a given aggregator declares are populated.
        private static class TradeWindows
        {
            public const int S1 = 0;
            public const int S2 = 1;
            public const int S5 = 2;
            public const int S30 = 3;
            public const int S60 = 4;
            public const int M5 = 5;
            public const int M15 = 6;
            public const int Count = 7;

            // L2 aggregator windows
            public const int L2Short = 0; // 60s
            public const int L2Long = 1;  // 900s
            public const int L2Count = 2;
        }

        /// <summary>
        /// Maintains incremental rolling sums of trade statistics over several nested time windows.
        /// A single time-ordered node store backs all windows; each window keeps a head cursor and
        /// running totals, so Add is O(1) amortized and reads are O(1).
        /// </summary>
        private sealed class TradeWindowAggregator
        {
            private static readonly long[] WindowTicks =
            {
                1 * TimeSpan.TicksPerSecond,
                2 * TimeSpan.TicksPerSecond,
                5 * TimeSpan.TicksPerSecond,
                30 * TimeSpan.TicksPerSecond,
                60 * TimeSpan.TicksPerSecond,
                300 * TimeSpan.TicksPerSecond,
                900 * TimeSpan.TicksPerSecond
            };

            private struct Node
            {
                public long Ticks;
                public double Price;
                public double Qty;
                public bool Buy;
                public bool Sell;
            }

            private readonly List<Node> nodes = new List<Node>(8192);
            private long lastTicks = long.MinValue;

            private readonly int[] cursor = new int[TradeWindows.Count];
            private readonly double[] totalQty = new double[TradeWindows.Count];
            private readonly double[] totalCount = new double[TradeWindows.Count];
            private readonly double[] sumPQ = new double[TradeWindows.Count];
            private readonly double[] buyQty = new double[TradeWindows.Count];
            private readonly double[] sellQty = new double[TradeWindows.Count];
            private readonly double[] buyCount = new double[TradeWindows.Count];
            private readonly double[] sellCount = new double[TradeWindows.Count];
            private readonly double[] buySumPQ = new double[TradeWindows.Count];
            private readonly double[] sellSumPQ = new double[TradeWindows.Count];

            public void Add(long ticks, double price, double qty, bool buy, bool sell)
            {
                if (ticks < lastTicks) ticks = lastTicks;
                lastTicks = ticks;

                nodes.Add(new Node { Ticks = ticks, Price = price, Qty = qty, Buy = buy, Sell = sell });

                double pq = price * qty;
                for (int w = 0; w < TradeWindows.Count; w++)
                {
                    totalQty[w] += qty;
                    totalCount[w] += 1;
                    sumPQ[w] += pq;
                    if (buy) { buyQty[w] += qty; buyCount[w] += 1; buySumPQ[w] += pq; }
                    else if (sell) { sellQty[w] += qty; sellCount[w] += 1; sellSumPQ[w] += pq; }
                }
            }

            public void Advance(long nowTicks)
            {
                for (int w = 0; w < TradeWindows.Count; w++)
                {
                    long cutoff = nowTicks - WindowTicks[w];
                    int c = cursor[w];
                    while (c < nodes.Count && nodes[c].Ticks < cutoff)
                    {
                        var n = nodes[c];
                        double pq = n.Price * n.Qty;
                        totalQty[w] -= n.Qty;
                        totalCount[w] -= 1;
                        sumPQ[w] -= pq;
                        if (n.Buy) { buyQty[w] -= n.Qty; buyCount[w] -= 1; buySumPQ[w] -= pq; }
                        else if (n.Sell) { sellQty[w] -= n.Qty; sellCount[w] -= 1; sellSumPQ[w] -= pq; }
                        c++;
                    }
                    cursor[w] = c;
                }

                Compact();
            }

            private void Compact()
            {
                // The largest window (M15) retains the most nodes, so its cursor is the smallest.
                int minCursor = cursor[TradeWindows.M15];
                if (minCursor < 4096) return;

                nodes.RemoveRange(0, minCursor);
                for (int w = 0; w < TradeWindows.Count; w++)
                    cursor[w] -= minCursor;
            }

            public void Clear()
            {
                nodes.Clear();
                lastTicks = long.MinValue;
                Array.Clear(cursor, 0, cursor.Length);
                Array.Clear(totalQty, 0, totalQty.Length);
                Array.Clear(totalCount, 0, totalCount.Length);
                Array.Clear(sumPQ, 0, sumPQ.Length);
                Array.Clear(buyQty, 0, buyQty.Length);
                Array.Clear(sellQty, 0, sellQty.Length);
                Array.Clear(buyCount, 0, buyCount.Length);
                Array.Clear(sellCount, 0, sellCount.Length);
                Array.Clear(buySumPQ, 0, buySumPQ.Length);
                Array.Clear(sellSumPQ, 0, sellSumPQ.Length);
            }

            public double TotalCount(int w) => totalCount[w];
            public double BuyQty(int w) => buyQty[w];
            public double SellQty(int w) => sellQty[w];
            public double BuyCount(int w) => buyCount[w];
            public double SellCount(int w) => sellCount[w];
            public double Delta(int w) => buyQty[w] - sellQty[w];
            public double Vwap(int w) => totalQty[w] == 0 ? 0 : sumPQ[w] / totalQty[w];
            public double BuyVwap(int w) => buyQty[w] == 0 ? 0 : buySumPQ[w] / buyQty[w];
            public double SellVwap(int w) => sellQty[w] == 0 ? 0 : sellSumPQ[w] / sellQty[w];

            /// <summary>
            /// Returns (totalVolume, priceRange, buyVolume, sellVolume) over the 5-second window.
            /// Scans only the (small) 5s slice for the price extremes, which are not maintained
            /// incrementally.
            /// </summary>
            public (double totalVol, double range, double buyVol, double sellVol) GetAbsorption5s()
            {
                int start = cursor[TradeWindows.S5];
                if (start >= nodes.Count)
                    return (0, 0, 0, 0);

                double max = double.MinValue, min = double.MaxValue;
                for (int i = start; i < nodes.Count; i++)
                {
                    double p = nodes[i].Price;
                    if (p > max) max = p;
                    if (p < min) min = p;
                }
                double range = max == double.MinValue ? 0 : max - min;
                return (totalQty[TradeWindows.S5], range, buyQty[TradeWindows.S5], sellQty[TradeWindows.S5]);
            }
        }

        /// <summary>
        /// Maintains incremental rolling sums of Level 2 addition/cancellation statistics over a
        /// short (60s) and long (900s) window, per book side, including the sum of squares needed
        /// for the standard-deviation metrics.
        /// </summary>
        private sealed class L2WindowAggregator
        {
            private static readonly long[] WindowTicks =
            {
                60 * TimeSpan.TicksPerSecond,
                900 * TimeSpan.TicksPerSecond
            };

            private struct Node
            {
                public long Ticks;
                public double Qty;
                public bool IsBid;
                public bool IsCancel;
            }

            private readonly List<Node> nodes = new List<Node>(8192);
            private long lastTicks = long.MinValue;

            private readonly int[] cursor = new int[TradeWindows.L2Count];

            // [window, side] where side 0 = Bid, 1 = Ask
            private readonly double[,] addCount = new double[TradeWindows.L2Count, 2];
            private readonly double[,] addQty = new double[TradeWindows.L2Count, 2];
            private readonly double[,] addQtySq = new double[TradeWindows.L2Count, 2];
            private readonly double[,] cancelCount = new double[TradeWindows.L2Count, 2];
            private readonly double[,] cancelQty = new double[TradeWindows.L2Count, 2];

            private static int SideIndex(BookSide s) => s == BookSide.Bid ? 0 : 1;

            public void AddAddition(long ticks, double qty, bool isBid) => Append(ticks, qty, isBid, false);
            public void AddCancellation(long ticks, double qty, bool isBid) => Append(ticks, qty, isBid, true);

            private void Append(long ticks, double qty, bool isBid, bool isCancel)
            {
                if (ticks < lastTicks) ticks = lastTicks;
                lastTicks = ticks;

                nodes.Add(new Node { Ticks = ticks, Qty = qty, IsBid = isBid, IsCancel = isCancel });

                int side = isBid ? 0 : 1;
                double qsq = qty * qty;
                for (int w = 0; w < TradeWindows.L2Count; w++)
                {
                    if (isCancel)
                    {
                        cancelCount[w, side] += 1;
                        cancelQty[w, side] += qty;
                    }
                    else
                    {
                        addCount[w, side] += 1;
                        addQty[w, side] += qty;
                        addQtySq[w, side] += qsq;
                    }
                }
            }

            public void Advance(long nowTicks)
            {
                for (int w = 0; w < TradeWindows.L2Count; w++)
                {
                    long cutoff = nowTicks - WindowTicks[w];
                    int c = cursor[w];
                    while (c < nodes.Count && nodes[c].Ticks < cutoff)
                    {
                        var n = nodes[c];
                        int side = n.IsBid ? 0 : 1;
                        if (n.IsCancel)
                        {
                            cancelCount[w, side] -= 1;
                            cancelQty[w, side] -= n.Qty;
                        }
                        else
                        {
                            addCount[w, side] -= 1;
                            addQty[w, side] -= n.Qty;
                            addQtySq[w, side] -= n.Qty * n.Qty;
                        }
                        c++;
                    }
                    cursor[w] = c;
                }

                Compact();
            }

            private void Compact()
            {
                int minCursor = cursor[TradeWindows.L2Long];
                if (minCursor < 4096) return;

                nodes.RemoveRange(0, minCursor);
                for (int w = 0; w < TradeWindows.L2Count; w++)
                    cursor[w] -= minCursor;
            }

            public void Clear()
            {
                nodes.Clear();
                lastTicks = long.MinValue;
                Array.Clear(cursor, 0, cursor.Length);
                Array.Clear(addCount, 0, addCount.Length);
                Array.Clear(addQty, 0, addQty.Length);
                Array.Clear(addQtySq, 0, addQtySq.Length);
                Array.Clear(cancelCount, 0, cancelCount.Length);
                Array.Clear(cancelQty, 0, cancelQty.Length);
            }

            public double AddCount(int w) => addCount[w, 0] + addCount[w, 1];
            public double AddCount(int w, BookSide s) => addCount[w, SideIndex(s)];
            public double AddQty(int w, BookSide s) => addQty[w, SideIndex(s)];

            public double CancelCount(int w) => cancelCount[w, 0] + cancelCount[w, 1];
            public double CancelCount(int w, BookSide s) => cancelCount[w, SideIndex(s)];
            public double CancelQty(int w, BookSide s) => cancelQty[w, SideIndex(s)];

            public double AddMean(int w, BookSide s)
            {
                int side = SideIndex(s);
                double n = addCount[w, side];
                return n <= 0 ? 0 : addQty[w, side] / n;
            }

            public double AddStdDev(int w, BookSide s)
            {
                int side = SideIndex(s);
                double n = addCount[w, side];
                if (n < 2) return 0.0;
                double sum = addQty[w, side];
                double sumSq = addQtySq[w, side];
                double variance = (sumSq - (sum * sum) / n) / (n - 1);
                return Math.Sqrt(Math.Max(0.0, variance));
            }
        }

        #endregion

        #region Inner Data Models

        private class TradeFillMatch
        {
            public DateTime Time { get; }
            public double Price { get; }
            public double TotalQty { get; }
            public double RemainingQty { get; set; }
            public AggressorFlag Side { get; }

            public TradeFillMatch(DateTime time, double price, double qty, AggressorFlag side)
            {
                Time = time;
                Price = price;
                TotalQty = qty;
                RemainingQty = qty;
                Side = side;
            }
        }

        private class L2AdditionTracker
        {
            public DateTime Time { get; }
            public double Price { get; }
            public double Qty { get; }
            public bool IsBid { get; }
            public bool HadTrades { get; set; }
            public string? OrderId { get; }  // set in MBO mode; null in MBP mode

            public L2AdditionTracker(DateTime time, double price, double qty, bool isBid, string? orderId = null)
            {
                Time = time;
                Price = price;
                Qty = qty;
                IsBid = isBid;
                HadTrades = false;
                OrderId = orderId;
            }
        }

        private class RecentExecutionTracker
        {
            public DateTime Time { get; }
            public double Price { get; }
            public bool IsBid { get; }

            public RecentExecutionTracker(DateTime time, double price, bool isBid)
            {
                Time = time;
                Price = price;
                IsBid = isBid;
            }
        }

        private class IcebergEvent
        {
            public DateTime Time { get; }
            public double Qty { get; }
            public bool IsBid { get; }

            public IcebergEvent(DateTime time, double qty, bool isBid)
            {
                Time = time;
                Qty = qty;
                IsBid = isBid;
            }
        }

        private class ReplenishmentEvent
        {
            public DateTime Time { get; }
            public double Qty { get; }
            public bool IsBid { get; }

            public ReplenishmentEvent(DateTime time, double qty, bool isBid)
            {
                Time = time;
                Qty = qty;
                IsBid = isBid;
            }
        }

        private class SpoofEvent
        {
            public DateTime Time { get; }
            public double Qty { get; }
            public bool IsBid { get; }

            public SpoofEvent(DateTime time, double qty, bool isBid)
            {
                Time = time;
                Qty = qty;
                IsBid = isBid;
            }
        }

        #endregion
    }
}
