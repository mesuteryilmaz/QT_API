using System.Drawing;
using System.Drawing.Drawing2D;
using QT.Core.Primitives;
using QT.Core.Quality;
using QT.Features.FloatingPairs;
using QT.Features.MarketState;
using QT.Runtime.AnalyticsRuntime;
using TradingPlatform.BusinessLayer;

namespace MBO_Market_Data_Analytics;

public class DataAnalyticsIndicator : Indicator
{
    [InputParameter("Preferred Book Mode: MBO", 0)]
    public bool InputPreferMbo = true;

    [InputParameter("Snapshot Depth Levels", 1, minimum: 5, maximum: 2000, increment: 5, decimalPlaces: 0)]
    public int InputSnapshotDepth = 360;

    [InputParameter("Panel Refresh (ms)", 2, minimum: 100, maximum: 5000, increment: 50, decimalPlaces: 0)]
    public int InputUpdateFrequencyMs = 250;

    [InputParameter("Stale Timeout (ms)", 3, minimum: 250, maximum: 30000, increment: 250, decimalPlaces: 0)]
    public int InputStaleTimeoutMs = 3000;

    [InputParameter("Enable Versioned Recorder", 4)]
    public bool InputEnableRecorder = false;

    [InputParameter("Recorder Output Path", 5)]
    public string InputRecorderPath = @"C:\Quantower\Settings\Scripts\ScriptsData\QT_API_V2";

    [InputParameter("Show Aggregate Lattice Diagnostic", 6)]
    public bool InputEnableAggregateLattice = false;

    [InputParameter("Floating Pair Large Threshold", 7, minimum: 1, maximum: 1000, increment: 1, decimalPlaces: 0)]
    public int InputLargeThreshold = 20;

    [InputParameter("Floating Pair Very Large Threshold", 8, minimum: 1, maximum: 5000, increment: 1, decimalPlaces: 0)]
    public int InputVeryLargeThreshold = 200;

    [InputParameter("Floating Pair Max Offset Ticks", 9, minimum: 1, maximum: 2000, increment: 1, decimalPlaces: 0)]
    public int InputFloatingPairMaxOffsetTicks = 250;

    private AnalyticsEngineHost? engine;

    private readonly Color bg = Color.FromArgb(220, 11, 18, 28);
    private readonly Color header = Color.FromArgb(245, 19, 30, 46);
    private readonly Color border = Color.FromArgb(255, 54, 68, 91);
    private readonly Color text = Color.FromArgb(255, 231, 236, 245);
    private readonly Color muted = Color.FromArgb(255, 151, 164, 184);
    private readonly Color ok = Color.FromArgb(255, 38, 180, 96);
    private readonly Color warn = Color.FromArgb(255, 235, 158, 52);
    private readonly Color bad = Color.FromArgb(255, 236, 76, 76);
    private readonly Color info = Color.FromArgb(255, 84, 153, 235);

    public DataAnalyticsIndicator()
    {
        Name = "QT API V2 Analytics";
        Description = "Analytics-only market microstructure platform. V2 contains no live trading strategy or order submission path.";
        SeparateWindow = false;
    }

    protected override void OnInit()
    {
        base.OnInit();
        var cfg = new AnalyticsEngineConfig
        {
            PreferMbo = InputPreferMbo,
            SnapshotDepth = InputSnapshotDepth,
            UpdateFrequencyMs = InputUpdateFrequencyMs,
            StaleTimeoutMs = InputStaleTimeoutMs,
            EnableRecorder = InputEnableRecorder,
            RecorderPath = InputRecorderPath,
            EnableAggregateLattice = InputEnableAggregateLattice,
            FloatingPairLargeThreshold = InputLargeThreshold,
            FloatingPairVeryLargeThreshold = InputVeryLargeThreshold,
            FloatingPairMaxOffsetTicks = InputFloatingPairMaxOffsetTicks
        };
        engine = new AnalyticsEngineHost(Symbol, cfg, (message, level) => Core.Instance.Loggers.Log(message, level));
        engine.Start();
    }

    protected override void OnUpdate(UpdateArgs args)
    {
    }

    public override void OnPaintChart(PaintChartEventArgs args)
    {
        base.OnPaintChart(args);
        var snap = engine?.CurrentSnapshot;
        if (snap == null || engine == null || !engine.IsInitialized)
            return;

        Graphics g = args.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        int x = 20;
        int y = 50;
        int w = 1080;
        int h = 500;
        using var path = Rounded(new Rectangle(x, y, w, h), 8);
        using (var b = new SolidBrush(bg)) g.FillPath(b, path);
        using (var p = new Pen(border, 1.5f)) g.DrawPath(p, path);
        using (var hb = new SolidBrush(header)) g.FillRectangle(hb, x, y, w, 34);
        using (var p = new Pen(border, 1.2f)) g.DrawLine(p, x, y + 34, x + w, y + 34);

        using var titleFont = new Font("Segoe UI", 10, FontStyle.Bold);
        using var sectionFont = new Font("Segoe UI", 8, FontStyle.Bold);
        using var labelFont = new Font("Segoe UI", 8, FontStyle.Regular);
        using var valueFont = new Font("Segoe UI", 8, FontStyle.Bold);
        using var smallFont = new Font("Segoe UI", 7, FontStyle.Regular);

        DrawText(g, "QT API V2 ANALYTICS-ONLY", titleFont, text, x + 12, y + 8);
        string right = $"{Symbol.Name} | {snap.Book.Mode} | epoch {snap.Book.BookEpoch}";
        DrawRight(g, right, labelFont, muted, x + w - 12, y + 9);

        int colW = 330;
        int c1 = x + 16;
        int c2 = x + 374;
        int c3 = x + 732;
        int yy1 = y + 48, yy2 = y + 48, yy3 = y + 48;

        Section(g, "A. FEED & BOOK HEALTH", sectionFont, c1, yy1, colW); yy1 += 20;
        Row(g, "Lifecycle", snap.Book.LifecycleState.ToString(), labelFont, valueFont, c1, yy1, colW, StateColor(snap.Book.DataQuality)); yy1 += 18;
        Row(g, "Book mode", snap.Book.Mode.ToString(), labelFont, valueFont, c1, yy1, colW, snap.Book.Mode == BookMode.Mbo ? ok : warn); yy1 += 18;
        Row(g, "Data quality", snap.Book.DataQuality.ToString(), labelFont, valueFont, c1, yy1, colW, StateColor(snap.Book.DataQuality)); yy1 += 18;
        Row(g, "Data age", FormatAge(snap.Book.LastEventAge), labelFont, valueFont, c1, yy1, colW, snap.Book.DataQuality == MetricQuality.Stale ? bad : text); yy1 += 18;
        Row(g, "Validity", BookValidity(snap), labelFont, valueFont, c1, yy1, colW, snap.Book.DataQuality == MetricQuality.Invalid ? bad : ok); yy1 += 18;
        Row(g, "MBO identity", snap.Book.Mode == BookMode.Mbo ? snap.Book.MboIdentityCompleteness.ToString("P0") : "Unavailable", labelFont, valueFont, c1, yy1, colW, snap.Book.Mode == BookMode.Mbo ? text : muted); yy1 += 18;
        Row(g, "Recorder", snap.RecorderStatus, labelFont, valueFont, c1, yy1, colW, snap.RecorderStatus == "active" ? ok : muted); yy1 += 18;
        Row(g, "Queue overflow", engine.QueueOverflowCount.ToString("N0"), labelFont, valueFont, c1, yy1, colW, engine.QueueOverflowCount > 0 ? bad : text); yy1 += 24;

        Section(g, "C. SPREAD & LIQUIDITY", sectionFont, c1, yy1, colW); yy1 += 20;
        Row(g, "Current spread", Metric(snap, "book.spread_ticks", "F0", " ticks"), labelFont, valueFont, c1, yy1, colW, text); yy1 += 18;
        Row(g, "Mean 30s / P90 60s", $"{Metric(snap, "spread.mean_30s", "F2", "")} / {Metric(snap, "spread.p90_60s", "F1", "")}", labelFont, valueFont, c1, yy1, colW, text); yy1 += 18;
        Row(g, "Time at 1 tick", Metric(snap, "spread.time_at_one_tick", "P0", ""), labelFont, valueFont, c1, yy1, colW, text); yy1 += 18;
        Row(g, "Widen / narrow rate", $"{snap.Features.MarketState.WidenRatePerSec:F2} / {snap.Features.MarketState.NarrowRatePerSec:F2}/s", labelFont, valueFont, c1, yy1, colW, text); yy1 += 18;
        Row(g, "Touch / top5 depth", $"{snap.Features.MarketState.TouchDepthRatio:F2} / {snap.Features.MarketState.Top5DepthRatio:F2}", labelFont, valueFont, c1, yy1, colW, text); yy1 += 18;
        Row(g, "Cancel pressure", snap.Features.MarketState.CancellationPressure.ToString("P0"), labelFont, valueFont, c1, yy1, colW, snap.Features.MarketState.CancellationPressure > 0.7 ? warn : text);

        Section(g, "B. MARKET STATE & REGIME", sectionFont, c2, yy2, colW); yy2 += 20;
        Row(g, "Current regime", snap.Features.MarketState.Regime.ToString(), labelFont, valueFont, c2, yy2, colW, RegimeColor(snap.Features.MarketState.Regime)); yy2 += 18;
        Row(g, "Risk environment", snap.Features.MarketState.Risk.ToString(), labelFont, valueFont, c2, yy2, colW, RiskColor(snap.Features.MarketState.Risk)); yy2 += 18;
        Row(g, "Activity score", snap.Features.MarketState.ActivityScore.ToString("F2"), labelFont, valueFont, c2, yy2, colW, text); yy2 += 18;
        Row(g, "Volatility score", snap.Features.MarketState.VolatilityScore.ToString("F2"), labelFont, valueFont, c2, yy2, colW, text); yy2 += 18;
        Row(g, "Liquidity stress", snap.Features.MarketState.LiquidityStressScore.ToString("F2"), labelFont, valueFont, c2, yy2, colW, StressColor(snap.Features.MarketState.LiquidityStressScore)); yy2 += 18;
        Row(g, "Confidence", snap.Features.MarketState.RegimeConfidence.ToString("P0"), labelFont, valueFont, c2, yy2, colW, text); yy2 += 18;
        Row(g, "Previous regime", snap.Features.MarketState.PreviousRegime.ToString(), labelFont, valueFont, c2, yy2, colW, muted); yy2 += 18;
        Row(g, "Transition age", FormatAge(snap.Features.MarketState.TransitionAge), labelFont, valueFont, c2, yy2, colW, text); yy2 += 18;
        Row(g, "Reason", Clip(snap.Features.MarketState.TransitionReason, 38), labelFont, valueFont, c2, yy2, colW, muted); yy2 += 24;

        Section(g, "D. ACTIVITY & VOLATILITY", sectionFont, c2, yy2, colW); yy2 += 20;
        Row(g, "BBO changes/sec", snap.Features.MarketState.BboChangesPerSec.ToString("F2"), labelFont, valueFont, c2, yy2, colW, text); yy2 += 18;
        Row(g, "Book events/sec", snap.Features.MarketState.BookEventsPerSec.ToString("F1"), labelFont, valueFont, c2, yy2, colW, text); yy2 += 18;
        Row(g, "Trades / volume sec", $"{snap.Features.MarketState.TradesPerSec:F1} / {snap.Features.MarketState.VolumePerSec:F0}", labelFont, valueFont, c2, yy2, colW, text); yy2 += 18;
        Row(g, "Mid RV 5s / 30s", $"{snap.Features.MarketState.MidRealizedVol5s:F2} / {snap.Features.MarketState.MidRealizedVol30s:F2}", labelFont, valueFont, c2, yy2, colW, text); yy2 += 18;
        Row(g, "Mid range 5s / 30s", $"{snap.Features.MarketState.MidRangeTicks5s} / {snap.Features.MarketState.MidRangeTicks30s} ticks", labelFont, valueFont, c2, yy2, colW, text); yy2 += 18;
        Row(g, "Multi-tick jumps", snap.Features.MarketState.MultiTickJumpRate.ToString("F2") + "/s", labelFont, valueFont, c2, yy2, colW, text);

        Section(g, "E. MBO FLOATING PAIRS", sectionFont, c3, yy3, colW); yy3 += 20;
        var fp = snap.Features.FloatingPairs;
        Row(g, "Detector status", fp.Status.ToString(), labelFont, valueFont, c3, yy3, colW, fp.Status == FloatingPairDetectorStatus.MboReady ? ok : muted); yy3 += 18;
        Row(g, "Large bid / ask", $"{DisplayCount(fp.Status, fp.EligibleLargeBidCount)} / {DisplayCount(fp.Status, fp.EligibleLargeAskCount)}", labelFont, valueFont, c3, yy3, colW, text); yy3 += 18;
        Row(g, "Very-large bid / ask", $"{DisplayCount(fp.Status, fp.EligibleVeryLargeBidCount)} / {DisplayCount(fp.Status, fp.EligibleVeryLargeAskCount)}", labelFont, valueFont, c3, yy3, colW, text); yy3 += 18;
        Row(g, "Candidate pairs", DisplayCount(fp.Status, fp.ExactSizeCandidateCount), labelFont, valueFont, c3, yy3, colW, text); yy3 += 18;
        Row(g, "Persistent / confirmed", $"{DisplayCount(fp.Status, fp.PersistentPairCount)} / {DisplayCount(fp.Status, fp.FloatingConfirmedPairCount)}", labelFont, valueFont, c3, yy3, colW, text); yy3 += 18;
        if (fp.TopPair != null)
        {
            Row(g, "Top size / tier", $"{fp.TopPair.Size} / {fp.TopPair.Tier}", labelFont, valueFont, c3, yy3, colW, text); yy3 += 18;
            Row(g, "Bid / ask offsets", $"{fp.TopPair.BidOffsetTicks} / {fp.TopPair.AskOffsetTicks} ticks", labelFont, valueFont, c3, yy3, colW, text); yy3 += 18;
            Row(g, "Age", FormatAge(fp.TopPair.Age), labelFont, valueFont, c3, yy3, colW, text); yy3 += 18;
            Row(g, "Sync / opps", $"{fp.TopPair.SynchronizedMoves} / {fp.TopPair.Opportunities}", labelFont, valueFont, c3, yy3, colW, text); yy3 += 18;
            Row(g, "Follow / confidence", $"{fp.TopPair.FollowRatio:P0} / {fp.TopPair.Confidence:P0}", labelFont, valueFont, c3, yy3, colW, fp.TopPair.State == FloatingPairState.FloatingConfirmed ? ok : text);
        }
        else
        {
            Row(g, "Top pair", fp.Status == FloatingPairDetectorStatus.MboReady ? "None" : fp.QualityReason, labelFont, valueFont, c3, yy3, colW, muted);
        }

        int footerY = y + h - 38;
        using (var p = new Pen(border, 1.2f)) g.DrawLine(p, x, footerY - 8, x + w, footerY - 8);
        DrawText(g, "Safety: V2 is analytics-only. No Flow Ratio Strategy, broker adapter, or live order submission is compiled into this host.", smallFont, muted, x + 14, footerY);
        DrawText(g, $"Session {snap.RuntimeSessionId[..8]} | cfg {snap.ConfigurationHash[..8]} | {snap.Features.EventTimeUtc:HH:mm:ss.fff} UTC", smallFont, muted, x + 14, footerY + 16);
    }

    protected override void OnClear()
    {
        engine?.Stop();
        engine = null;
        base.OnClear();
    }

    private string Metric(AnalyticsRuntimeSnapshot snapshot, string key, string format, string suffix)
    {
        if (!snapshot.Features.Values.TryGetValue(key, out var v) || !v.IsNumeric)
            return v.Reason ?? v.Quality.ToString();
        return v.Value.ToString(format) + suffix;
    }

    private static string DisplayCount(FloatingPairDetectorStatus status, int count)
        => status == FloatingPairDetectorStatus.MboReady ? count.ToString("N0") : "Unavailable";

    private static string BookValidity(AnalyticsRuntimeSnapshot snap)
    {
        if (snap.Book.IsCrossed) return "Crossed/invalid";
        if (snap.Book.IsLocked) return "Locked";
        if (!snap.Book.BidSideValid || !snap.Book.AskSideValid) return "One-sided/invalid";
        return "Two-sided";
    }

    private Color StateColor(MetricQuality q)
        => q == MetricQuality.Exact || q == MetricQuality.Derived ? ok
            : q == MetricQuality.WarmingUp || q == MetricQuality.Stale ? warn
            : q == MetricQuality.Invalid ? bad
            : muted;

    private Color RegimeColor(MarketRegime regime)
        => regime switch
        {
            MarketRegime.QuietTight => ok,
            MarketRegime.ActiveLiquid => text,
            MarketRegime.FastOrderly => info,
            MarketRegime.ThinFragile => warn,
            MarketRegime.VolatileDislocated => bad,
            MarketRegime.Recovering => warn,
            _ => muted
        };

    private Color RiskColor(RiskEnvironment risk)
        => risk switch
        {
            RiskEnvironment.Low => ok,
            RiskEnvironment.Normal => text,
            RiskEnvironment.Elevated => warn,
            RiskEnvironment.Critical => bad,
            _ => muted
        };

    private Color StressColor(double stress)
        => stress >= 0.75 ? bad : stress >= 0.50 ? warn : text;

    private static string FormatAge(TimeSpan age)
        => age == TimeSpan.MaxValue ? "Unavailable" : age.TotalSeconds >= 60 ? $"{age.TotalMinutes:F1}m" : $"{age.TotalSeconds:F1}s";

    private static string Clip(string value, int max)
        => value.Length <= max ? value : value[..Math.Max(0, max - 1)] + "...";

    private void Section(Graphics g, string title, Font font, int x, int y, int w)
    {
        using var b = new SolidBrush(Color.FromArgb(50, info));
        g.FillRectangle(b, x, y, w, 16);
        DrawText(g, title, font, info, x + 8, y + 2);
    }

    private void Row(Graphics g, string label, string value, Font labelFont, Font valueFont, int x, int y, int w, Color valueColor)
    {
        DrawText(g, label, labelFont, muted, x + 8, y);
        DrawRight(g, value, valueFont, valueColor, x + w - 8, y);
    }

    private static void DrawText(Graphics g, string text, Font font, Color color, float x, float y)
    {
        using var b = new SolidBrush(color);
        g.DrawString(text, font, b, x, y);
    }

    private static void DrawRight(Graphics g, string text, Font font, Color color, float right, float y)
    {
        using var b = new SolidBrush(color);
        SizeF size = g.MeasureString(text, font);
        g.DrawString(text, font, b, right - size.Width, y);
    }

    private static GraphicsPath Rounded(Rectangle bounds, int radius)
    {
        int d = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
