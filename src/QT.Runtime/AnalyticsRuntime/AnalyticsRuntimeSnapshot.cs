using QT.Features.Engine;
using QT.Market.Snapshots;
using QT.Runtime.Health;

namespace QT.Runtime.AnalyticsRuntime;

public sealed record AnalyticsRuntimeSnapshot(
    string RuntimeSessionId,
    string ConfigurationHash,
    BookSnapshot Book,
    FeatureSnapshot Features,
    RuntimeHealthSnapshot Health,
    string RecorderStatus);
