using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using TradingPlatform.BusinessLayer;

namespace MBO_OnePair_Grid_Strategy;

/// <summary>
/// Anti-martingale (pyramiding) momentum strategy. A manual directional bias fires an aggressive
/// market entry; the position adds one lot per favorable step up to a hard exposure cap, and the
/// whole stack is protected by a single ratcheting trailing stop plus a base hard stop. It never
/// averages down — adds happen only in profit. Direction is intentionally manual. Deliberately
/// self-contained: it takes no dependency on the analytics/indicator assembly, so it cannot bind
/// to a stale shared-assembly copy across Quantower script folders.
/// </summary>
public sealed class PyramidMomentumStrategy : Strategy, ICurrentAccount, ICurrentSymbol
{
    // Directional bias values. Backed by int + InputParameter variants so Quantower renders a
    // dropdown reliably (bare enum InputParameters are not shown in the settings dialog).
    private const int BiasOff = 0;
    private const int BiasLong = 1;
    private const int BiasShort = -1;

    private enum TradeState
    {
        Flat,
        InTrade
    }

    private readonly record struct MidSample(DateTime TimeUtc, double Mid);

    private readonly object sync = new();
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

        int period = Math.Max(50, RefreshMs);
        heartbeatEveryCycles = Math.Max(1, 10000 / period);
        timer = new System.Threading.Timer(_ => OnTimer(), null, 0, period);
        Log($"Started pyramid momentum strategy: maxContracts={MaxContracts}, addStep={AddStepTicks}t, trail={TrailTicks}t, hardSL={StopLossTicks}t, refresh={period}ms. Set 'Directional bias' to Long/Short to enter.");
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

        base.OnStop();
    }

    protected override void OnRemove()
    {
        timer?.Dispose();
        timer = null;
        lock (sync)
            FlattenAll("strategy remove");
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
            MaybeAdd(pos);
            UpdateProtectiveStop();
            CheckSoftExit();
        }
    }

    private void LogHeartbeat()
    {
        double mid = Mid();
        string midStr = double.IsNaN(mid) ? "n/a" : mid.ToString("0.##", CultureInfo.InvariantCulture);

        if (state == TradeState.Flat)
        {
            string reason = Bias == BiasOff ? "no bias set — idle"
                : consumed ? "armed & consumed (toggle bias Off->Long/Short to re-arm)"
                : double.IsNaN(mid) ? "waiting for quotes"
                : "ready — will enter on next cycle";
            Log($"Heartbeat: FLAT | bias={BiasLabel(Bias)} | mid={midStr} | cycles={cycleCount} | {reason}.", StrategyLoggingLevel.Trading);
            return;
        }

        var pos = GetOwnedPosition();
        int qty = pos != null ? (int)Math.Round(Math.Abs(pos.Quantity)) : 0;
        string stopStr = stopOrder != null && IsOpen(stopOrder)
            ? stopOrder.TriggerPrice.ToString("0.##", CultureInfo.InvariantCulture)
            : "none";
        double trailDist = TrailDistTicks() * CurrentSymbol.TickSize;
        double softLevel = positionSide == Side.Buy ? highWaterPrice - trailDist : highWaterPrice + trailDist;
        Log($"Heartbeat: IN-TRADE {positionSide} {qty}/{MaxContracts} | entry={baseEntryPrice.ToString("0.##", CultureInfo.InvariantCulture)} | mid={midStr} | high={highWaterPrice.ToString("0.##", CultureInfo.InvariantCulture)} | bStop={stopStr} | soft={softLevel.ToString("0.##", CultureInfo.InvariantCulture)} | noise={NoiseTicks():0.#}t | cycles={cycleCount}.", StrategyLoggingLevel.Trading);
    }

    private void TryEnterBase()
    {
        if (Bias == BiasOff)
        {
            consumed = false;
            return;
        }

        if (consumed)
            return; // one trade per arm; set Bias to Off then back to re-arm

        if (double.IsNaN(Mid()))
            return;

        Side side = Bias == BiasLong ? Side.Buy : Side.Sell;
        if (!PlaceMarket(side, 1, "BASE"))
            return;

        state = TradeState.InTrade;
        initialized = false;
        entryPendingCycles = 0;
        consumed = true;
        baseEntryCount++;
        Log($"Base entry fired: {side} 1 ({BiasLabel(Bias)}).", StrategyLoggingLevel.Trading);
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

            if (!PlaceMarket(positionSide, 1, "ADD"))
                break;

            projected++;
            addCount++;
            lastAddPrice = nextLevel;
            Log($"Pyramid add: {positionSide} 1 at {nextLevel} (target {projected}/{MaxContracts}).", StrategyLoggingLevel.Trading);
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
        Log($"Flat ({reason}). Set bias to Off then Long/Short to re-arm.", StrategyLoggingLevel.Trading);
    }

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
