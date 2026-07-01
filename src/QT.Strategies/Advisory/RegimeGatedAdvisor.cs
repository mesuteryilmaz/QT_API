using QT.Features.FloatingPairs;
using QT.Features.MarketState;

namespace QT.Strategies.Advisory;

public sealed record RegimeGatedAdvisorConfig
{
    /// <summary>Stress at or above which the position is fully scaled out (size reaches zero).</summary>
    public double MaxLiquidityStress { get; init; } = 0.45;

    /// <summary>Minimum touch-depth ratio (vs baseline) required to size any position at all.</summary>
    public double MinTouchDepthRatio { get; init; } = 0.90;

    /// <summary>Passive quoting requires a calm spread: widen rate must be at or below this.</summary>
    public double MaxWidenRatePerSec { get; init; } = 0.50;

    /// <summary>Passive quoting requires the spread to sit at one tick at least this fraction of the time.</summary>
    public double MinTimeAtOneTick { get; init; } = 0.80;

    /// <summary>Momentum style requires volatility at or above this (mirrors the FastOrderly cut).</summary>
    public double MomentumVolThreshold { get; init; } = 0.55;

    /// <summary>Advice below this size multiplier is treated as "not worth it" and disarms instead.</summary>
    public double MinSizeMultiplier { get; init; } = 0.05;

    /// <summary>Multiplier applied to size when mean-reverting toward a confirmed very-large pair.</summary>
    public double VeryLargePairSizeBoost { get; init; } = 1.25;

    public void Validate()
    {
        if (MaxLiquidityStress is <= 0 or > 1) throw new ArgumentOutOfRangeException(nameof(MaxLiquidityStress));
        if (MinTouchDepthRatio < 0) throw new ArgumentOutOfRangeException(nameof(MinTouchDepthRatio));
        if (MaxWidenRatePerSec < 0) throw new ArgumentOutOfRangeException(nameof(MaxWidenRatePerSec));
        if (MinTimeAtOneTick is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(MinTimeAtOneTick));
        if (MomentumVolThreshold is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(MomentumVolThreshold));
        if (MinSizeMultiplier is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(MinSizeMultiplier));
        if (VeryLargePairSizeBoost < 1) throw new ArgumentOutOfRangeException(nameof(VeryLargePairSizeBoost));
    }
}

/// <summary>
/// Filter-only advisor. It gates on feed/book health, regime, and risk; selects a style the
/// regime favors; and sizes inversely to stress and volatility. It never emits an entry — a
/// host must supply its own trigger and gate it with this advice. Stateless and deterministic:
/// the same view+events always yield the same advice.
/// </summary>
public sealed class RegimeGatedAdvisor : IAnalyticsAdvisor
{
    private readonly RegimeGatedAdvisorConfig cfg;

    public RegimeGatedAdvisor(RegimeGatedAdvisorConfig? config = null)
    {
        cfg = config ?? new RegimeGatedAdvisorConfig();
        cfg.Validate();
    }

    public TradingAdvice Evaluate(in MarketView view, in AnalyticsEvents events)
    {
        // (0) Event-driven kill switch. A transition into a fragile/dislocated regime lands the
        // instant it is classified (emergencies bypass dwell upstream), so react immediately.
        foreach (var transition in events.RegimeTransitions)
        {
            if (transition.Current is MarketRegime.ThinFragile or MarketRegime.VolatileDislocated)
                return TradingAdvice.FlattenNow($"transition->{transition.Current}: {transition.Reason}");
        }

        // (1) Hard gate: feed/book health, then regime, then risk.
        if (!view.BookUsable)
            return TradingAdvice.FlattenNow($"book unusable: {view.DataQuality}");

        if (!view.MarketStateReady)
            return TradingAdvice.Disarmed("market-state warming/stale");

        if (view.Risk is RiskEnvironment.Elevated or RiskEnvironment.Critical)
            return TradingAdvice.FlattenNow($"risk={view.Risk}");

        if (view.Regime is MarketRegime.ThinFragile
                        or MarketRegime.VolatileDislocated
                        or MarketRegime.Recovering
                        or MarketRegime.Unknown)
            return TradingAdvice.Disarmed($"regime={view.Regime}");

        // (2) Style selection from regime + microstructure.
        TradingStyle style = view.Regime switch
        {
            MarketRegime.QuietTight when view.TimeAtOneTick60s >= cfg.MinTimeAtOneTick
                                      && view.WidenRatePerSec <= cfg.MaxWidenRatePerSec
                => TradingStyle.PassiveQuote,
            MarketRegime.FastOrderly when view.VolatilityScore >= cfg.MomentumVolThreshold
                => TradingStyle.Momentum,
            MarketRegime.ActiveLiquid
                => TradingStyle.MeanRevert,
            _ => TradingStyle.None
        };

        if (style == TradingStyle.None)
            return TradingAdvice.Disarmed($"no style for regime={view.Regime}");

        // (3) Sizing: require real depth at the touch, then scale down with stress and volatility.
        if (view.TouchDepthRatio < cfg.MinTouchDepthRatio)
            return TradingAdvice.Disarmed($"touch depth {view.TouchDepthRatio:F2} below {cfg.MinTouchDepthRatio:F2}");

        double stressFactor = 1.0 - view.LiquidityStress / cfg.MaxLiquidityStress;
        double volFactor = 1.0 - view.VolatilityScore;
        double sizeMultiplier = Clamp01(stressFactor * volFactor);

        // (4) Context overlay: mean-reverting toward a confirmed very-large pair earns a size boost.
        if (style == TradingStyle.MeanRevert
            && view.TopConfirmedPair is { Tier: FloatingPairTier.VeryLarge })
            sizeMultiplier = Clamp01(sizeMultiplier * cfg.VeryLargePairSizeBoost);

        if (sizeMultiplier <= cfg.MinSizeMultiplier)
            return TradingAdvice.Disarmed($"size {sizeMultiplier:F2} scaled below floor {cfg.MinSizeMultiplier:F2}");

        return new TradingAdvice(
            TradingPosture.Armed,
            style,
            sizeMultiplier,
            $"{view.Regime}/{style} stress={view.LiquidityStress:F2} vol={view.VolatilityScore:F2} size={sizeMultiplier:F2}");
    }

    private static double Clamp01(double value) => Math.Max(0.0, Math.Min(1.0, value));
}
