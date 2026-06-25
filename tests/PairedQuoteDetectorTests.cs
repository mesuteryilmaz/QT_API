using System;
using QT.Core.Primitives;
using QT.Core.Quality;
using QT.Features.FloatingPairs;
using QT.Market.Snapshots;

namespace MBO_Market_Data_Analytics.Tests
{
    public static class PairedQuoteDetectorTests
    {
        private const double Tick = 0.25;
        private static readonly DateTime T = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        private static DateTime At(double seconds) => T.AddSeconds(seconds);

        public static void RunAll()
        {
            Console.WriteLine("V2 MBO FloatingPairDetector:");
            SymmetricPairBecomesCandidate();
            VeryLargeTierClassified();
            ExactSizeMismatchRejected();
            PerOrderNotAggregate();
            StaticPairBecomesPersistentNotConfirmed();
            FollowsTwoMovesBecomesConfirmed();
            OneLegMovesNotConfirmed();
            LateCounterpartFails();
            OutsideMaxDistanceIgnored();
            MultipleSameSizeOneToOne();
            SameIdRepricePreservesPair();
            StrictCancelReentryStitching();
            PartialFillBreaksPair();
            LegRemovalExpiresPair();
            MbpModeReportsUnavailable();
            EpochResetClearsState();
            EpochMismatchPreventsStitching();
        }

        private static MboFloatingPairDetector Fresh()
        {
            var d = new MboFloatingPairDetector();
            d.Reset(1, T);
            return d;
        }

        private static void SymmetricPairBecomesCandidate()
        {
            TestHarness.Begin("20/20 pair becomes Candidate");
            var d = Fresh();
            var s = d.OnBookSnapshot(Book(At(0), 1000, 1001, Order("B", BookSide.Bid, 995, 20, 0), Order("A", BookSide.Ask, 1006, 20, 0)), At(0));
            TestHarness.AreEqual(1, s.ExactSizeCandidateCount, "one exact-size candidate");
            TestHarness.AreEqual(0, s.FloatingConfirmedPairCount, "not confirmed");
            TestHarness.IsTrue(s.TopPair?.State == FloatingPairState.Candidate, "state Candidate");
            TestHarness.AreEqual(20, (int)(s.TopPair?.Size ?? 0), "top size 20");
        }

        private static void VeryLargeTierClassified()
        {
            TestHarness.Begin("200/200 pair is VeryLarge");
            var d = Fresh();
            var s = d.OnBookSnapshot(Book(At(0), 1000, 1001, Order("B", BookSide.Bid, 990, 200, 0), Order("A", BookSide.Ask, 1011, 200, 0)), At(0));
            TestHarness.IsTrue(s.TopPair?.Tier == FloatingPairTier.VeryLarge, "tier VeryLarge");
            TestHarness.AreEqual(1, s.EligibleVeryLargeBidCount, "very-large bid count");
        }

        private static void ExactSizeMismatchRejected()
        {
            TestHarness.Begin("20 bid vs 21 ask rejected");
            var d = Fresh();
            var s = d.OnBookSnapshot(Book(At(0), 1000, 1001, Order("B", BookSide.Bid, 995, 20, 0), Order("A", BookSide.Ask, 1006, 21, 0)), At(0));
            TestHarness.AreEqual(0, s.ExactSizeCandidateCount, "no exact-size candidate");
            TestHarness.IsTrue(s.TopPair == null, "no top pair");
        }

        private static void PerOrderNotAggregate()
        {
            TestHarness.Begin("individual 20-lot remains visible with same-price unrelated order");
            var d = Fresh();
            var s = d.OnBookSnapshot(Book(At(0), 1000, 1001,
                Order("B1", BookSide.Bid, 995, 20, 0),
                Order("B2", BookSide.Bid, 995, 7, 0),
                Order("A1", BookSide.Ask, 1006, 20, 0)), At(0));
            TestHarness.AreEqual(1, s.ExactSizeCandidateCount, "20/20 pair forms");
            TestHarness.AreEqual(1, s.EligibleLargeBidCount, "7-lot not eligible");
        }

        private static void StaticPairBecomesPersistentNotConfirmed()
        {
            TestHarness.Begin("static pair becomes Persistent but not FloatingConfirmed");
            var d = Fresh();
            d.OnBookSnapshot(Book(At(0), 1000, 1001, Order("B", BookSide.Bid, 995, 20, 0), Order("A", BookSide.Ask, 1006, 20, 0)), At(0));
            var s = d.OnBookSnapshot(Book(At(1.5), 1000, 1001, Order("B", BookSide.Bid, 995, 20, 0), Order("A", BookSide.Ask, 1006, 20, 0)), At(1.5));
            TestHarness.IsTrue(s.TopPair?.State == FloatingPairState.Persistent, "Persistent");
            TestHarness.AreEqual(0, s.FloatingConfirmedPairCount, "not confirmed");
        }

        private static void FollowsTwoMovesBecomesConfirmed()
        {
            TestHarness.Begin("two coordinated moves confirm the pair");
            var d = Fresh();
            d.OnBookSnapshot(Book(At(0), 1000, 1001, Order("B", BookSide.Bid, 995, 20, 0), Order("A", BookSide.Ask, 1006, 20, 0)), At(0));
            d.OnBookSnapshot(Book(At(0.6), 1001, 1002, Order("B", BookSide.Bid, 996, 20, 0.6), Order("A", BookSide.Ask, 1007, 20, 0.6)), At(0.6));
            var s = d.OnBookSnapshot(Book(At(1.2), 1002, 1003, Order("B", BookSide.Bid, 997, 20, 1.2), Order("A", BookSide.Ask, 1008, 20, 1.2)), At(1.2));
            TestHarness.IsTrue(s.TopPair?.State == FloatingPairState.FloatingConfirmed, "confirmed state");
            TestHarness.AreEqual(1, s.FloatingConfirmedPairCount, "one confirmed pair");
            TestHarness.AreEqual(2, s.TopPair?.SynchronizedMoves ?? 0, "two sync moves");
            TestHarness.AreEqual(1.0, s.TopPair?.FollowRatio ?? 0, 1e-9, "100% follow ratio");
        }

        private static void OneLegMovesNotConfirmed()
        {
            TestHarness.Begin("one-leg-only move fails");
            var d = Fresh();
            d.OnBookSnapshot(Book(At(0), 1000, 1001, Order("B", BookSide.Bid, 995, 20, 0), Order("A", BookSide.Ask, 1006, 20, 0)), At(0));
            d.OnBookSnapshot(Book(At(0.6), 1001, 1002, Order("B", BookSide.Bid, 996, 20, 0.6), Order("A", BookSide.Ask, 1006, 20, 0)), At(0.6));
            var s = d.OnBookSnapshot(Book(At(1.2), 1002, 1003, Order("B", BookSide.Bid, 997, 20, 1.2), Order("A", BookSide.Ask, 1006, 20, 0)), At(1.2));
            TestHarness.IsTrue(s.TopPair == null || s.TopPair.State != FloatingPairState.FloatingConfirmed, "not confirmed");
        }

        private static void LateCounterpartFails()
        {
            TestHarness.Begin("late counterpart beyond synchronization window fails");
            var d = Fresh();
            d.OnBookSnapshot(Book(At(0), 1000, 1001, Order("B", BookSide.Bid, 995, 20, 0), Order("A", BookSide.Ask, 1006, 20, 0)), At(0));
            d.OnBookSnapshot(Book(At(0.7), 1001, 1002, Order("B", BookSide.Bid, 996, 20, 0.1), Order("A", BookSide.Ask, 1007, 20, 0.7)), At(0.7));
            var s = d.OnBookSnapshot(Book(At(1.4), 1002, 1003, Order("B", BookSide.Bid, 997, 20, 0.8), Order("A", BookSide.Ask, 1008, 20, 1.4)), At(1.4));
            TestHarness.IsTrue(s.TopPair == null || s.TopPair.FollowRatio < 0.8, "follow ratio below confirmation threshold");
        }

        private static void OutsideMaxDistanceIgnored()
        {
            TestHarness.Begin("orders beyond max distance ignored");
            var d = Fresh();
            var s = d.OnBookSnapshot(Book(At(0), 1000, 1001, Order("B", BookSide.Bid, 700, 20, 0), Order("A", BookSide.Ask, 1301, 20, 0)), At(0));
            TestHarness.AreEqual(0, s.EligibleLargeBidCount, "far bid not eligible");
            TestHarness.AreEqual(0, s.ExactSizeCandidateCount, "no pair");
        }

        private static void MultipleSameSizeOneToOne()
        {
            TestHarness.Begin("multiple equal-size orders are matched one-to-one");
            var d = Fresh();
            var s = d.OnBookSnapshot(Book(At(0), 1000, 1001,
                Order("B1", BookSide.Bid, 995, 20, 0),
                Order("B2", BookSide.Bid, 994, 20, 0),
                Order("A1", BookSide.Ask, 1006, 20, 0),
                Order("A2", BookSide.Ask, 1007, 20, 0)), At(0));
            TestHarness.AreEqual(2, s.ExactSizeCandidateCount, "two candidates");
            TestHarness.AreEqual(2, s.TopPairs.Count, "two active pairs");
        }

        private static void SameIdRepricePreservesPair()
        {
            TestHarness.Begin("same-ID repricing preserves pair");
            var d = Fresh();
            d.OnBookSnapshot(Book(At(0), 1000, 1001, Order("B", BookSide.Bid, 995, 20, 0), Order("A", BookSide.Ask, 1006, 20, 0)), At(0));
            var s = d.OnBookSnapshot(Book(At(0.5), 1001, 1002, Order("B", BookSide.Bid, 996, 20, 0.5), Order("A", BookSide.Ask, 1007, 20, 0.5)), At(0.5));
            TestHarness.IsTrue(s.TopPair?.BidOrderId == "B", "bid id retained");
            TestHarness.IsTrue(s.TopPair?.AskOrderId == "A", "ask id retained");
        }

        private static void StrictCancelReentryStitching()
        {
            TestHarness.Begin("strict cancel/re-entry stitching");
            var d = Fresh();
            d.OnBookSnapshot(Book(At(0), 1000, 1001, Order("B", BookSide.Bid, 995, 20, 0), Order("A", BookSide.Ask, 1006, 20, 0)), At(0));
            d.OnOrderRemove("B", At(0.1));
            d.OnOrderUpsert("B2", BookSide.Bid, 995, 20, At(0.2));
            var s = d.OnBookSnapshot(Book(At(0.2), 1000, 1001, Order("B2", BookSide.Bid, 995, 20, 0.2), Order("A", BookSide.Ask, 1006, 20, 0)), At(0.2));
            TestHarness.IsTrue(s.TopPair?.BidOrderId == "B2", "new bid id stitched");
            TestHarness.IsTrue(s.TopPair?.UsesHeuristicStitching == true, "heuristic flag set");
        }

        private static void PartialFillBreaksPair()
        {
            TestHarness.Begin("partial fill breaks pair");
            var d = Fresh();
            d.OnBookSnapshot(Book(At(0), 1000, 1001, Order("B", BookSide.Bid, 995, 20, 0), Order("A", BookSide.Ask, 1006, 20, 0)), At(0));
            var s = d.OnBookSnapshot(Book(At(0.2), 1000, 1001, Order("B", BookSide.Bid, 995, 19, 0.2), Order("A", BookSide.Ask, 1006, 20, 0)), At(0.2));
            TestHarness.IsTrue(s.TopPair == null, "pair broken");
            TestHarness.IsTrue(s.LastBreak?.Reason.Contains("size mismatch") == true, "break reason");
        }

        private static void LegRemovalExpiresPair()
        {
            TestHarness.Begin("removing one leg expires pair");
            var d = Fresh();
            d.OnBookSnapshot(Book(At(0), 1000, 1001, Order("B", BookSide.Bid, 995, 20, 0), Order("A", BookSide.Ask, 1006, 20, 0)), At(0));
            var s = d.OnBookSnapshot(Book(At(0.5), 1000, 1001, Order("B", BookSide.Bid, 995, 20, 0)), At(0.5));
            TestHarness.IsTrue(s.TopPair == null, "no active displayed pair");
            s = d.OnBookSnapshot(Book(At(1.0), 1000, 1001, Order("B", BookSide.Bid, 995, 20, 0)), At(1.0));
            TestHarness.IsTrue(s.LastBreak?.Reason.Contains("missing leg") == true, "break emitted after stitching window");
        }

        private static void MbpModeReportsUnavailable()
        {
            TestHarness.Begin("MBO downgrade returns Unavailable");
            var d = Fresh();
            var s = d.OnBookSnapshot(Book(At(0), 1000, 1001, BookMode.Mbp, Array.Empty<MboOrderSnapshot>()), At(0));
            TestHarness.IsTrue(s.Status == FloatingPairDetectorStatus.Unavailable, "unavailable status");
            TestHarness.IsTrue(s.Quality == MetricQuality.Unavailable, "unavailable quality");
        }

        private static void EpochResetClearsState()
        {
            TestHarness.Begin("FLUSH/reconnect epoch reset clears pair state");
            var d = Fresh();
            d.OnBookSnapshot(Book(At(0), 1000, 1001, Order("B", BookSide.Bid, 995, 20, 0), Order("A", BookSide.Ask, 1006, 20, 0)), At(0));
            d.Reset(2, At(0.5));
            var s = d.OnBookSnapshot(Book(At(0.5), 1000, 1001, 2, Order("B", BookSide.Bid, 995, 20, 0.5)), At(0.5));
            TestHarness.IsTrue(s.TopPair == null, "state cleared");
        }

        private static void EpochMismatchPreventsStitching()
        {
            TestHarness.Begin("book epoch mismatch prevents stitching");
            var d = Fresh();
            d.OnBookSnapshot(Book(At(0), 1000, 1001, Order("B", BookSide.Bid, 995, 20, 0), Order("A", BookSide.Ask, 1006, 20, 0)), At(0));
            d.OnOrderRemove("B", At(0.1));
            d.Reset(2, At(0.15));
            d.OnOrderUpsert("B2", BookSide.Bid, 995, 20, At(0.2));
            var s = d.OnBookSnapshot(Book(At(0.2), 1000, 1001, 2, Order("B2", BookSide.Bid, 995, 20, 0.2), Order("A", BookSide.Ask, 1006, 20, 0.2)), At(0.2));
            TestHarness.IsTrue(s.TopPair?.UsesHeuristicStitching != true, "not stitched across epoch");
        }

        private static MboOrderSnapshot Order(string id, BookSide side, long ticks, long qty, double seenSeconds)
            => new(id, side, ticks, ticks * Tick, qty, 0, At(seenSeconds), At(seenSeconds), true, false);

        private static BookSnapshot Book(DateTime t, long bestBid, long bestAsk, params MboOrderSnapshot[] orders)
            => Book(t, bestBid, bestAsk, 1, BookMode.Mbo, orders);

        private static BookSnapshot Book(DateTime t, long bestBid, long bestAsk, long epoch, params MboOrderSnapshot[] orders)
            => Book(t, bestBid, bestAsk, epoch, BookMode.Mbo, orders);

        private static BookSnapshot Book(DateTime t, long bestBid, long bestAsk, BookMode mode, params MboOrderSnapshot[] orders)
            => Book(t, bestBid, bestAsk, 1, mode, orders);

        private static BookSnapshot Book(DateTime t, long bestBid, long bestAsk, long epoch, BookMode mode, params MboOrderSnapshot[] orders)
        {
            var bidLevels = orders.Where(o => o.Side == BookSide.Bid)
                .GroupBy(o => o.PriceTicks)
                .OrderByDescending(g => g.Key)
                .Select(g => new BookLevelSnapshot(BookSide.Bid, g.Key, g.Key * Tick, g.Sum(x => x.Quantity), g.Count()))
                .ToArray();
            var askLevels = orders.Where(o => o.Side == BookSide.Ask)
                .GroupBy(o => o.PriceTicks)
                .OrderBy(g => g.Key)
                .Select(g => new BookLevelSnapshot(BookSide.Ask, g.Key, g.Key * Tick, g.Sum(x => x.Quantity), g.Count()))
                .ToArray();

            if (bidLevels.Length == 0)
                bidLevels = new[] { new BookLevelSnapshot(BookSide.Bid, bestBid, bestBid * Tick, 1, 1) };
            if (askLevels.Length == 0)
                askLevels = new[] { new BookLevelSnapshot(BookSide.Ask, bestAsk, bestAsk * Tick, 1, 1) };

            return new BookSnapshot("MNQ", t, t, epoch, mode, BookLifecycleState.Valid,
                MetricQuality.Exact, true, true, mode == BookMode.Mbo ? 1.0 : 0.0, TimeSpan.Zero,
                bestBid == bestAsk, bestBid > bestAsk, bestBid, bestAsk, bestBid * Tick, bestAsk * Tick,
                bidLevels, askLevels, mode == BookMode.Mbo ? orders : Array.Empty<MboOrderSnapshot>(),
                new BookEventStats(0, 0, 0, 0, 0, 0, 0), null);
        }
    }
}
