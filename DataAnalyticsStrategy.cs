using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradingPlatform.BusinessLayer;
using TradingPlatform.BusinessLayer.Integration;

namespace MBO_Market_Data_Analytics
{
    public class DataAnalyticsStrategy : Strategy, ICurrentSymbol, ICurrentAccount
    {
        // -----------------------------------------------------
        // Input Parameters (Exposed to Quantower UI Settings)
        // -----------------------------------------------------
        [InputParameter("Symbol", 0)]
        public Symbol CurrentSymbol { get; set; } = null!;

        [InputParameter("Account", 1)]
        public Account CurrentAccount { get; set; } = null!;

        [InputParameter("Order Quantity", 2, minimum: 1, maximum: 100, increment: 1)]
        public double OrderQty = 1;

        [InputParameter("Take Profit (Ticks)", 3, minimum: 0, maximum: 1000, increment: 1)]
        public int TakeProfitTicks = 10;

        [InputParameter("Stop Loss (Ticks)", 4, minimum: 0, maximum: 1000, increment: 1)]
        public int StopLossTicks = 10;

        [InputParameter("Max Exposure (Contracts)", 5, minimum: 1, maximum: 100, increment: 1)]
        public int MaxExposure = 2;

        [InputParameter("Max Daily Loss ($)", 6, minimum: 0, maximum: 100000, increment: 10)]
        public double MaxDailyLoss = 500.0;

        [InputParameter("Buyer/Seller Ratio Buy Threshold", 7, minimum: 1.0, maximum: 10.0, increment: 0.1)]
        public double RatioBuyThreshold = 1.5;

        [InputParameter("Buyer/Seller Ratio Sell Threshold", 8, minimum: 0.1, maximum: 1.0, increment: 0.1)]
        public double RatioSellThreshold = 0.67;

        [InputParameter("Ratio Buy Reset Threshold", 17, minimum: 0.5, maximum: 5.0, increment: 0.05)]
        public double RatioBuyResetThreshold = 1.1;

        [InputParameter("Ratio Sell Reset Threshold", 18, minimum: 0.2, maximum: 2.0, increment: 0.05)]
        public double RatioSellResetThreshold = 0.9;

        [InputParameter("Max Consecutive Failures", 19, minimum: 1, maximum: 20, increment: 1)]
        public int MaxConsecutiveFailures = 5;

        [InputParameter("Calibration Mode", 9, variants: new object[] {
            "Auto (MTR Calibration)", CalibrationMode.AutoMTR,
            "Manual", CalibrationMode.Manual
        })]
        public CalibrationMode InputCalibrationMode = CalibrationMode.AutoMTR;

        [InputParameter("Short Trade Volume Window", 10, minimum: 10, maximum: 1000000, increment: 10, decimalPlaces: 0)]
        public double InputTradeVolumeShort = 1000;

        [InputParameter("Long Trade Volume Window", 11, minimum: 50, maximum: 5000000, increment: 50, decimalPlaces: 0)]
        public double InputTradeVolumeLong = 5000;

        [InputParameter("Manual Short L2 Window", 12, minimum: 100, maximum: 500000, increment: 100, decimalPlaces: 0)]
        public int InputOrderCountShort = 2000;

        [InputParameter("Manual Long L2 Window", 13, minimum: 500, maximum: 2000000, increment: 500, decimalPlaces: 0)]
        public int InputOrderCountLong = 10000;

        [InputParameter("MTR Calibration Trades Count", 14, minimum: 100, maximum: 10000, increment: 100, decimalPlaces: 0)]
        public int InputCalibrationTrades = 1000;

        [InputParameter("Initial Snapshot Depth Levels", 15, minimum: 5, maximum: 2000, increment: 5, decimalPlaces: 0)]
        public int InputSnapshotDepth = 360;

        [InputParameter("Order Cooldown (Seconds)", 16, minimum: 0, maximum: 300, increment: 1, decimalPlaces: 0)]
        public int InputCooldownSeconds = 5;

        // ---- Adaptive parameters (Phase 1) ----
        [InputParameter("Parameter Mode", 20, variants: new object[] {
            "Adaptive (self-tuning)", StrategyParameterMode.Adaptive,
            "Static (fixed inputs)", StrategyParameterMode.Static
        })]
        public StrategyParameterMode InputParameterMode = StrategyParameterMode.Adaptive;

        [InputParameter("Adaptive Entry Percentile", 21, minimum: 0.55, maximum: 0.99, increment: 0.01, decimalPlaces: 2)]
        public double InputEntryPercentile = 0.85;

        [InputParameter("Adaptive ATR Period (Minutes)", 22, minimum: 2, maximum: 100, increment: 1, decimalPlaces: 0)]
        public int InputAtrPeriod = 14;

        [InputParameter("Adaptive TP ATR Multiplier", 23, minimum: 0.1, maximum: 10.0, increment: 0.1, decimalPlaces: 1)]
        public double InputTpAtrMultiplier = 1.0;

        [InputParameter("Adaptive SL ATR Multiplier", 24, minimum: 0.1, maximum: 10.0, increment: 0.1, decimalPlaces: 1)]
        public double InputSlAtrMultiplier = 1.0;

        // ---- Execution envelope (shadow / paper → promotion gate) ----
        [InputParameter("Execution Mode", 25, variants: new object[] {
            "Shadow then promote", ExecutionMode.ShadowThenPromote,
            "Shadow only (paper)", ExecutionMode.ShadowOnly,
            "Live only (no gate)", ExecutionMode.LiveOnly
        })]
        public ExecutionMode InputExecutionMode = ExecutionMode.ShadowThenPromote;

        [InputParameter("Commission per Contract ($, per side)", 26, minimum: 0, maximum: 100, increment: 0.05, decimalPlaces: 2)]
        public double InputCommissionPerContract = 0.0;

        [InputParameter("Slippage (Ticks)", 27, minimum: 0, maximum: 50, increment: 1, decimalPlaces: 0)]
        public int InputSlippageTicks = 0;

        [InputParameter("Promotion Min Trades", 28, minimum: 5, maximum: 10000, increment: 5, decimalPlaces: 0)]
        public int InputPromotionMinTrades = 30;

        [InputParameter("Promotion/Demotion Confidence Z", 29, minimum: 0.0, maximum: 4.0, increment: 0.05, decimalPlaces: 2)]
        public double InputPromotionZ = 1.64;

        [InputParameter("Promotion Min Expectancy ($/trade)", 30, minimum: 0.0, maximum: 1000.0, increment: 0.5, decimalPlaces: 2)]
        public double InputPromotionMinExpectancy = 0.0;

        [InputParameter("Demotion Min Trades", 31, minimum: 5, maximum: 10000, increment: 5, decimalPlaces: 0)]
        public int InputDemotionMinTrades = 20;

        // Phase 3 — regime classifier gates
        [InputParameter("Volatility Gate Ratio (fastATR/ATR)", 32, minimum: 1.0, maximum: 5.0, increment: 0.1, decimalPlaces: 1)]
        public double InputVolatilityGateRatio = 1.5;

        [InputParameter("Thin Queue Stand-Aside (MBO orders, 0=off)", 33, minimum: 0, maximum: 20, increment: 1, decimalPlaces: 0)]
        public int InputThinQueueThreshold = 2;

        private AnalyticsEngineHost? engine;

        // Phase 1 adaptive controller (null in Static mode)
        private AdaptiveParameterController? adaptiveController;
        // Phase 2: last known polarity — used to detect transitions and reset signal state
        private SignalPolarity lastPolarity = SignalPolarity.Momentum;
        // Phase 3: last known regime — used to log stand-aside transitions
        private RegimeState lastRegime = RegimeState.Normal;

        // Execution envelope: paper simulator + promotion/demotion state
        private ShadowSimulator? shadowSim;
        private readonly TradePerformanceTracker livePerf = new TradePerformanceTracker();
        private volatile bool isLivePromoted = false;
        private double pnlAtLastFlat = 0.0; // live realized PnL at last flat; under stateLock

        private volatile bool isOrderPlacementPending = false;

        // Prioritized async executor queue
        private PrioritizedAsyncTaskQueue? prioritizedQueue;

        // Tracked active orders
        private readonly ConcurrentDictionary<string, TrackedOrderState> trackedOrders = new();

        // Guards all position / PnL / signal-state mutation below. Mutated from the worker thread
        // (EvaluateTradingSignal) and from platform callback threads (OnOrderUpdated, ReconcileBrokerState).
        private readonly object stateLock = new object();

        // Local state tracking (read/written only under stateLock)
        private double averageEntryPrice = 0.0;
        private double currentPositionSize = 0.0; // Positive for long, negative for short
        private double strategyRealizedPnL = 0.0;
        private DateTime lastOrderPlacementTime = DateTime.MinValue;
        private bool buySignalActive = false;
        private bool sellSignalActive = false;
        private int consecutiveOrderFailures = 0;

        // Cross-thread flags (volatile: read on worker thread, written on callback threads)
        // Data (dxFeed) and execution (IBKR) are separate connections and tracked independently.
        private volatile bool isDataConnectionActive = false;
        private volatile bool isExecutionConnectionActive = false;
        // Entries require both connections. Reconciliation and bracket management only need execution.
        private bool isConnectionActive => isDataConnectionActive && isExecutionConnectionActive;
        private volatile bool isRiskHalted = false;

        // Diagnostic logging state (worker thread only)
        private bool adaptiveReadyLogged = false;
        private DateTime lastStatusLogTime = DateTime.MinValue;

        // Order comment tags used to recover an order's role after a restart or reconnect.
        private const string EntryTag = "MBO:ENTRY";
        private const string BracketTag = "MBO:BRK";

        private static OrderRole RoleFromComment(string? comment)
        {
            if (!string.IsNullOrEmpty(comment))
            {
                if (comment.IndexOf(EntryTag, StringComparison.Ordinal) >= 0) return OrderRole.Entry;
                if (comment.IndexOf(BracketTag, StringComparison.Ordinal) >= 0) return OrderRole.Bracket;
            }
            return OrderRole.Reconciled;
        }

        public override string[] MonitoringConnectionsIds => new[]
        {
            this.CurrentSymbol?.ConnectionId ?? "",
            this.CurrentAccount?.ConnectionId ?? ""
        };

        public DataAnalyticsStrategy() : base()
        {
            this.Name = "Data Microstructure Analytics Strategy";
            this.Description = "Production-grade execution strategy using dxFeed Level 2 and Trade ticks for CME Futures microstructure order placement via Interactive Brokers.";
        }

        protected override void OnRun()
        {
            if (this.CurrentSymbol == null || this.CurrentAccount == null)
            {
                this.LogError("Incorrect input parameters... Symbol or Account are not specified.");
                return;
            }

            // Retrieve active mapped symbols / accounts
            this.CurrentSymbol = Core.Instance.GetSymbol(this.CurrentSymbol.CreateInfo());
            var accountInfo = this.CurrentAccount.CreateInfo();
            this.CurrentAccount = Core.Instance.Accounts.FirstOrDefault(a => a.Id == accountInfo.Id) ?? this.CurrentAccount;

            Log($"Initializing DataAnalyticsStrategy for Symbol: {CurrentSymbol.Name}, Account: {CurrentAccount.Name}", StrategyLoggingLevel.Info);

            isRiskHalted = false;
            strategyRealizedPnL = 0.0;
            averageEntryPrice = 0.0;
            currentPositionSize = 0.0;
            trackedOrders.Clear();
            buySignalActive = false;
            sellSignalActive = false;
            lastOrderPlacementTime = DateTime.MinValue;
            consecutiveOrderFailures = 0;
            adaptiveReadyLogged = false;
            lastStatusLogTime = DateTime.MinValue;

            // Initialize prioritized order execution queue
            prioritizedQueue = new PrioritizedAsyncTaskQueue();

            // Build and start the shared analytics engine (worker thread, auto-MTR calibration,
            // book seed, overflow recovery, session rollover). Trading signals are evaluated on the
            // worker thread via the OnEventProcessed hook after each market event is applied.
            var config = new AnalyticsEngineConfig
            {
                CalibrationMode = InputCalibrationMode,
                TradeVolumeShort = InputTradeVolumeShort,
                TradeVolumeLong = InputTradeVolumeLong,
                OrderCountShort = InputOrderCountShort,
                OrderCountLong = InputOrderCountLong,
                CalibrationTrades = InputCalibrationTrades,
                SnapshotDepth = InputSnapshotDepth,
                UpdateFrequencyMs = 250,
                EnableFeatureStore = false,
                FeatureStorePath = ""
            };

            engine = new AnalyticsEngineHost(this.CurrentSymbol, config, (msg, lvl) => Core.Instance.Loggers.Log(msg, lvl))
            {
                OnEventProcessed = EvaluateTradingSignal,
                OnSessionRollover = OnSessionRollover
            };
            // H-01: engine.Start() deferred to after all dependent state (adaptive controller,
            // shadow sim, connection flags, and broker reconciliation) is fully initialized.

            // Phase 1: build the self-tuning parameter controller and seed its ATR from history.
            if (InputParameterMode == StrategyParameterMode.Adaptive)
            {
                double upper = Math.Clamp(InputEntryPercentile, 0.55, 0.99);
                adaptiveController = new AdaptiveParameterController(new AdaptiveParameterConfig
                {
                    EntryUpperPercentile = upper,
                    EntryLowerPercentile = 1.0 - upper,
                    AtrPeriod = InputAtrPeriod,
                    TpAtrMultiplier = InputTpAtrMultiplier,
                    SlAtrMultiplier = InputSlAtrMultiplier,
                    VolatilityGateRatio = Math.Max(1.0, InputVolatilityGateRatio)
                }, this.CurrentSymbol.TickSize);

                SeedAdaptiveAtrFromHistory();
            }

            // Execution envelope: build the paper simulator and set the starting routing.
            shadowSim = new ShadowSimulator(new ShadowSimulator.SimConfig
            {
                TickSize = this.CurrentSymbol.TickSize,
                PointValue = GetPointCost(),
                OrderQty = OrderQty,
                CommissionPerContract = InputCommissionPerContract,
                SlippageTicks = InputSlippageTicks,
                CooldownSeconds = InputCooldownSeconds
            });
            livePerf.Reset();
            pnlAtLastFlat = 0.0;
            isLivePromoted = InputExecutionMode == ExecutionMode.LiveOnly;
            Log($"[Execution] Mode: {InputExecutionMode}. Starting in {(isLivePromoted ? "LIVE" : "SHADOW (paper)")}.", StrategyLoggingLevel.Info);

            // Subscribe to order/position events on Core
            Core.OrderAdded += OnCoreOrderAdded;
            Core.OrderRemoved += OnCoreOrderRemoved;
            Core.PositionAdded += OnCorePositionAdded;
            Core.PositionRemoved += OnCorePositionRemoved;

            // Subscribe to data connection (dxFeed) and execution connection (IBKR) separately.
            var dataConn = Core.Instance.Connections.Connected.FirstOrDefault(c => c.Id == CurrentSymbol.ConnectionId);
            if (dataConn != null)
            {
                dataConn.StateChanged += OnConnectionStateChanged;
                isDataConnectionActive = dataConn.State == ConnectionState.Connected;
            }

            var execConn = Core.Instance.Connections.Connected.FirstOrDefault(c => c.Id == CurrentAccount.ConnectionId);
            if (execConn != null)
            {
                // Only subscribe once — if both connections share the same ID they share one handler.
                if (execConn.Id != dataConn?.Id)
                    execConn.StateChanged += OnConnectionStateChanged;
                isExecutionConnectionActive = execConn.State == ConnectionState.Connected;
            }

            // Perform initial reconciliation before any market events can trigger signal evaluation.
            ReconcileBrokerState();

            // H-01: start the engine only after all state is ready — adaptive controller, shadow sim,
            // connection flags, and broker position are all set. The worker's OnEventProcessed callback
            // (EvaluateTradingSignal) cannot fire until Start() is called.
            engine.Start();
        }

        protected override void OnStop()
        {
            // H-24: Cancel all working broker orders and flatten any open position before teardown.
            // Done synchronously here (not via prioritizedQueue) so it executes even if the queue
            // is already draining or not yet processing.
            foreach (var kv in trackedOrders)
            {
                var state = kv.Value;
                if (state.OrderInstance != null &&
                    (state.Status == OrderStatus.Opened || state.Status == OrderStatus.PartiallyFilled))
                {
                    try { state.OrderInstance.Cancel(); }
                    catch (Exception ex) { Log($"[OnStop] Order cancel failed: {ex.Message}", StrategyLoggingLevel.Error); }
                }
            }
            double stopPosSize = GetBrokerPositionSize();
            if (stopPosSize != 0.0 && CurrentSymbol != null && CurrentAccount != null)
            {
                Log($"[OnStop] Flattening open position ({stopPosSize:+0.##;-0.##} contracts) on strategy stop.", StrategyLoggingLevel.Error);
                Side flatSide = stopPosSize > 0.0 ? Side.Sell : Side.Buy;
                var mktType = CurrentSymbol.GetAlowedOrderTypes(OrderTypeUsage.Order)
                                 .FirstOrDefault(ot => ot.Behavior == OrderTypeBehavior.Market) ??
                             CurrentSymbol.GetAlowedOrderTypes(OrderTypeUsage.All)
                                 .FirstOrDefault(ot => ot.Behavior == OrderTypeBehavior.Market);
                try
                {
                    Core.Instance.PlaceOrder(new PlaceOrderRequestParameters
                    {
                        Symbol = CurrentSymbol,
                        Account = CurrentAccount,
                        Side = flatSide,
                        OrderTypeId = mktType?.Id ?? OrderType.Market,
                        Quantity = Math.Abs(stopPosSize)
                    });
                }
                catch (Exception ex) { Log($"[OnStop] Market flatten failed: {ex.Message}", StrategyLoggingLevel.Error); }
            }

            // Unsubscribe from order/position events on Core
            Core.OrderAdded -= OnCoreOrderAdded;
            Core.OrderRemoved -= OnCoreOrderRemoved;
            Core.PositionAdded -= OnCorePositionAdded;
            Core.PositionRemoved -= OnCorePositionRemoved;

            // Unsubscribe from all monitored connections.
            var dataConnId = this.CurrentSymbol?.ConnectionId;
            var execConnId = this.CurrentAccount?.ConnectionId;
            foreach (var conn in Core.Instance.Connections.Connected)
            {
                if (conn.Id == dataConnId || conn.Id == execConnId)
                    conn.StateChanged -= OnConnectionStateChanged;
            }

            // Stop the analytics engine (also tears down its worker thread feeding signals)
            engine?.Stop();
            engine = null;
            adaptiveController = null;
            shadowSim = null;

            // Unsubscribe order updates
            foreach (var state in trackedOrders.Values)
            {
                if (state.OrderInstance != null)
                {
                    state.OrderInstance.Updated -= OnOrderUpdated;
                }
            }
            trackedOrders.Clear();

            prioritizedQueue?.Shutdown();

            isOrderPlacementPending = false;

            // Force garbage collection to flush and release any open Serilog files
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        // Invoked by the engine (on the worker thread) when a CME session rollover is detected,
        // after the calculator's session state has been reset.
        private void OnSessionRollover()
        {
            // Flat open positions at CME session end
            prioritizedQueue?.Enqueue(() =>
            {
                FlattenPosition();
                return Task.CompletedTask;
            }, 0);

            lock (stateLock)
            {
                strategyRealizedPnL = 0.0; // Reset daily realized PnL on session rollover
            }
        }

        // Helper to get point cost mathematically from tick cost and size
        private double GetPointCost()
        {
            if (CurrentSymbol == null) return 1.0;
            double price = CurrentSymbol.Last;
            if (price <= 0.0) price = CurrentSymbol.Bid > 0.0 ? CurrentSymbol.Bid : 1.0;
            double tickCost = CurrentSymbol.GetTickCost(price);
            double tickSize = CurrentSymbol.TickSize;
            if (tickSize <= 0.0) tickSize = 1.0;
            return tickCost > 0.0 ? (tickCost / tickSize) : 1.0;
        }

        // Seeds the adaptive controller's ATR from 1-minute history so brackets are sized sensibly
        // before enough live minute bars have formed.
        private void SeedAdaptiveAtrFromHistory()
        {
            if (adaptiveController == null || CurrentSymbol == null) return;

            try
            {
                var hist = CurrentSymbol.GetHistory(new HistoryRequestParameters
                {
                    Symbol = CurrentSymbol,
                    Aggregation = new HistoryAggregationTime(Period.MIN1, HistoryType.Last),
                    FromTime = Core.Instance.TimeUtils.DateTimeUtcNow.AddDays(-2),
                    ToTime = default
                });

                if (hist == null || hist.Count < 2) return;

                double prevClose = double.NaN;
                double atrSum = 0;
                int trCount = 0;
                double lastClose = 0;
                int period = Math.Max(1, InputAtrPeriod);

                // Average true range of the most recent `period` bars (simple average seed).
                int start = Math.Max(0, hist.Count - (period + 1));
                for (int i = start; i < hist.Count; i++)
                {
                    var bar = hist[i];
                    if (bar == null) continue;

                    double high = bar[PriceType.High];
                    double low = bar[PriceType.Low];
                    double close = bar[PriceType.Close];

                    double tr = high - low;
                    if (!double.IsNaN(prevClose))
                        tr = Math.Max(tr, Math.Max(Math.Abs(high - prevClose), Math.Abs(low - prevClose)));

                    if (i >= hist.Count - period)
                    {
                        atrSum += tr;
                        trCount++;
                    }

                    prevClose = close;
                    lastClose = close;
                }

                if (trCount > 0)
                {
                    double atr = atrSum / trCount;
                    adaptiveController.SeedAtr(atr, lastClose);
                    double tickSize = CurrentSymbol.TickSize > 0 ? CurrentSymbol.TickSize : 1.0;
                    Log($"[Adaptive] ATR seeded from history: {atr:F4} ({atr / tickSize:F1} ticks) over {trCount} bars.", StrategyLoggingLevel.Info);
                }
            }
            catch (Exception ex)
            {
                Log($"[Adaptive] ATR history seed failed: {ex.Message}", StrategyLoggingLevel.Error);
            }
        }

        // Returns the effective (TP, SL) bracket distances in ticks: adaptive when ready, else the
        // static operator inputs.
        private (int tp, int sl) GetEffectiveBracketTicks()
        {
            if (InputParameterMode == StrategyParameterMode.Adaptive && adaptiveController != null)
            {
                var ap = adaptiveController.Current;
                if (ap != null && ap.IsReady)
                    return (ap.TakeProfitTicks, ap.StopLossTicks);
            }
            return (TakeProfitTicks, StopLossTicks);
        }

        // -----------------------------------------------------
        // Strategy Execution & Trading Logic
        // -----------------------------------------------------
        private void EvaluateTradingSignal()
        {
            var calc = engine?.Calculator;
            if (calc == null || engine == null) return;

            DateTime now = DateTime.UtcNow;

            if (!engine.IsBookValid || !calc.IsCalibrated)
            {
                if ((now - lastStatusLogTime).TotalSeconds >= 30)
                {
                    lastStatusLogTime = now;
                    string reason = !engine.IsBookValid ? "L2 book not yet seeded" : "engine calibrating (MTR warmup)";
                    Log($"[Status] {reason} — no signals yet.", StrategyLoggingLevel.Info);
                }
                return;
            }
            double lastPrice = CurrentSymbol?.Last ?? 0.0;
            double bid = CurrentSymbol?.Bid ?? 0.0;
            double ask = CurrentSymbol?.Ask ?? 0.0;

            // 1. Process paper bracket exits every event, then re-evaluate promotion/demotion.
            if (shadowSim != null && shadowSim.InPosition && lastPrice > 0)
            {
                if (shadowSim.OnMarket(lastPrice, now))
                {
                    var sp = shadowSim.Performance;
                    Log($"[Shadow] Paper trade closed. Net {sp.NetPnL:C2} over {sp.Count} trades, mean {sp.Mean:C2}, LCB {sp.LowerConfidenceBound(InputPromotionZ):C2}.", StrategyLoggingLevel.Trading);
                }
            }
            UpdateExecutionMode(lastPrice, now);

            bool wantLive = ResolveWantLive();

            // 2. Signal ratio + effective parameters (shared by live and shadow paths).
            var ratioVal = calc.GetBuyerSellerQtyRatioShort();
            bool ratioUsable = ratioVal.IsWarm && ratioVal.Quality != MetricQuality.Unavailable;

            double buyTh = RatioBuyThreshold, sellTh = RatioSellThreshold;
            double buyReset = RatioBuyResetThreshold, sellReset = RatioSellResetThreshold;
            int effTp = TakeProfitTicks, effSl = StopLossTicks;

            if (InputParameterMode == StrategyParameterMode.Adaptive && adaptiveController != null)
            {
                adaptiveController.Observe(lastPrice, lastPrice > 0.0, ratioVal.Value, ratioUsable, now);
                var ap = adaptiveController.Current;
                if (ap == null || !ap.IsReady)
                {
                    // Log warmup progress every 30 s so the user can see the engine is alive.
                    if ((now - lastStatusLogTime).TotalSeconds >= 30)
                    {
                        lastStatusLogTime = now;
                        Log($"[Adaptive] Warming up: {ap?.SampleCount ?? 0} ratio samples collected (need ~480). Ratio usable={ratioUsable}.", StrategyLoggingLevel.Info);
                    }
                    return;
                }

                // One-shot log the first time the controller becomes ready.
                if (!adaptiveReadyLogged)
                {
                    adaptiveReadyLogged = true;
                    Log($"[Adaptive] Controller ready — BuyTh={ap.BuyThreshold:F3} SellTh={ap.SellThreshold:F3} Reset={ap.BuyResetThreshold:F3} ATR={ap.AtrTicks:F1}t TP={ap.TakeProfitTicks} SL={ap.StopLossTicks} ticks ({ap.SampleCount} samples). Polarity={ap.Polarity}.", StrategyLoggingLevel.Info);
                }

                // Log polarity transitions and reset signal state so a zombie signal from
                // the prior regime doesn't fire on the wrong side of the next tick.
                if (ap.Polarity != lastPolarity)
                {
                    Log($"[Polarity] {lastPolarity} → {ap.Polarity} (ACF={ap.Autocorrelation:F3}). Signal state reset.", StrategyLoggingLevel.Info);
                    lastPolarity = ap.Polarity;
                    lock (stateLock)
                    {
                        buySignalActive = false;
                        sellSignalActive = false;
                    }
                }

                buyTh = ap.BuyThreshold; sellTh = ap.SellThreshold;
                buyReset = ap.BuyResetThreshold; sellReset = ap.SellResetThreshold;
                effTp = ap.TakeProfitTicks; effSl = ap.StopLossTicks;
            }

            if (!ratioUsable)
            {
                // Log every 60 s so a persistent unavailable ratio is visible in the log.
                if ((now - lastStatusLogTime).TotalSeconds >= 60)
                {
                    lastStatusLogTime = now;
                    Log($"[Status] Ratio not usable (IsWarm={ratioVal.IsWarm}, Quality={ratioVal.Quality}). Engine calibrated but no tradeable signal.", StrategyLoggingLevel.Info);
                }
                return;
            }

            double ratio = ratioVal.Value;

            // Periodic heartbeat: log current ratio vs thresholds every 60 s so the user can confirm
            // the engine is evaluating signals even when nothing crosses the threshold.
            if ((now - lastStatusLogTime).TotalSeconds >= 60)
            {
                lastStatusLogTime = now;
                var sp = shadowSim?.Performance;
                string shadowInfo = sp != null ? $"shadow trades={sp.Count} net={sp.NetPnL:C2}" : "";
                Log($"[Status] ratio={ratio:F3} buyTh={buyTh:F3} sellTh={sellTh:F3} polarity={lastPolarity} {shadowInfo}", StrategyLoggingLevel.Info);
            }

            lock (stateLock)
            {
                // Live-only Max Daily Loss guard. currentPositionSize carries the sign, so
                // currentPositionSize * (lastPrice - averageEntryPrice) * pointCost is correct both ways.
                if (wantLive && lastPrice > 0.0)
                {
                    double pointCost = GetPointCost();
                    double openPnL = currentPositionSize * (lastPrice - averageEntryPrice) * pointCost;
                    double totalPnL = strategyRealizedPnL + openPnL;
                    if (totalPnL <= -MaxDailyLoss)
                    {
                        isRiskHalted = true;
                        Log($"[Risk Limit] Daily loss threshold of -{MaxDailyLoss:C2} exceeded. PnL: {totalPnL:C2}. Flattening and Halting.", StrategyLoggingLevel.Error);
                        FlattenPosition();
                        return;
                    }
                }

                bool isWithinCooldown = (now - lastOrderPlacementTime) < TimeSpan.FromSeconds(InputCooldownSeconds);

                // Phase 3: regime gate — stand aside on volatility spike, extreme ratio, or thin queue.
                var bidOC = calc.GetBestBidOrderCount();
                var askOC = calc.GetBestAskOrderCount();
                bool orderCountExact = bidOC.Quality == MetricQuality.Exact;
                int totalBestOrders = (int)(bidOC.Value + askOC.Value);

                // Controller gates: volatility spike + extreme ratio
                RegimeState currentRegime = (InputParameterMode == StrategyParameterMode.Adaptive && adaptiveController != null)
                    ? adaptiveController.Current.Regime
                    : RegimeState.Normal;

                // Thin-queue gate (MBO only, disabled when InputThinQueueThreshold == 0)
                bool thinQueue = orderCountExact && InputThinQueueThreshold > 0
                    && totalBestOrders <= InputThinQueueThreshold;
                if (thinQueue) currentRegime = RegimeState.StandAside;

                // Log regime transitions
                if (currentRegime != lastRegime)
                {
                    var ap2 = (InputParameterMode == StrategyParameterMode.Adaptive && adaptiveController != null)
                        ? adaptiveController.Current : null;
                    string reason = thinQueue
                        ? $"thin queue ({totalBestOrders} orders at best)"
                        : $"voltRatio={ap2?.VolatilityRatio:F2} extremeRatio or spike";
                    Log($"[Regime] {lastRegime} → {currentRegime}: {reason}", StrategyLoggingLevel.Info);
                    lastRegime = currentRegime;
                }

                if (currentRegime == RegimeState.StandAside)
                    return;

                if (lastPolarity == SignalPolarity.Momentum)
                {
                    // Buy when buyers dominate, sell when sellers dominate.
                    if (ratio >= buyTh)
                    {
                        if (!buySignalActive)
                        {
                            buySignalActive = true;
                            EnterSignal(Side.Buy, wantLive, effTp, effSl, bid, ask, now, isWithinCooldown);
                        }
                    }
                    else if (ratio <= buyReset) buySignalActive = false;

                    if (ratio <= sellTh)
                    {
                        if (!sellSignalActive)
                        {
                            sellSignalActive = true;
                            EnterSignal(Side.Sell, wantLive, effTp, effSl, bid, ask, now, isWithinCooldown);
                        }
                    }
                    else if (ratio >= sellReset) sellSignalActive = false;
                }
                else
                {
                    // Mean-reversion: buy when sellers dominate (expect bounce), sell when buyers dominate (expect fade).
                    // Reset when flow returns to neutral (median). buyReset/sellReset are both median, so direction is symmetric.
                    if (ratio <= sellTh)
                    {
                        if (!buySignalActive)
                        {
                            buySignalActive = true;
                            EnterSignal(Side.Buy, wantLive, effTp, effSl, bid, ask, now, isWithinCooldown);
                        }
                    }
                    else if (ratio >= buyReset) buySignalActive = false;

                    if (ratio >= buyTh)
                    {
                        if (!sellSignalActive)
                        {
                            sellSignalActive = true;
                            EnterSignal(Side.Sell, wantLive, effTp, effSl, bid, ask, now, isWithinCooldown);
                        }
                    }
                    else if (ratio <= sellReset) sellSignalActive = false;
                }
            }
        }

        // Routes a newly-activated signal to the live broker path or the paper simulator.
        // Called under stateLock.
        private void EnterSignal(Side side, bool wantLive, int tpTicks, int slTicks, double bid, double ask, DateTime now, bool isWithinCooldown)
        {
            if (wantLive)
            {
                if (isRiskHalted || !isConnectionActive || isOrderPlacementPending || isWithinCooldown)
                    return;

                // Worst-case exposure = confirmed position + unfilled working entries (same side) + proposed order.
                // Checking only currentPositionSize would allow over-exposure when entry orders are pending.
                double pendingSameSide = trackedOrders.Values
                    .Where(o => o.Side == side && o.Role == OrderRole.Entry &&
                                (o.Status == OrderStatus.Opened || o.Status == OrderStatus.PartiallyFilled))
                    .Sum(o => o.Quantity - o.FilledQuantity);
                bool exposureOk = side == Side.Buy
                    ? currentPositionSize + pendingSameSide + OrderQty <= MaxExposure
                    : -currentPositionSize + pendingSameSide + OrderQty <= MaxExposure;
                if (!exposureOk) return;

                bool hasPending = trackedOrders.Values.Any(o => o.Side == side && o.Role != OrderRole.Bracket &&
                    (o.Status == OrderStatus.Opened || o.Status == OrderStatus.PartiallyFilled));
                if (!hasPending)
                    PlaceOrderAsync(side);
            }
            else
            {
                if (shadowSim != null && shadowSim.CanEnter(now) && (tpTicks > 0 || slTicks > 0))
                {
                    if (shadowSim.Enter(side, bid, ask, tpTicks, slTicks, now))
                        Log($"[Shadow] Paper {side} entered (TP {tpTicks} / SL {slTicks} ticks).", StrategyLoggingLevel.Trading);
                }
            }
        }

        private bool ResolveWantLive()
        {
            switch (InputExecutionMode)
            {
                case ExecutionMode.LiveOnly: return true;
                case ExecutionMode.ShadowOnly: return false;
                default: return isLivePromoted;
            }
        }

        // Promotes shadow→live once the paper record clears the gate, and demotes live→shadow when the
        // live record degrades or a hard risk halt fires. Only active in ShadowThenPromote mode.
        private void UpdateExecutionMode(double lastPrice, DateTime now)
        {
            if (InputExecutionMode != ExecutionMode.ShadowThenPromote || shadowSim == null)
                return;

            if (!isLivePromoted)
            {
                var p = shadowSim.Performance;
                if (p.MeetsBar(InputPromotionMinTrades, InputPromotionZ, InputPromotionMinExpectancy))
                {
                    isLivePromoted = true;
                    if (lastPrice > 0) shadowSim.ForceCloseAt(lastPrice, now);
                    lock (stateLock)
                    {
                        livePerf.Reset();
                        pnlAtLastFlat = strategyRealizedPnL;
                    }
                    Log($"[Promotion] Shadow cleared the gate: {p.Count} trades, net {p.NetPnL:C2}, mean {p.Mean:C2}, LCB {p.LowerConfidenceBound(InputPromotionZ):C2}. LIVE execution ENABLED.", StrategyLoggingLevel.Info);
                }
            }
            else
            {
                bool demote = isRiskHalted;
                int liveCount;
                double liveLcb = 0;
                lock (stateLock)
                {
                    liveCount = livePerf.Count;
                    if (liveCount >= InputDemotionMinTrades)
                        liveLcb = livePerf.LowerConfidenceBound(InputPromotionZ);
                }
                if (liveCount >= InputDemotionMinTrades && liveLcb < 0)
                    demote = true;

                if (demote)
                {
                    isLivePromoted = false;
                    FlattenPosition();
                    shadowSim.Reset();
                    Log($"[Demotion] Reverting to SHADOW (paper). Live trades: {liveCount}, riskHalted: {isRiskHalted}. Re-validating on fresh paper record.", StrategyLoggingLevel.Error);
                }
            }
        }

        private void PlaceOrderAsync(Side side)
        {
            double price = side == Side.Buy ? (CurrentSymbol?.Bid ?? 0.0) : (CurrentSymbol?.Ask ?? 0.0);
            if (price == 0.0) price = CurrentSymbol?.Last ?? 0.0;
            if (price == 0.0) return;

            double qty = OrderQty;
            Log($"[Execution Signal] Ratio: {side}. Scheduling placement of {qty} contracts @ {price}.", StrategyLoggingLevel.Trading);

            // Set the flag to block duplicate order placements on subsequent events
            isOrderPlacementPending = true;
            lastOrderPlacementTime = DateTime.UtcNow;

            prioritizedQueue?.Enqueue(() =>
            {
                // Resolve limit order type dynamically from the symbol allowed types
                var limitType = CurrentSymbol?.GetAlowedOrderTypes(OrderTypeUsage.Order)
                                   .FirstOrDefault(ot => ot.Behavior == OrderTypeBehavior.Limit) ??
                               CurrentSymbol?.GetAlowedOrderTypes(OrderTypeUsage.All)
                                   .FirstOrDefault(ot => ot.Behavior == OrderTypeBehavior.Limit);
                string orderTypeId = limitType?.Id ?? OrderType.Limit;

                // Place limit order
                var request = new PlaceOrderRequestParameters
                {
                    Symbol = CurrentSymbol,
                    Account = CurrentAccount,
                    Side = side,
                    OrderTypeId = orderTypeId,
                    Price = price,
                    Quantity = qty,
                    Comment = EntryTag
                };
                
                try
                {
                    // Omit REDUCE_ONLY and POST_ONLY as they are unsupported by IB
                    var result = Core.Instance.PlaceOrder(request);
                    
                    if (result.Status == TradingOperationResultStatus.Success)
                    {
                        consecutiveOrderFailures = 0; // Reset counter on successful placement
                        trackedOrders[result.OrderId] = new TrackedOrderState
                        {
                            LocalId = Guid.NewGuid().ToString(),
                            OrderId = result.OrderId,
                            Side = side,
                            Price = price,
                            Quantity = qty,
                            Status = OrderStatus.Opened,
                            Role = OrderRole.Entry
                        };
                        Log($"[Order Placed] Server ID: {result.OrderId}.", StrategyLoggingLevel.Trading);
                    }
                    else
                    {
                        consecutiveOrderFailures++;
                        Log($"[Order Failed] {result.Message} (Consecutive failures: {consecutiveOrderFailures})", StrategyLoggingLevel.Error);

                        if (consecutiveOrderFailures >= MaxConsecutiveFailures)
                        {
                            isRiskHalted = true;
                            Log($"[Risk Limit] Strategy halted due to {consecutiveOrderFailures} consecutive order placement failures. Check broker settings.", StrategyLoggingLevel.Error);
                            FlattenPosition();
                        }
                    }
                }
                catch (Exception ex)
                {
                    consecutiveOrderFailures++;
                    Log($"[Order Placement Exception] {ex.Message} (Consecutive failures: {consecutiveOrderFailures})", StrategyLoggingLevel.Error);

                    if (consecutiveOrderFailures >= MaxConsecutiveFailures)
                    {
                        isRiskHalted = true;
                        Log($"[Risk Limit] Strategy halted due to {consecutiveOrderFailures} consecutive order placement failures.", StrategyLoggingLevel.Error);
                        FlattenPosition();
                    }
                }
                finally
                {
                    // Reset pending flag once broker request completes
                    isOrderPlacementPending = false;
                }
                return Task.CompletedTask;
            }, 2);
        }

        private void FlattenPosition()
        {
            Log("[Flattening] Cancelling pending orders and market closing position...", StrategyLoggingLevel.Info);

            // 1. Cancel all working orders in the prioritized queue (priority 0)
            foreach (var trackedOrder in trackedOrders.Values)
            {
                if (trackedOrder.OrderInstance != null && 
                    (trackedOrder.Status == OrderStatus.Opened || trackedOrder.Status == OrderStatus.PartiallyFilled))
                {
                    var ord = trackedOrder.OrderInstance;
                    prioritizedQueue?.Enqueue(() =>
                    {
                        ord.Cancel();
                        return Task.CompletedTask;
                    }, 0);
                }
            }

            // 2. Place market order to close open position (priority 1)
            prioritizedQueue?.Enqueue(() =>
            {
                double posSize = GetBrokerPositionSize();
                if (posSize != 0.0)
                {
                    Side flatSide = posSize > 0.0 ? Side.Sell : Side.Buy;
                    double qty = Math.Abs(posSize);
                    Log($"[Flatten Placement] Placing Market order on {flatSide} for {qty} contracts.", StrategyLoggingLevel.Info);
                    
                    var marketType = CurrentSymbol?.GetAlowedOrderTypes(OrderTypeUsage.Order)
                                        .FirstOrDefault(ot => ot.Behavior == OrderTypeBehavior.Market) ??
                                    CurrentSymbol?.GetAlowedOrderTypes(OrderTypeUsage.All)
                                        .FirstOrDefault(ot => ot.Behavior == OrderTypeBehavior.Market);
                    string orderTypeId = marketType?.Id ?? OrderType.Market;

                    Core.Instance.PlaceOrder(new PlaceOrderRequestParameters
                    {
                        Symbol = CurrentSymbol,
                        Account = CurrentAccount,
                        Side = flatSide,
                        OrderTypeId = orderTypeId,
                        Quantity = qty
                    });
                }
                return Task.CompletedTask;
            }, 1);
        }

        private double GetBrokerPositionSize()
        {
            var pos = Core.Instance.Positions.FirstOrDefault(p => p.Symbol?.Id == CurrentSymbol.Id && p.Account?.Id == CurrentAccount.Id);
            return pos != null ? (pos.Side == Side.Buy ? pos.Quantity : -pos.Quantity) : 0.0;
        }

        // -----------------------------------------------------
        // Order Lifecycle & Reconciliation
        // -----------------------------------------------------
        private void OnCoreOrderAdded(Order order)
        {
            if (order.Symbol?.Id == CurrentSymbol.Id && order.Account?.Id == CurrentAccount.Id)
            {
                if (trackedOrders.TryGetValue(order.Id, out var state))
                {
                    state.OrderInstance = order;
                    order.Updated += OnOrderUpdated;
                    Log($"[Order Linked] Connected instance for tracked order: {order.Id}.", StrategyLoggingLevel.Info);
                }
                else
                {
                    // Tracked order placed outside this running instance or during disconnection
                    var newState = new TrackedOrderState
                    {
                        LocalId = Guid.NewGuid().ToString(),
                        OrderId = order.Id,
                        Side = order.Side,
                        Price = order.Price,
                        Quantity = order.TotalQuantity,
                        FilledQuantity = order.FilledQuantity,
                        Status = order.Status,
                        OrderInstance = order,
                        Role = RoleFromComment(order.Comment) // Recover our own role from the tag; foreign orders stay Reconciled
                    };
                    trackedOrders[order.Id] = newState;
                    order.Updated += OnOrderUpdated;
                    Log($"[Reconciled Order] Tracking working order: {order.Id} ({order.Side} {order.TotalQuantity} @ {order.Price}), role {newState.Role}.", StrategyLoggingLevel.Info);
                }
            }
        }

        private void OnCoreOrderRemoved(Order order)
        {
            if (trackedOrders.TryRemove(order.Id, out var state))
            {
                order.Updated -= OnOrderUpdated;
                Log($"[Order Removed] Stopped tracking order: {order.Id}.", StrategyLoggingLevel.Info);
            }
        }

        private void OnCorePositionAdded(Position position)
        {
            if (position.Symbol?.Id == CurrentSymbol.Id && position.Account?.Id == CurrentAccount.Id)
            {
                Log("[Position Added] Reconciling broker position state...", StrategyLoggingLevel.Info);
                ReconcileBrokerState();
            }
        }

        private void OnCorePositionRemoved(Position position)
        {
            if (position.Symbol?.Id == CurrentSymbol.Id && position.Account?.Id == CurrentAccount.Id)
            {
                Log("[Position Removed] Reconciling broker position state...", StrategyLoggingLevel.Info);
                ReconcileBrokerState();
            }
        }

        private void OnOrderUpdated(IOrder order)
        {
            if (order is Order ord && trackedOrders.TryGetValue(ord.Id, out var state))
            {
                var oldStatus = state.Status;
                var newStatus = ord.Status;
                var oldFilled = state.FilledQuantity;
                var newFilled = ord.FilledQuantity;

                state.Status = newStatus;
                state.FilledQuantity = newFilled;

                if (newFilled > oldFilled)
                {
                    double filledDiff = newFilled - oldFilled;
                    Log($"[Fill Alert] Order {ord.Id} filled: {oldFilled} -> {newFilled} (Diff: {filledDiff})", StrategyLoggingLevel.Trading);

                    // OCO guard: if a bracket (SL or TP) fills after the position is already flat,
                    // the sibling bracket closed it first. Processing this fill would open a new
                    // position in the opposite direction. Reject it and cancel the order.
                    bool rejectFill = false;
                    if (state.Role == OrderRole.Bracket)
                    {
                        lock (stateLock) { rejectFill = currentPositionSize == 0.0; }
                        if (rejectFill)
                        {
                            Log($"[OCO Guard] Bracket {ord.Id} filled after position was already flat — rejecting to prevent reversal.", StrategyLoggingLevel.Error);
                            var ordRefOco = ord;
                            prioritizedQueue?.Enqueue(() => { ordRefOco.Cancel(); return Task.CompletedTask; }, 0);
                        }
                    }

                    if (!rejectFill)
                    {
                        ProcessExecutionFill(ord.Side, ord.AverageFillPrice > 0 ? ord.AverageFillPrice : ord.Price, filledDiff);

                        // Only our own signal entries spawn protective brackets. Bracket fills and
                        // reconciled (post-reconnect, role-unknown) orders must NOT spawn new brackets,
                        // otherwise a TP/SL fill after a reconnect would cascade into fresh orders.
                        if (state.Role == OrderRole.Entry)
                        {
                            ManageBracketsForFill(ord.Side, ord.AverageFillPrice > 0 ? ord.AverageFillPrice : ord.Price, filledDiff);
                        }
                    }
                }

                if (newStatus == OrderStatus.Filled || newStatus == OrderStatus.Cancelled || newStatus == OrderStatus.Refused)
                {
                    Log($"[Order Finished] Order {ord.Id} finished with status: {newStatus}", StrategyLoggingLevel.Trading);
                    ord.Updated -= OnOrderUpdated;
                    trackedOrders.TryRemove(ord.Id, out _);
                }
            }
        }

        private void ProcessExecutionFill(Side fillSide, double fillPrice, double fillQty)
        {
          lock (stateLock)
          {
            double pointCost = GetPointCost();
            double fillQtySigned = fillSide == Side.Buy ? fillQty : -fillQty;

            if (currentPositionSize == 0.0)
            {
                averageEntryPrice = fillPrice;
                currentPositionSize = fillQtySigned;
            }
            else if (Math.Sign(currentPositionSize) == Math.Sign(fillQtySigned))
            {
                double existingAbs = Math.Abs(currentPositionSize);
                averageEntryPrice = ((averageEntryPrice * existingAbs) + (fillPrice * fillQty)) / (existingAbs + fillQty);
                currentPositionSize += fillQtySigned;
            }
            else
            {
                double existingAbs = Math.Abs(currentPositionSize);
                double reductionQty = Math.Min(existingAbs, fillQty);
                double remainingFillQty = fillQty - reductionQty;

                double pnlCoeff = currentPositionSize > 0 ? 1.0 : -1.0;
                double realized = (fillPrice - averageEntryPrice) * reductionQty * pointCost * pnlCoeff;
                strategyRealizedPnL += realized;

                Log($"[PnL Update] Realized {realized:C2} on reduction of {reductionQty} contracts. Total realized PnL: {strategyRealizedPnL:C2}.", StrategyLoggingLevel.Info);

                currentPositionSize += (fillSide == Side.Buy ? reductionQty : -reductionQty);

                if (remainingFillQty > 0)
                {
                    averageEntryPrice = fillPrice;
                    currentPositionSize = fillSide == Side.Buy ? remainingFillQty : -remainingFillQty;
                }
                else if (currentPositionSize == 0.0)
                {
                    averageEntryPrice = 0.0;
                }
            }

            Log($"[Local Position] Size: {currentPositionSize}, Average Price: {averageEntryPrice}.", StrategyLoggingLevel.Info);

            // Reconcile stop loss and take profit bracket quantities
            if (currentPositionSize == 0.0)
            {
                // Round-trip complete: record realized PnL since the last flat for demotion monitoring.
                double roundTripPnL = strategyRealizedPnL - pnlAtLastFlat;
                pnlAtLastFlat = strategyRealizedPnL;
                livePerf.Add(roundTripPnL);

                // Position went flat, cancel all active bracket orders
                foreach (var trackedOrder in trackedOrders.Values)
                {
                    if (trackedOrder.Role == OrderRole.Bracket && trackedOrder.OrderInstance != null &&
                        (trackedOrder.Status == OrderStatus.Opened || trackedOrder.Status == OrderStatus.PartiallyFilled))
                    {
                        var ord = trackedOrder.OrderInstance;
                        Log($"[OCO Action] Position flat. Cancelling bracket order {ord.Id}.", StrategyLoggingLevel.Trading);
                        prioritizedQueue?.Enqueue(() =>
                        {
                            ord.Cancel();
                            return Task.CompletedTask;
                        }, 0);
                    }
                }
            }
            else
            {
                ReconcileBracketQuantities();
            }
          }
        }

        private void ManageBracketsForFill(Side entrySide, double fillPrice, double qty)
        {
            var (tpTicks, slTicks) = GetEffectiveBracketTicks();
            if (slTicks <= 0 && tpTicks <= 0) return;

            Side bracketSide = entrySide == Side.Buy ? Side.Sell : Side.Buy;
            double tickSize = CurrentSymbol.TickSize;

            prioritizedQueue?.Enqueue(() =>
            {
                if (slTicks > 0)
                {
                    double slPrice = entrySide == Side.Buy
                        ? fillPrice - (slTicks * tickSize)
                        : fillPrice + (slTicks * tickSize);

                    slPrice = Math.Round(slPrice / tickSize) * tickSize;

                    var stopOrderType = CurrentSymbol.GetAlowedOrderTypes(OrderTypeUsage.CloseOrder)
                        .FirstOrDefault(ot => ot.Behavior == OrderTypeBehavior.Stop) ?? 
                        CurrentSymbol.GetAlowedOrderTypes(OrderTypeUsage.All)
                        .FirstOrDefault(ot => ot.Behavior == OrderTypeBehavior.Stop);

                    if (stopOrderType != null)
                    {
                        Log($"[Bracket SL] Scheduling SL Stop order on {bracketSide} at {slPrice} for quantity {qty}.", StrategyLoggingLevel.Trading);

                        var slResult = Core.Instance.PlaceOrder(new PlaceOrderRequestParameters
                        {
                            Symbol = CurrentSymbol,
                            Account = CurrentAccount,
                            Side = bracketSide,
                            OrderTypeId = stopOrderType.Id,
                            TriggerPrice = slPrice,
                            Quantity = qty,
                            Comment = BracketTag
                        });

                        if (slResult.Status == TradingOperationResultStatus.Success)
                        {
                            trackedOrders[slResult.OrderId] = new TrackedOrderState
                            {
                                LocalId = Guid.NewGuid().ToString(),
                                OrderId = slResult.OrderId,
                                Side = bracketSide,
                                Price = slPrice,
                                Quantity = qty,
                                Status = OrderStatus.Opened,
                                Role = OrderRole.Bracket
                            };
                        }
                        else
                        {
                            // A missing SL means the position has no downside protection.
                            // Flatten immediately regardless of consecutive-failure count.
                            isRiskHalted = true;
                            Log($"[Risk Limit] SL placement failed: {slResult.Message}. Flattening — open position has no stop loss.", StrategyLoggingLevel.Error);
                            FlattenPosition();
                            return Task.CompletedTask; // skip TP placement — no point protecting upside without a stop
                        }
                    }
                }

                if (tpTicks > 0)
                {
                    double tpPrice = entrySide == Side.Buy
                        ? fillPrice + (tpTicks * tickSize)
                        : fillPrice - (tpTicks * tickSize);

                    tpPrice = Math.Round(tpPrice / tickSize) * tickSize;

                    Log($"[Bracket TP] Scheduling TP Limit order on {bracketSide} at {tpPrice} for quantity {qty}.", StrategyLoggingLevel.Trading);

                    var limitType = CurrentSymbol?.GetAlowedOrderTypes(OrderTypeUsage.CloseOrder)
                                       .FirstOrDefault(ot => ot.Behavior == OrderTypeBehavior.Limit) ??
                                   CurrentSymbol?.GetAlowedOrderTypes(OrderTypeUsage.All)
                                       .FirstOrDefault(ot => ot.Behavior == OrderTypeBehavior.Limit);
                    string limitTypeId = limitType?.Id ?? OrderType.Limit;

                    var tpResult = Core.Instance.PlaceOrder(new PlaceOrderRequestParameters
                    {
                        Symbol = CurrentSymbol,
                        Account = CurrentAccount,
                        Side = bracketSide,
                        OrderTypeId = limitTypeId,
                        Price = tpPrice,
                        Quantity = qty,
                        Comment = BracketTag
                    });

                    if (tpResult.Status == TradingOperationResultStatus.Success)
                    {
                        trackedOrders[tpResult.OrderId] = new TrackedOrderState
                        {
                            LocalId = Guid.NewGuid().ToString(),
                            OrderId = tpResult.OrderId,
                            Side = bracketSide,
                            Price = tpPrice,
                            Quantity = qty,
                            Status = OrderStatus.Opened,
                            Role = OrderRole.Bracket
                        };
                    }
                    else
                    {
                        Log($"[Bracket TP] Placement failed: {tpResult.Message}", StrategyLoggingLevel.Error);
                        consecutiveOrderFailures++;
                        if (consecutiveOrderFailures >= MaxConsecutiveFailures)
                        {
                            isRiskHalted = true;
                            Log($"[Risk Limit] Strategy halted due to bracket TP placement failure.", StrategyLoggingLevel.Error);
                            FlattenPosition();
                        }
                    }
                }
                return Task.CompletedTask;
            }, 2);
        }

        private void ReconcileBracketQuantities()
        {
            double absPosition = Math.Abs(currentPositionSize);

            var activeSLs = trackedOrders.Values.Where(o => o.Role == OrderRole.Bracket &&
                                                            (o.Status == OrderStatus.Opened || o.Status == OrderStatus.PartiallyFilled) &&
                                                            IsStopOrder(o.OrderInstance)).ToList();

            var activeTPs = trackedOrders.Values.Where(o => o.Role == OrderRole.Bracket &&
                                                            (o.Status == OrderStatus.Opened || o.Status == OrderStatus.PartiallyFilled) &&
                                                            IsLimitOrder(o.OrderInstance)).ToList();

            AdjustBracketGroup(activeSLs, absPosition);
            AdjustBracketGroup(activeTPs, absPosition);
        }

        private bool IsStopOrder(Order? order)
        {
            if (order == null) return false;
            var ot = Core.Instance.GetOrderType(order.OrderTypeId);
            return ot != null && (ot.Behavior == OrderTypeBehavior.Stop || ot.Behavior == OrderTypeBehavior.StopLimit);
        }

        private bool IsLimitOrder(Order? order)
        {
            if (order == null) return false;
            var ot = Core.Instance.GetOrderType(order.OrderTypeId);
            return ot != null && ot.Behavior == OrderTypeBehavior.Limit;
        }

        private void AdjustBracketGroup(List<TrackedOrderState> brackets, double targetQty)
        {
            if (brackets.Count == 0) return;
            double totalQty = brackets.Sum(o => o.Quantity - o.FilledQuantity);

            if (totalQty > targetQty)
            {
                double excess = totalQty - targetQty;
                Log($"[Bracket Adjust] Excess qty detected: {totalQty} > {targetQty}. Adjusting...", StrategyLoggingLevel.Trading);

                foreach (var state in brackets)
                {
                    if (excess <= 0) break;

                    double remainingOrderQty = state.Quantity - state.FilledQuantity;
                    if (remainingOrderQty <= excess)
                    {
                        var ord = state.OrderInstance;
                        if (ord != null)
                        {
                            Log($"[Bracket Adjust] Cancelling bracket order {ord.Id}.", StrategyLoggingLevel.Trading);
                            prioritizedQueue?.Enqueue(() =>
                            {
                                ord.Cancel();
                                return Task.CompletedTask;
                            }, 0);
                        }
                        excess -= remainingOrderQty;
                    }
                    else
                    {
                        var ord = state.OrderInstance;
                        if (ord != null)
                        {
                            double newQty = remainingOrderQty - excess;
                            Log($"[Bracket Adjust] Modifying bracket order {ord.Id} quantity to {newQty}.", StrategyLoggingLevel.Trading);
                            prioritizedQueue?.Enqueue(() =>
                            {
                                var modifyParams = new ModifyOrderRequestParameters(ord)
                                {
                                    Quantity = newQty
                                };
                                Core.Instance.ModifyOrder(modifyParams);
                                return Task.CompletedTask;
                            }, 1);
                        }
                        excess = 0;
                    }
                }
            }
        }

        private void OnConnectionStateChanged(object? sender, ConnectionStateChangedEventArgs e)
        {
            string? changedId = (sender as Connection)?.Id;
            bool isExec = changedId == CurrentAccount?.ConnectionId;
            bool isData = changedId == CurrentSymbol?.ConnectionId;

            if (e.NewState == ConnectionState.Connected)
            {
                if (isData) isDataConnectionActive = true;
                if (isExec)
                {
                    isExecutionConnectionActive = true;
                    Log("Execution (broker) connection re-established. Triggering reconciliation...", StrategyLoggingLevel.Info);
                    ReconcileBrokerState();
                }
                else if (isData)
                {
                    Log("Data connection re-established.", StrategyLoggingLevel.Info);
                }
            }
            else if (e.NewState == ConnectionState.Disconnected || e.NewState == ConnectionState.Fail)
            {
                if (isData)
                {
                    isDataConnectionActive = false;
                    Log("Data connection lost. New entries paused; open position management continues.", StrategyLoggingLevel.Error);
                }
                if (isExec)
                {
                    isExecutionConnectionActive = false;
                    Log("Execution (broker) connection lost. All order placement paused.", StrategyLoggingLevel.Error);
                }
            }
        }

        private void ReconcileBrokerState()
        {
            var execConn = Core.Instance.Connections.Connected.FirstOrDefault(c => c.Id == CurrentAccount.ConnectionId);
            isExecutionConnectionActive = execConn != null && execConn.State == ConnectionState.Connected;

            if (!isExecutionConnectionActive)
            {
                Log("[Reconciliation] Skipped — execution (broker) connection is not active.", StrategyLoggingLevel.Info);
                return;
            }

            // 1. Reconcile Positions
            var pos = Core.Instance.Positions.FirstOrDefault(p => p.Symbol?.Id == CurrentSymbol.Id && p.Account?.Id == CurrentAccount.Id);
            lock (stateLock)
            {
                if (pos != null)
                {
                    double posSize = pos.Side == Side.Buy ? pos.Quantity : -pos.Quantity;
                    currentPositionSize = posSize;
                    averageEntryPrice = pos.OpenPrice;
                    Log($"[Reconciliation] Restored Position: {currentPositionSize} contracts @ {averageEntryPrice}.", StrategyLoggingLevel.Info);
                }
                else
                {
                    currentPositionSize = 0.0;
                    averageEntryPrice = 0.0;
                    Log("[Reconciliation] Restored Position: Flat.", StrategyLoggingLevel.Info);
                }
            }

            // 2. Reconcile Orders
            var activeOrders = Core.Instance.Orders.Where(o => o.Symbol?.Id == CurrentSymbol.Id && o.Account?.Id == CurrentAccount.Id && 
                                                              (o.Status == OrderStatus.Opened || o.Status == OrderStatus.PartiallyFilled)).ToList();

            var brokerOrderIds = new HashSet<string>(activeOrders.Select(o => o.Id));

            // Clear tracked orders that are no longer working
            foreach (var orderId in trackedOrders.Keys.ToList())
            {
                if (!brokerOrderIds.Contains(orderId))
                {
                    if (trackedOrders.TryRemove(orderId, out var state))
                    {
                        if (state.OrderInstance != null)
                        {
                            state.OrderInstance.Updated -= OnOrderUpdated;
                        }
                        Log($"[Reconciliation] Cleared terminated tracked order {orderId}.", StrategyLoggingLevel.Info);
                    }
                }
            }

            // Link existing active orders
            foreach (var ord in activeOrders)
            {
                if (!trackedOrders.TryGetValue(ord.Id, out var state))
                {
                    var newState = new TrackedOrderState
                    {
                        LocalId = Guid.NewGuid().ToString(),
                        OrderId = ord.Id,
                        Side = ord.Side,
                        Price = ord.Price,
                        Quantity = ord.TotalQuantity,
                        FilledQuantity = ord.FilledQuantity,
                        Status = ord.Status,
                        OrderInstance = ord,
                        Role = RoleFromComment(ord.Comment) // Recover our own role from the tag; foreign orders stay Reconciled
                    };
                    trackedOrders[ord.Id] = newState;
                    ord.Updated += OnOrderUpdated;
                    Log($"[Reconciliation] Linked working order: {ord.Id}, role {newState.Role}.", StrategyLoggingLevel.Info);
                }
                else
                {
                    state.OrderInstance = ord;
                    ord.Updated -= OnOrderUpdated;
                    ord.Updated += OnOrderUpdated;
                }
            }
        }
    }

    // Classification of a tracked order. Reconciled orders (re-discovered after a restart or
    // reconnect) are deliberately NOT treated as Entry, so their fills do not spawn fresh brackets.
    internal enum OrderRole : byte
    {
        Entry,       // Our own signal entry order
        Bracket,     // Our own SL/TP protective order
        Reconciled   // Re-discovered from the broker; role unknown
    }

    // Helper state structures
    internal class TrackedOrderState
    {
        public string LocalId { get; init; } = null!;
        public string? OrderId { get; init; }
        public Side Side { get; init; }
        public double Price { get; init; }
        public double Quantity { get; init; }
        public double FilledQuantity { get; set; }
        public OrderStatus Status { get; set; }
        public Order? OrderInstance { get; set; }
        public OrderRole Role { get; init; }
    }

    internal class PrioritizedAsyncTaskQueue
    {
        private readonly PriorityQueue<Func<Task>, int> queue = new();
        private readonly object lockObj = new();
        private readonly SemaphoreSlim semaphore = new(0);
        private readonly CancellationTokenSource cts = new();
        private readonly Thread workerThread;

        public PrioritizedAsyncTaskQueue()
        {
            workerThread = new Thread(ProcessQueueLoop)
            {
                IsBackground = true,
                Name = "PrioritizedAsyncTaskQueueWorker"
            };
            workerThread.Start();
        }

        public void Enqueue(Func<Task> taskAction, int priority)
        {
            lock (lockObj)
            {
                queue.Enqueue(taskAction, priority);
            }
            semaphore.Release();
        }

        public void Shutdown()
        {
            cts.Cancel();
            semaphore.Release();
        }

        private async void ProcessQueueLoop()
        {
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    await semaphore.WaitAsync(cts.Token);

                    Func<Task>? taskAction = null;
                    lock (lockObj)
                    {
                        if (queue.Count > 0)
                        {
                            taskAction = queue.Dequeue();
                        }
                    }

                    if (taskAction != null)
                    {
                        try
                        {
                            await taskAction();
                        }
                        catch (Exception ex)
                        {
                            Core.Instance.Loggers.Log($"Error executing prioritized task: {ex.Message}", LoggingLevel.Error);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Core.Instance.Loggers.Log($"Error in PrioritizedAsyncTaskQueue: {ex.Message}", LoggingLevel.Error);
                }
            }
        }
    }
}
