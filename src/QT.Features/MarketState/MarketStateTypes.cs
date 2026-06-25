using QT.Core.Quality;

namespace QT.Features.MarketState;

public enum MarketRegime : byte
{
    Unknown = 0,
    QuietTight = 1,
    ActiveLiquid = 2,
    FastOrderly = 3,
    ThinFragile = 4,
    VolatileDislocated = 5,
    Recovering = 6
}

public enum RiskEnvironment : byte
{
    Unknown = 0,
    Low = 1,
    Normal = 2,
    Elevated = 3,
    Critical = 4
}

public sealed class MarketStateConfig
{
    public TimeSpan WarmupDuration { get; init; } = TimeSpan.FromSeconds(10);
    public TimeSpan SpreadMeanWindow { get; init; } = TimeSpan.FromSeconds(30);
    public TimeSpan SpreadDistributionWindow { get; init; } = TimeSpan.FromSeconds(60);
    public TimeSpan ActivityWindow { get; init; } = TimeSpan.FromSeconds(30);
    public TimeSpan BurstWindow { get; init; } = TimeSpan.FromSeconds(5);
    public TimeSpan VolShortWindow { get; init; } = TimeSpan.FromSeconds(5);
    public TimeSpan VolLongWindow { get; init; } = TimeSpan.FromSeconds(30);
    public TimeSpan CandidatePersistence { get; init; } = TimeSpan.FromSeconds(1);
    public TimeSpan MinimumDwell { get; init; } = TimeSpan.FromSeconds(2);
    public long OneTickSpread { get; init; } = 1;
    public long EmergencySpreadTicks { get; init; } = 8;

    public void Validate()
    {
        if (WarmupDuration < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(WarmupDuration));
        if (SpreadMeanWindow <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(SpreadMeanWindow));
        if (SpreadDistributionWindow <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(SpreadDistributionWindow));
        if (ActivityWindow <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(ActivityWindow));
        if (BurstWindow <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(BurstWindow));
        if (VolShortWindow <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(VolShortWindow));
        if (VolLongWindow <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(VolLongWindow));
        if (CandidatePersistence < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(CandidatePersistence));
        if (MinimumDwell < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(MinimumDwell));
    }
}

public sealed record SpreadEpisodeSnapshot(
    bool IsActive,
    DateTime? StartedUtc,
    TimeSpan Age,
    long StartSpreadTicks,
    long PeakSpreadTicks,
    TimeSpan LastRecoveryDuration);

public sealed record MarketStateTransition(
    DateTime EventTimeUtc,
    long BookEpoch,
    MarketRegime Previous,
    MarketRegime Current,
    string Reason,
    double ActivityScore,
    double VolatilityScore,
    double LiquidityStressScore);

public sealed record MarketStateSnapshot(
    DateTime EventTimeUtc,
    long BookEpoch,
    MarketRegime Regime,
    RiskEnvironment Risk,
    MetricQuality Quality,
    string QualityReason,
    double ActivityScore,
    double VolatilityScore,
    double LiquidityStressScore,
    double RegimeConfidence,
    MarketRegime PreviousRegime,
    DateTime? LastTransitionUtc,
    TimeSpan TransitionAge,
    string TransitionReason,
    long CurrentSpreadTicks,
    double TimeWeightedMeanSpread30s,
    double SpreadP90Ticks60s,
    long MaxSpreadTicks60s,
    double TimeAtOneTick60s,
    double WidenRatePerSec,
    double NarrowRatePerSec,
    SpreadEpisodeSnapshot SpreadEpisode,
    double TouchDepthRatio,
    double Top5DepthRatio,
    double CancellationPressure,
    double BboChangesPerSec,
    double BookEventsPerSec,
    double BookEventBurstRatio,
    double TradesPerSec,
    double VolumePerSec,
    double TradeBurstRatio,
    double MidRealizedVol5s,
    double MidRealizedVol30s,
    long MidRangeTicks5s,
    long MidRangeTicks30s,
    double MultiTickJumpRate,
    IReadOnlyList<MarketStateTransition> RecentTransitions)
{
    public static MarketStateSnapshot Empty(DateTime eventTimeUtc, long epoch, MetricQuality quality, string reason)
        => new(eventTimeUtc, epoch, MarketRegime.Unknown, RiskEnvironment.Unknown, quality, reason, 0, 0, 0,
            0, MarketRegime.Unknown, null, TimeSpan.Zero, reason, 0, 0, 0, 0, 0, 0, 0,
            new SpreadEpisodeSnapshot(false, null, TimeSpan.Zero, 0, 0, TimeSpan.Zero),
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, Array.Empty<MarketStateTransition>());
}
