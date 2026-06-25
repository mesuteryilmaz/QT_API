using System.Security.Cryptography;
using System.Text;
using QT.Core.Primitives;
using QT.Features.Engine;
using QT.Market.Lifecycle;
using QT.Storage.Schemas;

namespace QT.Runtime.AnalyticsRuntime;

public sealed class AnalyticsRuntimeConfig
{
    public string Symbol { get; init; } = "";
    public string RuntimeSessionId { get; init; } = Guid.NewGuid().ToString("N");
    public string SourceCommit { get; init; } = "unknown";
    public string BuildConfiguration { get; init; } = "unknown";
    public BookMode PreferredBookMode { get; init; } = BookMode.Mbo;
    public OrderBookEngineConfig Book { get; init; } = new();
    public FeatureEngineConfig Features { get; init; } = new();
    public RecorderConfig Recorder { get; init; } = new();
    public TimeSpan FeatureCadence { get; init; } = TimeSpan.FromMilliseconds(250);

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Symbol)) throw new ArgumentException("Symbol is required.", nameof(Symbol));
        if (FeatureCadence <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(FeatureCadence));
        Book.Validate();
        Recorder.Validate();
    }

    public string StableHash()
    {
        string payload = string.Join("|",
            Symbol,
            PreferredBookMode,
            Book.TickSize.ToString("G17", System.Globalization.CultureInfo.InvariantCulture),
            Book.TopDepth,
            Book.StaleTimeout.Ticks,
            Features.MarketState.WarmupDuration.Ticks,
            Features.MarketState.CandidatePersistence.Ticks,
            Features.MarketState.MinimumDwell.Ticks,
            Features.FloatingPairs.LargeThreshold,
            Features.FloatingPairs.VeryLargeThreshold,
            Features.FloatingPairs.OffsetToleranceTicks,
            Features.FloatingPairs.MaximumOffsetTicks,
            Features.FloatingPairs.PersistenceTime.Ticks,
            Features.FloatingPairs.SynchronizationWindow.Ticks,
            Features.FloatingPairs.RequiredCoordinatedMoves,
            Features.FloatingPairs.MinimumFollowRatio.ToString("G17", System.Globalization.CultureInfo.InvariantCulture),
            Features.FloatingPairs.ReplacementStitchingWindow.Ticks,
            Features.FloatingPairs.MaximumInactivity.Ticks,
            RecorderConfig.CurrentSchemaVersion);
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
    }
}
