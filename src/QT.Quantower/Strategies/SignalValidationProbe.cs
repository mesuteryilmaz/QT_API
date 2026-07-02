using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MBO_Market_Data_Analytics;
using QT.Core.Quality;
using QT.Features.OrderFlow;
using QT.Runtime.AnalyticsRuntime;
using TradingPlatform.BusinessLayer;

namespace MBO_OnePair_Grid_Strategy;

/// <summary>
/// Log-only validation harness for the order-flow directional signal. It runs the analytics engine,
/// samples the signal (bias/confidence/components) with the current mid, then after a fixed forward
/// horizon records the realized mid move versus what the signal predicted — to a CSV, plus a periodic
/// summary (hit rate + mean forward return in ticks). It PLACES NO ORDERS. Use it to decide whether
/// the signal actually leads price before letting it drive live entries.
/// </summary>
public sealed class SignalValidationProbe : Strategy, ICurrentSymbol
{
    private readonly object sync = new();
    private AnalyticsEngineHost? analytics;
    private System.Threading.Timer? timer;
    private StreamWriter? writer;
    private readonly Queue<Sample> pending = new();

    private long rows;
    private long upCount, upHits, downCount, downHits;
    private double upSignedSum, downSignedSum;
    private DateTime lastSummaryUtc = DateTime.MinValue;
    private string filePath = "";

    [InputParameter("Symbol", 0)]
    public Symbol CurrentSymbol { get; set; } = null!;

    [InputParameter("Analytics prefer MBO", 1)]
    public bool AnalyticsPreferMbo { get; set; } = true;

    [InputParameter("Sample interval (ms)", 2, minimum: 100, maximum: 5000, increment: 50, decimalPlaces: 0)]
    public int SampleMs { get; set; } = 250;

    [InputParameter("Forward horizon (sec)", 3, minimum: 1, maximum: 600, increment: 1, decimalPlaces: 0)]
    public int HorizonSeconds { get; set; } = 10;

    [InputParameter("Min confidence for summary", 4, minimum: 0.0, maximum: 1.0, increment: 0.05, decimalPlaces: 2)]
    public double MinConfidence { get; set; } = 0.50;

    [InputParameter("Summary every (sec)", 5, minimum: 5, maximum: 3600, increment: 5, decimalPlaces: 0)]
    public int SummaryEverySeconds { get; set; } = 30;

    [InputParameter("Output folder", 6)]
    public string OutputFolder { get; set; } = @"C:\Quantower\Settings\Scripts\ScriptsData\QT_API_V2\SignalProbe";

    public override string[] MonitoringConnectionsIds
        => string.IsNullOrWhiteSpace(CurrentSymbol?.ConnectionId) ? Array.Empty<string>() : new[] { CurrentSymbol.ConnectionId };

    public SignalValidationProbe() : base()
    {
        Name = "MBO Signal Probe (log-only)";
        Description = "Analytics-only. Records the order-flow signal vs. realized forward price move to a CSV for offline validation. Places no orders.";
    }

    protected override void OnRun()
    {
        base.OnRun();

        lock (sync)
        {
            pending.Clear();
            rows = 0;
            upCount = upHits = downCount = downHits = 0;
            upSignedSum = downSignedSum = 0;
            lastSummaryUtc = Core.Instance.TimeUtils.DateTimeUtcNow;
        }

        if (CurrentSymbol != null && CurrentSymbol.State == BusinessObjectState.Fake)
            CurrentSymbol = Core.Instance.GetSymbol(CurrentSymbol.CreateInfo());

        if (CurrentSymbol == null || CurrentSymbol.TickSize <= 0)
        {
            Log("Symbol is not specified or has no tick size.", StrategyLoggingLevel.Error);
            Stop();
            return;
        }

        try
        {
            Directory.CreateDirectory(OutputFolder);
            string stamp = Core.Instance.TimeUtils.DateTimeUtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            filePath = Path.Combine(OutputFolder, $"signal_probe_{CurrentSymbol.Name}_{stamp}.csv");
            writer = new StreamWriter(filePath, append: false) { AutoFlush = false };
            writer.WriteLine("timestamp_utc,bias,confidence,lean_score,imbalance,book_pressure,cvd_slope,mid_then,mid_now,fwd_ticks,signed_ticks,correct,conf_ge_min,horizon_sec");
            writer.Flush();
        }
        catch (Exception ex)
        {
            Log($"Failed to open probe CSV ({ex.Message}).", StrategyLoggingLevel.Error);
            Stop();
            return;
        }

        try
        {
            analytics = new AnalyticsEngineHost(CurrentSymbol, new AnalyticsEngineConfig { PreferMbo = AnalyticsPreferMbo }, MapAnalyticsLog);
            analytics.Start();
        }
        catch (Exception ex)
        {
            analytics = null;
            Log($"Analytics failed to start: {ex.Message}", StrategyLoggingLevel.Error);
            Stop();
            return;
        }

        int period = Math.Max(100, SampleMs);
        timer = new System.Threading.Timer(_ => OnTimer(), null, 0, period);
        Log($"Signal probe started (log-only): horizon={HorizonSeconds}s, sample={period}ms, out={filePath}. No orders are placed.");
    }

    protected override void OnStop()
    {
        timer?.Dispose();
        timer = null;
        analytics?.Stop();
        analytics = null;
        lock (sync)
        {
            LogSummary(Core.Instance.TimeUtils.DateTimeUtcNow, force: true);
            try { writer?.Flush(); writer?.Dispose(); } catch { }
            writer = null;
        }
        Log($"Signal probe stopped. Rows written: {rows}. File: {filePath}");
        base.OnStop();
    }

    protected override void OnRemove()
    {
        timer?.Dispose();
        timer = null;
        analytics?.Stop();
        analytics = null;
        lock (sync)
        {
            try { writer?.Flush(); writer?.Dispose(); } catch { }
            writer = null;
        }
        base.OnRemove();
    }

    private void OnTimer()
    {
        try
        {
            lock (sync)
                SampleAndDrain();
        }
        catch (Exception ex)
        {
            Log($"Signal probe loop error: {ex.Message}", StrategyLoggingLevel.Error);
        }
    }

    private void SampleAndDrain()
    {
        if (writer == null || analytics == null || !analytics.IsInitialized)
            return;

        AnalyticsRuntimeSnapshot? snap = analytics.CurrentSnapshot;
        double mid = Mid();
        if (snap == null || double.IsNaN(mid))
            return;

        var of = snap.Features.OrderFlow;
        var now = Core.Instance.TimeUtils.DateTimeUtcNow;

        // Only sample when the signal is usable; still log every usable sample (including Neutral).
        if (IsUsable(of.Quality))
        {
            int sign = of.Bias == DirectionalBias.Up ? 1 : of.Bias == DirectionalBias.Down ? -1 : 0;
            pending.Enqueue(new Sample(now, of.Bias, sign, of.Confidence, of.LeanScore, of.TradeImbalance,
                of.BookPressure, of.CvdSlopePerSec, mid));
        }

        double tick = CurrentSymbol.TickSize;
        var horizon = TimeSpan.FromSeconds(Math.Max(1, HorizonSeconds));
        while (pending.Count > 0 && now - pending.Peek().SampledUtc >= horizon)
        {
            var s = pending.Dequeue();
            double fwdTicks = (mid - s.Mid) / tick;
            double signedTicks = fwdTicks * s.Sign;
            bool confGe = s.Confidence >= MinConfidence;
            bool correct = signedTicks > 0;

            writer.WriteLine(string.Join(",",
                s.SampledUtc.ToString("o", CultureInfo.InvariantCulture),
                s.Bias,
                s.Confidence.ToString("0.####", CultureInfo.InvariantCulture),
                s.LeanScore.ToString("0.####", CultureInfo.InvariantCulture),
                s.Imbalance.ToString("0.####", CultureInfo.InvariantCulture),
                s.BookPressure.ToString("0.####", CultureInfo.InvariantCulture),
                s.CvdSlope.ToString("0.##", CultureInfo.InvariantCulture),
                s.Mid.ToString("0.#####", CultureInfo.InvariantCulture),
                mid.ToString("0.#####", CultureInfo.InvariantCulture),
                fwdTicks.ToString("0.##", CultureInfo.InvariantCulture),
                signedTicks.ToString("0.##", CultureInfo.InvariantCulture),
                correct ? "1" : "0",
                confGe ? "1" : "0",
                HorizonSeconds.ToString(CultureInfo.InvariantCulture)));
            rows++;

            if (confGe && s.Sign != 0)
            {
                if (s.Sign > 0)
                {
                    upCount++;
                    upSignedSum += signedTicks;
                    if (correct) upHits++;
                }
                else
                {
                    downCount++;
                    downSignedSum += signedTicks;
                    if (correct) downHits++;
                }
            }
        }

        LogSummary(now, force: false);
    }

    private void LogSummary(DateTime now, bool force)
    {
        if (!force && (now - lastSummaryUtc).TotalSeconds < Math.Max(5, SummaryEverySeconds))
            return;
        lastSummaryUtc = now;

        try { writer?.Flush(); } catch { }

        string up = upCount > 0
            ? $"Up n={upCount} mean={(upSignedSum / upCount):+0.00;-0.00}t hit={(100.0 * upHits / upCount):0}%"
            : "Up n=0";
        string down = downCount > 0
            ? $"Down n={downCount} mean={(downSignedSum / downCount):+0.00;-0.00}t hit={(100.0 * downHits / downCount):0}%"
            : "Down n=0";
        Log($"Signal probe [conf>={MinConfidence:0.00}, h={HorizonSeconds}s]: {up} | {down} | rows={rows}. (mean>0 = signal led price)", StrategyLoggingLevel.Trading);
    }

    private double Mid()
    {
        double bid = CurrentSymbol.Bid;
        double ask = CurrentSymbol.Ask;
        if (bid <= 0 || ask <= 0 || double.IsNaN(bid) || double.IsNaN(ask) || ask < bid)
            return double.NaN;
        return (bid + ask) / 2.0;
    }

    private static bool IsUsable(MetricQuality q)
        => q is MetricQuality.Exact or MetricQuality.Derived;

    private void MapAnalyticsLog(string message, LoggingLevel level)
        => Log(message, level == LoggingLevel.Error ? StrategyLoggingLevel.Error : StrategyLoggingLevel.Info);

    private readonly record struct Sample(
        DateTime SampledUtc,
        DirectionalBias Bias,
        int Sign,
        double Confidence,
        double LeanScore,
        double Imbalance,
        double BookPressure,
        double CvdSlope,
        double Mid);
}
