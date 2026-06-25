using System.Globalization;
using System.Text;
using QT.Core.Diagnostics;
using QT.Core.Quality;
using QT.Features.Engine;
using QT.Features.FloatingPairs;
using QT.Features.MarketState;
using QT.Market.Events;
using QT.Storage.Schemas;

namespace QT.Storage.Features;

public sealed class CsvAnalyticsRecorder : IAnalyticsRecorder
{
    private readonly RecorderConfig config;
    private readonly StreamWriter? raw;
    private readonly StreamWriter? features;
    private readonly StreamWriter? transitions;
    private readonly StreamWriter? pairBreaks;
    private readonly StreamWriter? diagnostics;
    private DateTime lastFlushUtc = DateTime.UtcNow;

    public CsvAnalyticsRecorder(RecorderConfig config)
    {
        this.config = config ?? throw new ArgumentNullException(nameof(config));
        this.config.Validate();
        if (!config.Enabled)
        {
            Status = "disabled";
            return;
        }

        Directory.CreateDirectory(config.OutputPath);
        if (config.RawEvents)
            raw = Open("raw_events_v2.csv", RawHeader);
        if (config.FeatureSnapshots)
            features = Open("feature_snapshots_v2.csv", FeatureHeader);
        if (config.Transitions)
        {
            transitions = Open("regime_transitions_v2.csv", TransitionHeader);
            pairBreaks = Open("floating_pair_events_v2.csv", PairBreakHeader);
        }
        if (config.Diagnostics)
            diagnostics = Open("diagnostics_v2.csv", DiagnosticHeader);

        Status = "active";
    }

    public bool Enabled => config.Enabled;
    public string Status { get; private set; }

    public void RecordRawEvent(in NormalizedMarketEvent evt, string sessionId, string sourceCommit, string buildConfiguration, string configHash)
    {
        if (raw == null) return;
        raw.WriteLine(Csv(
            RecorderConfig.CurrentSchemaVersion,
            sourceCommit,
            buildConfiguration,
            sessionId,
            configHash,
            evt.Symbol,
            FormatTime(evt.EventTimeUtc),
            FormatTime(evt.ReceiveTimeUtc),
            evt.BookEpoch,
            evt.Kind,
            evt.BookAction,
            evt.Quality,
            evt.Side,
            evt.PriceTicks,
            evt.Price,
            evt.Quantity,
            evt.OrderId ?? "",
            evt.Closed,
            evt.Aggressor,
            evt.Priority,
            evt.NumberOrders,
            evt.IsSnapshotSeed));
        FlushIfDue();
    }

    public void RecordFeatureSnapshot(FeatureSnapshot snapshot, string sourceCommit, string buildConfiguration)
    {
        if (features == null) return;
        foreach (var kv in snapshot.Values.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            FeatureValue v = kv.Value;
            string numeric = v.IsNumeric ? v.Value.ToString("G17", CultureInfo.InvariantCulture) : "";
            features.WriteLine(Csv(
                RecorderConfig.CurrentSchemaVersion,
                sourceCommit,
                buildConfiguration,
                snapshot.RuntimeSessionId,
                snapshot.ConfigurationHash,
                snapshot.Symbol,
                FormatTime(snapshot.EventTimeUtc),
                FormatTime(snapshot.ReceiveTimeUtc),
                snapshot.Book.BookEpoch,
                snapshot.Book.Mode,
                snapshot.Book.LifecycleState,
                snapshot.Book.DataQuality,
                kv.Key,
                v.DisplayName,
                numeric,
                v.Quality,
                v.Unit,
                v.Reason ?? ""));
        }
        FlushIfDue();
    }

    public void RecordRegimeTransition(MarketStateTransition transition, string sessionId, string symbol, string sourceCommit, string buildConfiguration, string configHash)
    {
        if (transitions == null) return;
        transitions.WriteLine(Csv(
            RecorderConfig.CurrentSchemaVersion,
            sourceCommit,
            buildConfiguration,
            sessionId,
            configHash,
            symbol,
            FormatTime(transition.EventTimeUtc),
            transition.BookEpoch,
            transition.Previous,
            transition.Current,
            transition.Reason,
            transition.ActivityScore,
            transition.VolatilityScore,
            transition.LiquidityStressScore));
        FlushIfDue();
    }

    public void RecordFloatingPairBreak(FloatingPairBreakEvent breakEvent, string sessionId, string symbol, string sourceCommit, string buildConfiguration, string configHash)
    {
        if (pairBreaks == null) return;
        pairBreaks.WriteLine(Csv(
            RecorderConfig.CurrentSchemaVersion,
            sourceCommit,
            buildConfiguration,
            sessionId,
            configHash,
            symbol,
            FormatTime(breakEvent.EventTimeUtc),
            breakEvent.BookEpoch,
            breakEvent.PairId,
            breakEvent.PreviousState,
            breakEvent.Reason));
        FlushIfDue();
    }

    public void RecordDiagnostic(RuntimeDiagnostic diagnostic, string sessionId, string symbol, string sourceCommit, string buildConfiguration, string configHash)
    {
        if (diagnostics == null) return;
        diagnostics.WriteLine(Csv(
            RecorderConfig.CurrentSchemaVersion,
            sourceCommit,
            buildConfiguration,
            sessionId,
            configHash,
            symbol,
            FormatTime(diagnostic.TimeUtc),
            diagnostic.BookEpoch,
            diagnostic.Severity,
            diagnostic.Component,
            diagnostic.Code,
            diagnostic.Message));
        FlushIfDue();
    }

    public void Flush()
    {
        raw?.Flush();
        features?.Flush();
        transitions?.Flush();
        pairBreaks?.Flush();
        diagnostics?.Flush();
        lastFlushUtc = DateTime.UtcNow;
    }

    public void Dispose()
    {
        Flush();
        raw?.Dispose();
        features?.Dispose();
        transitions?.Dispose();
        pairBreaks?.Dispose();
        diagnostics?.Dispose();
    }

    private StreamWriter Open(string fileName, string header)
    {
        string path = Path.Combine(config.OutputPath, fileName);
        bool exists = File.Exists(path);
        var writer = new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read), new UTF8Encoding(false));
        if (!exists || new FileInfo(path).Length == 0)
            writer.WriteLine(header);
        return writer;
    }

    private void FlushIfDue()
    {
        if (DateTime.UtcNow - lastFlushUtc >= config.FlushInterval)
            Flush();
    }

    private static string FormatTime(DateTime utc)
        => utc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

    private static string Csv(params object?[] values)
        => string.Join(",", values.Select(Escape));

    private static string Escape(object? value)
    {
        string s = value switch
        {
            null => "",
            double d => d.ToString("G17", CultureInfo.InvariantCulture),
            float f => f.ToString("G9", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? ""
        };
        if (s.Contains('"')) s = s.Replace("\"", "\"\"");
        return s.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0 ? $"\"{s}\"" : s;
    }

    private const string RawHeader =
        "schema_version,source_commit,build_configuration,session_id,config_hash,symbol,event_time_utc,receive_time_utc,book_epoch,kind,book_action,quality,side,price_ticks,price,quantity,order_id,closed,aggressor,priority,number_orders,is_snapshot_seed";

    private const string FeatureHeader =
        "schema_version,source_commit,build_configuration,session_id,config_hash,symbol,event_time_utc,receive_time_utc,book_epoch,book_mode,book_lifecycle,book_quality,feature_key,display_name,numeric_value,metric_quality,unit,reason";

    private const string TransitionHeader =
        "schema_version,source_commit,build_configuration,session_id,config_hash,symbol,event_time_utc,book_epoch,previous_regime,current_regime,reason,activity_score,volatility_score,liquidity_stress_score";

    private const string PairBreakHeader =
        "schema_version,source_commit,build_configuration,session_id,config_hash,symbol,event_time_utc,book_epoch,pair_id,previous_state,reason";

    private const string DiagnosticHeader =
        "schema_version,source_commit,build_configuration,session_id,config_hash,symbol,event_time_utc,book_epoch,severity,component,code,message";
}
