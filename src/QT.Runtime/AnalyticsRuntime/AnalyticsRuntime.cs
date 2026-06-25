using System.Diagnostics;
using QT.Core.Diagnostics;
using QT.Core.Primitives;
using QT.Features.Engine;
using QT.Market.Events;
using QT.Market.Lifecycle;
using QT.Market.Snapshots;
using QT.Runtime.Health;
using QT.Storage.Features;

namespace QT.Runtime.AnalyticsRuntime;

public sealed class AnalyticsRuntime : IAnalyticsRuntime
{
    private readonly object gate = new();
    private readonly AnalyticsRuntimeConfig config;
    private readonly string configHash;
    private readonly OrderBookEngine bookEngine;
    private readonly FeatureEngine featureEngine;
    private readonly IAnalyticsRecorder recorder;
    private readonly List<RuntimeDiagnostic> diagnostics = new();
    private AnalyticsRuntimeSnapshot? current;
    private bool running;
    private long processedEvents;
    private long rejectedEvents;
    private TimeSpan lastProcessingLatency;
    private TimeSpan lastFeatureDuration;
    private TimeSpan lastSnapshotDuration;
    private DateTime lastFeaturePublishUtc = DateTime.MinValue;

    public AnalyticsRuntime(AnalyticsRuntimeConfig config, IAnalyticsRecorder? recorder = null)
    {
        this.config = config ?? throw new ArgumentNullException(nameof(config));
        this.config.Validate();
        configHash = config.StableHash();
        bookEngine = new OrderBookEngine(config.Book.With(config.Symbol, config.PreferredBookMode));
        featureEngine = new FeatureEngine(config.Features);
        this.recorder = recorder ?? (config.Recorder.Enabled ? new CsvAnalyticsRecorder(config.Recorder) : new NullAnalyticsRecorder());
    }

    public AnalyticsRuntimeSnapshot Current
    {
        get
        {
            lock (gate)
                return current ?? BuildCurrent(BookSnapshot.Empty(config.Symbol, DateTime.UtcNow, 0, config.PreferredBookMode, BookLifecycleState.Disconnected, "not started"));
        }
    }

    public void Start(DateTime nowUtc)
    {
        lock (gate)
        {
            running = true;
            var book = bookEngine.StartSubscription(config.Symbol, config.PreferredBookMode, nowUtc);
            current = Publish(book);
            lastFeaturePublishUtc = nowUtc;
            AddDiagnostic(nowUtc, DiagnosticSeverity.Info, "runtime", "start", "analytics runtime started", book.BookEpoch);
        }
    }

    public void Stop()
    {
        lock (gate)
        {
            running = false;
            recorder.Flush();
            recorder.Dispose();
        }
    }

    public void BeginSnapshot(DateTime eventTimeUtc)
        => BeginSnapshot(eventTimeUtc, config.PreferredBookMode);

    public void BeginSnapshot(DateTime eventTimeUtc, BookMode mode)
    {
        lock (gate)
        {
            var book = bookEngine.BeginSnapshot(mode, eventTimeUtc, "snapshot begin");
            current = Publish(book);
            lastFeaturePublishUtc = eventTimeUtc;
        }
    }

    public void ApplySnapshotLevel(DateTime eventTimeUtc, BookSide side, long priceTicks, long quantity, string? orderId = null, long priority = 0, int numberOrders = 0)
    {
        lock (gate)
            bookEngine.ApplySnapshotLevel(eventTimeUtc, side, priceTicks, quantity, orderId, priority, numberOrders);
    }

    public AnalyticsRuntimeSnapshot EndSnapshot(DateTime eventTimeUtc, DateTime receiveTimeUtc)
    {
        lock (gate)
        {
            var sw = Stopwatch.StartNew();
            var book = bookEngine.EndSnapshot(eventTimeUtc, receiveTimeUtc);
            sw.Stop();
            lastSnapshotDuration = sw.Elapsed;
            current = Publish(book);
            lastFeaturePublishUtc = receiveTimeUtc;
            return current;
        }
    }

    public AnalyticsRuntimeSnapshot OnMarketEvent(in NormalizedMarketEvent marketEvent)
    {
        lock (gate)
        {
            var sw = Stopwatch.StartNew();
            processedEvents++;
            recorder.RecordRawEvent(marketEvent, config.RuntimeSessionId, config.SourceCommit, config.BuildConfiguration, configHash);

            featureEngine.OnMarketEvent(marketEvent);
            var result = bookEngine.ApplyMarketEvent(marketEvent);
            rejectedEvents = result.RejectedEventCount;
            sw.Stop();
            lastProcessingLatency = sw.Elapsed;

            if (result.ForcePublish || ShouldPublish(marketEvent.ReceiveTimeUtc))
            {
                var book = bookEngine.Snapshot(marketEvent.ReceiveTimeUtc);
                current = Publish(book);
                lastFeaturePublishUtc = marketEvent.ReceiveTimeUtc;
            }

            return current ?? BuildCurrent(bookEngine.Snapshot(marketEvent.ReceiveTimeUtc));
        }
    }

    public AnalyticsRuntimeSnapshot AdvanceTime(DateTime eventTimeUtc)
    {
        lock (gate)
        {
            var book = bookEngine.Snapshot(eventTimeUtc);
            current = Publish(book);
            lastFeaturePublishUtc = eventTimeUtc;
            return current;
        }
    }

    public AnalyticsRuntimeSnapshot MarkBufferOverflow(DateTime eventTimeUtc, string reason)
    {
        lock (gate)
        {
            var book = bookEngine.MarkBufferOverflow(eventTimeUtc, reason);
            AddDiagnostic(eventTimeUtc, DiagnosticSeverity.Critical, "book", "buffer_overflow", reason, book.BookEpoch);
            current = Publish(book);
            lastFeaturePublishUtc = eventTimeUtc;
            return current;
        }
    }

    public AnalyticsRuntimeSnapshot MarkReconnect(DateTime eventTimeUtc, string reason)
    {
        lock (gate)
        {
            var book = bookEngine.MarkReconnect(eventTimeUtc, reason);
            AddDiagnostic(eventTimeUtc, DiagnosticSeverity.Warning, "book", "reconnect", reason, book.BookEpoch);
            current = Publish(book);
            lastFeaturePublishUtc = eventTimeUtc;
            return current;
        }
    }

    public AnalyticsRuntimeSnapshot DowngradeToMbp(DateTime eventTimeUtc, string reason)
    {
        lock (gate)
        {
            var book = bookEngine.DowngradeToMbp(eventTimeUtc, reason);
            AddDiagnostic(eventTimeUtc, DiagnosticSeverity.Warning, "book", "mode_downgrade", reason, book.BookEpoch);
            current = Publish(book);
            lastFeaturePublishUtc = eventTimeUtc;
            return current;
        }
    }

    private bool ShouldPublish(DateTime receiveTimeUtc)
        => current == null ||
           lastFeaturePublishUtc == DateTime.MinValue ||
           receiveTimeUtc - lastFeaturePublishUtc >= config.FeatureCadence;

    private AnalyticsRuntimeSnapshot Publish(BookSnapshot book)
    {
        var sw = Stopwatch.StartNew();
        var features = featureEngine.OnBookSnapshot(book, config.RuntimeSessionId, configHash);
        sw.Stop();
        lastFeatureDuration = sw.Elapsed;

        recorder.RecordFeatureSnapshot(features, config.SourceCommit, config.BuildConfiguration);
        foreach (var tr in featureEngine.DrainRegimeTransitions())
            recorder.RecordRegimeTransition(tr, config.RuntimeSessionId, config.Symbol, config.SourceCommit, config.BuildConfiguration, configHash);
        foreach (var br in featureEngine.DrainPairBreaks())
            recorder.RecordFloatingPairBreak(br, config.RuntimeSessionId, config.Symbol, config.SourceCommit, config.BuildConfiguration, configHash);

        return BuildCurrent(book, features);
    }

    private AnalyticsRuntimeSnapshot BuildCurrent(BookSnapshot book)
        => BuildCurrent(book, FeatureSnapshot.Empty(config.RuntimeSessionId, configHash, book));

    private AnalyticsRuntimeSnapshot BuildCurrent(BookSnapshot book, FeatureSnapshot features)
        => new(config.RuntimeSessionId, configHash, book, features,
            new RuntimeHealthSnapshot(running, 0, rejectedEvents, processedEvents, lastProcessingLatency,
                lastFeatureDuration, lastSnapshotDuration, diagnostics.ToArray()), recorder.Status);

    private void AddDiagnostic(DateTime t, DiagnosticSeverity severity, string component, string code, string message, long epoch)
    {
        var diagnostic = new RuntimeDiagnostic(t, severity, component, code, message, epoch);
        diagnostics.Add(diagnostic);
        if (diagnostics.Count > 100)
            diagnostics.RemoveAt(0);
        recorder.RecordDiagnostic(diagnostic, config.RuntimeSessionId, config.Symbol, config.SourceCommit, config.BuildConfiguration, configHash);
    }
}

internal static class OrderBookEngineConfigExtensions
{
    public static OrderBookEngineConfig With(this OrderBookEngineConfig source, string? symbol = null, BookMode? preferredMode = null)
        => new()
        {
            Symbol = symbol ?? source.Symbol,
            TickSize = source.TickSize,
            PreferredMode = preferredMode ?? source.PreferredMode,
            TopDepth = source.TopDepth,
            StaleTimeout = source.StaleTimeout,
            BufferLimit = source.BufferLimit
        };
}
