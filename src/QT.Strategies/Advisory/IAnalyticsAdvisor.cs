using QT.Features.FloatingPairs;
using QT.Features.MarketState;

namespace QT.Strategies.Advisory;

/// <summary>
/// Discrete events drained from the feature engine for the tick being evaluated. These are the
/// same lists produced by <c>FeatureEngine.DrainRegimeTransitions()</c> and
/// <c>DrainPairBreaks()</c>. They let the advisor react to a state change the instant it lands,
/// rather than waiting for the continuous view to settle.
/// </summary>
public readonly record struct AnalyticsEvents(
    IReadOnlyList<MarketStateTransition> RegimeTransitions,
    IReadOnlyList<FloatingPairBreakEvent> PairBreaks)
{
    public static AnalyticsEvents None { get; } = new(
        Array.Empty<MarketStateTransition>(),
        Array.Empty<FloatingPairBreakEvent>());
}

/// <summary>
/// Maps the current market view plus this tick's events to trading advice. Implementations must
/// be filter-only: they may disarm, flatten, or size down, but must never originate an entry.
/// </summary>
public interface IAnalyticsAdvisor
{
    TradingAdvice Evaluate(in MarketView view, in AnalyticsEvents events);
}
