using QT.Strategies.Grid;

namespace MBO_Market_Data_Analytics.Tests
{
    public static class OnePairGridQuoteEngineTests
    {
        public static void RunAll()
        {
            Console.WriteLine("OnePairGridQuoteEngine:");
            CentersPairAtTargetWidth();
            UsesMinimumPassiveWidthInsideTolerance();
            RejectsWidthWhenPassiveConstraintExceedsTolerance();
            RejectsLockedMarket();
            RepriceThresholdIsInclusive();
        }

        private static OnePairGridQuoteConfig Config() => new()
        {
            TargetWidthTicks = 100,
            WidthToleranceTicks = 20,
            MinPassiveOffsetTicks = 1,
            RepriceThresholdTicks = 5
        };

        private static void CentersPairAtTargetWidth()
        {
            TestHarness.Begin("one-pair grid centers 100-tick width passively");
            var decision = OnePairGridQuoteEngine.Calculate(1000, 1002, Config());

            TestHarness.IsTrue(decision.ShouldQuote, "decision ready");
            TestHarness.AreEqual(951, (int)decision.BidOrderTicks, "bid ticks");
            TestHarness.AreEqual(1051, (int)decision.AskOrderTicks, "ask ticks");
            TestHarness.AreEqual(100, (int)decision.WidthTicks, "width ticks");
            TestHarness.IsTrue(decision.BidOrderTicks <= 999, "bid remains passive");
            TestHarness.IsTrue(decision.AskOrderTicks >= 1003, "ask remains passive");
        }

        private static void UsesMinimumPassiveWidthInsideTolerance()
        {
            TestHarness.Begin("one-pair grid widens to passive minimum inside tolerance");
            var decision = OnePairGridQuoteEngine.Calculate(1000, 1110, Config());

            TestHarness.IsTrue(decision.ShouldQuote, "decision ready");
            TestHarness.AreEqual(999, (int)decision.BidOrderTicks, "bid is one tick below best bid");
            TestHarness.AreEqual(1111, (int)decision.AskOrderTicks, "ask is one tick above best ask");
            TestHarness.AreEqual(112, (int)decision.WidthTicks, "width widened inside tolerance");
        }

        private static void RejectsWidthWhenPassiveConstraintExceedsTolerance()
        {
            TestHarness.Begin("one-pair grid rejects passive width outside tolerance");
            var decision = OnePairGridQuoteEngine.Calculate(1000, 1130, Config());

            TestHarness.IsTrue(!decision.ShouldQuote, "decision rejected");
            TestHarness.IsTrue(decision.Status == OnePairGridQuoteDecisionStatus.WidthOutsideTolerance, "width status");
        }

        private static void RejectsLockedMarket()
        {
            TestHarness.Begin("one-pair grid rejects locked market");
            var decision = OnePairGridQuoteEngine.Calculate(1000, 1000, Config());

            TestHarness.IsTrue(!decision.ShouldQuote, "decision rejected");
            TestHarness.IsTrue(decision.Status == OnePairGridQuoteDecisionStatus.InvalidMarket, "invalid market status");
        }

        private static void RepriceThresholdIsInclusive()
        {
            TestHarness.Begin("one-pair grid reprice threshold is inclusive");
            var config = Config();

            TestHarness.IsTrue(!OnePairGridQuoteEngine.ShouldReprice(1000, 1004, config), "four ticks does not reprice");
            TestHarness.IsTrue(OnePairGridQuoteEngine.ShouldReprice(1000, 1005, config), "five ticks reprices");
        }
    }
}
