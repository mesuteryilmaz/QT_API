using QT.Core.Diagnostics;

namespace QT.Runtime.Health;

public sealed record RuntimeHealthSnapshot(
    bool IsRunning,
    long QueueDepth,
    long DroppedOrRejectedEvents,
    long ProcessedEvents,
    TimeSpan LastProcessingLatency,
    TimeSpan LastFeatureCalculationDuration,
    TimeSpan LastSnapshotDuration,
    IReadOnlyList<RuntimeDiagnostic> RecentDiagnostics);
