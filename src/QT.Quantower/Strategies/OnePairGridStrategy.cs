using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using MBO_Market_Data_Analytics;
using QT.Strategies.Advisory;
using QT.Strategies.Grid;
using TradingPlatform.BusinessLayer;

namespace MBO_OnePair_Grid_Strategy;

public sealed class OnePairGridStrategy : Strategy, ICurrentAccount, ICurrentSymbol
{
    private readonly object sync = new();
    private readonly Queue<DateTime> cancelReplaceActions = new();
    private readonly List<PendingLatencyOperation> pendingLatencyOperations = new();
    private readonly Dictionary<string, OrderSubscription> orderSubscriptions = new(StringComparer.Ordinal);
    private readonly Dictionary<string, LatencyAccumulator> latencyAccumulators = new(StringComparer.Ordinal);

    private readonly IAnalyticsAdvisor advisor = new RegimeGatedAdvisor();
    private AnalyticsEngineHost? analytics;
    private TradingAdvice? lastAdvice;
    private int gatePullCount;
    private int gateWarmupCount;

    private System.Threading.Timer? timer;
    private Order? bidOrder;
    private Order? askOrder;
    private string? bidOrderId;
    private string? askOrderId;
    private string commentPrefix = "";
    private string orderTypeId = OrderType.Limit;
    private bool halted;
    private bool coreEventsSubscribed;
    private string haltReason = "";
    private int cycleCount;
    private int placedCount;
    private int modifiedCount;
    private int cancelCount;
    private int skipCount;
    private int positionHaltCount;
    private long lastBidTicks;
    private long lastAskTicks;
    private long lastWidthTicks;
    private long nextLatencyOperationId;

    [InputParameter("Symbol", 0)]
    public Symbol CurrentSymbol { get; set; } = null!;

    [InputParameter("Account", 1)]
    public Account CurrentAccount { get; set; } = null!;

    [InputParameter("Arm live order placement", 2)]
    public bool ArmLiveOrderPlacement { get; set; }

    [InputParameter("Enable live orders", 3)]
    public bool EnableLiveOrders { get; set; }

    [InputParameter("Quantity", 4, minimum: 1, maximum: 1000, increment: 1, decimalPlaces: 0)]
    public double Quantity { get; set; } = 1;

    [InputParameter("Target pair width ticks", 5, minimum: 1, maximum: 2000, increment: 1, decimalPlaces: 0)]
    public int TargetPairWidthTicks { get; set; } = 100;

    [InputParameter("Width tolerance ticks", 6, minimum: 0, maximum: 500, increment: 1, decimalPlaces: 0)]
    public int WidthToleranceTicks { get; set; } = 20;

    [InputParameter("Min passive offset ticks", 7, minimum: 0, maximum: 100, increment: 1, decimalPlaces: 0)]
    public int MinPassiveOffsetTicks { get; set; } = 1;

    [InputParameter("Refresh seconds", 8, minimum: 1, maximum: 60, increment: 1, decimalPlaces: 0)]
    public int RefreshSeconds { get; set; } = 1;

    [InputParameter("Reprice threshold ticks", 9, minimum: 1, maximum: 500, increment: 1, decimalPlaces: 0)]
    public int RepriceThresholdTicks { get; set; } = 5;

    [InputParameter("Max cancel/replace per minute", 10, minimum: 1, maximum: 120, increment: 1, decimalPlaces: 0)]
    public int MaxCancelReplacePerMinute { get; set; } = 20;

    [InputParameter("Cancel pair on stop", 11)]
    public bool CancelPairOnStop { get; set; } = true;

    [InputParameter("Log order latency telemetry", 12)]
    public bool LogOrderLatencyTelemetry { get; set; } = true;

    [InputParameter("Enable analytics gate", 13)]
    public bool EnableAnalyticsGate { get; set; } = true;

    [InputParameter("Analytics prefer MBO", 14)]
    public bool AnalyticsPreferMbo { get; set; } = true;

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

    public OnePairGridStrategy()
        : base()
    {
        Name = "MBO One Pair Grid Strategy";
        Description = "Places one passive bid/ask quote pair and floats it at a fixed tick width. Disabled unless explicitly armed.";
    }

    protected override void OnRun()
    {
        base.OnRun();
        UnsubscribeCoreEvents();

        analytics?.Stop();
        analytics = null;
        lastAdvice = null;
        gatePullCount = 0;
        gateWarmupCount = 0;

        lock (sync)
        {
            halted = false;
            haltReason = "";
            cycleCount = 0;
            placedCount = 0;
            modifiedCount = 0;
            cancelCount = 0;
            skipCount = 0;
            positionHaltCount = 0;
            lastBidTicks = 0;
            lastAskTicks = 0;
            lastWidthTicks = 0;
            nextLatencyOperationId = 0;
            bidOrder = null;
            askOrder = null;
            bidOrderId = null;
            askOrderId = null;
            commentPrefix = $"MBO one-pair grid {Guid.NewGuid():N}";
            cancelReplaceActions.Clear();
            pendingLatencyOperations.Clear();
            latencyAccumulators.Clear();
            UnsubscribeOrderUpdates();
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

        orderTypeId = Core.OrderTypes.FirstOrDefault(x => x.ConnectionId == CurrentSymbol.ConnectionId && x.Behavior == OrderTypeBehavior.Limit)?.Id ?? OrderType.Limit;

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

        if (EnableAnalyticsGate)
        {
            try
            {
                analytics = new AnalyticsEngineHost(CurrentSymbol, new AnalyticsEngineConfig { PreferMbo = AnalyticsPreferMbo }, MapAnalyticsLog);
                analytics.Start();
                Log("Analytics gate enabled; quoting is filtered by the market-state advisor.", StrategyLoggingLevel.Trading);
            }
            catch (Exception ex)
            {
                analytics = null;
                Halt($"analytics gate failed to start: {ex.Message}");
                return;
            }
        }

        timer = new System.Threading.Timer(_ => OnTimer(), null, TimeSpan.Zero, TimeSpan.FromSeconds(Math.Max(1, RefreshSeconds)));
        Log($"Started one-pair grid strategy: qty={Quantity}, width={TargetPairWidthTicks}+/-{WidthToleranceTicks} ticks, refresh={RefreshSeconds}s.");
    }

    protected override void OnStop()
    {
        timer?.Dispose();
        timer = null;

        analytics?.Stop();
        analytics = null;

        if (CancelPairOnStop)
            CancelOwnedPair("strategy stop");

        base.OnStop();
    }

    protected override void OnRemove()
    {
        timer?.Dispose();
        timer = null;
        analytics?.Stop();
        analytics = null;
        CancelOwnedPair("strategy remove");
        UnsubscribeCoreEvents();
        lock (sync)
        {
            pendingLatencyOperations.Clear();
            UnsubscribeOrderUpdates();
        }
        base.OnRemove();
    }

    protected override void OnInitializeMetrics(Meter meter)
    {
        base.OnInitializeMetrics(meter);
        meter.CreateObservableCounter("grid-cycles", () => cycleCount, description: "Quote refresh cycles");
        meter.CreateObservableCounter("grid-orders-placed", () => placedCount, description: "Orders placed by this strategy run");
        meter.CreateObservableCounter("grid-orders-modified", () => modifiedCount, description: "Orders modified by this strategy run");
        meter.CreateObservableCounter("grid-orders-cancelled", () => cancelCount, description: "Orders cancelled by this strategy run");
        meter.CreateObservableCounter("grid-skips", () => skipCount, description: "Skipped quote cycles");
        meter.CreateObservableCounter("grid-position-halts", () => positionHaltCount, description: "Halts caused by detected position");
        meter.CreateObservableCounter("grid-analytics-pulls", () => gatePullCount, description: "Cycles where the analytics advisor withheld or pulled quotes");
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

        var config = BuildConfig();
        if (!config.IsValid(out var reason))
        {
            Log($"Invalid quote configuration: {reason}", StrategyLoggingLevel.Error);
            return false;
        }

        if (CurrentSymbol.TickSize <= 0 || double.IsNaN(CurrentSymbol.TickSize))
        {
            Log("Symbol tick size is unavailable.", StrategyLoggingLevel.Error);
            return false;
        }

        if (Quantity <= 0)
        {
            Log("Quantity must be positive.", StrategyLoggingLevel.Error);
            return false;
        }

        return true;
    }

    private void OnTimer()
    {
        lock (sync)
        {
            if (halted)
                return;

            cycleCount++;

            if (HasOpenPosition())
            {
                positionHaltCount++;
                CancelOwnedPair("position detected");
                Halt("position detected; quoting halted");
                return;
            }

            RefreshOwnedReferences();

            var unknownOpenOrders = GetOpenOrdersForSymbolAccount()
                .Where(o => !IsOwnedReference(o))
                .ToArray();

            if (unknownOpenOrders.Length > 0)
            {
                CancelOwnedPair("unknown open order detected");
                Halt("unknown open order detected on selected symbol/account");
                return;
            }

            if (!PassesAnalyticsGate())
                return;

            var bestBid = CurrentSymbol.Bid;
            var bestAsk = CurrentSymbol.Ask;
            var tickSize = CurrentSymbol.TickSize;
            if (bestBid <= 0 || bestAsk <= 0 || tickSize <= 0 || double.IsNaN(bestBid) || double.IsNaN(bestAsk) || double.IsNaN(tickSize))
            {
                skipCount++;
                return;
            }

            var bestBidTicks = PriceToTicks(bestBid, tickSize);
            var bestAskTicks = PriceToTicks(bestAsk, tickSize);
            var decision = OnePairGridQuoteEngine.Calculate(bestBidTicks, bestAskTicks, BuildConfig());
            if (!decision.ShouldQuote)
            {
                skipCount++;
                Log($"Skipping quote cycle: {decision.Reason}", StrategyLoggingLevel.Trading);
                return;
            }

            lastBidTicks = decision.BidOrderTicks;
            lastAskTicks = decision.AskOrderTicks;
            lastWidthTicks = decision.WidthTicks;

            EnsureSide(Side.Buy, ref bidOrder, ref bidOrderId, decision.BidOrderTicks, tickSize);
            EnsureSide(Side.Sell, ref askOrder, ref askOrderId, decision.AskOrderTicks, tickSize);
        }
    }

    /// <summary>
    /// Consults the analytics advisor. Returns true only when quoting is permitted. While analytics
    /// are warming it skips the cycle; when the advisor is not Armed it pulls any working quotes and
    /// skips. The advisor is filter-only — it never tells this strategy to quote, only whether it may.
    /// </summary>
    private bool PassesAnalyticsGate()
    {
        if (!EnableAnalyticsGate || analytics == null)
            return true;

        var snapshot = analytics.CurrentSnapshot;
        if (snapshot == null || !analytics.IsInitialized)
        {
            gateWarmupCount++;
            skipCount++;
            return false;
        }

        var advice = advisor.Evaluate(MarketView.From(snapshot), AnalyticsEvents.None);
        lastAdvice = advice;

        if (advice.Posture == TradingPosture.Armed)
            return true;

        if (HasOwnedPairReference())
            CancelOwnedPair($"analytics gate: {advice.Reason}");

        gatePullCount++;
        skipCount++;
        Log($"Analytics gate {advice.Posture} ({advice.Style}): {advice.Reason}", StrategyLoggingLevel.Trading);
        return false;
    }

    private bool HasOwnedPairReference()
        => bidOrder != null || askOrder != null
            || !string.IsNullOrWhiteSpace(bidOrderId) || !string.IsNullOrWhiteSpace(askOrderId);

    private void MapAnalyticsLog(string message, LoggingLevel level)
        => Log(message, level == LoggingLevel.Error ? StrategyLoggingLevel.Error : StrategyLoggingLevel.Info);

    private void EnsureSide(Side side, ref Order? order, ref string? orderId, long desiredTicks, double tickSize)
    {
        order = RefreshSideReference(side, order, orderId);

        if (order == null && !string.IsNullOrWhiteSpace(orderId))
        {
            skipCount++;
            return;
        }

        var config = BuildConfig();
        if (order != null && IsOpen(order))
        {
            long currentTicks = PriceToTicks(order.Price, tickSize);
            if (!OnePairGridQuoteEngine.ShouldReprice(currentTicks, desiredTicks, config))
                return;

            if (!AllowCancelReplace())
            {
                skipCount++;
                return;
            }

            ModifyOrder(order, desiredTicks, tickSize);
            return;
        }

        if (order == null)
        {
            var placed = PlaceLimit(side, TicksToPrice(desiredTicks, tickSize));
            order = placed.Order;
            orderId = placed.OrderId;
        }
    }

    private PlacedOrderRef PlaceLimit(Side side, double price)
    {
        var roundedPrice = CurrentSymbol.RoundPriceToTickSize(price);
        string comment = $"{commentPrefix} {side}";
        long latencyOperationId = BeginLatencyOperation("Place", side, null, roundedPrice, comment);
        var result = Core.Instance.PlaceOrder(new PlaceOrderRequestParameters
        {
            Account = CurrentAccount,
            Symbol = CurrentSymbol,
            Side = side,
            OrderTypeId = orderTypeId,
            Quantity = Quantity,
            Price = roundedPrice,
            TimeInForce = TimeInForce.GTC,
            Comment = comment
        });
        AttachLatencyResult(latencyOperationId, result.RequestId, result.OrderId);

        if (result.Status == TradingOperationResultStatus.Failure)
        {
            FailLatencyOperation(latencyOperationId, result.Message);
            Halt($"place {side} failed: {(string.IsNullOrWhiteSpace(result.Message) ? result.Status.ToString() : result.Message)}");
            return new PlacedOrderRef(null, null);
        }

        placedCount++;
        Log($"Placed {side} limit qty={Quantity} price={roundedPrice} orderId={result.OrderId}", StrategyLoggingLevel.Trading);
        var order = FindOpenOrderById(result.OrderId) ?? FindNewestOpenOrder(side, roundedPrice);
        if (order != null)
        {
            SubscribeOrderUpdates(order);
            CompleteLatencyFromOrder(order, "OrderFoundAfterPlace", false);
        }

        return new PlacedOrderRef(order, result.OrderId);
    }

    private void ModifyOrder(Order order, long desiredTicks, double tickSize)
    {
        var roundedPrice = CurrentSymbol.RoundPriceToTickSize(TicksToPrice(desiredTicks, tickSize));
        long latencyOperationId = BeginLatencyOperation("Modify", order.Side, OrderIdentity(order), roundedPrice, order.Comment);
        var result = Core.Instance.ModifyOrder(order, TimeInForce.GTC, Quantity, roundedPrice);
        AttachLatencyResult(latencyOperationId, result.RequestId, result.OrderId);
        if (result.Status == TradingOperationResultStatus.Failure)
        {
            FailLatencyOperation(latencyOperationId, result.Message);
            Halt($"modify {order.Side} failed: {(string.IsNullOrWhiteSpace(result.Message) ? result.Status.ToString() : result.Message)}");
            return;
        }

        modifiedCount++;
        RegisterCancelReplaceAction();
        Log($"Modified {order.Side} limit price={roundedPrice}", StrategyLoggingLevel.Trading);
    }

    private Order? FindNewestOpenOrder(Side side, double price)
    {
        return GetOpenOrdersForSymbolAccount()
            .Where(o => o.Side == side && Math.Abs(o.Price - price) <= CurrentSymbol.TickSize / 2.0)
            .OrderByDescending(o => o.LastUpdateTime)
            .FirstOrDefault();
    }

    private Order? FindOpenOrderById(string? orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            return null;

        return GetOpenOrdersForSymbolAccount().FirstOrDefault(o => MatchesOrderId(o, orderId));
    }

    private void CancelOwnedPair(string reason)
    {
        lock (sync)
        {
            if (bidOrder != null)
                CancelOrder(bidOrder, reason);
            else
                CancelOrder(FindOpenOrderById(bidOrderId), reason);

            if (askOrder != null)
                CancelOrder(askOrder, reason);
            else
                CancelOrder(FindOpenOrderById(askOrderId), reason);

            bidOrder = null;
            askOrder = null;
            bidOrderId = null;
            askOrderId = null;
        }
    }

    private void CancelOrder(Order? order, string reason)
    {
        if (order == null)
            return;

        if (!IsOpen(order))
            return;

        long latencyOperationId = BeginLatencyOperation("Cancel", order.Side, OrderIdentity(order), order.Price, order.Comment);
        var result = order.Cancel(commentPrefix);
        AttachLatencyResult(latencyOperationId, result.RequestId, result.OrderId);
        if (result.Status == TradingOperationResultStatus.Failure)
        {
            FailLatencyOperation(latencyOperationId, result.Message);
            Log($"Cancel failed ({reason}): {(string.IsNullOrWhiteSpace(result.Message) ? result.Status.ToString() : result.Message)}", StrategyLoggingLevel.Error);
            return;
        }

        cancelCount++;
        RegisterCancelReplaceAction();
        Log($"Cancelled order ({reason}) price={order.Price}", StrategyLoggingLevel.Trading);
    }

    private void RefreshOwnedReferences()
    {
        bidOrder = RefreshSideReference(Side.Buy, bidOrder, bidOrderId);
        askOrder = RefreshSideReference(Side.Sell, askOrder, askOrderId);
    }

    private Order? RefreshSideReference(Side side, Order? current, string? orderId)
    {
        if (current != null && IsOpen(current))
        {
            SubscribeOrderUpdates(current);
            return current;
        }

        var refreshed = FindOpenOrderById(orderId)
            ?? GetOpenOrdersForSymbolAccount()
                .Where(o => o.Side == side && IsOwnedComment(o.Comment))
                .OrderByDescending(o => o.LastUpdateTime)
                .FirstOrDefault();

        if (refreshed != null)
            SubscribeOrderUpdates(refreshed);

        return refreshed;
    }

    private bool IsOwnedReference(Order order)
        => (bidOrder != null && bidOrder.Equals(order))
            || (askOrder != null && askOrder.Equals(order))
            || MatchesOrderId(order, bidOrderId)
            || MatchesOrderId(order, askOrderId)
            || IsOwnedComment(order.Comment);

    private static bool MatchesOrderId(Order order, string? orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            return false;

        return string.Equals(order.Id, orderId, StringComparison.Ordinal)
            || string.Equals(order.UniqueId, orderId, StringComparison.Ordinal);
    }

    private static bool MatchesOrderId(OrderHistory history, string? orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            return false;

        return string.Equals(history.Id, orderId, StringComparison.Ordinal)
            || string.Equals(history.UniqueId, orderId, StringComparison.Ordinal);
    }

    private static string? OrderIdentity(Order order)
        => !string.IsNullOrWhiteSpace(order.Id) ? order.Id : order.UniqueId;

    private static string? OrderIdentity(OrderHistory history)
        => !string.IsNullOrWhiteSpace(history.Id) ? history.Id : history.UniqueId;

    private static string OrderKey(Order order)
        => order.UniqueId ?? order.Id ?? "";

    private bool IsOwnedComment(string? comment)
        => !string.IsNullOrWhiteSpace(comment) && comment.StartsWith(commentPrefix, StringComparison.Ordinal);

    private void Core_OrderAdded(Order order)
    {
        if (!IsPotentialOwnedOrder(order))
            return;

        SubscribeOrderUpdates(order);
        CompleteLatencyFromOrder(order, "OrderAdded", false);
    }

    private void Core_OrderRemoved(Order order)
    {
        if (!IsPotentialOwnedOrder(order))
            return;

        CompleteLatencyFromOrder(order, "OrderRemoved", true);
        UnsubscribeOrderUpdates(order);
    }

    private void Core_OrdersHistoryAdded(OrderHistory history)
    {
        if (!IsPotentialOwnedOrder(history))
            return;

        CompleteLatencyFromHistory(history, "OrdersHistoryAdded");
    }

    private void SubscribeCoreEvents()
    {
        if (coreEventsSubscribed)
            return;

        Core.OrderAdded += Core_OrderAdded;
        Core.OrderRemoved += Core_OrderRemoved;
        Core.OrdersHistoryAdded += Core_OrdersHistoryAdded;
        coreEventsSubscribed = true;
    }

    private void UnsubscribeCoreEvents()
    {
        if (!coreEventsSubscribed)
            return;

        Core.OrderAdded -= Core_OrderAdded;
        Core.OrderRemoved -= Core_OrderRemoved;
        Core.OrdersHistoryAdded -= Core_OrdersHistoryAdded;
        coreEventsSubscribed = false;
    }

    private void SubscribeOrderUpdates(Order order)
    {
        string key = OrderKey(order);
        if (string.IsNullOrWhiteSpace(key) || orderSubscriptions.ContainsKey(key))
            return;

        Action<IOrder> handler = _ => CompleteLatencyFromOrder(order, "OrderUpdated", false);
        order.Updated += handler;
        orderSubscriptions[key] = new OrderSubscription(order, handler);
    }

    private void UnsubscribeOrderUpdates(Order order)
    {
        string key = OrderKey(order);
        if (string.IsNullOrWhiteSpace(key) || !orderSubscriptions.TryGetValue(key, out var subscription))
            return;

        subscription.Order.Updated -= subscription.Handler;
        orderSubscriptions.Remove(key);
    }

    private void UnsubscribeOrderUpdates()
    {
        foreach (var subscription in orderSubscriptions.Values)
            subscription.Order.Updated -= subscription.Handler;

        orderSubscriptions.Clear();
    }

    private long BeginLatencyOperation(string operation, Side side, string? orderId, double price, string? comment)
    {
        if (!LogOrderLatencyTelemetry)
            return 0;

        long id = ++nextLatencyOperationId;
        pendingLatencyOperations.Add(new PendingLatencyOperation(
            id,
            operation,
            side,
            orderId,
            price,
            comment ?? "",
            Stopwatch.GetTimestamp(),
            Core.Instance.TimeUtils.DateTimeUtcNow,
            null,
            null));

        return id;
    }

    private void AttachLatencyResult(long latencyOperationId, long requestId, string? brokerOrderId)
    {
        if (!LogOrderLatencyTelemetry || latencyOperationId <= 0)
            return;

        for (int i = 0; i < pendingLatencyOperations.Count; i++)
        {
            var pending = pendingLatencyOperations[i];
            if (pending.Id != latencyOperationId)
                continue;

            pendingLatencyOperations[i] = pending with
            {
                RequestId = requestId,
                OrderId = string.IsNullOrWhiteSpace(pending.OrderId) ? brokerOrderId : pending.OrderId
            };
            return;
        }
    }

    private void FailLatencyOperation(long latencyOperationId, string? reason)
    {
        if (!LogOrderLatencyTelemetry || latencyOperationId <= 0)
            return;

        for (int i = pendingLatencyOperations.Count - 1; i >= 0; i--)
        {
            if (pendingLatencyOperations[i].Id != latencyOperationId)
                continue;

            var pending = pendingLatencyOperations[i];
            pendingLatencyOperations.RemoveAt(i);
            Log($"ORDER_LATENCY_FAILED op={pending.Operation} side={pending.Side} orderId={pending.OrderId ?? "-"} requestId={pending.RequestId?.ToString(CultureInfo.InvariantCulture) ?? "-"} reason=\"{reason ?? "unknown"}\"",
                StrategyLoggingLevel.Trading);
            return;
        }
    }

    private void CompleteLatencyFromOrder(Order order, string ackEvent, bool removed)
    {
        if (!LogOrderLatencyTelemetry)
            return;

        CompleteLatency(order.Side, OrderIdentity(order), order.Price, order.Comment, order.Status, ackEvent, removed);
    }

    private void CompleteLatencyFromHistory(OrderHistory history, string ackEvent)
    {
        if (!LogOrderLatencyTelemetry)
            return;

        CompleteLatency(history.Side, OrderIdentity(history), history.Price, history.Comment, history.Status, ackEvent, false);
    }

    private void CompleteLatency(Side side, string? orderId, double price, string? comment, OrderStatus status, string ackEvent, bool removed)
    {
        int matchIndex = -1;
        PendingLatencyOperation pending = default;

        for (int i = 0; i < pendingLatencyOperations.Count; i++)
        {
            var candidate = pendingLatencyOperations[i];
            if (!LatencyMatches(candidate, side, orderId, price, comment, status, removed))
                continue;

            matchIndex = i;
            pending = candidate;
            break;
        }

        if (matchIndex < 0)
            return;

        pendingLatencyOperations.RemoveAt(matchIndex);
        double latencyMs = (Stopwatch.GetTimestamp() - pending.StartTimestamp) * 1000.0 / Stopwatch.Frequency;
        string key = pending.Operation;
        if (!latencyAccumulators.TryGetValue(key, out var accumulator))
        {
            accumulator = new LatencyAccumulator();
            latencyAccumulators[key] = accumulator;
        }

        accumulator.Add(latencyMs);

        Log(
            "ORDER_LATENCY " +
            $"op={pending.Operation} side={pending.Side} orderId={orderId ?? pending.OrderId ?? "-"} " +
            $"requestId={pending.RequestId?.ToString(CultureInfo.InvariantCulture) ?? "-"} " +
            $"targetPrice={pending.Price.ToString("0.########", CultureInfo.InvariantCulture)} " +
            $"ackEvent={ackEvent} ackStatus={status} latencyMs={latencyMs.ToString("0.###", CultureInfo.InvariantCulture)} " +
            $"avgMs={accumulator.AverageMs.ToString("0.###", CultureInfo.InvariantCulture)} " +
            $"minMs={accumulator.MinMs.ToString("0.###", CultureInfo.InvariantCulture)} " +
            $"maxMs={accumulator.MaxMs.ToString("0.###", CultureInfo.InvariantCulture)} " +
            $"count={accumulator.Count}",
            StrategyLoggingLevel.Trading);
    }

    private bool LatencyMatches(PendingLatencyOperation pending, Side side, string? orderId, double price, string? comment, OrderStatus status, bool removed)
    {
        if (pending.Side != side)
            return false;

        bool idMatches = !string.IsNullOrWhiteSpace(pending.OrderId)
            && !string.IsNullOrWhiteSpace(orderId)
            && string.Equals(pending.OrderId, orderId, StringComparison.Ordinal);

        bool commentMatches = !string.IsNullOrWhiteSpace(pending.Comment)
            && !string.IsNullOrWhiteSpace(comment)
            && string.Equals(pending.Comment, comment, StringComparison.Ordinal);

        if (!idMatches && !commentMatches)
            return false;

        return pending.Operation switch
        {
            "Place" => status is OrderStatus.Opened or OrderStatus.PartiallyFilled
                && Math.Abs(pending.Price - price) <= CurrentSymbol.TickSize / 2.0,
            "Modify" => status is OrderStatus.Opened or OrderStatus.PartiallyFilled
                && Math.Abs(pending.Price - price) <= CurrentSymbol.TickSize / 2.0,
            "Cancel" => removed || status == OrderStatus.Cancelled,
            _ => false
        };
    }

    private bool IsPotentialOwnedOrder(Order order)
        => order.Symbol == CurrentSymbol
            && order.Account == CurrentAccount
            && (IsOwnedComment(order.Comment)
                || MatchesOrderId(order, bidOrderId)
                || MatchesOrderId(order, askOrderId)
                || HasPendingLatencyFor(order.Side, OrderIdentity(order), order.Comment));

    private bool IsPotentialOwnedOrder(OrderHistory history)
        => history.Symbol == CurrentSymbol
            && history.Account == CurrentAccount
            && (IsOwnedComment(history.Comment)
                || MatchesOrderId(history, bidOrderId)
                || MatchesOrderId(history, askOrderId)
                || HasPendingLatencyFor(history.Side, OrderIdentity(history), history.Comment));

    private bool HasPendingLatencyFor(Side side, string? orderId, string? comment)
    {
        foreach (var pending in pendingLatencyOperations)
        {
            if (pending.Side != side)
                continue;

            if (!string.IsNullOrWhiteSpace(pending.OrderId)
                && !string.IsNullOrWhiteSpace(orderId)
                && string.Equals(pending.OrderId, orderId, StringComparison.Ordinal))
                return true;

            if (!string.IsNullOrWhiteSpace(pending.Comment)
                && !string.IsNullOrWhiteSpace(comment)
                && string.Equals(pending.Comment, comment, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private IEnumerable<Order> GetOpenOrdersForSymbolAccount()
    {
        return Core.Instance.Orders.Where(o => o.Symbol == CurrentSymbol && o.Account == CurrentAccount && IsOpen(o));
    }

    private bool HasOpenPosition()
    {
        return Core.Instance.Positions.Any(p => p.Symbol == CurrentSymbol && p.Account == CurrentAccount && Math.Abs(p.Quantity) > 0);
    }

    private static bool IsOpen(Order order)
        => order.Status is OrderStatus.Opened or OrderStatus.PartiallyFilled;

    private bool AllowCancelReplace()
    {
        var now = Core.Instance.TimeUtils.DateTimeUtcNow;
        while (cancelReplaceActions.Count > 0 && now - cancelReplaceActions.Peek() > TimeSpan.FromMinutes(1))
            cancelReplaceActions.Dequeue();

        if (cancelReplaceActions.Count >= MaxCancelReplacePerMinute)
        {
            Log("Cancel/replace rate limit reached; skipping reprice.", StrategyLoggingLevel.Trading);
            return false;
        }

        return true;
    }

    private void RegisterCancelReplaceAction()
    {
        var now = Core.Instance.TimeUtils.DateTimeUtcNow;
        cancelReplaceActions.Enqueue(now);
    }

    private void Halt(string reason)
    {
        halted = true;
        haltReason = reason;
        Log($"HALTED: {reason}", StrategyLoggingLevel.Error);
        Stop();
    }

    private OnePairGridQuoteConfig BuildConfig()
        => new()
        {
            TargetWidthTicks = TargetPairWidthTicks,
            WidthToleranceTicks = WidthToleranceTicks,
            MinPassiveOffsetTicks = MinPassiveOffsetTicks,
            RepriceThresholdTicks = RepriceThresholdTicks
        };

    private static long PriceToTicks(double price, double tickSize)
        => (long)Math.Round(price / tickSize, MidpointRounding.AwayFromZero);

    private static double TicksToPrice(long ticks, double tickSize)
        => ticks * tickSize;

    private readonly record struct PlacedOrderRef(Order? Order, string? OrderId);

    private readonly record struct OrderSubscription(Order Order, Action<IOrder> Handler);

    private readonly record struct PendingLatencyOperation(
        long Id,
        string Operation,
        Side Side,
        string? OrderId,
        double Price,
        string Comment,
        long StartTimestamp,
        DateTime StartUtc,
        long? RequestId,
        string? BrokerOrderId);

    private sealed class LatencyAccumulator
    {
        public long Count { get; private set; }
        public double SumMs { get; private set; }
        public double MinMs { get; private set; } = double.PositiveInfinity;
        public double MaxMs { get; private set; }
        public double AverageMs => Count == 0 ? 0 : SumMs / Count;

        public void Add(double latencyMs)
        {
            Count++;
            SumMs += latencyMs;
            MinMs = Math.Min(MinMs, latencyMs);
            MaxMs = Math.Max(MaxMs, latencyMs);
        }
    }
}
