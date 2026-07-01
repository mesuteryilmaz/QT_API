using QT.Core.Primitives;
using QT.Core.Quality;
using QT.Features.FloatingPairs;
using QT.Features.MarketState;
using QT.Runtime.AnalyticsRuntime;

namespace QT.Strategies.Advisory;

/// <summary>
/// A flat, typed projection of the fields a strategy actually needs from an
/// <see cref="AnalyticsRuntimeSnapshot"/>. The advisor depends only on this struct — never on
/// the runtime, the book, or Quantower — so it can be exercised with hand-built values in a
/// dependency-free test. <see cref="From"/> is the single adapter from the live snapshot.
/// </summary>
public readonly record struct MarketView(
    // --- A. feed / book health (gate inputs) ---
    bool BookUsable,
    MetricQuality DataQuality,
    BookMode Mode,
    // --- B. market state / regime ---
    MarketRegime Regime,
    MarketRegime PreviousRegime,
    RiskEnvironment Risk,
    double ActivityScore,
    double VolatilityScore,
    double LiquidityStress,
    double Confidence,
    bool MarketStateReady,
    // --- C. spread / liquidity ---
    long SpreadTicks,
    double MeanSpread30s,
    double TimeAtOneTick60s,
    double WidenRatePerSec,
    double TouchDepthRatio,
    // --- D. activity / volatility ---
    double MidRv5s,
    double MidRv30s,
    double MultiTickJumpRate,
    // --- E. MBO floating pairs (context only) ---
    bool MboPairsReady,
    FloatingPairSummary? TopConfirmedPair)
{
    /// <summary>
    /// A benign, healthy baseline (ActiveLiquid, low stress) intended as a starting point for
    /// tests and callers that build a view incrementally with <c>with { ... }</c>.
    /// </summary>
    public static MarketView Default => new(
        BookUsable: true,
        DataQuality: MetricQuality.Derived,
        Mode: BookMode.Mbo,
        Regime: MarketRegime.ActiveLiquid,
        PreviousRegime: MarketRegime.ActiveLiquid,
        Risk: RiskEnvironment.Normal,
        ActivityScore: 0.50,
        VolatilityScore: 0.30,
        LiquidityStress: 0.20,
        Confidence: 0.80,
        MarketStateReady: true,
        SpreadTicks: 1,
        MeanSpread30s: 1.0,
        TimeAtOneTick60s: 0.90,
        WidenRatePerSec: 0.10,
        TouchDepthRatio: 1.0,
        MidRv5s: 0.50,
        MidRv30s: 0.50,
        MultiTickJumpRate: 0.0,
        MboPairsReady: true,
        TopConfirmedPair: null);

    public static MarketView From(AnalyticsRuntimeSnapshot snapshot)
    {
        var b = snapshot.Book;
        var ms = snapshot.Features.MarketState;
        var fp = snapshot.Features.FloatingPairs;

        bool bookUsable = b.HasTwoSidedBook
            && !b.IsCrossed
            && !b.IsLocked
            && b.DataQuality is MetricQuality.Exact or MetricQuality.Derived;

        bool marketStateReady = ms.Quality is MetricQuality.Exact or MetricQuality.Derived;

        FloatingPairSummary? topConfirmed =
            fp.TopPair is { State: FloatingPairState.FloatingConfirmed } confirmed ? confirmed : null;

        return new MarketView(
            bookUsable,
            b.DataQuality,
            b.Mode,
            ms.Regime,
            ms.PreviousRegime,
            ms.Risk,
            ms.ActivityScore,
            ms.VolatilityScore,
            ms.LiquidityStressScore,
            ms.RegimeConfidence,
            marketStateReady,
            ms.CurrentSpreadTicks,
            ms.TimeWeightedMeanSpread30s,
            ms.TimeAtOneTick60s,
            ms.WidenRatePerSec,
            ms.TouchDepthRatio,
            ms.MidRealizedVol5s,
            ms.MidRealizedVol30s,
            ms.MultiTickJumpRate,
            fp.Status == FloatingPairDetectorStatus.MboReady,
            topConfirmed);
    }
}
