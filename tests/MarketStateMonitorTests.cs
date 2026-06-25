using QT.Core.Primitives;
using QT.Core.Quality;
using QT.Features.MarketState;
using QT.Market.Snapshots;

namespace MBO_Market_Data_Analytics.Tests
{
    public static class MarketStateMonitorTests
    {
        private const double Tick = 0.25;
        private static readonly DateTime T = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        public static void RunAll()
        {
            Console.WriteLine("MarketStateMonitor:");
            ConstantOneTickSpread();
            TimeWeightedMeanSpread();
            TimeAtOneTick();
            WideningEpisodeAndRecovery();
            BboPriceChangeVersusSizeOnly();
            CancellationPressure();
            QuietTightRegime();
            VolatileDislocatedRegime();
            StaleFeedIsNotQuietTight();
            InvalidBookIsUnknown();
        }

        private static MarketStateMonitor Monitor() => new(new MarketStateConfig
        {
            WarmupDuration = TimeSpan.Zero,
            CandidatePersistence = TimeSpan.Zero,
            MinimumDwell = TimeSpan.Zero
        });

        private static void ConstantOneTickSpread()
        {
            TestHarness.Begin("constant one-tick spread");
            var m = Monitor();
            m.Reset(1, T);
            var s = m.OnBookSnapshot(Book(T, 1000, 1001), T);
            TestHarness.AreEqual(1, (int)s.CurrentSpreadTicks, "spread is one tick");
            TestHarness.AreEqual(1.0, s.TimeWeightedMeanSpread30s, 1e-9, "mean spread");
        }

        private static void TimeWeightedMeanSpread()
        {
            TestHarness.Begin("time-weighted mean spread");
            var m = Monitor();
            m.Reset(1, T);
            m.OnBookSnapshot(Book(T, 1000, 1001), T);
            m.OnBookSnapshot(Book(T.AddSeconds(10), 1000, 1003), T.AddSeconds(10));
            var s = m.OnBookSnapshot(Book(T.AddSeconds(20), 1000, 1003), T.AddSeconds(20));
            TestHarness.AreEqual(2.0, s.TimeWeightedMeanSpread30s, 0.05, "mean of 10s at 1 tick and 10s at 3 ticks");
        }

        private static void TimeAtOneTick()
        {
            TestHarness.Begin("time at one tick");
            var m = Monitor();
            m.Reset(1, T);
            m.OnBookSnapshot(Book(T, 1000, 1001), T);
            m.OnBookSnapshot(Book(T.AddSeconds(1), 1000, 1001), T.AddSeconds(1));
            var s = m.OnBookSnapshot(Book(T.AddSeconds(2), 1000, 1003), T.AddSeconds(2));
            TestHarness.IsTrue(s.TimeAtOneTick60s > 0.60 && s.TimeAtOneTick60s < 0.70, "two of three samples at one tick");
        }

        private static void WideningEpisodeAndRecovery()
        {
            TestHarness.Begin("spread episode age, peak, recovery");
            var m = Monitor();
            m.Reset(1, T);
            m.OnBookSnapshot(Book(T, 1000, 1001), T);
            var active = m.OnBookSnapshot(Book(T.AddSeconds(1), 1000, 1004), T.AddSeconds(1));
            TestHarness.IsTrue(active.SpreadEpisode.IsActive, "episode active");
            TestHarness.AreEqual(4, (int)active.SpreadEpisode.PeakSpreadTicks, "peak spread");
            var recovered = m.OnBookSnapshot(Book(T.AddSeconds(3), 1000, 1001), T.AddSeconds(3));
            TestHarness.IsTrue(!recovered.SpreadEpisode.IsActive, "episode recovered");
            TestHarness.AreEqual(2.0, recovered.SpreadEpisode.LastRecoveryDuration.TotalSeconds, 0.01, "recovery duration");
        }

        private static void BboPriceChangeVersusSizeOnly()
        {
            TestHarness.Begin("BBO price update versus size-only update");
            var m = Monitor();
            m.Reset(1, T);
            m.OnBookSnapshot(Book(T, 1000, 1001), T);
            var sizeOnly = m.OnBookSnapshot(Book(T.AddSeconds(1), 1000, 1001, bidQty: 20), T.AddSeconds(1));
            TestHarness.AreEqual(0.0, sizeOnly.BboChangesPerSec, 1e-9, "size-only no BBO price change");
            var priceMove = m.OnBookSnapshot(Book(T.AddSeconds(2), 1001, 1002), T.AddSeconds(2));
            TestHarness.IsTrue(priceMove.BboChangesPerSec > 0, "price move counted");
        }

        private static void CancellationPressure()
        {
            TestHarness.Begin("cancellation pressure");
            var m = Monitor();
            m.Reset(1, T);
            m.OnBookSnapshot(Book(T, 1000, 1001, stats: new BookEventStats(0, 0, 0, 0, 0, 0, 0)), T);
            var s = m.OnBookSnapshot(Book(T.AddSeconds(1), 1000, 1001, stats: new BookEventStats(0, 10, 2, 0, 8, 0, 0)), T.AddSeconds(1));
            TestHarness.AreEqual(0.8, s.CancellationPressure, 1e-9, "remove/(add+remove)");
        }

        private static void QuietTightRegime()
        {
            TestHarness.Begin("QuietTight regime");
            var m = Monitor();
            m.Reset(1, T);
            var s = m.OnBookSnapshot(Book(T, 1000, 1001), T);
            TestHarness.IsTrue(s.Regime == MarketRegime.QuietTight, "quiet tight");
        }

        private static void VolatileDislocatedRegime()
        {
            TestHarness.Begin("VolatileDislocated emergency transition");
            var m = Monitor();
            m.Reset(1, T);
            var s = m.OnBookSnapshot(Book(T, 1000, 1010), T);
            TestHarness.IsTrue(s.Regime == MarketRegime.VolatileDislocated, "volatile dislocated");
        }

        private static void StaleFeedIsNotQuietTight()
        {
            TestHarness.Begin("stale feed is not QuietTight");
            var m = Monitor();
            m.Reset(1, T);
            var s = m.OnBookSnapshot(Book(T, 1000, 1001, quality: MetricQuality.Stale), T.AddSeconds(10));
            TestHarness.IsTrue(s.Regime == MarketRegime.Unknown, "unknown regime");
            TestHarness.IsTrue(s.Quality == MetricQuality.Stale, "stale quality");
        }

        private static void InvalidBookIsUnknown()
        {
            TestHarness.Begin("invalid book is Unknown");
            var m = Monitor();
            m.Reset(1, T);
            var s = m.OnBookSnapshot(Book(T, 1002, 1001, quality: MetricQuality.Invalid, crossed: true), T);
            TestHarness.IsTrue(s.Regime == MarketRegime.Unknown, "unknown regime");
            TestHarness.IsTrue(s.Quality == MetricQuality.Invalid, "invalid quality");
        }

        private static BookSnapshot Book(DateTime t, long bid, long ask, long bidQty = 10, long askQty = 10,
            MetricQuality quality = MetricQuality.Exact, bool crossed = false, BookEventStats? stats = null)
        {
            return new BookSnapshot("MNQ", t, t, 1, BookMode.Mbo,
                quality == MetricQuality.Invalid ? BookLifecycleState.Invalid : BookLifecycleState.Valid,
                quality, true, true, 1.0, TimeSpan.Zero, bid == ask, crossed || bid > ask, bid, ask,
                bid * Tick, ask * Tick,
                new[] { new BookLevelSnapshot(BookSide.Bid, bid, bid * Tick, bidQty, 1) },
                new[] { new BookLevelSnapshot(BookSide.Ask, ask, ask * Tick, askQty, 1) },
                Array.Empty<MboOrderSnapshot>(),
                stats ?? new BookEventStats(0, 0, 0, 0, 0, 0, 0),
                quality == MetricQuality.Invalid ? "invalid" : null);
        }
    }
}
