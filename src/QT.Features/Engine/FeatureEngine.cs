using QT.Core.Primitives;
using QT.Core.Quality;
using QT.Features.AggregateLattice;
using QT.Features.FloatingPairs;
using QT.Features.MarketState;
using QT.Market.Events;
using QT.Market.Snapshots;

namespace QT.Features.Engine;

public sealed class FeatureEngineConfig
{
    public MarketStateConfig MarketState { get; init; } = new();
    public FloatingPairConfig FloatingPairs { get; init; } = new();
    public SymmetricAggregateLatticeConfig AggregateLattice { get; init; } = new();
}

public sealed class FeatureEngine
{
    private readonly MarketStateMonitor marketState;
    private readonly MboFloatingPairDetector floatingPairs;
    private readonly SymmetricAggregateLatticeDetector aggregateLattice;
    private long sequence;
    private long epoch = -1;

    public FeatureEngine(FeatureEngineConfig? config = null)
    {
        config ??= new FeatureEngineConfig();
        marketState = new MarketStateMonitor(config.MarketState);
        floatingPairs = new MboFloatingPairDetector(config.FloatingPairs);
        aggregateLattice = new SymmetricAggregateLatticeDetector(config.AggregateLattice);
    }

    public IReadOnlyList<MarketStateTransition> DrainRegimeTransitions() => marketState.DrainTransitions();
    public IReadOnlyList<FloatingPairBreakEvent> DrainPairBreaks() => floatingPairs.DrainBreakEvents();

    public void OnMarketEvent(in NormalizedMarketEvent evt)
    {
        marketState.OnMarketEvent(evt);
        floatingPairs.OnMarketEvent(evt);
    }

    public FeatureSnapshot OnBookSnapshot(BookSnapshot book, string sessionId, string configHash)
    {
        if (book.BookEpoch != epoch)
        {
            epoch = book.BookEpoch;
            marketState.Reset(epoch, book.EventTimeUtc);
            floatingPairs.Reset(epoch, book.EventTimeUtc);
        }

        var ms = marketState.OnBookSnapshot(book, book.ReceiveTimeUtc);
        var fp = floatingPairs.OnBookSnapshot(book, book.ReceiveTimeUtc);
        var lattice = aggregateLattice.Scan(book);
        var values = BuildValues(book, ms, fp, lattice);
        return new FeatureSnapshot(++sequence, book.EventTimeUtc, book.ReceiveTimeUtc, book.Symbol,
            sessionId, configHash, book, ms, fp, lattice, values);
    }

    private static IReadOnlyDictionary<string, FeatureValue> BuildValues(
        BookSnapshot book,
        MarketStateSnapshot ms,
        FloatingPairSnapshot fp,
        SymmetricAggregateLatticeSnapshot lattice)
    {
        var t = book.EventTimeUtc;
        var q = book.DataQuality;
        var values = new Dictionary<string, FeatureValue>(StringComparer.Ordinal)
        {
            ["book.epoch"] = FeatureValue.Exact("book.epoch", "Book Epoch", book.BookEpoch, t),
            ["book.mode"] = FeatureValue.Exact("book.mode", "Book Mode", (double)book.Mode, t),
            ["book.lifecycle"] = FeatureValue.Exact("book.lifecycle", "Book Lifecycle", (double)book.LifecycleState, t),
            ["book.spread_ticks"] = book.HasTwoSidedBook && q != MetricQuality.Invalid
                ? FeatureValue.Derived("book.spread_ticks", "Current Spread", book.SpreadTicks, t, "ticks")
                : FeatureValue.Invalid("book.spread_ticks", "Current Spread", t, "ticks", book.InvalidReason),
            ["book.mbo_identity"] = FeatureValue.Derived("book.mbo_identity", "MBO Identity Coverage", book.MboIdentityCompleteness, t, "ratio"),

            ["market.regime"] = FeatureValue.Derived("market.regime", "Market Regime", (double)ms.Regime, t),
            ["market.risk"] = FeatureValue.Derived("market.risk", "Risk Environment", (double)ms.Risk, t),
            ["market.activity_score"] = FeatureValue.Derived("market.activity_score", "Activity Score", ms.ActivityScore, t),
            ["market.volatility_score"] = FeatureValue.Derived("market.volatility_score", "Volatility Score", ms.VolatilityScore, t),
            ["market.liquidity_stress"] = FeatureValue.Derived("market.liquidity_stress", "Liquidity Stress", ms.LiquidityStressScore, t),
            ["market.confidence"] = FeatureValue.Derived("market.confidence", "Regime Confidence", ms.RegimeConfidence, t),
            ["spread.mean_30s"] = FeatureValue.Derived("spread.mean_30s", "Mean Spread 30s", ms.TimeWeightedMeanSpread30s, t, "ticks"),
            ["spread.p90_60s"] = FeatureValue.Derived("spread.p90_60s", "P90 Spread 60s", ms.SpreadP90Ticks60s, t, "ticks"),
            ["spread.time_at_one_tick"] = FeatureValue.Derived("spread.time_at_one_tick", "Time at 1 Tick", ms.TimeAtOneTick60s, t, "ratio"),
            ["activity.book_events_sec"] = FeatureValue.Derived("activity.book_events_sec", "Book Events/sec", ms.BookEventsPerSec, t),
            ["activity.trades_sec"] = FeatureValue.Derived("activity.trades_sec", "Trades/sec", ms.TradesPerSec, t),
            ["vol.mid_rv_5s"] = FeatureValue.Derived("vol.mid_rv_5s", "Mid RV 5s", ms.MidRealizedVol5s, t, "ticks"),
            ["vol.mid_rv_30s"] = FeatureValue.Derived("vol.mid_rv_30s", "Mid RV 30s", ms.MidRealizedVol30s, t, "ticks"),

            ["fp.status"] = FeatureValue.Derived("fp.status", "Floating Pair Status", (double)fp.Status, t),
            ["fp.eligible_large_bids"] = FeatureValue.Derived("fp.eligible_large_bids", "Eligible Large Bids", fp.EligibleLargeBidCount, t),
            ["fp.eligible_large_asks"] = FeatureValue.Derived("fp.eligible_large_asks", "Eligible Large Asks", fp.EligibleLargeAskCount, t),
            ["fp.candidates"] = FeatureValue.Derived("fp.candidates", "Exact-size Candidates", fp.ExactSizeCandidateCount, t),
            ["fp.persistent"] = FeatureValue.Derived("fp.persistent", "Persistent Pairs", fp.PersistentPairCount, t),
            ["fp.confirmed"] = FeatureValue.Derived("fp.confirmed", "Confirmed Floating Pairs", fp.FloatingConfirmedPairCount, t),
            ["fp.top_size"] = fp.TopPair != null
                ? FeatureValue.Derived("fp.top_size", "Top Pair Size", fp.TopPair.Size, t)
                : FeatureValue.Unavailable("fp.top_size", "Top Pair Size", t, reason: "no active pair"),
            ["fp.top_follow_ratio"] = fp.TopPair != null
                ? FeatureValue.Derived("fp.top_follow_ratio", "Top Follow Ratio", fp.TopPair.FollowRatio, t, "ratio")
                : FeatureValue.Unavailable("fp.top_follow_ratio", "Top Follow Ratio", t, reason: "no active pair"),

            ["lattice.score"] = lattice.Enabled
                ? FeatureValue.Derived("lattice.score", "Aggregate Lattice Score", lattice.Score, t)
                : FeatureValue.Unavailable("lattice.score", "Aggregate Lattice Score", t, reason: "disabled")
        };

        if (fp.Status != FloatingPairDetectorStatus.MboReady)
        {
            values["fp.candidates"] = FeatureValue.Unavailable("fp.candidates", "Exact-size Candidates", t, reason: fp.QualityReason);
            values["fp.persistent"] = FeatureValue.Unavailable("fp.persistent", "Persistent Pairs", t, reason: fp.QualityReason);
            values["fp.confirmed"] = FeatureValue.Unavailable("fp.confirmed", "Confirmed Floating Pairs", t, reason: fp.QualityReason);
        }

        if (ms.Quality is MetricQuality.Stale or MetricQuality.Invalid)
        {
            values["market.regime"] = FeatureValue.Invalid("market.regime", "Market Regime", t, reason: ms.QualityReason);
            values["market.activity_score"] = FeatureValue.Invalid("market.activity_score", "Activity Score", t, reason: ms.QualityReason);
            values["market.volatility_score"] = FeatureValue.Invalid("market.volatility_score", "Volatility Score", t, reason: ms.QualityReason);
            values["market.liquidity_stress"] = FeatureValue.Invalid("market.liquidity_stress", "Liquidity Stress", t, reason: ms.QualityReason);
        }

        return values;
    }
}
