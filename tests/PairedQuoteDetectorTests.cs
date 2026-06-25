using System;

namespace MBO_Market_Data_Analytics.Tests
{
    // Deterministic tests for the MBO Paired Floating Quote detector (audit 4b). Covers exact-size
    // pairing, tiers, per-order (not aggregate) identity, distance gating, persistence, market-follow
    // confirmation, one-to-one assignment, leg removal, MBP-unavailable, and epoch reset.
    public static class PairedQuoteDetectorTests
    {
        private static readonly long T0 = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc).Ticks;
        private static long At(double seconds) => T0 + (long)(seconds * TimeSpan.TicksPerSecond);

        public static void RunAll()
        {
            Console.WriteLine("PairedQuoteDetector:");
            SymmetricPairBecomesCandidate();
            VeryLargeTierClassified();
            ExactSizeMismatchRejected();
            PerOrderNotAggregate();
            StaticPairBecomesPersistentNotConfirmed();
            FollowsTwoMovesBecomesConfirmed();
            OneLegMovesNotConfirmed();
            OutsideMaxDistanceIgnored();
            MultipleSameSizeOneToOne();
            LegRemovalExpiresPair();
            MbpModeReportsUnavailable();
            EpochResetClearsState();
        }

        private static PairedQuoteDetector Fresh()
        {
            var d = new PairedQuoteDetector();
            d.Reset(1);
            return d;
        }

        // best bid 1000 / ask 1001 (1-tick spread); a 20-lot bid at 995 and 20-lot ask at 1006 (both 5 off).
        private static void SymmetricPairBecomesCandidate()
        {
            TestHarness.Begin("20/20 symmetric pair becomes a candidate");
            var d = Fresh();
            d.OnOrderUpsert("B", true, 995, 20, At(0));
            d.OnOrderUpsert("A", false, 1006, 20, At(0));
            d.AdvanceTime(At(0), true, 1000, 1001, PqDataStatus.MboReady);
            var s = d.Snapshot;
            TestHarness.AreEqual(1, s.ActivePairs, "one active pair");
            TestHarness.AreEqual(0, s.ConfirmedPairs, "not yet confirmed");
            TestHarness.IsTrue(s.TopPairState == PairState.Candidate, "state Candidate");
            TestHarness.AreEqual(20, (int)s.TopPairSize, "top pair size 20");
            TestHarness.IsTrue(s.TopPairTier == PairTier.Large, "tier Large");
            TestHarness.AreEqual(1, s.EligibleLargeBids, "eligible large bids");
            TestHarness.AreEqual(1, s.EligibleLargeAsks, "eligible large asks");
        }

        private static void VeryLargeTierClassified()
        {
            TestHarness.Begin("200/200 pair is Very Large tier");
            var d = Fresh();
            d.OnOrderUpsert("B", true, 990, 200, At(0));
            d.OnOrderUpsert("A", false, 1011, 200, At(0));
            d.AdvanceTime(At(0), true, 1000, 1001, PqDataStatus.MboReady);
            var s = d.Snapshot;
            TestHarness.AreEqual(1, s.ActivePairs, "one active pair");
            TestHarness.IsTrue(s.TopPairTier == PairTier.VeryLarge, "tier VeryLarge");
            TestHarness.AreEqual(1, s.EligibleVeryLargeBids, "very large bid count");
        }

        private static void ExactSizeMismatchRejected()
        {
            TestHarness.Begin("20 bid vs 21 ask rejected under exact-size");
            var d = Fresh();
            d.OnOrderUpsert("B", true, 995, 20, At(0));
            d.OnOrderUpsert("A", false, 1006, 21, At(0));
            d.AdvanceTime(At(0), true, 1000, 1001, PqDataStatus.MboReady);
            TestHarness.AreEqual(0, d.Snapshot.ActivePairs, "no pair when sizes differ");
        }

        // A 20-lot order remains pairable even when an unrelated order shares its price level — the
        // detector keys on individual order IDs, not aggregate level volume (audit PQ-02).
        private static void PerOrderNotAggregate()
        {
            TestHarness.Begin("per-order identity: unrelated same-level order does not break the pair");
            var d = Fresh();
            d.OnOrderUpsert("B1", true, 995, 20, At(0));
            d.OnOrderUpsert("B2", true, 995, 7, At(0));   // unrelated, same price, sub-threshold
            d.OnOrderUpsert("A1", false, 1006, 20, At(0));
            d.AdvanceTime(At(0), true, 1000, 1001, PqDataStatus.MboReady);
            var s = d.Snapshot;
            TestHarness.AreEqual(1, s.ActivePairs, "20/20 pair still forms");
            TestHarness.AreEqual(20, (int)s.TopPairSize, "paired on the 20-lot, not 27 aggregate");
            TestHarness.AreEqual(1, s.EligibleLargeBids, "sub-threshold order not eligible");
        }

        private static void StaticPairBecomesPersistentNotConfirmed()
        {
            TestHarness.Begin("static pair becomes Persistent but not FloatingConfirmed");
            var d = Fresh();
            d.OnOrderUpsert("B", true, 995, 20, At(0));
            d.OnOrderUpsert("A", false, 1006, 20, At(0));
            d.AdvanceTime(At(0), true, 1000, 1001, PqDataStatus.MboReady);   // candidate, center set
            d.AdvanceTime(At(1.5), true, 1000, 1001, PqDataStatus.MboReady); // no market move, aged
            var s = d.Snapshot;
            TestHarness.IsTrue(s.TopPairState == PairState.Persistent, "state Persistent after aging");
            TestHarness.AreEqual(0, s.ConfirmedPairs, "no confirmation without market-follow");
        }

        private static void FollowsTwoMovesBecomesConfirmed()
        {
            TestHarness.Begin("pair following two BBO moves becomes FloatingConfirmed");
            var d = Fresh();
            d.OnOrderUpsert("B", true, 995, 20, At(0));
            d.OnOrderUpsert("A", false, 1006, 20, At(0));
            d.AdvanceTime(At(0.0), true, 1000, 1001, PqDataStatus.MboReady);

            // market +1 tick; both legs reprice up 1 tick to keep offsets
            d.OnOrderUpsert("B", true, 996, 20, At(0.6));
            d.OnOrderUpsert("A", false, 1007, 20, At(0.6));
            d.AdvanceTime(At(0.6), true, 1001, 1002, PqDataStatus.MboReady);

            // market +1 tick again; both legs follow
            d.OnOrderUpsert("B", true, 997, 20, At(1.2));
            d.OnOrderUpsert("A", false, 1008, 20, At(1.2));
            d.AdvanceTime(At(1.2), true, 1002, 1003, PqDataStatus.MboReady);

            var s = d.Snapshot;
            TestHarness.IsTrue(s.TopPairState == PairState.FloatingConfirmed, "state FloatingConfirmed");
            TestHarness.AreEqual(1, s.ConfirmedPairs, "one confirmed pair");
            TestHarness.AreEqual(2, s.TopPairSyncMoves, "two synchronized moves");
            TestHarness.AreEqual(1.0, s.TopPairFollowRatio, 1e-9, "follow ratio 100%");
        }

        private static void OneLegMovesNotConfirmed()
        {
            TestHarness.Begin("only one leg follows -> no confirmation, follow ratio falls");
            var d = Fresh();
            d.OnOrderUpsert("B", true, 995, 20, At(0));
            d.OnOrderUpsert("A", false, 1006, 20, At(0));
            d.AdvanceTime(At(0.0), true, 1000, 1001, PqDataStatus.MboReady);

            // market moves up twice; only the bid follows, the ask stays put
            d.OnOrderUpsert("B", true, 996, 20, At(0.6));
            d.AdvanceTime(At(0.6), true, 1001, 1002, PqDataStatus.MboReady);
            d.OnOrderUpsert("B", true, 997, 20, At(1.2));
            d.AdvanceTime(At(1.2), true, 1002, 1003, PqDataStatus.MboReady);

            var s = d.Snapshot;
            TestHarness.IsTrue(s.TopPairState != PairState.FloatingConfirmed, "not confirmed");
            TestHarness.IsTrue(s.TopPairFollowRatio < 0.8, "follow ratio below threshold");
        }

        private static void OutsideMaxDistanceIgnored()
        {
            TestHarness.Begin("orders beyond max distance from touch are ignored");
            var d = Fresh();
            d.OnOrderUpsert("B", true, 800, 20, At(0));   // 200 ticks below best bid (> 100)
            d.OnOrderUpsert("A", false, 1201, 20, At(0)); // 200 ticks above best ask
            d.AdvanceTime(At(0), true, 1000, 1001, PqDataStatus.MboReady);
            var s = d.Snapshot;
            TestHarness.AreEqual(0, s.ActivePairs, "no pair outside distance");
            TestHarness.AreEqual(0, s.EligibleLargeBids, "far order not eligible");
        }

        private static void MultipleSameSizeOneToOne()
        {
            TestHarness.Begin("multiple same-size orders get one-to-one assignment (no double count)");
            var d = Fresh();
            d.OnOrderUpsert("B1", true, 995, 20, At(0));
            d.OnOrderUpsert("B2", true, 994, 20, At(0));
            d.OnOrderUpsert("A1", false, 1006, 20, At(0));
            d.OnOrderUpsert("A2", false, 1007, 20, At(0));
            d.AdvanceTime(At(0), true, 1000, 1001, PqDataStatus.MboReady);
            var s = d.Snapshot;
            TestHarness.AreEqual(2, s.EligibleLargeBids, "two eligible bids");
            TestHarness.AreEqual(2, s.EligibleLargeAsks, "two eligible asks");
            TestHarness.AreEqual(2, s.ActivePairs, "exactly two one-to-one pairs");
        }

        private static void LegRemovalExpiresPair()
        {
            TestHarness.Begin("removing one leg expires the pair");
            var d = Fresh();
            d.OnOrderUpsert("B", true, 995, 20, At(0));
            d.OnOrderUpsert("A", false, 1006, 20, At(0));
            d.AdvanceTime(At(0), true, 1000, 1001, PqDataStatus.MboReady);
            TestHarness.AreEqual(1, d.Snapshot.ActivePairs, "pair present");
            d.OnOrderRemove("A", At(0.5));
            d.AdvanceTime(At(0.5), true, 1000, 1001, PqDataStatus.MboReady);
            TestHarness.AreEqual(0, d.Snapshot.ActivePairs, "pair expired after leg removal");
        }

        private static void MbpModeReportsUnavailable()
        {
            TestHarness.Begin("MBP / non-ready status reports Unavailable, not zero pairs");
            var d = Fresh();
            d.OnOrderUpsert("B", true, 995, 20, At(0));
            d.OnOrderUpsert("A", false, 1006, 20, At(0));
            d.AdvanceTime(At(0), true, 1000, 1001, PqDataStatus.Unavailable);
            var s = d.Snapshot;
            TestHarness.IsTrue(s.Status == PqDataStatus.Unavailable, "status Unavailable");
            TestHarness.AreEqual(0, s.ActivePairs, "no pairs published when unavailable");
        }

        private static void EpochResetClearsState()
        {
            TestHarness.Begin("epoch reset clears all order and pair state");
            var d = Fresh();
            d.OnOrderUpsert("B", true, 995, 20, At(0));
            d.OnOrderUpsert("A", false, 1006, 20, At(0));
            d.AdvanceTime(At(0), true, 1000, 1001, PqDataStatus.MboReady);
            TestHarness.AreEqual(1, d.Snapshot.ActivePairs, "pair present before reset");
            d.Reset(2);
            d.AdvanceTime(At(1), true, 1000, 1001, PqDataStatus.MboReady);
            TestHarness.AreEqual(0, d.Snapshot.ActivePairs, "no pairs after epoch reset");
            TestHarness.IsTrue(d.BookEpoch == 2, "epoch advanced");
        }
    }
}
