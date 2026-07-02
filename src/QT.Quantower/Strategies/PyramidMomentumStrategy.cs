using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using MBO_Market_Data_Analytics;
using QT.Core.Quality;
using QT.Features.MarketState;
using QT.Features.OrderFlow;
using QT.Runtime.AnalyticsRuntime;
using TradingPlatform.BusinessLayer;

namespace MBO_OnePair_Grid_Strategy;

/// <summary>
/// Anti-martingale (pyramiding) momentum strategy. Every action — enter, add, soft-exit, flatten —
/// carries an evidence reason drawn from the V2 analytics: entry direction from the order-flow
/// signal (or manual bias), risk gating from the market regime, and a wide hard stop as the
/// evidence-independent disaster backstop. Adds happen only in profit; it never averages down.
/// It consumes analytics through the shared QT_API.V2.QuantowerHost assembly (no dependency on the
/// indicator DLL), so it cannot bind to a stale cross-folder copy.
/// </summary>
public sealed class PyramidMomentumStrategy : Strategy, ICurrentAccount, ICurrentSymbol
{
    // Directional bias values. Backed by int + InputParameter variants so Quantower renders a
    // dropdown reliably (bare enum InputParameters are not shown in the settings dialog).
    private const int BiasOff = 0;
    private const int BiasLong = 1;
    private const int BiasShort = -1;

    // Entry-direction source.
    private const int EntryManual = 0;
    private const int EntrySignal = 1;

    private enum TradeState
    {
        Flat,
        InTrade
    }

    private readonly record struct MidSample(DateTime TimeUtc, double Mid);

    private readonly object sync = new();
    private AnalyticsEngineHost? analytics;
    private System.Threading.Timer? timer;

    private TradeState state = TradeState.Flat;
    private bool initialized;
    private bool consumed;
    private int entryPendingCycles;
    private Side positionSide;
    private double baseEntryPrice;
    private double lastAddPrice;
    private double highWaterPrice;
    private DateTime? softBreachStartUtc;
    private readonly Queue<MidSample> midWindow = new();

    private Order? stopOrder;
    private string? stopOrderId;
    private string marketOrderTypeId = OrderType.Market;
    private string stopOrderTypeId = OrderType.Stop;
    private string commentPrefix = "";
    private bool halted;
    private bool coreEventsSubscribed;

    private int cycleCount;
    private int baseEntryCount;
    private int addCount;
    private int exitCount;
    private int flattenCount;
    private int heartbeatEveryCycles = 40;
    private DateTime lastStopModifyUtc = DateTime.MinValue;
    private DateTime lastFlatUtc = DateTime.MinValue;

    [InputParameter("Symbol", 0)]
    public Symbol CurrentSymbol { get; set; } = null!;

    [InputParameter("Account", 1)]
    public Account CurrentAccount { get; set; } = null!;

    [InputParameter("Arm live order placement", 2)]
    public bool ArmLiveOrderPlacement { get; set; }

    [InputParameter("Enable live orders", 3)]
    public bool EnableLiveOrders { get; set; }

    [InputParameter("Directional bias", 4, variants: new object[]
    {
        "Off", BiasOff,
        "Long", BiasLong,
        "Short", BiasShort
    })]
    public int Bias { get; set; } = BiasOff;

    [InputParameter("Max contracts", 5, minimum: 1, maximum: 20, increment: 1, decimalPlaces: 0)]
    public int MaxContracts { get; set; } = 3;

    [InputParameter("Add step ticks", 6, minimum: 1, maximum: 4000, increment: 1, decimalPlaces: 0)]
    public int AddStepTicks { get; set; } = 20;

    [InputParameter("Trail ticks", 7, minimum: 1, maximum: 4000, increment: 1, decimalPlaces: 0)]
    public int TrailTicks { get; set; } = 20;

    [InputParameter("Stop loss ticks", 8, minimum: 1, maximum: 4000, increment: 1, decimalPlaces: 0)]
    public int StopLossTicks { get; set; } = 40;

    [InputParameter("Refresh (ms)", 9, minimum: 50, maximum: 5000, increment: 50, decimalPlaces: 0)]
    public int RefreshMs { get; set; } = 250;

    [InputParameter("Flatten on stop", 10)]
    public bool FlattenOnStop { get; set; } = true;

    [InputParameter("Trail update step ticks", 11, minimum: 1, maximum: 500, increment: 1, decimalPlaces: 0)]
    public int TrailUpdateTicks { get; set; } = 4;

    [InputParameter("Volatility-scaled stops", 12)]
    public bool EnableVolatilityScaling { get; set; } = true;

    [InputParameter("Volatility lookback (sec)", 13, minimum: 5, maximum: 600, increment: 5, decimalPlaces: 0)]
    public int VolLookbackSeconds { get; set; } = 30;

    [InputParameter("Hard-stop vol multiple", 14, minimum: 0.1, maximum: 20, increment: 0.1, decimalPlaces: 1)]
    public double VolStopMultiple { get; set; } = 2.0;

    [InputParameter("Soft-trail vol multiple", 15, minimum: 0.1, maximum: 20, increment: 0.1, decimalPlaces: 1)]
    public double VolTrailMultiple { get; set; } = 1.0;

    [InputParameter("Soft-exit persistence (ms)", 16, minimum: 0, maximum: 10000, increment: 50, decimalPlaces: 0)]
    public int SoftExitPersistenceMs { get; set; } = 750;

    [InputParameter("Entry direction source", 17, variants: new object[]
    {
        "Manual bias", EntryManual,
        "Order-flow signal", EntrySignal
    })]
    public int EntryMode { get; set; } = EntryManual;

    [InputParameter("Use analytics (evidence + risk)", 18)]
    public bool EnableAnalytics { get; set; } = true;

    [InputParameter("Analytics prefer MBO", 19)]
    public bool AnalyticsPreferMbo { get; set; } = true;

    [InputParameter("Min signal confidence", 20, minimum: 0.0, maximum: 1.0, increment: 0.05, decimalPlaces: 2)]
    public double MinSignalConfidence { get; set; } = 0.50;

    [InputParameter("Flatten on regime risk-off", 21)]
    public bool FlattenOnRiskOff { get; set; } = true;

    [InputParameter("Require flow support for adds", 22)]
    public bool RequireFlowForAdds { get; set; } = true;

    [InputParameter("Signal re-entry cooldown (sec)", 23, minimum: 0, maximum: 600, increment: 1, decimalPlaces: 0)]
    public int ReentryCooldownSec { get; set; } = 5;

    public override string[] MonitoringConnectionsIds
    {
        get
        {
            var ids = new List<string>(2);
            if (!string.IsNullOrWhiteSpace(CurrentSymbol?.ConnectionId))
                ids.Add(CurrentSymbol.ConnectionId);
            if (!string.IsNullOrWhiteSpace(CurrentAccount?.ConnectionId))
                ids.Add(CurrentAccount.ConnectionId);
            return ids.ToArray();
        }
    }

    public PyramidMomentumStrategy()
        : base()
    {
        Name = "MBO Pyramid Momentum Strategy";
        Description = "Manual-bias aggressive entry that pyramids into profit (max N lots) under a ratcheting trailing stop and hard stop. Anti-martingale; never averages down. Disabled unless explicitly armed.";
    }

    protected override void OnRun()
    {
        base.OnRun();
        UnsubscribeCoreEvents();

        lock (sync)
        {
            halted = false;
            state = TradeState.Flat;
            initialized = false;
            consumed = false;
            entryPendingCycles = 0;
            positionSide = default;
            baseEntryPrice = 0;
            lastAddPrice = 0;
            highWaterPrice = 0;
            softBreachStartUtc = null;
            midWindow.Clear();
            stopOrder = null;
            stopOrderId = null;
            lastStopModifyUtc = DateTime.MinValue;
            lastFlatUtc = DateTime.MinValue;
            cycleCount = 0;
            baseEntryCount = 0;
            addCount = 0;
            exitCount = 0;
            flattenCount = 0;
            commentPrefix = $"pyramid {Guid.NewGuid():N}";
        }

        if (!ValidateInputs())
        {
            Stop();
            return;
        }

        if (!ArmLiveOrderPlacement || !EnableLiveOrders)
        {
            Halt("live order placement is disabled; set both arm inputs to true before starting");
            return;
        }

        marketOrderTypeId = Core.OrderTypes.FirstOrDefault(x => x.ConnectionId == CurrentSymbol.ConnectionId && x.Behavior == OrderTypeBehavior.Market)?.Id ?? OrderType.Market;
        stopOrderTypeId = Core.OrderTypes.FirstOrDefault(x => x.ConnectionId == CurrentSymbol.ConnectionId && x.Behavior == OrderTypeBehavior.Stop)?.Id ?? OrderType.Stop;

        if (HasOpenPosition())
        {
            Halt("selected symbol/account is not flat at startup");
            return;
        }

        if (GetOpenOrdersForSymbolAccount().Any())
        {
            Halt("existing open orders detected on selected symbol/account; refusing to guess ownership");
            return;
        }

        SubscribeCoreEvents();

        bool needAnalytics = EnableAnalytics || EntryMode == EntrySignal;
        if (needAnalytics)
        {
            try
            {
                analytics = new AnalyticsEngineHost(CurrentSymbol, new AnalyticsEngineConfig { PreferMbo = AnalyticsPreferMbo }, MapAnalyticsLog);
                analytics.Start();
                Log("Analytics started: entry/adds/exits are evidence-gated by regime + order-flow.", StrategyLoggingLevel.Trading);
            }
            catch (Exception ex)
            {
                analytics = null;
                if (EntryMode == EntrySignal)
                {
                    Halt($"analytics failed to start and entry mode is signal-driven: {ex.Message}");
                    return;
                }
                Log($"Analytics failed to start ({ex.Message}); continuing on manual bias with no evidence gating.", StrategyLoggingLevel.Error);
            }
        }

        int period = Math.Max(50, RefreshMs);
        heartbeatEveryCycles = Math.Max(1, 10000 / period);
        timer = new System.Threading.Timer(_ => OnTimer(), null, 0, period);
        string mode = EntryMode == EntrySignal ? "order-flow signal" : "manual bias";
        Log($"Started pyramid momentum strategy: entry={mode}, maxContracts={MaxContracts}, addStep={AddStepTicks}t, trail={TrailTicks}t, hardSL={StopLossTicks}t, refresh={period}ms.");
    }

    protected override void OnStop()
    {
        timer?.Dispose();
        timer = null;

        if (FlattenOnStop)
        {
            lock (sync)
                FlattenAll("strategy stop");
        }
        else
        {
            Log("Strategy stopped without flattening; existing position and stop remain live at the broker.", StrategyLoggingLevel.Error);
        }

        analytics?.Stop();
        analytics = null;
        base.OnStop();
    }

    protected override void OnRemove()
    {
        timer?.Dispose();
        timer = null;
        lock (sync)
            FlattenAll("strategy remove");
        analytics?.Stop();
        analytics = null;
        UnsubscribeCoreEvents();
        base.OnRemove();
    }

    protected override void OnInitializeMetrics(Meter meter)
    {
        base.OnInitializeMetrics(meter);
        meter.CreateObservableCounter("pyramid-cycles", () => cycleCount, description: "Management cycles");
        meter.CreateObservableCounter("pyramid-base-entries", () => baseEntryCount, description: "Base entries fired");
        meter.CreateObservableCounter("pyramid-adds", () => addCount, description: "Pyramided add lots");
        meter.CreateObservableCounter("pyramid-exits", () => exitCount, description: "Completed trade exits");
        meter.CreateObservableCounter("pyramid-flattens", () => flattenCount, description: "Forced flattens (stop-placement failure / management error)");
    }

    private bool ValidateInputs()
    {
        if (CurrentSymbol != null && CurrentSymbol.State == BusinessObjectState.Fake)
            CurrentSymbol = Core.Instance.GetSymbol(CurrentSymbol.CreateInfo());

        if (CurrentAccount != null && CurrentAccount.State == BusinessObjectState.Fake)
            CurrentAccount = Core.Instance.GetAccount(CurrentAccount.CreateInfo());

        if (CurrentSymbol == null)
        {
            Log("Symbol is not specified.", StrategyLoggingLevel.Error);
            return false;
        }

        if (CurrentAccount == null)
        {
            Log("Account is not specified.", StrategyLoggingLevel.Error);
            return false;
        }

        if (CurrentSymbol.ConnectionId != CurrentAccount.ConnectionId)
        {
            Log("Symbol and account belong to different connections.", StrategyLoggingLevel.Error);
            return false;
        }

        if (CurrentSymbol.TickSize <= 0 || double.IsNaN(CurrentSymbol.TickSize))
        {
            Log("Symbol tick size is unavailable.", StrategyLoggingLevel.Error);
            return false;
        }

        if (MaxContracts < 1)
        {
            Log("Max contracts must be at least one.", StrategyLoggingLevel.Error);
            return false;
        }

        return true;
    }

    private void OnTimer()
    {
        // A System.Threading.Timer callback runs on a thread-pool thread; an exception that escapes
        // here would be unhandled and terminate the whole Quantower process. Contain it: on any error
        // flatten and halt, never let it reach the host.
        try
        {
            OnTimerCore();
        }
        catch (Exception ex)
        {
            Log($"Pyramid management loop error; flattening and halting: {ex}", StrategyLoggingLevel.Error);
            try
            {
                lock (sync)
                    FlattenAll("management loop exception");
            }
            catch (Exception flattenEx)
            {
                Log($"Flatten after loop error also failed: {flattenEx}", StrategyLoggingLevel.Error);
            }
            halted = true;
            Stop();
        }
    }

    private void OnTimerCore()
    {
        lock (sync)
        {
            if (halted)
                return;

            cycleCount++;
            PushMid(Mid());
            if (cycleCount % heartbeatEveryCycles == 0)
                LogHeartbeat();

            var pos = GetOwnedPosition();

            if (state == TradeState.Flat)
            {
                if (pos != null)
                {
                    Halt("unexpected position detected while flat");
                    return;
                }

                TryEnterBase();
                return;
            }

            // state == InTrade
            if (pos == null)
            {
                if (!initialized)
                {
                    // Base market order not yet reflected as a position; wait a few cycles.
                    if (++entryPendingCycles > 40)
                        Halt("base entry not confirmed as a position; check the account manually");
                    return;
                }

                ResetToFlat("position closed");
                return;
            }

            if (!initialized)
                InitializeFromPosition(pos);
            else
            {
                positionSide = pos.Side;
            }

            UpdateHighWater();
            if (CheckRiskOffFlatten())
                return;
            MaybeAdd(pos);
            UpdateProtectiveStop();
            CheckSoftExit();
        }
    }

    /// <summary>Evidence-based inventory kill switch: flatten the stack when the regime turns risk-off.</summary>
    private bool CheckRiskOffFlatten()
    {
        if (!FlattenOnRiskOff)
            return false;

        var ms = AnalyticsSnapshot()?.Features.MarketState;
        if (ms == null || !IsUsable(ms.Quality) || !IsRiskOff(ms))
            return false;

        FlattenAll($"regime risk-off: {ms.Regime}/{ms.Risk}");
        return true;
    }

    private void LogHeartbeat()
    {
        double mid = Mid();
        string midStr = double.IsNaN(mid) ? "n/a" : mid.ToString("0.##", CultureInfo.InvariantCulture);

        var snap = AnalyticsSnapshot();
        var ms = snap?.Features.MarketState;
        var of = snap?.Features.OrderFlow;
        string flowStr = of != null && IsUsable(of.Quality) ? $"{of.Bias} {of.Confidence:0.00}" : "n/a";
        string regimeStr = ms != null && IsUsable(ms.Quality) ? ms.Regime.ToString() : "n/a";

        if (state == TradeState.Flat)
        {
            string reason = EntryMode == EntrySignal
                ? (of == null || !IsUsable(of.Quality) ? "signal warming"
                    : of.Bias == DirectionalBias.Neutral ? "signal neutral — waiting"
                    : of.Confidence < MinSignalConfidence ? "signal below confidence"
                    : "signal ready — evaluating entry")
                : Bias == BiasOff ? "no bias set — idle"
                : consumed ? "armed & consumed (toggle bias to re-arm)"
                : "ready — will enter on next cycle";
            Log($"Heartbeat: FLAT | entry={(EntryMode == EntrySignal ? "signal" : BiasLabel(Bias))} | flow={flowStr} | regime={regimeStr} | mid={midStr} | cycles={cycleCount} | {reason}.", StrategyLoggingLevel.Trading);
            return;
        }

        var pos = GetOwnedPosition();
        int qty = pos != null ? (int)Math.Round(Math.Abs(pos.Quantity)) : 0;
        string stopStr = stopOrder != null && IsOpen(stopOrder)
            ? stopOrder.TriggerPrice.ToString("0.##", CultureInfo.InvariantCulture)
            : "none";
        double trailDist = TrailDistTicks() * CurrentSymbol.TickSize;
        double softLevel = positionSide == Side.Buy ? highWaterPrice - trailDist : highWaterPrice + trailDist;
        Log($"Heartbeat: IN-TRADE {positionSide} {qty}/{MaxContracts} | entry={baseEntryPrice.ToString("0.##", CultureInfo.InvariantCulture)} | mid={midStr} | high={highWaterPrice.ToString("0.##", CultureInfo.InvariantCulture)} | bStop={stopStr} | soft={softLevel.ToString("0.##", CultureInfo.InvariantCulture)} | flow={flowStr} | regime={regimeStr} | cycles={cycleCount}.", StrategyLoggingLevel.Trading);
    }

    private void TryEnterBase()
    {
        if (double.IsNaN(Mid()))
            return;

        var snap = AnalyticsSnapshot();
        MarketStateSnapshot? ms = snap?.Features.MarketState;
        OrderFlowSnapshot? of = snap?.Features.OrderFlow;

        Side side;
        string evidence;

        if (EntryMode == EntrySignal)
        {
            // Direction from the order-flow signal. No manual arm; each flat cycle re-evaluates.
            if (of == null || !IsUsable(of.Quality) || of.Bias == DirectionalBias.Neutral)
                return;
            if (of.Confidence < MinSignalConfidence)
                return;
            if ((Core.Instance.TimeUtils.DateTimeUtcNow - lastFlatUtc).TotalSeconds < ReentryCooldownSec)
                return; // avoid churn right after an exit
            side = of.Bias == DirectionalBias.Up ? Side.Buy : Side.Sell;
            evidence = $"signal {of.Bias} conf={of.Confidence:0.00} [{of.Reason}]";
        }
        else
        {
            // Manual bias: one trade per arm (toggle Off -> Long/Short to re-arm).
            if (Bias == BiasOff) { consumed = false; return; }
            if (consumed) return;
            side = Bias == BiasLong ? Side.Buy : Side.Sell;
            evidence = $"manual {BiasLabel(Bias)}";
        }

        // Evidence-based veto: never enter into a risk-off regime.
        if (EnableAnalytics && ms != null && IsUsable(ms.Quality))
        {
            if (IsRiskOff(ms))
                return;
            evidence += $", regime={ms.Regime}/{ms.Risk}";
        }
        else
        {
            evidence += ", no-analytics";
        }

        if (!PlaceMarket(side, 1, "BASE"))
            return;

        state = TradeState.InTrade;
        initialized = false;
        entryPendingCycles = 0;
        consumed = EntryMode == EntryManual; // signal mode re-arms on its own
        baseEntryCount++;
        Log($"Base entry: {side} 1 — evidence: {evidence}.", StrategyLoggingLevel.Trading);
    }

    private static string BiasLabel(int bias)
        => bias == BiasLong ? "Long" : bias == BiasShort ? "Short" : "Off";

    private void InitializeFromPosition(Position pos)
    {
        positionSide = pos.Side;
        baseEntryPrice = pos.OpenPrice;
        lastAddPrice = baseEntryPrice;
        highWaterPrice = baseEntryPrice;
        softBreachStartUtc = null;
        initialized = true;
        Log($"In trade: {positionSide} {Math.Abs(pos.Quantity)} @ {baseEntryPrice}; hardDist={HardDistTicks():0.#}t soft={TrailDistTicks():0.#}t noise={NoiseTicks():0.#}t.", StrategyLoggingLevel.Trading);
        UpdateProtectiveStop();
    }

    // --- Internal volatility (rolling mid range, in ticks) ---------------------------------------

    private void PushMid(double mid)
    {
        if (double.IsNaN(mid))
            return;

        var now = Core.Instance.TimeUtils.DateTimeUtcNow;
        midWindow.Enqueue(new MidSample(now, mid));
        var cutoff = now - TimeSpan.FromSeconds(Math.Max(5, VolLookbackSeconds));
        while (midWindow.Count > 0 && midWindow.Peek().TimeUtc < cutoff)
            midWindow.Dequeue();
    }

    private double NoiseTicks()
    {
        if (midWindow.Count < 3)
            return double.NaN;

        double min = double.MaxValue, max = double.MinValue;
        foreach (var s in midWindow)
        {
            if (s.Mid < min) min = s.Mid;
            if (s.Mid > max) max = s.Mid;
        }

        return (max - min) / CurrentSymbol.TickSize;
    }

    private double HardDistTicks()
    {
        if (!EnableVolatilityScaling)
            return StopLossTicks;
        double noise = NoiseTicks();
        return double.IsNaN(noise) ? StopLossTicks : Math.Max(StopLossTicks, VolStopMultiple * noise);
    }

    private double TrailDistTicks()
    {
        if (!EnableVolatilityScaling)
            return TrailTicks;
        double noise = NoiseTicks();
        return double.IsNaN(noise) ? TrailTicks : Math.Max(TrailTicks, VolTrailMultiple * noise);
    }

    private void UpdateHighWater()
    {
        double mid = Mid();
        if (double.IsNaN(mid))
            return;

        highWaterPrice = positionSide == Side.Buy
            ? Math.Max(highWaterPrice, mid)
            : Math.Min(highWaterPrice, mid);
    }

    private void MaybeAdd(Position pos)
    {
        double mid = Mid();
        if (double.IsNaN(mid))
            return;

        var snap = AnalyticsSnapshot();
        MarketStateSnapshot? ms = snap?.Features.MarketState;
        OrderFlowSnapshot? of = snap?.Features.OrderFlow;

        double tick = CurrentSymbol.TickSize;
        int projected = (int)Math.Round(Math.Abs(pos.Quantity));

        while (projected < MaxContracts)
        {
            double nextLevel = positionSide == Side.Buy
                ? lastAddPrice + AddStepTicks * tick
                : lastAddPrice - AddStepTicks * tick;

            bool reached = positionSide == Side.Buy ? mid >= nextLevel : mid <= nextLevel;
            if (!reached)
                break;

            // Evidence gates for pyramiding: don't add into a risk-off regime, and (optionally)
            // don't add when order flow no longer supports the position side.
            if (EnableAnalytics && ms != null && IsUsable(ms.Quality) && IsRiskOff(ms))
                break;
            if (RequireFlowForAdds && of != null && IsUsable(of.Quality) && FlowOpposes(of, positionSide))
                break;

            if (!PlaceMarket(positionSide, 1, "ADD"))
                break;

            projected++;
            addCount++;
            lastAddPrice = nextLevel;
            string ev = ms != null && IsUsable(ms.Quality) ? $"regime={ms.Regime}" : "no-analytics";
            if (of != null && IsUsable(of.Quality)) ev += $", flow={of.Bias}";
            Log($"Pyramid add: {positionSide} 1 at {nextLevel} (target {projected}/{MaxContracts}) — {ev}.", StrategyLoggingLevel.Trading);
        }
    }

    private void UpdateProtectiveStop()
    {
        var pos = GetOwnedPosition();
        if (pos == null)
            return;

        double tick = CurrentSymbol.TickSize;
        int qty = Math.Max(1, (int)Math.Round(Math.Abs(pos.Quantity)));
        // Broker stop is the WIDE disaster/backstop at the (vol-scaled) hard distance, trailing only
        // in the favorable direction. The tighter profit-taking exit is the soft, persistence-filtered
        // trail in CheckSoftExit — so the broker stop rarely gets wicked.
        double hardDist = HardDistTicks() * tick;
        double desired = positionSide == Side.Buy
            ? highWaterPrice - hardDist
            : highWaterPrice + hardDist;
        desired = CurrentSymbol.RoundPriceToTickSize(desired);
        Side closeSide = positionSide == Side.Buy ? Side.Sell : Side.Buy;

        stopOrder = (stopOrder != null && IsOpen(stopOrder)) ? stopOrder : FindOpenOrderById(stopOrderId);

        if (stopOrder == null)
        {
            PlaceStop(closeSide, qty, desired);
            return;
        }

        double currentTrigger = stopOrder.TriggerPrice;
        bool qtyChanged = Math.Abs(stopOrder.TotalQuantity - qty) > 0.0001;

        // A quantity change (a new lot to protect) must apply immediately.
        if (qtyChanged)
        {
            ModifyStop(stopOrder, qty, desired);
            lastStopModifyUtc = Core.Instance.TimeUtils.DateTimeUtcNow;
            return;
        }

        // Otherwise only re-trail on a meaningful favorable step, throttled to ~1/sec. Re-pricing the
        // stop every tick hammered the broker (30+ modifies/min) and left it perpetually in-flight,
        // which can miss triggers. The stop only ever moves in the favorable direction.
        double improvement = positionSide == Side.Buy
            ? desired - currentTrigger
            : currentTrigger - desired;
        bool bigEnoughStep = improvement >= TrailUpdateTicks * tick;
        bool throttleElapsed = Core.Instance.TimeUtils.DateTimeUtcNow - lastStopModifyUtc >= TimeSpan.FromSeconds(1);

        if (bigEnoughStep && throttleElapsed)
        {
            ModifyStop(stopOrder, qty, desired);
            lastStopModifyUtc = Core.Instance.TimeUtils.DateTimeUtcNow;
        }
    }

    /// <summary>
    /// Soft, persistence-filtered trailing exit. Price must hold beyond the (vol-scaled) soft trail
    /// for <see cref="SoftExitPersistenceMs"/> before flattening; a wick that reverts within the
    /// window resets the timer, so a brief stop-hunt sweep does not exit. The wide broker stop remains
    /// as the disaster backstop.
    /// </summary>
    private void CheckSoftExit()
    {
        double mid = Mid();
        if (double.IsNaN(mid))
            return;

        double trailDist = TrailDistTicks() * CurrentSymbol.TickSize;
        double softLevel = positionSide == Side.Buy
            ? highWaterPrice - trailDist
            : highWaterPrice + trailDist;

        bool breached = positionSide == Side.Buy ? mid < softLevel : mid > softLevel;
        var now = Core.Instance.TimeUtils.DateTimeUtcNow;

        if (!breached)
        {
            softBreachStartUtc = null; // reverted within window — wick/hunt dodged
            return;
        }

        // Order-flow confirmation: if flow has flipped against the position, the breach is evidence-backed
        // (a real move, not a wick) and we exit immediately instead of waiting out the persistence window.
        var of = AnalyticsSnapshot()?.Features.OrderFlow;
        if (of != null && IsUsable(of.Quality) && FlowOpposes(of, positionSide))
        {
            FlattenAll($"soft trail + flow reversal {of.Bias} conf={of.Confidence:0.00} beyond {softLevel:0.##}");
            return;
        }

        softBreachStartUtc ??= now;
        if (now - softBreachStartUtc.Value >= TimeSpan.FromMilliseconds(Math.Max(0, SoftExitPersistenceMs)))
            FlattenAll($"soft trail confirmed: held beyond {softLevel:0.##} for {SoftExitPersistenceMs}ms");
    }

    private bool PlaceMarket(Side side, double quantity, string tag)
    {
        TradingOperationResult result;
        try
        {
            result = Core.Instance.PlaceOrder(new PlaceOrderRequestParameters
            {
                Account = CurrentAccount,
                Symbol = CurrentSymbol,
                Side = side,
                OrderTypeId = marketOrderTypeId,
                Quantity = quantity,
                TimeInForce = TimeInForce.GTC,
                Comment = $"{commentPrefix} {tag}"
            });
        }
        catch (Exception ex)
        {
            Log($"{tag} market {side} threw: {ex.Message}", StrategyLoggingLevel.Error);
            if (tag == "BASE")
                Halt($"base entry threw: {ex.Message}");
            return false;
        }

        if (result.Status == TradingOperationResultStatus.Failure)
        {
            Log($"{tag} market {side} failed: {(string.IsNullOrWhiteSpace(result.Message) ? result.Status.ToString() : result.Message)}", StrategyLoggingLevel.Error);
            if (tag == "BASE")
                Halt($"base entry failed: {result.Message}");
            return false;
        }

        return true;
    }

    private void PlaceStop(Side closeSide, int quantity, double triggerPrice)
    {
        TradingOperationResult result;
        try
        {
            result = Core.Instance.PlaceOrder(new PlaceOrderRequestParameters
            {
                Account = CurrentAccount,
                Symbol = CurrentSymbol,
                Side = closeSide,
                OrderTypeId = stopOrderTypeId,
                Quantity = quantity,
                TriggerPrice = triggerPrice,
                TimeInForce = TimeInForce.GTC,
                Comment = $"{commentPrefix} STOP"
            });
        }
        catch (Exception ex)
        {
            Log($"Protective stop placement threw ({ex.Message}); flattening.", StrategyLoggingLevel.Error);
            FlattenAll("protective stop placement threw");
            return;
        }

        if (result.Status == TradingOperationResultStatus.Failure)
        {
            Log($"Protective stop placement failed ({(string.IsNullOrWhiteSpace(result.Message) ? result.Status.ToString() : result.Message)}); flattening.", StrategyLoggingLevel.Error);
            FlattenAll("protective stop placement failed");
            return;
        }

        stopOrderId = result.OrderId;
        stopOrder = FindOpenOrderById(result.OrderId);
        Log($"Placed protective stop: {closeSide} {quantity} @ {triggerPrice}.", StrategyLoggingLevel.Trading);
    }

    private void ModifyStop(Order order, int quantity, double triggerPrice)
    {
        try
        {
            var result = Core.Instance.ModifyOrder(new ModifyOrderRequestParameters(order)
            {
                Quantity = quantity,
                TriggerPrice = triggerPrice,
                TimeInForce = TimeInForce.GTC
            });

            if (result.Status == TradingOperationResultStatus.Failure)
                Log($"Protective stop modify failed: {(string.IsNullOrWhiteSpace(result.Message) ? result.Status.ToString() : result.Message)}", StrategyLoggingLevel.Error);
        }
        catch (Exception ex)
        {
            Log($"Protective stop modify threw: {ex.Message}", StrategyLoggingLevel.Error);
        }
    }

    private void FlattenAll(string reason)
    {
        CancelStop(reason);

        var pos = GetOwnedPosition();
        if (pos != null)
        {
            try
            {
                var result = pos.Close();
                flattenCount++;
                if (result.Status == TradingOperationResultStatus.Failure)
                    Log($"Flatten failed ({reason}): {(string.IsNullOrWhiteSpace(result.Message) ? result.Status.ToString() : result.Message)}", StrategyLoggingLevel.Error);
                else
                    Log($"Flattened stack ({reason}).", StrategyLoggingLevel.Trading);
            }
            catch (Exception ex)
            {
                Log($"Flatten threw ({reason}): {ex.Message}", StrategyLoggingLevel.Error);
            }
        }

        ResetToFlat(reason);
    }

    private void ResetToFlat(string reason)
    {
        CancelStop(reason);
        if (state == TradeState.InTrade)
            exitCount++;
        state = TradeState.Flat;
        initialized = false;
        entryPendingCycles = 0;
        positionSide = default;
        baseEntryPrice = 0;
        lastAddPrice = 0;
        highWaterPrice = 0;
        softBreachStartUtc = null;
        stopOrder = null;
        stopOrderId = null;
        lastFlatUtc = Core.Instance.TimeUtils.DateTimeUtcNow;
        string rearm = EntryMode == EntrySignal ? "awaiting next order-flow signal" : "toggle bias Off->Long/Short to re-arm";
        Log($"Flat ({reason}). {rearm}.", StrategyLoggingLevel.Trading);
    }

    // --- Analytics evidence helpers --------------------------------------------------------------

    private AnalyticsRuntimeSnapshot? AnalyticsSnapshot()
        => analytics != null && analytics.IsInitialized ? analytics.CurrentSnapshot : null;

    private static bool IsUsable(MetricQuality q)
        => q is MetricQuality.Exact or MetricQuality.Derived;

    private static bool IsRiskOff(MarketStateSnapshot ms)
        => ms.Regime is MarketRegime.ThinFragile or MarketRegime.VolatileDislocated
            || ms.Risk == RiskEnvironment.Critical;

    private static bool FlowOpposes(OrderFlowSnapshot of, Side side)
        => side == Side.Buy ? of.Bias == DirectionalBias.Down : of.Bias == DirectionalBias.Up;

    private void MapAnalyticsLog(string message, LoggingLevel level)
        => Log(message, level == LoggingLevel.Error ? StrategyLoggingLevel.Error : StrategyLoggingLevel.Info);

    private void CancelStop(string reason)
    {
        var order = (stopOrder != null && IsOpen(stopOrder)) ? stopOrder : FindOpenOrderById(stopOrderId);
        if (order != null && IsOpen(order))
        {
            try
            {
                var result = order.Cancel(commentPrefix);
                if (result.Status == TradingOperationResultStatus.Failure)
                    Log($"Stop cancel failed ({reason}): {(string.IsNullOrWhiteSpace(result.Message) ? result.Status.ToString() : result.Message)}", StrategyLoggingLevel.Error);
            }
            catch (Exception ex)
            {
                Log($"Stop cancel threw ({reason}): {ex.Message}", StrategyLoggingLevel.Error);
            }
        }

        stopOrder = null;
        stopOrderId = null;
    }

    private void Core_PositionRemoved(Position position)
    {
        // Runs on a Quantower callback thread; never let an exception escape into the host.
        try
        {
            if (position.Symbol != CurrentSymbol || position.Account != CurrentAccount)
                return;

            lock (sync)
            {
                if (state != TradeState.InTrade)
                    return;
                if (GetOwnedPosition() != null)
                    return;

                ResetToFlat("position removed");
            }
        }
        catch (Exception ex)
        {
            Log($"Position-removed handler error: {ex}", StrategyLoggingLevel.Error);
        }
    }

    private void SubscribeCoreEvents()
    {
        if (coreEventsSubscribed)
            return;

        Core.PositionRemoved += Core_PositionRemoved;
        coreEventsSubscribed = true;
    }

    private void UnsubscribeCoreEvents()
    {
        if (!coreEventsSubscribed)
            return;

        Core.PositionRemoved -= Core_PositionRemoved;
        coreEventsSubscribed = false;
    }

    private double Mid()
    {
        double bid = CurrentSymbol.Bid;
        double ask = CurrentSymbol.Ask;
        if (bid <= 0 || ask <= 0 || double.IsNaN(bid) || double.IsNaN(ask) || ask < bid)
            return double.NaN;
        return (bid + ask) / 2.0;
    }

    private Position? GetOwnedPosition()
        => Core.Instance.Positions.FirstOrDefault(p =>
            p.Symbol == CurrentSymbol && p.Account == CurrentAccount && Math.Abs(p.Quantity) > 0);

    private bool HasOpenPosition()
        => GetOwnedPosition() != null;

    private IEnumerable<Order> GetOpenOrdersForSymbolAccount()
        => Core.Instance.Orders.Where(o => o.Symbol == CurrentSymbol && o.Account == CurrentAccount && IsOpen(o));

    private Order? FindOpenOrderById(string? orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            return null;

        return GetOpenOrdersForSymbolAccount().FirstOrDefault(o =>
            string.Equals(o.Id, orderId, StringComparison.Ordinal) || string.Equals(o.UniqueId, orderId, StringComparison.Ordinal));
    }

    private static bool IsOpen(Order order)
        => order.Status is OrderStatus.Opened or OrderStatus.PartiallyFilled;

    private void Halt(string reason)
    {
        halted = true;
        Log($"HALTED: {reason}", StrategyLoggingLevel.Error);
        Stop();
    }
}
