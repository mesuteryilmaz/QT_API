using QT.Features.FloatingPairs;
using QT.Features.MarketState;
using QT.Strategies.Advisory;

namespace MBO_Market_Data_Analytics.Tests
{
    /// <summary>
    /// Filter-only advisor tests. Every case builds a <see cref="MarketView"/> from the healthy
    /// baseline and overrides only the fields under test, so each assertion isolates one rule.
    /// </summary>
    public static class RegimeGatedAdvisorTests
    {
        public static void RunAll()
        {
            Console.WriteLine("RegimeGatedAdvisor:");
            HealthyActiveLiquidArmsMeanRevert();
            QuietTightArmsPassiveQuote();
            QuietTightWithFlickeringSpreadDisarms();
            FastOrderlyWithHighVolArmsMomentum();
            UnusableBookFlattens();
            CriticalRiskFlattens();
            ThinFragileRegimeDisarms();
            WarmingMarketStateDisarms();
            TransitionIntoDislocationFlattensDespiteHealthyView();
            ThinTouchDepthDisarms();
            HighStressScalesSizeToDisarm();
            ConfirmedVeryLargePairBoostsSize();
            NeverArmsWithoutHostTrigger();
        }

        private static readonly RegimeGatedAdvisor Advisor = new();

        private static TradingAdvice Evaluate(MarketView view)
            => Advisor.Evaluate(view, AnalyticsEvents.None);

        private static void HealthyActiveLiquidArmsMeanRevert()
        {
            TestHarness.Begin("healthy ActiveLiquid arms mean-revert with positive size");
            var advice = Evaluate(MarketView.Default);

            TestHarness.IsTrue(advice.Posture == TradingPosture.Armed, "armed");
            TestHarness.IsTrue(advice.Style == TradingStyle.MeanRevert, "mean-revert style");
            TestHarness.IsTrue(advice.SizeMultiplier > 0, "size multiplier positive");
            TestHarness.IsTrue(advice.SizeMultiplier <= 1.0, "size multiplier bounded");
        }

        private static void QuietTightArmsPassiveQuote()
        {
            TestHarness.Begin("QuietTight with stable one-tick spread arms passive quoting");
            var view = MarketView.Default with
            {
                Regime = MarketRegime.QuietTight,
                TimeAtOneTick60s = 0.95,
                WidenRatePerSec = 0.05
            };
            var advice = Evaluate(view);

            TestHarness.IsTrue(advice.Posture == TradingPosture.Armed, "armed");
            TestHarness.IsTrue(advice.Style == TradingStyle.PassiveQuote, "passive-quote style");
        }

        private static void QuietTightWithFlickeringSpreadDisarms()
        {
            TestHarness.Begin("QuietTight with flickering spread does not passively quote");
            var view = MarketView.Default with
            {
                Regime = MarketRegime.QuietTight,
                TimeAtOneTick60s = 0.40,
                WidenRatePerSec = 2.0
            };
            var advice = Evaluate(view);

            TestHarness.IsTrue(advice.Posture == TradingPosture.Disarmed, "disarmed (no viable style)");
        }

        private static void FastOrderlyWithHighVolArmsMomentum()
        {
            TestHarness.Begin("FastOrderly with high volatility arms momentum");
            var view = MarketView.Default with
            {
                Regime = MarketRegime.FastOrderly,
                VolatilityScore = 0.60
            };
            var advice = Evaluate(view);

            TestHarness.IsTrue(advice.Posture == TradingPosture.Armed, "armed");
            TestHarness.IsTrue(advice.Style == TradingStyle.Momentum, "momentum style");
        }

        private static void UnusableBookFlattens()
        {
            TestHarness.Begin("unusable book flattens");
            var view = MarketView.Default with { BookUsable = false };
            var advice = Evaluate(view);

            TestHarness.IsTrue(advice.Posture == TradingPosture.Flatten, "flatten");
        }

        private static void CriticalRiskFlattens()
        {
            TestHarness.Begin("critical risk environment flattens");
            var view = MarketView.Default with { Risk = RiskEnvironment.Critical };
            var advice = Evaluate(view);

            TestHarness.IsTrue(advice.Posture == TradingPosture.Flatten, "flatten");
        }

        private static void ThinFragileRegimeDisarms()
        {
            TestHarness.Begin("ThinFragile regime disarms");
            var view = MarketView.Default with { Regime = MarketRegime.ThinFragile };
            var advice = Evaluate(view);

            TestHarness.IsTrue(advice.Posture == TradingPosture.Disarmed, "disarmed");
        }

        private static void WarmingMarketStateDisarms()
        {
            TestHarness.Begin("warming market state disarms");
            var view = MarketView.Default with { MarketStateReady = false };
            var advice = Evaluate(view);

            TestHarness.IsTrue(advice.Posture == TradingPosture.Disarmed, "disarmed");
        }

        private static void TransitionIntoDislocationFlattensDespiteHealthyView()
        {
            TestHarness.Begin("transition into dislocation flattens even with an otherwise healthy view");
            var events = new AnalyticsEvents(
                new[]
                {
                    new MarketStateTransition(
                        DateTime.UtcNow, 1, MarketRegime.ActiveLiquid, MarketRegime.VolatileDislocated,
                        "spread blowout", 0.9, 0.8, 0.95)
                },
                Array.Empty<FloatingPairBreakEvent>());
            var advice = Advisor.Evaluate(MarketView.Default, events);

            TestHarness.IsTrue(advice.Posture == TradingPosture.Flatten, "flatten from event kill switch");
        }

        private static void ThinTouchDepthDisarms()
        {
            TestHarness.Begin("thin touch depth disarms");
            var view = MarketView.Default with { TouchDepthRatio = 0.50 };
            var advice = Evaluate(view);

            TestHarness.IsTrue(advice.Posture == TradingPosture.Disarmed, "disarmed on thin depth");
        }

        private static void HighStressScalesSizeToDisarm()
        {
            TestHarness.Begin("stress near the ceiling scales size to zero and disarms");
            var view = MarketView.Default with { LiquidityStress = 0.44 };
            var advice = Evaluate(view);

            TestHarness.IsTrue(advice.Posture == TradingPosture.Disarmed, "disarmed when size floors out");
        }

        private static void ConfirmedVeryLargePairBoostsSize()
        {
            TestHarness.Begin("confirmed very-large pair boosts mean-revert size");
            var baseline = Evaluate(MarketView.Default);
            var boosted = Evaluate(MarketView.Default with
            {
                TopConfirmedPair = ConfirmedPair(FloatingPairTier.VeryLarge)
            });

            TestHarness.IsTrue(baseline.Posture == TradingPosture.Armed, "baseline armed");
            TestHarness.IsTrue(boosted.Posture == TradingPosture.Armed, "boosted armed");
            TestHarness.IsTrue(boosted.SizeMultiplier > baseline.SizeMultiplier, "size boosted by very-large pair");
        }

        private static void NeverArmsWithoutHostTrigger()
        {
            TestHarness.Begin("advice never carries an entry direction (filter-only invariant)");
            var advice = Evaluate(MarketView.Default);

            // Armed advice is a style + size only; there is no side/price/quantity field to place an order from.
            TestHarness.IsTrue(advice.Style != TradingStyle.None, "armed advice names a style, not an order");
            TestHarness.IsTrue(advice.SizeMultiplier <= 1.0, "size is a relative multiplier, not an absolute quantity");
        }

        private static FloatingPairSummary ConfirmedPair(FloatingPairTier tier) => new(
            PairId: "test-pair",
            State: FloatingPairState.FloatingConfirmed,
            Tier: tier,
            Size: tier == FloatingPairTier.VeryLarge ? 250 : 50,
            BidOrderId: "b1",
            AskOrderId: "a1",
            BidPriceTicks: 1000,
            AskPriceTicks: 1004,
            BidOffsetTicks: 2,
            AskOffsetTicks: 2,
            Age: TimeSpan.FromSeconds(5),
            SynchronizedMoves: 4,
            Opportunities: 5,
            FollowRatio: 0.9,
            MeanSyncDelayMs: 40,
            MaxSyncDelayMs: 120,
            Confidence: 0.85,
            UsesHeuristicStitching: false);
    }
}
