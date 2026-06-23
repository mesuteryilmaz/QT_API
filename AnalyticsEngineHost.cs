using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TradingPlatform.BusinessLayer;
using TradingPlatform.BusinessLayer.Integration;

namespace MBO_Market_Data_Analytics
{
    /// <summary>
    /// Configuration shared by the indicator and strategy hosts.
    /// </summary>
    public sealed class AnalyticsEngineConfig
    {
        public CalibrationMode CalibrationMode = CalibrationMode.AutoMTR;
        public double TradeVolumeShort = 1000;
        public double TradeVolumeLong = 5000;
        public int OrderCountShort = 2000;
        public int OrderCountLong = 10000;
        public int CalibrationTrades = 1000;
        public int SnapshotDepth = 360;
        public int UpdateFrequencyMs = 250;
        public bool EnableFeatureStore = false;
        public string FeatureStorePath = "";
        public double AbsorptionThreshold = 0; // 0 = auto from MTR calibration; >0 = fixed override
    }

    /// <summary>
    /// Encapsulates everything shared between the indicator and the strategy: the background worker
    /// thread, the bounded event queue with overflow recovery, capability probing, the auto-MTR
    /// volume warmup, the initial book seed, CME session-rollover detection, snapshot publishing and
    /// the optional feature-store export.
    ///
    /// Because Quantower's <c>Indicator</c> and <c>Strategy</c> base classes cannot share a common
    /// base, this is consumed by composition: each owns an instance, supplies a logger, and hooks the
    /// optional per-event / per-rollover callbacks it needs.
    /// </summary>
    public sealed class AnalyticsEngineHost
    {
        private readonly Symbol symbol;
        private readonly AnalyticsEngineConfig config;
        private readonly Action<string, LoggingLevel> log;

        public DataAnalyticsCalculator? Calculator { get; private set; }

        private volatile AnalyticsSnapshot? currentSnapshot;
        public AnalyticsSnapshot? CurrentSnapshot => currentSnapshot;

        // Optional hooks. Both run on the worker thread.
        public Action? OnEventProcessed;     // after each market event is applied (strategy signal eval)
        public Action? OnSessionRollover;    // after the calculator's session state is reset at rollover

        private BlockingCollection<MarketEvent>? eventQueue;
        private Thread? workerThread;
        private CancellationTokenSource? workerCts;
        private volatile bool isQueueOverflowed = false;
        private long queueOverflowCount = 0;
        private volatile bool isBookValid = false;
        private volatile bool initialized = false;

        // Order-level book used when MboMode is active. Normalises raw Level2Quote events into
        // typed MboEvent objects (Add/Update/Remove) on the worker thread before passing them to
        // the calculator, enabling exact per-order arrival/cancel/spoof tracking.
        private MboOrderBook? mboBook;

        private DateTime lastSessionDate = DateTime.MinValue;

        public bool IsBookValid => isBookValid;
        public bool IsInitialized => initialized;
        public long QueueOverflowCount => Volatile.Read(ref queueOverflowCount);

        public AnalyticsEngineHost(Symbol symbol, AnalyticsEngineConfig config, Action<string, LoggingLevel> log)
        {
            this.symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.log = log ?? ((m, l) => { });
        }

        #region Lifecycle

        public void Start()
        {
            double shortVol = config.TradeVolumeShort;
            double longVol = config.TradeVolumeLong;

            if (config.CalibrationMode == CalibrationMode.AutoMTR)
                CalibrateVolumeWindows(ref shortVol, ref longVol);

            FeedCapabilities capabilities = ProbeCapabilities();

            Calculator = new DataAnalyticsCalculator(symbol, capabilities)
            {
                TradeVolumeWindowShort = shortVol,
                TradeVolumeWindowLong = longVol,
                CalibrationTradesThreshold = config.CalibrationTrades
            };

            if (config.CalibrationMode == CalibrationMode.Manual)
            {
                Calculator.OrderCountWindowShort = config.OrderCountShort;
                Calculator.OrderCountWindowLong = config.OrderCountLong;
                Calculator.IsManualMode = true;
                Calculator.IsCalibrated = true;
            }

            if (config.AbsorptionThreshold > 0)
                Calculator.AbsorptionVolumeThreshold = config.AbsorptionThreshold;
            else if (config.CalibrationMode == CalibrationMode.Manual)
            {
                // H-09: manual mode skips live calibration so the threshold would stay zero, making
                // every 5s cluster trigger absorption. Derive from the volume window as calibration would.
                double typical5sVol = shortVol / 12.0;
                Calculator.AbsorptionVolumeThreshold = Math.Max(10, typical5sVol * 2.0);
                log($"[Manual] Absorption threshold derived from volume window: {Calculator.AbsorptionVolumeThreshold:F0} contracts/5s.", LoggingLevel.System);
            }

            // Always attempt MBO mode. SeedBookSnapshot downgrades to MBP if the feed returns
            // no per-order snapshot, and ProcessMboEvent falls back gracefully when order IDs
            // are absent — so MBP feeds are handled transparently.
            Calculator.MboMode = true;
            mboBook = new MboOrderBook(symbol.TickSize);

            // C-01: subscribe and open the queue BEFORE requesting the snapshot so that no
            // exchange updates are lost during the seed window. Events buffer in the queue
            // while SeedBookSnapshot() runs on this thread; the worker starts after the
            // snapshot is applied and replays the buffered post-subscription incrementals.
            eventQueue = new BlockingCollection<MarketEvent>(10000);
            workerCts = new CancellationTokenSource();

            symbol.NewLast += OnNewLast;
            symbol.NewLevel2 += OnNewLevel2;
            initialized = true; // gates callbacks — must be set before subscription is active

            SeedBookSnapshot();
            PublishSnapshot();

            // Worker starts after the snapshot baseline is established; it will drain any
            // events that buffered during the seed in correct sequence.
            workerThread = new Thread(ProcessQueueLoop)
            {
                IsBackground = true,
                Name = "AnalyticsEngineWorker",
                Priority = ThreadPriority.AboveNormal
            };
            workerThread.Start();
        }

        public void Stop()
        {
            initialized = false;

            symbol.NewLast -= OnNewLast;
            symbol.NewLevel2 -= OnNewLevel2;

            workerCts?.Cancel();
            eventQueue?.CompleteAdding();

            if (workerThread != null && workerThread.IsAlive)
                workerThread.Join(1000);

            Calculator?.Reset();
            Calculator = null;
            mboBook = null;
        }

        #endregion

        #region Warmup / Capability Probe / Seed

        private void CalibrateVolumeWindows(ref double shortVol, ref double longVol)
        {
            try
            {
                DateTime toTime = Core.Instance.TimeUtils.DateTimeUtcNow;
                DateTime fromTime = toTime.AddDays(-2);

                var history = symbol.GetHistory(new HistoryRequestParameters
                {
                    Symbol = symbol,
                    Aggregation = new HistoryAggregationTime(Period.MIN1, HistoryType.Last),
                    FromTime = fromTime,
                    ToTime = toTime
                });

                if (history == null || history.Count == 0)
                    return;

                var volumes = new List<double>();
                for (int i = 0; i < history.Count; i++)
                {
                    if (history[i] is HistoryItemBar bar)
                    {
                        DateTime barEst = ConvertUtcToEst(bar.TimeLeft);
                        // Filter for active RTH hours: 09:30 to 16:00 EST
                        if (barEst.TimeOfDay >= new TimeSpan(9, 30, 0) && barEst.TimeOfDay <= new TimeSpan(16, 0, 0) && bar.Volume > 0)
                            volumes.Add(bar.Volume);
                    }
                }

                if (volumes.Count == 0)
                    return;

                volumes.Sort();
                int mid = volumes.Count / 2;
                double medianVolume = volumes.Count % 2 != 0
                    ? volumes[mid]
                    : (volumes[mid - 1] + volumes[mid]) / 2.0;

                medianVolume = Math.Round(medianVolume / 10.0) * 10.0; // round to nearest 10
                if (medianVolume >= 10)
                {
                    shortVol = medianVolume;
                    longVol = medianVolume * 5.0;
                    log($"[Self-Warmup] Dynamic volume calibration for {symbol.Name}. Samples: {volumes.Count} active mins. " +
                        $"Median/min: {medianVolume} contracts. Short: {shortVol}, Long: {longVol}.", LoggingLevel.System);
                }
            }
            catch (Exception ex)
            {
                log($"[Self-Warmup] Dynamic volume calibration failed: {ex.Message}", LoggingLevel.Error);
            }
        }

        private FeedCapabilities ProbeCapabilities()
        {
            FeedCapabilities caps = FeedCapabilities.None;

            caps |= FeedCapabilities.Trades;
            caps |= FeedCapabilities.TopOfBook;
            caps |= FeedCapabilities.HistoricalBars;

            var connections = Core.Instance.Connections.Connected;
            var symbolConn = connections.FirstOrDefault(c => c.Id == symbol.ConnectionId);
            string vendor = symbolConn?.VendorName ?? "";
            string name = symbolConn?.Name ?? "";

            bool isDxFeed = vendor.IndexOf("dxfeed", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            name.IndexOf("dxfeed", StringComparison.OrdinalIgnoreCase) >= 0;

            if (isDxFeed)
            {
                caps |= FeedCapabilities.TradeAggressor;
                caps |= FeedCapabilities.MarketByPrice;
                caps |= FeedCapabilities.HistoricalTrades;
            }
            else if (vendor.IndexOf("interactive brokers", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     name.IndexOf("ib", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                caps |= FeedCapabilities.MarketByPrice;
            }

            log($"[Capability Probe] Symbol: {symbol.Name}, Connection: {name}, Vendor: {vendor}. Caps: {caps}. " +
                $"Depth: {config.SnapshotDepth}. Quantower: {Core.Instance.CurrentVersion}. " +
                $"Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}.", LoggingLevel.System);

            return caps;
        }

        private void SeedBookSnapshot()
        {
            try
            {
                if (Calculator?.MboMode == true)
                {
                    // Per-order MBO seed: AggregateMethod.None + GetMBOItems gives individual orders
                    // with unique Level2Item.Id values that become the mboOrders dictionary keys.
                    var snap = symbol.DepthOfMarket.GetDepthOfMarketAggregatedCollections(new GetLevel2ItemsParameters
                    {
                        AggregateMethod = AggregateMethod.None,
                        GetMBOItems = true,
                        LevelsCount = config.SnapshotDepth
                    });

                    if (snap == null)
                    {
                        log("MBO snapshot returned null — downgrading to MBP mode for this session.", LoggingLevel.System);
                        Calculator.MboMode = false;
                        // fall through to MBP path below
                    }
                    else
                    {
                        DateTime seedTime = DateTime.UtcNow;
                        int bc = 0, ac = 0;
                        int totalItems = (snap.Bids != null ? snap.Bids.Length : 0) + (snap.Asks != null ? snap.Asks.Length : 0);

                        if (snap.Bids != null)
                            foreach (var b in snap.Bids)
                                if (b?.Id != null)
                                {
                                    Calculator.ProcessLevel2ItemMbo(b.Id, b.Price, b.Size, true);
                                    mboBook?.ApplySnapshot(seedTime, b.Id, true, b.Price, b.Size, b.Priority, b.NumberOrders);
                                    bc++;
                                }
                        if (snap.Asks != null)
                            foreach (var a in snap.Asks)
                                if (a?.Id != null)
                                {
                                    Calculator.ProcessLevel2ItemMbo(a.Id, a.Price, a.Size, false);
                                    mboBook?.ApplySnapshot(seedTime, a.Id, false, a.Price, a.Size, a.Priority, a.NumberOrders);
                                    ac++;
                                }

                        // C-04: if the snapshot contained items but none had real order IDs,
                        // the feed is delivering MBP-aggregated data labelled as MBO. Downgrade.
                        if (totalItems > 0 && bc + ac == 0)
                        {
                            log("MBO snapshot contained no real order IDs — feed appears MBP-aggregated. Downgrading to MBP mode.", LoggingLevel.System);
                            Calculator.MboMode = false;
                            mboBook = null;
                            // fall through to MBP path below
                        }
                        else
                        {
                            isBookValid = true;
                            log($"MBO book seeded: {bc} bid orders, {ac} ask orders tracked.", LoggingLevel.System);
                            return;
                        }
                    }
                }

                // MBP path (fallback or non-dxFeed connections)
                var snapshot = symbol.DepthOfMarket.GetDepthOfMarketAggregatedCollections(new GetLevel2ItemsParameters
                {
                    AggregateMethod = AggregateMethod.ByPriceLVL,
                    LevelsCount = config.SnapshotDepth
                });

                if (snapshot == null)
                {
                    log("Initial L2 snapshot returned null (L2 feed not yet active). Book will be seeded from the live stream.", LoggingLevel.System);
                    return;
                }

                if (snapshot.Bids != null)
                    foreach (var bid in snapshot.Bids)
                        if (bid != null) Calculator?.ProcessLevel2Item(bid.Price, bid.Size, true);

                if (snapshot.Asks != null)
                    foreach (var ask in snapshot.Asks)
                        if (ask != null) Calculator?.ProcessLevel2Item(ask.Price, ask.Size, false);

                isBookValid = true;
            }
            catch (Exception ex)
            {
                log("Error loading initial Level 2 snapshot: " + ex.Message, LoggingLevel.Error);
            }
        }

        #endregion

        #region Event Intake

        private void OnNewLast(Symbol sym, Last last)
        {
            if (!initialized || eventQueue == null || last == null) return;
            // H-02: prefer feed-reported event time over wall-clock receive time.
            DateTime eventTime = last.Time != default ? last.Time : DateTime.UtcNow;
            EnqueueEvent(new MarketEvent(eventTime, last.Price, last.Size, last.AggressorFlag));
        }

        private void OnNewLevel2(Symbol sym, Level2Quote level2, DOMQuote dom)
        {
            if (!initialized || eventQueue == null || level2 == null) return;
            bool isBid = level2.PriceType == QuotePriceType.Bid;
            EnqueueEvent(new MarketEvent(DateTime.UtcNow, level2.Id, level2.Price, level2.Size, isBid, level2.Closed,
                                         level2.Priority, level2.NumberOrders));
        }

        private void EnqueueEvent(MarketEvent evt)
        {
            if (eventQueue == null) return;

            if (!eventQueue.TryAdd(evt))
            {
                isQueueOverflowed = true;
                Interlocked.Increment(ref queueOverflowCount);
                isBookValid = false;
            }
        }

        #endregion

        #region Worker

        private void ProcessQueueLoop()
        {
            if (workerCts == null || eventQueue == null) return;

            var token = workerCts.Token;
            long lastPublishTicks = 0;
            long publishIntervalTicks = TimeSpan.TicksPerMillisecond * config.UpdateFrequencyMs;
            bool dirty = false;

            try
            {
                while (!token.IsCancellationRequested && !eventQueue.IsCompleted)
                {
                    if (eventQueue.TryTake(out MarketEvent evt, 10, token))
                    {
                        if (isQueueOverflowed)
                        {
                            // Discard the already-dequeued event — it belongs to the pre-overflow
                            // epoch and must not be applied to the freshly rebuilt book.
                            RecoverFromOverflow();
                            continue;
                        }

                        ProcessEventOnWorker(evt);
                        dirty = true;

                        long now = DateTime.UtcNow.Ticks;
                        if (now - lastPublishTicks >= publishIntervalTicks)
                        {
                            // H-03: the event already advanced the event clock; passing false prevents
                            // PublishSnapshot from calling AdvanceTime(wall-clock) which would skew
                            // currentTicks past any events still buffered in the queue.
                            PublishSnapshot(advanceTime: false);
                            lastPublishTicks = now;
                            dirty = false;
                        }
                    }
                    else if (dirty)
                    {
                        long now = DateTime.UtcNow.Ticks;
                        if (now - lastPublishTicks >= publishIntervalTicks)
                        {
                            // Idle path: advance time so rolling windows decay during quiet periods.
                            PublishSnapshot(advanceTime: true);
                            lastPublishTicks = now;
                            dirty = false;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal termination
            }
            catch (Exception ex)
            {
                log($"Error in background worker thread: {ex.Message}", LoggingLevel.Error);
            }
        }

        private void ProcessEventOnWorker(MarketEvent evt)
        {
            if (Calculator == null) return;

            CheckSessionRollover(evt.Time);

            if (evt.Kind == MarketEventKind.Trade)
                Calculator.ProcessTrade(evt.Time, evt.Price, evt.Size, evt.Aggressor);
            else if (evt.Kind == MarketEventKind.BookLevel)
            {
                if (Calculator.MboMode && mboBook != null)
                {
                    // Normalise raw quote into a typed MboEvent (Add/Update/Remove) and process
                    // via the order-ID-aware pipeline for exact arrival/cancel/spoof tracking.
                    var mboEvent = mboBook.Apply(evt.Time, evt.Id, evt.IsBid, evt.Price, evt.Size,
                                                 evt.Priority, evt.NumberOrders, evt.Closed);
                    Calculator.ProcessMboEvent(mboEvent);
                }
                else
                {
                    Calculator.ProcessLevel2(evt.Time, evt.Id, evt.Price, evt.Size, evt.IsBid, evt.Closed);
                }

                // Require both bid and ask sides before declaring the book valid.
                // A single incremental update cannot establish a complete book.
                if (!isBookValid && Calculator.HasTwoSidedBook)
                {
                    isBookValid = true;
                    log("L2 book bootstrapped from live stream (both sides now present).", LoggingLevel.System);
                }

                // C-04: verify MBO real-ID coverage after the first 100 live BookLevel events.
                // If fewer than half carry real order IDs the feed is aggregated (MBP-shaped),
                // so synthetic keys would dominate and Exact-quality labels would be false.
                if (Calculator.MboMode && mboBook != null)
                {
                    int totalSeen = mboBook.RealIdEvents + mboBook.SyntheticIdEvents;
                    if (totalSeen == 100)
                    {
                        double coverage = mboBook.RealIdCoverage;
                        if (coverage < 0.5)
                        {
                            log($"MBO real-ID coverage too low ({coverage:P0} over first {totalSeen} events) — downgrading to MBP mode.", LoggingLevel.System);
                            Calculator.MboMode = false;
                            mboBook = null;
                        }
                        else
                        {
                            log($"MBO capability confirmed: {coverage:P0} real-ID coverage over first {totalSeen} events.", LoggingLevel.System);
                        }
                    }
                }
            }

            OnEventProcessed?.Invoke();
        }

        private void RecoverFromOverflow()
        {
            log("Queue overflow detected. Initiating recovery state machine...", LoggingLevel.System);

            if (eventQueue != null)
                while (eventQueue.TryTake(out _)) { }

            Calculator?.ResetBookStateOnly();
            mboBook?.Clear();
            isBookValid = false;

            try
            {
                if (Calculator?.MboMode == true)
                {
                    var snap = symbol.DepthOfMarket.GetDepthOfMarketAggregatedCollections(new GetLevel2ItemsParameters
                    {
                        AggregateMethod = AggregateMethod.None,
                        GetMBOItems = true,
                        LevelsCount = config.SnapshotDepth
                    });

                    if (snap == null)
                    {
                        log("MBO recovery snapshot returned null — downgrading to MBP mode.", LoggingLevel.System);
                        Calculator.MboMode = false;
                        mboBook = null;
                        // fall through to MBP path below
                    }
                    else
                    {
                        DateTime seedTime = DateTime.UtcNow;
                        int bc = 0, ac = 0;
                        if (snap.Bids != null)
                            foreach (var b in snap.Bids)
                                if (b?.Id != null)
                                {
                                    Calculator.ProcessLevel2ItemMbo(b.Id, b.Price, b.Size, true);
                                    mboBook?.ApplySnapshot(seedTime, b.Id, true, b.Price, b.Size, b.Priority, b.NumberOrders);
                                    bc++;
                                }
                        if (snap.Asks != null)
                            foreach (var a in snap.Asks)
                                if (a?.Id != null)
                                {
                                    Calculator.ProcessLevel2ItemMbo(a.Id, a.Price, a.Size, false);
                                    mboBook?.ApplySnapshot(seedTime, a.Id, false, a.Price, a.Size, a.Priority, a.NumberOrders);
                                    ac++;
                                }
                        isBookValid = true;
                        log($"Queue Overflow Recovery complete. MBO book rebuilt: {bc} bid orders, {ac} ask orders.", LoggingLevel.System);
                        isQueueOverflowed = false;
                        return;
                    }
                }

                var snapshot = symbol.DepthOfMarket.GetDepthOfMarketAggregatedCollections(new GetLevel2ItemsParameters
                {
                    AggregateMethod = AggregateMethod.ByPriceLVL,
                    LevelsCount = config.SnapshotDepth
                });

                if (snapshot != null)
                {
                    if (snapshot.Bids != null)
                        foreach (var bid in snapshot.Bids)
                            if (bid != null) Calculator?.ProcessLevel2Item(bid.Price, bid.Size, true);

                    if (snapshot.Asks != null)
                        foreach (var ask in snapshot.Asks)
                            if (ask != null) Calculator?.ProcessLevel2Item(ask.Price, ask.Size, false);

                    isBookValid = true;
                    log("Queue Overflow Recovery complete. L2 book successfully rebuilt.", LoggingLevel.System);
                }
            }
            catch (Exception ex)
            {
                log("Queue Overflow Recovery failed snapshot request: " + ex.Message, LoggingLevel.Error);
            }

            // Only clear the overflow flag when a snapshot actually rebuilt the book.
            // If the snapshot call failed or returned null, leave isQueueOverflowed=true so
            // the next event triggers another recovery attempt rather than re-entering normal
            // processing against an invalid book.
            if (isBookValid) isQueueOverflowed = false;
        }

        private void CheckSessionRollover(DateTime utcTime)
        {
            DateTime chicagoTime = ConvertUtcToChicago(utcTime);

            // CME futures session rollover happens at 17:00 Chicago time
            DateTime sessionDate = chicagoTime.TimeOfDay >= new TimeSpan(17, 0, 0)
                ? chicagoTime.Date.AddDays(1)
                : chicagoTime.Date;

            if (lastSessionDate != DateTime.MinValue && sessionDate != lastSessionDate)
            {
                log($"[Session Rollover] Rollover from {lastSessionDate:yyyy-MM-dd} to {sessionDate:yyyy-MM-dd}. Resetting session metrics.", LoggingLevel.System);
                Calculator?.ResetSessionState();
                OnSessionRollover?.Invoke();
            }

            lastSessionDate = sessionDate;
        }

        #endregion

        #region Snapshot

        public void PublishSnapshot(bool advanceTime = true)
        {
            if (Calculator == null) return;

            // Decay rolling windows up to the current platform time before sampling, so metrics
            // still age out during quiet periods with no incoming events.
            // H-03: only advance when the queue is idle; skip when events were just processed so
            // the event clock is not pushed past events that are still buffered in the queue.
            if (advanceTime)
                Calculator.AdvanceTime(Core.Instance.TimeUtils.DateTimeUtcNow);

            var calc = Calculator;
            currentSnapshot = new AnalyticsSnapshot
            {
                Version = (currentSnapshot?.Version ?? 0) + 1,
                ReceiveTicks = DateTime.UtcNow.Ticks,
                BookValid = isBookValid,
                IsCalibrated = calc.IsCalibrated,
                IsMboActive = calc.MboMode,
                OrderCountWindowShort = calc.OrderCountWindowShort,
                OrderCountWindowLong = calc.OrderCountWindowLong,
                TradeVolumeWindowShort = calc.TradeVolumeWindowShort,
                TradeVolumeWindowLong = calc.TradeVolumeWindowLong,

                BuyerTradeCountShort = calc.GetBuyerInitiatedTradeCountShort(),
                SellerTradeCountShort = calc.GetSellerInitiatedTradeCountShort(),
                BuyerSellerTradeCountRatioShort = calc.GetBuyerSellerTradeCountRatioShort(),
                BuyerSellerQtyRatioShort = calc.GetBuyerSellerQtyRatioShort(),
                CumulativeBuyerSellerTradeCountRatio = calc.GetCumulativeBuyerSellerTradeCountRatio(),

                BuyTradeCount60 = calc.GetBuyTradeCount60(),
                SellTradeCount60 = calc.GetSellTradeCount60(),
                BuyVolume60 = calc.GetBuyVolume60(),
                SellVolume60 = calc.GetSellVolume60(),
                BuySellCountRatio60 = calc.GetBuySellCountRatio60(),
                BuySellVolumeRatio60 = calc.GetBuySellVolumeRatio60(),

                CumBuyVolume = calc.GetCumBuyVolume(),
                CumSellVolume = calc.GetCumSellVolume(),
                CumDelta = calc.GetCumDelta(),
                CumTradeCount = calc.GetCumTradeCount(),

                TradesVwapLong = calc.GetTradesVwapLong(),
                BuyerTradesVwapLong = calc.GetBuyerTradesVwapLong(),
                SellerTradesVwapLong = calc.GetSellerTradesVwapLong(),
                CumulativeTradesVwap = calc.GetCumulativeTradesVwap(),

                RollingVwap1m = calc.GetRollingVwap1m(),
                RollingVwap5m = calc.GetRollingVwap5m(),
                RollingVwap15m = calc.GetRollingVwap15m(),
                SessionVwap = calc.GetSessionVwap(),
                VwapDistance = calc.GetVwapDistance(),
                TradePriceStdDev = calc.GetTradePriceStdDev(),
                BuyVwap = calc.GetBuyVwap(),
                SellVwap = calc.GetSellVwap(),

                BidL2AddEventCountShort = calc.GetBidL2AddEventCountShort(),
                BidL2AddedVisibleQtyShort = calc.GetBidL2AddedVisibleQtyShort(),
                AskL2AddEventCountShort = calc.GetAskL2AddEventCountShort(),
                AskL2AddedVisibleQtyShort = calc.GetAskL2AddedVisibleQtyShort(),
                CumulativeL2AddEventCount = calc.GetCumulativeL2AddEventCount(),

                NewOrderCount = calc.GetNewOrderCount60(),
                NewBidCount = calc.GetNewBidCount60(),
                NewAskCount = calc.GetNewAskCount60(),
                NewBidVolume = calc.GetNewBidVolume60(),
                NewAskVolume = calc.GetNewAskVolume60(),

                MeanBidAddedVisibleQtyLong = calc.GetMeanBidAddedVisibleQtyLong(),
                MeanAskAddedVisibleQtyLong = calc.GetMeanAskAddedVisibleQtyLong(),
                StdDevBidAddedVisibleQtyLong = calc.GetStdDevBidAddedVisibleQtyLong(),
                StdDevAskAddedVisibleQtyLong = calc.GetStdDevAskAddedVisibleQtyLong(),

                L2RemoveEventCountShort = calc.GetL2RemoveEventCountShort(),
                EstimatedCancelQtyShort = calc.GetEstimatedCancelQtyShort(),
                RemovedToAddedVisibleRatioCountShort = calc.GetRemovedToAddedVisibleRatioCountShort(),
                RemovedToAddedVisibleRatioQtyShort = calc.GetRemovedToAddedVisibleRatioQtyShort(),
                CumulativeEstimatedCancelVwap = calc.GetCumulativeEstimatedCancelVwap(),

                CancelCount = calc.GetCancelCount60(),
                CancelBidCount = calc.GetCancelBidCount60(),
                CancelAskCount = calc.GetCancelAskCount60(),
                CancelBidVolume = calc.GetCancelBidVolume60(),
                CancelAskVolume = calc.GetCancelAskVolume60(),
                CancelRatio = calc.GetCancelRatio60(),
                CancelVolumeRatio = calc.GetCancelVolumeRatio60(),

                DOMImbalance3 = calc.GetDOMImbalance3(),
                DOMImbalance5 = calc.GetDOMImbalance5(),
                DOMImbalance10 = calc.GetDOMImbalance10(),
                QueueImbalance = calc.GetQueueImbalance(),
                BookPressure = calc.GetBookPressure(),
                BestBidOrderCount = calc.GetBestBidOrderCount(),
                BestAskOrderCount = calc.GetBestAskOrderCount(),
                BestBidAvgOrderSize = calc.GetBestBidAvgOrderSize(),
                BestAskAvgOrderSize = calc.GetBestAskAvgOrderSize(),

                AbsorptionBuy = calc.GetAbsorptionBuy(),
                AbsorptionSell = calc.GetAbsorptionSell(),
                IcebergScoreBid = calc.GetIcebergScoreBid(),
                IcebergScoreAsk = calc.GetIcebergScoreAsk(),
                ReplenishmentBid = calc.GetReplenishmentBid(),
                ReplenishmentAsk = calc.GetReplenishmentAsk(),
                SpoofScoreBid = calc.GetSpoofScoreBid(),
                SpoofScoreAsk = calc.GetSpoofScoreAsk(),

                Delta1s = calc.GetDelta1s(),
                Delta5s = calc.GetDelta5s(),
                Delta30s = calc.GetDelta30s(),
                Delta60s = calc.GetDelta60s(),
                DeltaVelocity = calc.GetDeltaVelocity()
            };

            if (config.EnableFeatureStore && !string.IsNullOrEmpty(config.FeatureStorePath))
                calc.AppendToCsvFeatureStore(config.FeatureStorePath);
        }

        #endregion

        #region Time Helpers

        private static DateTime ConvertUtcToEst(DateTime utcDateTime)
        {
            try
            {
                TimeZoneInfo estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, estZone);
            }
            catch
            {
                return utcDateTime.AddHours(-5);
            }
        }

        private static DateTime ConvertUtcToChicago(DateTime utcDateTime)
        {
            try
            {
                TimeZoneInfo chicagoZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, chicagoZone);
            }
            catch
            {
                return utcDateTime.AddHours(-5);
            }
        }

        #endregion
    }
}
