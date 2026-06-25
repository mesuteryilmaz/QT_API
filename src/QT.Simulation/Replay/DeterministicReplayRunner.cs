using System.Diagnostics;
using QT.Market.Events;
using QT.Runtime.AnalyticsRuntime;

namespace QT.Simulation.Replay;

public sealed class DeterministicReplayRunner
{
    private readonly AnalyticsRuntimeConfig config;

    public DeterministicReplayRunner(AnalyticsRuntimeConfig config)
    {
        this.config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public ReplayResult Run(IEnumerable<NormalizedMarketEvent> events)
    {
        var ordered = events.OrderBy(e => e.LocalSequence).ToArray();
        var runtime = new AnalyticsRuntime(config);
        var snapshots = new List<QT.Features.Engine.FeatureSnapshot>();
        var sw = Stopwatch.StartNew();

        DateTime start = ordered.Length > 0 ? ordered[0].EventTimeUtc : DateTime.UtcNow;
        runtime.Start(start);
        foreach (var evt in ordered)
        {
            var snapshot = runtime.OnMarketEvent(evt);
            snapshots.Add(snapshot.Features);
        }
        runtime.Stop();

        sw.Stop();
        double eps = ordered.Length / Math.Max(0.001, sw.Elapsed.TotalSeconds);
        return new ReplayResult(config.RuntimeSessionId, ordered.Length, snapshots, sw.Elapsed, eps);
    }
}
