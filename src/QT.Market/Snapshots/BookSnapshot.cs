using QT.Core.Primitives;
using QT.Core.Quality;

namespace QT.Market.Snapshots;

public readonly record struct BookLevelSnapshot(
    BookSide Side,
    long PriceTicks,
    double Price,
    long Quantity,
    int OrderCount);

public readonly record struct MboOrderSnapshot(
    string OrderId,
    BookSide Side,
    long PriceTicks,
    double Price,
    long Quantity,
    long Priority,
    DateTime FirstSeenUtc,
    DateTime LastUpdateUtc,
    bool HasRealOrderId,
    bool WasHeuristicallyStitched);

public readonly record struct BookEventStats(
    long SnapshotSeedEventCount,
    long LiveBookEventCount,
    long LiveAddCount,
    long LiveUpdateCount,
    long LiveRemoveCount,
    long LiveRepriceCount,
    long RejectedEventCount);

public sealed record BookSnapshot(
    string Symbol,
    DateTime EventTimeUtc,
    DateTime ReceiveTimeUtc,
    long BookEpoch,
    BookMode Mode,
    BookLifecycleState LifecycleState,
    MetricQuality DataQuality,
    bool BidSideValid,
    bool AskSideValid,
    double MboIdentityCompleteness,
    TimeSpan LastEventAge,
    bool IsLocked,
    bool IsCrossed,
    long? BestBidTicks,
    long? BestAskTicks,
    double? BestBidPrice,
    double? BestAskPrice,
    IReadOnlyList<BookLevelSnapshot> Bids,
    IReadOnlyList<BookLevelSnapshot> Asks,
    IReadOnlyList<MboOrderSnapshot> MboOrders,
    BookEventStats Stats,
    string? InvalidReason)
{
    public bool HasTwoSidedBook => BidSideValid && AskSideValid && BestBidTicks.HasValue && BestAskTicks.HasValue;
    public long SpreadTicks => HasTwoSidedBook ? Math.Max(0, BestAskTicks!.Value - BestBidTicks!.Value) : 0;

    public static BookSnapshot Empty(string symbol, DateTime nowUtc, long epoch, BookMode mode, BookLifecycleState state, string? reason)
        => new(symbol, nowUtc, nowUtc, epoch, mode, state, MetricQuality.Unavailable, false, false, 0,
            TimeSpan.Zero, false, false, null, null, null, null, Array.Empty<BookLevelSnapshot>(),
            Array.Empty<BookLevelSnapshot>(), Array.Empty<MboOrderSnapshot>(), default, reason);
}
