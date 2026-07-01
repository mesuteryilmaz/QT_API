using QT.Core.Primitives;
using QT.Core.Quality;
using QT.Features.OrderFlow;
using QT.Market.Events;
using QT.Market.Snapshots;

namespace MBO_Market_Data_Analytics.Tests
{
    public static class OrderFlowMonitorTests
    {
        private const double Tick = 0.25;
        private static readonly DateTime T = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        public static void RunAll()
        {
            Console.WriteLine("OrderFlowMonitor:");
            BuyDominatedFlowLeansUp();
            SellDominatedFlowLeansDown();
            BalancedFlowIsNeutral();
            TradeImbalanceValue();
            BookPressureAloneCanLeanUp();
            WarmupIsNeutral();
            EpochResetClearsState();
        }

        private static OrderFlowConfig Config(double warmupSec = 0) => new()
        {
            WarmupDuration = TimeSpan.FromSeconds(warmupSec)
        };

        private static void FeedTrades(OrderFlowMonitor m, DateTime start, TradeAggressor side, int count, long qtyEach)
        {
            for (int i = 0; i < count; i++)
                m.OnMarketEvent(NormalizedMarketEvent.Trade(i, start.AddMilliseconds(i * 100), start.AddMilliseconds(i * 100), "MNQ", 1000 * Tick, qtyEach, side, 1));
        }

        private static void BuyDominatedFlowLeansUp()
        {
            TestHarness.Begin("buy-dominated flow leans up");
            var m = new OrderFlowMonitor(Config());
            m.Reset(1, T);
            FeedTrades(m, T, TradeAggressor.Buy, 10, 10);
            var s = m.OnBookSnapshot(Book(T.AddSeconds(1), 1000, 1001), T.AddSeconds(1));

            TestHarness.IsTrue(s.Bias == DirectionalBias.Up, "bias up");
            TestHarness.IsTrue(s.LeanScore > 0, "positive lean score");
            TestHarness.IsTrue(s.SignedVolDeltaShort > 0, "positive signed delta");
        }

        private static void SellDominatedFlowLeansDown()
        {
            TestHarness.Begin("sell-dominated flow leans down");
            var m = new OrderFlowMonitor(Config());
            m.Reset(1, T);
            FeedTrades(m, T, TradeAggressor.Sell, 10, 10);
            var s = m.OnBookSnapshot(Book(T.AddSeconds(1), 1000, 1001), T.AddSeconds(1));

            TestHarness.IsTrue(s.Bias == DirectionalBias.Down, "bias down");
            TestHarness.IsTrue(s.LeanScore < 0, "negative lean score");
        }

        private static void BalancedFlowIsNeutral()
        {
            TestHarness.Begin("balanced flow is neutral");
            var m = new OrderFlowMonitor(Config());
            m.Reset(1, T);
            FeedTrades(m, T, TradeAggressor.Buy, 5, 10);
            FeedTrades(m, T, TradeAggressor.Sell, 5, 10);
            var s = m.OnBookSnapshot(Book(T.AddSeconds(1), 1000, 1001), T.AddSeconds(1));

            TestHarness.IsTrue(s.Bias == DirectionalBias.Neutral, "neutral bias");
            TestHarness.AreEqual(0.5, s.TradeImbalance, 1e-9, "imbalance 0.5");
        }

        private static void TradeImbalanceValue()
        {
            TestHarness.Begin("trade imbalance value");
            var m = new OrderFlowMonitor(Config());
            m.Reset(1, T);
            FeedTrades(m, T, TradeAggressor.Buy, 1, 8);
            FeedTrades(m, T.AddMilliseconds(50), TradeAggressor.Sell, 1, 2);
            var s = m.OnBookSnapshot(Book(T.AddSeconds(1), 1000, 1001), T.AddSeconds(1));

            TestHarness.AreEqual(0.8, s.TradeImbalance, 1e-9, "8 buy / 2 sell -> 0.80");
        }

        private static void BookPressureAloneCanLeanUp()
        {
            TestHarness.Begin("book pressure alone can lean up");
            var m = new OrderFlowMonitor(Config());
            m.Reset(1, T);
            // No trades; heavily bid-weighted book.
            var s = m.OnBookSnapshot(Book(T.AddSeconds(1), 1000, 1001, bidQty: 90, askQty: 10), T.AddSeconds(1));

            TestHarness.IsTrue(s.BookPressure > 0.7, "strong positive book pressure");
            TestHarness.IsTrue(s.Bias == DirectionalBias.Up, "bias up from book pressure");
        }

        private static void WarmupIsNeutral()
        {
            TestHarness.Begin("warmup is neutral regardless of flow");
            var m = new OrderFlowMonitor(Config(warmupSec: 10));
            m.Reset(1, T);
            FeedTrades(m, T, TradeAggressor.Buy, 10, 10);
            var s = m.OnBookSnapshot(Book(T.AddSeconds(1), 1000, 1001), T.AddSeconds(1));

            TestHarness.IsTrue(s.Quality == MetricQuality.WarmingUp, "warming up");
            TestHarness.IsTrue(s.Bias == DirectionalBias.Neutral, "neutral during warmup");
        }

        private static void EpochResetClearsState()
        {
            TestHarness.Begin("epoch reset clears trade history");
            var m = new OrderFlowMonitor(Config());
            m.Reset(1, T);
            FeedTrades(m, T, TradeAggressor.Buy, 10, 10);
            m.OnBookSnapshot(Book(T.AddSeconds(1), 1000, 1001), T.AddSeconds(1));

            // New epoch: prior buy pressure must not carry over.
            var s = m.OnBookSnapshot(Book(T.AddSeconds(2), 1000, 1001, epoch: 2), T.AddSeconds(2));
            TestHarness.AreEqual(0, (int)s.SignedVolDeltaShort, "signed delta reset");
            TestHarness.IsTrue(s.Bias == DirectionalBias.Neutral, "neutral after reset");
        }

        private static BookSnapshot Book(DateTime t, long bid, long ask, long bidQty = 10, long askQty = 10, long epoch = 1)
        {
            return new BookSnapshot("MNQ", t, t, epoch, BookMode.Mbo, BookLifecycleState.Valid,
                MetricQuality.Exact, true, true, 1.0, TimeSpan.Zero, false, false, bid, ask,
                bid * Tick, ask * Tick,
                new[] { new BookLevelSnapshot(BookSide.Bid, bid, bid * Tick, bidQty, 1) },
                new[] { new BookLevelSnapshot(BookSide.Ask, ask, ask * Tick, askQty, 1) },
                Array.Empty<MboOrderSnapshot>(),
                new BookEventStats(0, 0, 0, 0, 0, 0, 0),
                null);
        }
    }
}
