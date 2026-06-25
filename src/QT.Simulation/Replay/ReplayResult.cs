using QT.Features.Engine;

namespace QT.Simulation.Replay;

public sealed record ReplayResult(
    string SessionId,
    int EventCount,
    IReadOnlyList<FeatureSnapshot> FeatureSnapshots,
    TimeSpan Elapsed,
    double EventsPerSecond);
