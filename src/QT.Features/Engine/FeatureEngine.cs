using QT.Core.Primitives;
using QT.Core.Quality;
using QT.Features.AggregateLattice;
using QT.Features.FloatingPairs;
using QT.Features.MarketState;
using QT.Features.OrderFlow;
using QT.Market.Events;
using QT.Market.Snapshots;

namespace QT.Features.Engine;

public sealed class FeatureEngineConfig
{
    public MarketStateConfig MarketState { get; init; } = new();
    public FloatingPairConfig FloatingPairs { get; init; } = new();
    public SymmetricAggregateLatticeConfig AggregateLattice { get; init; } = new();
    public OrderFlowConfig OrderFlow { get; init; } = new();
}

public sealed class FeatureEngine
{
    private readonly MarketStateMonitor marketState;
    private readonly MboFloatingPairDetector floatingPairs;
    private readonly SymmetricAggregateLatticeDetector aggregateLattice;
    private readonly OrderFlowMonitor orderFlow;
    private long sequence;
    private long epoch = -1;

    public FeatureEngine(FeatureEngineConfig? config = null)
    {
        config ??= new FeatureEngineConfig();
        marketState = new MarketStateMonitor(config.MarketState);
        floatingPairs = new MboFloatingPairDetector(config.FloatingPairs);
        aggregateLattice = new SymmetricAggregateLatticeDetector(config.AggregateLattice);
        orderFlow = new OrderFlowMonitor(config.OrderFlow);
    }

    public IReadOnlyList<MarketStateTransition> DrainRegimeTransitions() => marketState.DrainTransitions();
    public IReadOnlyList<FloatingPairBreakEvent> DrainPairBreaks() => floatingPairs.DrainBreakEvents();

    public void OnMarketEvent(in NormalizedMarketEvent evt)
    {
        marketState.OnMarketEvent(evt);
        floatingPairs.OnMarketEvent(evt);
        orderFlow.OnMarketEvent(evt);
    }

    public FeatureSnapshot OnBookSnapshot(BookSnapshot book, string sessionId, string configHash)
    {
        if (book.BookEpoch != epoch)
        {
            epoch = book.BookEpoch;
            marketState.Reset(epoch, book.EventTimeUtc);
            floatingPairs.Reset(epoch, book.EventTimeUtc);
            orderFlow.Reset(epoch, book.EventTimeUtc);
        }

        var ms = marketState.OnBookSnapshot(book, book.ReceiveTimeUtc);
        var fp = floatingPairs.OnBookSnapshot(book, book.ReceiveTimeUtc);
        var of = orderFlow.OnBookSnapshot(book, book.ReceiveTimeUtc);
        var lattice = aggregateLattice.Scan(book);
        var values = BuildValues(book, ms, fp, of, lattice);
        return new FeatureSnapshot(++sequence, book.EventTimeUtc, book.ReceiveTimeUtc, book.Symbol,
            sessionId, configHash, book, ms, fp, of, lattice, values);
    }

    private static IReadOnlyDictionary<string, FeatureValue> BuildValues(
        BookSnapshot book,
        MarketStateSnapshot ms,
        FloatingPairSnapshot fp,
        OrderFlowSnapshot of,
        SymmetricAggregateLatticeSnapshot lattice)
    {
        var t = book.EventTimeUtc;
        var q = book.DataQuality;
        var values = new Dictionary<string, FeatureValue>(StringComparer.Ordinal)
        {
            ["book.epoch"] = FeatureValue.Exact("book.epoch", "Book Epoch", book.BookEpoch, t),
            ["book.mode"] = FeatureValue.Exact("book.mode", "Book Mode", (double)book.Mode, t),
            ["book.lifecycle"] = FeatureValue.Exact("book.lifecycle", "Book Lifecycle", (double)book.LifecycleState, t),
            ["book.spread_ticks"] = book.HasTwoSidedBook
                ? QualityValue("book.spread_ticks", "Current Spread", book.SpreadTicks, q, t, "ticks", book.InvalidReason)
                : QualityValue("book.spread_ticks", "Current Spread", 0, NonUsableBookQuality(q), t, "ticks", book.InvalidReason ?? "two-sided book unavailable"),
            ["book.mbo_identity"] = book.Mode == BookMode.Mbo
                ? QualityValue("book.mbo_identity", "MBO Identity Coverage", book.MboIdentityCompleteness, q, t, "ratio", book.InvalidReason)
                : FeatureValue.Unavailable("book.mbo_identity", "MBO Identity Coverage", t, "ratio", "MBP mode has no per-order identity"),

            ["market.regime"] = MarketValue("market.regime", "Market Regime", (double)ms.Regime, ms),
            ["market.risk"] = MarketValue("market.risk", "Risk Environment", (double)ms.Risk, ms),
            ["market.activity_score"] = MarketValue("market.activity_score", "Activity Score", ms.ActivityScore, ms),
            ["market.volatility_score"] = MarketValue("market.volatility_score", "Volatility Score", ms.VolatilityScore, ms),
            ["market.liquidity_stress"] = MarketValue("market.liquidity_stress", "Liquidity Stress", ms.LiquidityStressScore, ms),
            ["market.confidence"] = MarketValue("market.confidence", "Regime Confidence", ms.RegimeConfidence, ms),
            ["spread.mean_30s"] = MarketValue("spread.mean_30s", "Mean Spread 30s", ms.TimeWeightedMeanSpread30s, ms, "ticks"),
            ["spread.p90_60s"] = MarketValue("spread.p90_60s", "P90 Spread 60s", ms.SpreadP90Ticks60s, ms, "ticks"),
            ["spread.time_at_one_tick"] = MarketValue("spread.time_at_one_tick", "Time at 1 Tick", ms.TimeAtOneTick60s, ms, "ratio"),
            ["spread.widen_rate_sec"] = MarketValue("spread.widen_rate_sec", "Spread Widen Rate", ms.WidenRatePerSec, ms),
            ["spread.narrow_rate_sec"] = MarketValue("spread.narrow_rate_sec", "Spread Narrow Rate", ms.NarrowRatePerSec, ms),
            ["liquidity.touch_depth_ratio"] = MarketValue("liquidity.touch_depth_ratio", "Touch Depth Ratio", ms.TouchDepthRatio, ms, "ratio"),
            ["liquidity.top5_depth_ratio"] = MarketValue("liquidity.top5_depth_ratio", "Top 5 Depth Ratio", ms.Top5DepthRatio, ms, "ratio"),
            ["liquidity.cancel_pressure"] = MarketValue("liquidity.cancel_pressure", "Cancellation Pressure", ms.CancellationPressure, ms, "ratio"),
            ["activity.bbo_changes_sec"] = MarketValue("activity.bbo_changes_sec", "BBO Changes/sec", ms.BboChangesPerSec, ms),
            ["activity.book_events_sec"] = MarketValue("activity.book_events_sec", "Book Events/sec", ms.BookEventsPerSec, ms),
            ["activity.trades_sec"] = MarketValue("activity.trades_sec", "Trades/sec", ms.TradesPerSec, ms),
            ["activity.volume_sec"] = MarketValue("activity.volume_sec", "Volume/sec", ms.VolumePerSec, ms),
            ["vol.mid_rv_5s"] = MarketValue("vol.mid_rv_5s", "Mid RV 5s", ms.MidRealizedVol5s, ms, "ticks"),
            ["vol.mid_rv_30s"] = MarketValue("vol.mid_rv_30s", "Mid RV 30s", ms.MidRealizedVol30s, ms, "ticks"),
            ["vol.mid_range_5s"] = MarketValue("vol.mid_range_5s", "Mid Range 5s", ms.MidRangeTicks5s, ms, "ticks"),
            ["vol.mid_range_30s"] = MarketValue("vol.mid_range_30s", "Mid Range 30s", ms.MidRangeTicks30s, ms, "ticks"),
            ["vol.multi_tick_jumps_sec"] = MarketValue("vol.multi_tick_jumps_sec", "Multi-tick Jumps/sec", ms.MultiTickJumpRate, ms),

            ["fp.status"] = QualityValue("fp.status", "Floating Pair Status", (double)fp.Status, fp.Quality, t, reason: fp.QualityReason),
            ["fp.eligible_large_bids"] = FloatingPairValue("fp.eligible_large_bids", "Eligible Large Bids", fp.EligibleLargeBidCount, fp, t),
            ["fp.eligible_large_asks"] = FloatingPairValue("fp.eligible_large_asks", "Eligible Large Asks", fp.EligibleLargeAskCount, fp, t),
            ["fp.eligible_very_large_bids"] = FloatingPairValue("fp.eligible_very_large_bids", "Eligible Very-Large Bids", fp.EligibleVeryLargeBidCount, fp, t),
            ["fp.eligible_very_large_asks"] = FloatingPairValue("fp.eligible_very_large_asks", "Eligible Very-Large Asks", fp.EligibleVeryLargeAskCount, fp, t),
            ["fp.candidates"] = FloatingPairValue("fp.candidates", "Exact-size Candidates", fp.ExactSizeCandidateCount, fp, t),
            ["fp.persistent"] = FloatingPairValue("fp.persistent", "Persistent Pairs", fp.PersistentPairCount, fp, t),
            ["fp.confirmed"] = FloatingPairValue("fp.confirmed", "Confirmed Floating Pairs", fp.FloatingConfirmedPairCount, fp, t),
            ["fp.top_size"] = fp.TopPair != null
                ? FeatureValue.Derived("fp.top_size", "Top Pair Size", fp.TopPair.Size, t)
                : FeatureValue.Unavailable("fp.top_size", "Top Pair Size", t, reason: "no active pair"),
            ["fp.top_follow_ratio"] = fp.TopPair != null
                ? FeatureValue.Derived("fp.top_follow_ratio", "Top Follow Ratio", fp.TopPair.FollowRatio, t, "ratio")
                : FeatureValue.Unavailable("fp.top_follow_ratio", "Top Follow Ratio", t, reason: "no active pair"),

            ["flow.bias"] = OrderFlowValue("flow.bias", "Order-Flow Bias", (double)of.Bias, of, t),
            ["flow.lean_score"] = OrderFlowValue("flow.lean_score", "Order-Flow Lean Score", of.LeanScore, of, t),
            ["flow.confidence"] = OrderFlowValue("flow.confidence", "Order-Flow Confidence", of.Confidence, of, t, "ratio"),
            ["flow.trade_imbalance"] = OrderFlowValue("flow.trade_imbalance", "Trade Imbalance", of.TradeImbalance, of, t, "ratio"),
            ["flow.book_pressure"] = OrderFlowValue("flow.book_pressure", "Book Pressure", of.BookPressure, of, t),
            ["flow.cvd_slope_sec"] = OrderFlowValue("flow.cvd_slope_sec", "CVD Slope", of.CvdSlopePerSec, of, t),

            ["lattice.score"] = lattice.Enabled
                ? FeatureValue.Derived("lattice.score", "Aggregate Lattice Score", lattice.Score, t)
                : FeatureValue.Unavailable("lattice.score", "Aggregate Lattice Score", t, reason: "disabled")
        };

        return values;
    }

    private static FeatureValue OrderFlowValue(string key, string displayName, double value, OrderFlowSnapshot of, DateTime eventTimeUtc, string unit = "")
        => QualityValue(key, displayName, value, of.Quality, eventTimeUtc, unit, of.QualityReason);

    private static FeatureValue MarketValue(string key, string displayName, double value, MarketStateSnapshot ms, string unit = "")
        => QualityValue(key, displayName, value, ms.Quality, ms.EventTimeUtc, unit, ms.QualityReason);

    private static FeatureValue FloatingPairValue(string key, string displayName, double value, FloatingPairSnapshot fp, DateTime eventTimeUtc, string unit = "")
        => fp.Status == FloatingPairDetectorStatus.MboReady
            ? QualityValue(key, displayName, value, fp.Quality, eventTimeUtc, unit, fp.QualityReason)
            : QualityValue(key, displayName, 0, NonUsablePairQuality(fp.Quality), eventTimeUtc, unit, fp.QualityReason);

    private static FeatureValue QualityValue(
        string key,
        string displayName,
        double value,
        MetricQuality quality,
        DateTime eventTimeUtc,
        string unit = "",
        string? reason = null)
        => quality switch
        {
            MetricQuality.Exact => FeatureValue.Exact(key, displayName, value, eventTimeUtc, unit, reason),
            MetricQuality.Derived => FeatureValue.Derived(key, displayName, value, eventTimeUtc, unit, reason),
            MetricQuality.WarmingUp => FeatureValue.Warming(key, displayName, eventTimeUtc, unit, reason),
            MetricQuality.Stale => FeatureValue.Stale(key, displayName, eventTimeUtc, unit, reason),
            MetricQuality.Invalid => FeatureValue.Invalid(key, displayName, eventTimeUtc, unit, reason),
            _ => FeatureValue.Unavailable(key, displayName, eventTimeUtc, unit, reason)
        };

    private static MetricQuality NonUsableBookQuality(MetricQuality quality)
        => quality is MetricQuality.Unavailable or MetricQuality.WarmingUp or MetricQuality.Stale or MetricQuality.Invalid
            ? quality
            : MetricQuality.Invalid;

    private static MetricQuality NonUsablePairQuality(MetricQuality quality)
        => quality is MetricQuality.Unavailable or MetricQuality.WarmingUp or MetricQuality.Stale or MetricQuality.Invalid
            ? quality
            : MetricQuality.Unavailable;
}
