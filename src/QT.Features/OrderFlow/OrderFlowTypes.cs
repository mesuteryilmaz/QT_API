using QT.Core.Quality;

namespace QT.Features.OrderFlow;

/// <summary>
/// Directional lean inferred from order flow. This is a hypothesis, not a proven edge — it must be
/// validated offline before it is trusted to originate live entries.
/// </summary>
public enum DirectionalBias : byte
{
    Neutral = 0,
    Up = 1,
    Down = 2
}

public sealed class OrderFlowConfig
{
    public TimeSpan WarmupDuration { get; init; } = TimeSpan.FromSeconds(10);
    public TimeSpan ShortWindow { get; init; } = TimeSpan.FromSeconds(5);
    public TimeSpan LongWindow { get; init; } = TimeSpan.FromSeconds(30);
    public int BookPressureDepth { get; init; } = 5;

    /// <summary>Weights of the three transparent components; normalized internally.</summary>
    public double WeightImbalance { get; init; } = 0.50;
    public double WeightBookPressure { get; init; } = 0.30;
    public double WeightCvdSlope { get; init; } = 0.20;

    /// <summary>Net signed volume/sec that maps the CVD-slope component to its full ±1 contribution.</summary>
    public double CvdSlopeScalePerSec { get; init; } = 50.0;

    /// <summary>|lean score| at or above which a direction is called (below it, Neutral).</summary>
    public double LeanThreshold { get; init; } = 0.20;

    /// <summary>|lean score| that corresponds to full (1.0) confidence.</summary>
    public double StrongLeanThreshold { get; init; } = 0.50;

    public void Validate()
    {
        if (WarmupDuration < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(WarmupDuration));
        if (ShortWindow <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(ShortWindow));
        if (LongWindow <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(LongWindow));
        if (BookPressureDepth < 1) throw new ArgumentOutOfRangeException(nameof(BookPressureDepth));
        if (WeightImbalance < 0 || WeightBookPressure < 0 || WeightCvdSlope < 0)
            throw new ArgumentOutOfRangeException(nameof(WeightImbalance));
        if (WeightImbalance + WeightBookPressure + WeightCvdSlope <= 0)
            throw new ArgumentOutOfRangeException(nameof(WeightImbalance), "weights must sum to a positive value");
        if (CvdSlopeScalePerSec <= 0) throw new ArgumentOutOfRangeException(nameof(CvdSlopeScalePerSec));
        if (LeanThreshold < 0 || LeanThreshold > 1) throw new ArgumentOutOfRangeException(nameof(LeanThreshold));
        if (StrongLeanThreshold <= 0 || StrongLeanThreshold > 1) throw new ArgumentOutOfRangeException(nameof(StrongLeanThreshold));
    }
}

public sealed record OrderFlowSnapshot(
    DateTime EventTimeUtc,
    long BookEpoch,
    MetricQuality Quality,
    string QualityReason,
    DirectionalBias Bias,
    double LeanScore,
    double Confidence,
    long SignedVolDeltaShort,
    long SignedVolDeltaLong,
    double CvdSlopePerSec,
    double TradeImbalance,
    double BookPressure,
    double BuyVolumePerSec,
    double SellVolumePerSec,
    string Reason)
{
    public static OrderFlowSnapshot Empty(DateTime eventTimeUtc, long epoch, MetricQuality quality, string reason)
        => new(eventTimeUtc, epoch, quality, reason, DirectionalBias.Neutral, 0, 0, 0, 0, 0, 0.5, 0, 0, 0, reason);
}
