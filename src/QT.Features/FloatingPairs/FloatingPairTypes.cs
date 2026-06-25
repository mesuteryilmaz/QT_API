using QT.Core.Quality;

namespace QT.Features.FloatingPairs;

public enum FloatingPairDetectorStatus : byte
{
    Unavailable = 0,
    WarmingUp = 1,
    MboReady = 2,
    BookInvalid = 3
}

public enum FloatingPairTier : byte
{
    None = 0,
    Large = 1,
    VeryLarge = 2
}

public enum FloatingPairState : byte
{
    Candidate = 1,
    Persistent = 2,
    FloatingConfirmed = 3,
    Broken = 4
}

public sealed class FloatingPairConfig
{
    public long LargeThreshold { get; init; } = 20;
    public long VeryLargeThreshold { get; init; } = 200;
    public long OffsetToleranceTicks { get; init; } = 1;
    public long MaximumOffsetTicks { get; init; } = 250;
    public TimeSpan PersistenceTime { get; init; } = TimeSpan.FromSeconds(1);
    public TimeSpan SynchronizationWindow { get; init; } = TimeSpan.FromMilliseconds(300);
    public int RequiredCoordinatedMoves { get; init; } = 2;
    public double MinimumFollowRatio { get; init; } = 0.80;
    public TimeSpan ReplacementStitchingWindow { get; init; } = TimeSpan.FromMilliseconds(300);
    public TimeSpan MaximumInactivity { get; init; } = TimeSpan.FromSeconds(5);
    public int MaxPairsTracked { get; init; } = 32;

    public void Validate()
    {
        if (LargeThreshold <= 0) throw new ArgumentOutOfRangeException(nameof(LargeThreshold));
        if (VeryLargeThreshold < LargeThreshold) throw new ArgumentOutOfRangeException(nameof(VeryLargeThreshold));
        if (OffsetToleranceTicks < 0) throw new ArgumentOutOfRangeException(nameof(OffsetToleranceTicks));
        if (MaximumOffsetTicks < OffsetToleranceTicks) throw new ArgumentOutOfRangeException(nameof(MaximumOffsetTicks));
        if (PersistenceTime < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(PersistenceTime));
        if (SynchronizationWindow <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(SynchronizationWindow));
        if (RequiredCoordinatedMoves < 0) throw new ArgumentOutOfRangeException(nameof(RequiredCoordinatedMoves));
        if (MinimumFollowRatio < 0 || MinimumFollowRatio > 1) throw new ArgumentOutOfRangeException(nameof(MinimumFollowRatio));
        if (ReplacementStitchingWindow < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(ReplacementStitchingWindow));
        if (MaximumInactivity <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(MaximumInactivity));
    }
}

public sealed record FloatingPairSummary(
    string PairId,
    FloatingPairState State,
    FloatingPairTier Tier,
    long Size,
    string BidOrderId,
    string AskOrderId,
    long BidPriceTicks,
    long AskPriceTicks,
    long BidOffsetTicks,
    long AskOffsetTicks,
    TimeSpan Age,
    int SynchronizedMoves,
    int Opportunities,
    double FollowRatio,
    double MeanSyncDelayMs,
    double MaxSyncDelayMs,
    double Confidence,
    bool UsesHeuristicStitching);

public sealed record FloatingPairBreakEvent(
    DateTime EventTimeUtc,
    long BookEpoch,
    string PairId,
    string Reason,
    FloatingPairState PreviousState);

public sealed record FloatingPairSnapshot(
    FloatingPairDetectorStatus Status,
    MetricQuality Quality,
    string QualityReason,
    long BookEpoch,
    int EligibleLargeBidCount,
    int EligibleLargeAskCount,
    int EligibleVeryLargeBidCount,
    int EligibleVeryLargeAskCount,
    int ExactSizeCandidateCount,
    int PersistentPairCount,
    int FloatingConfirmedPairCount,
    int LargePairCount,
    int VeryLargePairCount,
    FloatingPairSummary? TopPair,
    IReadOnlyList<FloatingPairSummary> TopPairs,
    FloatingPairBreakEvent? LastBreak,
    IReadOnlyList<FloatingPairBreakEvent> RecentBreaks)
{
    public static FloatingPairSnapshot Unavailable(long epoch, FloatingPairDetectorStatus status, MetricQuality quality, string reason)
        => new(status, quality, reason, epoch, 0, 0, 0, 0, 0, 0, 0, 0, 0, null,
            Array.Empty<FloatingPairSummary>(), null, Array.Empty<FloatingPairBreakEvent>());
}
