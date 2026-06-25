using QT.Core.Primitives;
using QT.Core.Quality;
using QT.Market.Events;
using QT.Market.OrderBook;
using QT.Market.Snapshots;

namespace QT.Market.Lifecycle;

public sealed class OrderBookEngineConfig
{
    public string Symbol { get; init; } = "";
    public double TickSize { get; init; } = 0.25;
    public BookMode PreferredMode { get; init; } = BookMode.Mbo;
    public int TopDepth { get; init; } = 10;
    public TimeSpan StaleTimeout { get; init; } = TimeSpan.FromSeconds(3);
    public int BufferLimit { get; init; } = 10_000;

    public void Validate()
    {
        if (TickSize <= 0) throw new ArgumentOutOfRangeException(nameof(TickSize), "Tick size must be positive.");
        if (TopDepth <= 0) throw new ArgumentOutOfRangeException(nameof(TopDepth), "Top depth must be positive.");
        if (StaleTimeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(StaleTimeout), "Stale timeout must be positive.");
    }
}

public readonly record struct BookUpdateResult(
    long BookEpoch,
    BookLifecycleState LifecycleState,
    bool EpochChanged,
    bool LifecycleChanged,
    bool BookChanged,
    bool ForcePublish,
    long RejectedEventCount);

public sealed class OrderBookEngine
{
    private readonly OrderBookEngineConfig config;
    private readonly MboOrderBook mbo;
    private readonly MbpOrderBook mbp;

    private long epoch;
    private BookMode mode;
    private BookLifecycleState lifecycle;
    private string symbol;
    private DateTime lastEventTimeUtc;
    private DateTime lastReceiveTimeUtc;
    private string? invalidReason;

    private long snapshotSeedCount;
    private long liveBookEventCount;
    private long liveAddCount;
    private long liveUpdateCount;
    private long liveRemoveCount;
    private long liveRepriceCount;
    private long rejectedEventCount;

    public OrderBookEngine(OrderBookEngineConfig config)
    {
        this.config = config ?? throw new ArgumentNullException(nameof(config));
        this.config.Validate();
        symbol = config.Symbol;
        mode = BookMode.Unknown;
        lifecycle = BookLifecycleState.Disconnected;
        mbo = new MboOrderBook(config.TickSize);
        mbp = new MbpOrderBook(config.TickSize);
    }

    public long BookEpoch => epoch;
    public BookMode Mode => mode;
    public BookLifecycleState LifecycleState => lifecycle;

    public BookSnapshot StartSubscription(string runtimeSymbol, BookMode preferredMode, DateTime nowUtc)
    {
        symbol = string.IsNullOrWhiteSpace(runtimeSymbol) ? config.Symbol : runtimeSymbol;
        mode = preferredMode == BookMode.Unknown ? config.PreferredMode : preferredMode;
        IncrementEpoch(nowUtc, BookLifecycleState.AwaitingSnapshot, "subscription started");
        return Snapshot(nowUtc);
    }

    public BookSnapshot BeginSnapshot(BookMode snapshotMode, DateTime nowUtc, string reason)
    {
        if (snapshotMode == BookMode.Unknown)
            snapshotMode = config.PreferredMode;

        mode = snapshotMode;
        IncrementEpoch(nowUtc, BookLifecycleState.ApplyingSnapshot, reason);
        return Snapshot(nowUtc);
    }

    public void ApplySnapshotLevel(DateTime eventTimeUtc, BookSide side, long priceTicks, long quantity, string? orderId = null, long priority = 0, int numberOrders = 0)
    {
        if (mode == BookMode.Mbo)
            mbo.Apply(eventTimeUtc, orderId, side, priceTicks, quantity, priority, closed: quantity <= 0, isSnapshotSeed: true);
        else
            mbp.Apply(side, priceTicks, quantity, closed: quantity <= 0);

        snapshotSeedCount++;
        lastEventTimeUtc = eventTimeUtc;
        lastReceiveTimeUtc = eventTimeUtc;
    }

    public BookSnapshot EndSnapshot(DateTime eventTimeUtc, DateTime receiveTimeUtc)
    {
        lastEventTimeUtc = eventTimeUtc;
        lastReceiveTimeUtc = receiveTimeUtc;
        ValidateBook();
        return Snapshot(receiveTimeUtc);
    }

    public BookSnapshot OnMarketEvent(in NormalizedMarketEvent marketEvent)
    {
        ApplyMarketEvent(marketEvent);
        return Snapshot(marketEvent.ReceiveTimeUtc);
    }

    public BookUpdateResult ApplyMarketEvent(in NormalizedMarketEvent marketEvent)
    {
        long previousEpoch = epoch;
        var previousLifecycle = lifecycle;

        if (!string.IsNullOrWhiteSpace(marketEvent.Symbol))
            symbol = marketEvent.Symbol;

        lastEventTimeUtc = marketEvent.EventTimeUtc;
        lastReceiveTimeUtc = marketEvent.ReceiveTimeUtc;

        if (marketEvent.Kind == MarketEventKind.BookFlush ||
            marketEvent.BookAction == NormalizedBookAction.Flush ||
            string.Equals(marketEvent.OrderId, "FLUSH", StringComparison.OrdinalIgnoreCase))
        {
            IncrementEpoch(marketEvent.EventTimeUtc, BookLifecycleState.AwaitingSnapshot, "feed FLUSH");
            return Result(previousEpoch, previousLifecycle, bookChanged: true, forcePublish: true);
        }

        if (marketEvent.Kind == MarketEventKind.ConnectionState)
        {
            IncrementEpoch(marketEvent.EventTimeUtc, BookLifecycleState.Recovering, "connection state change");
            return Result(previousEpoch, previousLifecycle, bookChanged: true, forcePublish: true);
        }

        if (marketEvent.Kind != MarketEventKind.BookLevel && marketEvent.Kind != MarketEventKind.BookSnapshotLevel)
            return Result(previousEpoch, previousLifecycle, bookChanged: false, forcePublish: false);

        if (marketEvent.IsSnapshotSeed)
        {
            ApplySnapshotLevel(marketEvent.EventTimeUtc, marketEvent.Side, marketEvent.PriceTicks,
                marketEvent.Quantity, marketEvent.OrderId, marketEvent.Priority, marketEvent.NumberOrders);
            return Result(previousEpoch, previousLifecycle, bookChanged: true, forcePublish: false);
        }

        liveBookEventCount++;
        if (mode == BookMode.Mbo)
        {
            var mutation = mbo.Apply(marketEvent.EventTimeUtc, marketEvent.OrderId, marketEvent.Side,
                marketEvent.PriceTicks, marketEvent.Quantity, marketEvent.Priority, marketEvent.Closed, false);
            if (!mutation.Accepted)
            {
                rejectedEventCount++;
            }
            else
            {
                CountMutation(mutation.Action);
                if (mutation.IsReprice) liveRepriceCount++;
            }
        }
        else
        {
            if (marketEvent.Side is not (BookSide.Bid or BookSide.Ask) || marketEvent.PriceTicks <= 0)
            {
                rejectedEventCount++;
            }
            else
            {
                bool wasRemove = marketEvent.Closed || marketEvent.Quantity <= 0;
                mbp.Apply(marketEvent.Side, marketEvent.PriceTicks, marketEvent.Quantity, wasRemove);
                CountMutation(wasRemove ? NormalizedBookAction.Remove : NormalizedBookAction.Update);
            }
        }

        ValidateBook();
        return Result(previousEpoch, previousLifecycle, bookChanged: true, forcePublish: false);
    }

    public BookSnapshot DowngradeToMbp(DateTime nowUtc, string reason)
    {
        mode = BookMode.Mbp;
        IncrementEpoch(nowUtc, BookLifecycleState.DowngradedToMbp, reason);
        return Snapshot(nowUtc);
    }

    public BookSnapshot MarkBufferOverflow(DateTime nowUtc, string reason)
    {
        rejectedEventCount++;
        IncrementEpoch(nowUtc, BookLifecycleState.Invalid, reason);
        return Snapshot(nowUtc);
    }

    public BookSnapshot MarkReconnect(DateTime nowUtc, string reason)
    {
        IncrementEpoch(nowUtc, BookLifecycleState.Recovering, reason);
        return Snapshot(nowUtc);
    }

    public BookSnapshot Snapshot(DateTime nowUtc)
    {
        bool hasBid = TryBestBid(out long bidTicks, out long bidQty);
        bool hasAsk = TryBestAsk(out long askTicks, out long askQty);
        _ = bidQty;
        _ = askQty;
        bool locked = hasBid && hasAsk && bidTicks == askTicks;
        bool crossed = hasBid && hasAsk && bidTicks > askTicks;
        TimeSpan age = lastReceiveTimeUtc == default ? TimeSpan.MaxValue : nowUtc - lastReceiveTimeUtc;
        MetricQuality quality = QualityFor(age, crossed);
        var state = quality == MetricQuality.Stale && lifecycle == BookLifecycleState.Valid
            ? BookLifecycleState.Valid
            : lifecycle;

        return new BookSnapshot(
            symbol,
            lastEventTimeUtc == default ? nowUtc : lastEventTimeUtc,
            lastReceiveTimeUtc == default ? nowUtc : lastReceiveTimeUtc,
            epoch,
            mode,
            state,
            quality,
            hasBid,
            hasAsk,
            mode == BookMode.Mbo ? mbo.RealIdCoverage : 0.0,
            age < TimeSpan.Zero ? TimeSpan.Zero : age,
            locked,
            crossed,
            hasBid ? bidTicks : null,
            hasAsk ? askTicks : null,
            hasBid ? bidTicks * config.TickSize : null,
            hasAsk ? askTicks * config.TickSize : null,
            mode == BookMode.Mbo ? mbo.Top(BookSide.Bid, config.TopDepth) : mbp.Top(BookSide.Bid, config.TopDepth),
            mode == BookMode.Mbo ? mbo.Top(BookSide.Ask, config.TopDepth) : mbp.Top(BookSide.Ask, config.TopDepth),
            mode == BookMode.Mbo ? mbo.Orders() : Array.Empty<MboOrderSnapshot>(),
            new BookEventStats(snapshotSeedCount, liveBookEventCount, liveAddCount, liveUpdateCount,
                liveRemoveCount, liveRepriceCount, rejectedEventCount),
            invalidReason);
    }

    private void IncrementEpoch(DateTime nowUtc, BookLifecycleState state, string reason)
    {
        epoch++;
        lifecycle = state;
        invalidReason = reason;
        mbo.Clear();
        mbp.Clear();
        snapshotSeedCount = 0;
        liveBookEventCount = 0;
        liveAddCount = 0;
        liveUpdateCount = 0;
        liveRemoveCount = 0;
        liveRepriceCount = 0;
        lastEventTimeUtc = nowUtc;
        lastReceiveTimeUtc = nowUtc;
    }

    private void ValidateBook()
    {
        bool hasBid = TryBestBid(out long bid, out _);
        bool hasAsk = TryBestAsk(out long ask, out _);

        if (!hasBid && !hasAsk)
        {
            lifecycle = BookLifecycleState.Invalid;
            invalidReason = "empty book";
            return;
        }

        if (!hasBid || !hasAsk)
        {
            lifecycle = BookLifecycleState.Invalid;
            invalidReason = hasBid ? "missing ask side" : "missing bid side";
            return;
        }

        if (bid > ask)
        {
            lifecycle = BookLifecycleState.Invalid;
            invalidReason = "crossed book";
            return;
        }

        lifecycle = BookLifecycleState.Valid;
        invalidReason = bid == ask ? "locked book" : null;
    }

    private MetricQuality QualityFor(TimeSpan age, bool crossed)
    {
        if (crossed || lifecycle == BookLifecycleState.Invalid)
            return MetricQuality.Invalid;
        if (age > config.StaleTimeout)
            return MetricQuality.Stale;
        if (lifecycle is BookLifecycleState.AwaitingSnapshot or BookLifecycleState.ApplyingSnapshot or BookLifecycleState.Recovering)
            return MetricQuality.WarmingUp;
        return mode == BookMode.Mbo && mbo.RealIdCoverage >= 0.5 ? MetricQuality.Exact : MetricQuality.Derived;
    }

    private bool TryBestBid(out long ticks, out long quantity)
        => mode == BookMode.Mbo ? mbo.TryBestBid(out ticks, out quantity) : mbp.TryBestBid(out ticks, out quantity);

    private bool TryBestAsk(out long ticks, out long quantity)
        => mode == BookMode.Mbo ? mbo.TryBestAsk(out ticks, out quantity) : mbp.TryBestAsk(out ticks, out quantity);

    private BookUpdateResult Result(long previousEpoch, BookLifecycleState previousLifecycle, bool bookChanged, bool forcePublish)
    {
        bool epochChanged = previousEpoch != epoch;
        bool lifecycleChanged = previousLifecycle != lifecycle;
        return new BookUpdateResult(epoch, lifecycle, epochChanged, lifecycleChanged, bookChanged,
            forcePublish || epochChanged || lifecycleChanged, rejectedEventCount);
    }

    private void CountMutation(NormalizedBookAction action)
    {
        switch (action)
        {
            case NormalizedBookAction.Add:
                liveAddCount++;
                break;
            case NormalizedBookAction.Remove:
                liveRemoveCount++;
                break;
            case NormalizedBookAction.Update:
                liveUpdateCount++;
                break;
        }
    }
}
