using System;
using QT.Core.Primitives;
using QT.Core.Quality;
using QT.Market.Events;
using QT.Market.Lifecycle;
using QT.Market.OrderBook;

namespace MBO_Market_Data_Analytics.Tests
{
    public static class MboOrderBookTests
    {
        private const double Tick = 0.25;
        private static readonly DateTime T = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        public static void RunAll()
        {
            Console.WriteLine("V2 OrderBook lifecycle:");
            AddCreatesOrder();
            UpdateChangesSize();
            RemoveByClosedKeepsStoredFields();
            CoverageCountersRealVsSynthetic();
            ResetCoverageCounters();
            SyntheticKeyFallback();
            BestBidAskSelection();
            QueueAheadSize();
            InitialSnapshotToValidTwoSidedBook();
            EmptySnapshotRejected();
            OneSidedSnapshotRejected();
            FlushIncrementsEpochAndClearsBook();
            ReconnectIncrementsEpoch();
            BufferOverflowInvalidatesEpoch();
            MboToMbpClearsIncompatibleState();
            MbpToMboDoesNotSynthesizeIdentities();
            LockedBookHandled();
            CrossedBookInvalidated();
            SnapshotSeedExcludedFromLiveCounts();
            StaleFeedProducesStaleQuality();
        }

        private static MboOrderBook FreshMbo() => new(Tick);

        private static void AddCreatesOrder()
        {
            TestHarness.Begin("Add creates a tracked V2 MBO order");
            var ob = FreshMbo();
            var ev = ob.Apply(T, "A", BookSide.Bid, 400, 5, priority: 1, closed: false, isSnapshotSeed: false);
            TestHarness.IsTrue(ev.Action == NormalizedBookAction.Add, "action is Add");
            TestHarness.AreEqual(1, ob.OrderCount, "order count");
            TestHarness.AreEqual(1, ob.BidLevels, "bid levels");
        }

        private static void UpdateChangesSize()
        {
            TestHarness.Begin("Re-applying an existing V2 id updates it");
            var ob = FreshMbo();
            ob.Apply(T, "A", BookSide.Bid, 400, 5, 1, false, false);
            var ev = ob.Apply(T, "A", BookSide.Bid, 400, 9, 1, false, false);
            TestHarness.IsTrue(ev.Action == NormalizedBookAction.Update, "action is Update");
            TestHarness.AreEqual(1, ob.OrderCount, "still one order");
            ob.TryBestBid(out _, out long size);
            TestHarness.AreEqual(9, (int)size, "level size updated");
        }

        private static void RemoveByClosedKeepsStoredFields()
        {
            TestHarness.Begin("Remove emits stored order fields when feed sends empty/zero");
            var ob = FreshMbo();
            ob.Apply(T, "A", BookSide.Bid, 400, 5, 7, false, false);
            var ev = ob.Apply(T, "A", BookSide.Ask, 0, 0, 0, true, false);
            TestHarness.IsTrue(ev.Action == NormalizedBookAction.Remove, "action is Remove");
            TestHarness.IsTrue(ev.Before?.Side == BookSide.Bid, "stored side retained");
            TestHarness.AreEqual(400, (int)(ev.Before?.PriceTicks ?? 0), "stored price retained");
            TestHarness.AreEqual(5, (int)(ev.Before?.Quantity ?? 0), "stored size retained");
            TestHarness.AreEqual(0, ob.OrderCount, "order removed");
        }

        private static void CoverageCountersRealVsSynthetic()
        {
            TestHarness.Begin("Coverage counters separate real ids from synthetic");
            var ob = FreshMbo();
            ob.Apply(T, "real-1", BookSide.Bid, 400, 1, 0, false, false);
            ob.Apply(T, "real-2", BookSide.Bid, 401, 1, 0, false, false);
            ob.Apply(T, null, BookSide.Bid, 399, 1, 0, false, false);
            TestHarness.AreEqual(2, (int)ob.RealIdEvents, "real id events");
            TestHarness.AreEqual(1, (int)ob.SyntheticIdEvents, "synthetic id events");
            TestHarness.AreEqual(2.0 / 3.0, ob.RealIdCoverage, 1e-9, "coverage fraction");
        }

        private static void ResetCoverageCounters()
        {
            TestHarness.Begin("Coverage counters reset on new epoch");
            var ob = FreshMbo();
            ob.Apply(T, "real-1", BookSide.Bid, 400, 1, 0, false, false);
            ob.Apply(T, null, BookSide.Bid, 399, 1, 0, false, false);
            ob.ResetCoverageCounters();
            TestHarness.AreEqual(0, (int)ob.RealIdEvents, "real id events reset");
            TestHarness.AreEqual(0, (int)ob.SyntheticIdEvents, "synthetic id events reset");
        }

        private static void SyntheticKeyFallback()
        {
            TestHarness.Begin("Synthetic key: same price+side without id collapses to one order");
            var ob = FreshMbo();
            ob.Apply(T, null, BookSide.Bid, 400, 5, 0, false, false);
            var ev = ob.Apply(T, "", BookSide.Bid, 400, 3, 0, false, false);
            TestHarness.IsTrue(ev.Action == NormalizedBookAction.Update, "second event is Update");
            TestHarness.AreEqual(1, ob.OrderCount, "still one synthetic order");
            ob.Apply(T, null, BookSide.Bid, 399, 2, 0, false, false);
            TestHarness.AreEqual(2, ob.OrderCount, "different price is distinct synthetic order");
        }

        private static void BestBidAskSelection()
        {
            TestHarness.Begin("Best bid is highest tick; best ask is lowest tick");
            var ob = FreshMbo();
            ob.Apply(T, "b1", BookSide.Bid, 400, 5, 0, false, false);
            ob.Apply(T, "b2", BookSide.Bid, 404, 3, 0, false, false);
            ob.Apply(T, "a1", BookSide.Ask, 412, 4, 0, false, false);
            ob.Apply(T, "a2", BookSide.Ask, 408, 6, 0, false, false);
            TestHarness.IsTrue(ob.TryBestBid(out long bid, out long bidSz), "has best bid");
            TestHarness.AreEqual(404, (int)bid, "best bid ticks");
            TestHarness.AreEqual(3, (int)bidSz, "best bid size");
            TestHarness.IsTrue(ob.TryBestAsk(out long ask, out long askSz), "has best ask");
            TestHarness.AreEqual(408, (int)ask, "best ask ticks");
            TestHarness.AreEqual(6, (int)askSz, "best ask size");
        }

        private static void QueueAheadSize()
        {
            TestHarness.Begin("Queue-ahead sums same price+side size with better priority");
            var ob = FreshMbo();
            ob.Apply(T, "P1", BookSide.Bid, 400, 5, 1, false, false);
            ob.Apply(T, "P2", BookSide.Bid, 400, 3, 2, false, false);
            ob.Apply(T, "P3", BookSide.Bid, 400, 7, 3, false, false);
            TestHarness.AreEqual(0, (int)ob.QueueAheadSize("P1"), "front has nothing ahead");
            TestHarness.AreEqual(5, (int)ob.QueueAheadSize("P2"), "P2 has P1 ahead");
            TestHarness.AreEqual(8, (int)ob.QueueAheadSize("P3"), "P3 has P1+P2 ahead");
        }

        private static void InitialSnapshotToValidTwoSidedBook()
        {
            TestHarness.Begin("Initial snapshot to valid two-sided book");
            var e = Engine();
            e.StartSubscription("MNQ", BookMode.Mbo, T);
            e.BeginSnapshot(BookMode.Mbo, T, "test");
            e.ApplySnapshotLevel(T, BookSide.Bid, 400, 10, "B");
            e.ApplySnapshotLevel(T, BookSide.Ask, 401, 12, "A");
            var s = e.EndSnapshot(T, T);
            TestHarness.IsTrue(s.LifecycleState == BookLifecycleState.Valid, "state Valid");
            TestHarness.IsTrue(s.HasTwoSidedBook, "two-sided");
            TestHarness.AreEqual(0, (int)s.Stats.LiveBookEventCount, "snapshot seed excluded from live count");
        }

        private static void EmptySnapshotRejected()
        {
            TestHarness.Begin("Empty snapshot rejected");
            var e = Engine();
            e.StartSubscription("MNQ", BookMode.Mbo, T);
            e.BeginSnapshot(BookMode.Mbo, T, "test");
            var s = e.EndSnapshot(T, T);
            TestHarness.IsTrue(s.LifecycleState == BookLifecycleState.Invalid, "invalid state");
            TestHarness.IsTrue(s.DataQuality == MetricQuality.Invalid, "invalid quality");
        }

        private static void OneSidedSnapshotRejected()
        {
            TestHarness.Begin("One-sided snapshot rejected");
            var e = Engine();
            e.StartSubscription("MNQ", BookMode.Mbp, T);
            e.BeginSnapshot(BookMode.Mbp, T, "test");
            e.ApplySnapshotLevel(T, BookSide.Bid, 400, 10);
            var s = e.EndSnapshot(T, T);
            TestHarness.IsTrue(s.LifecycleState == BookLifecycleState.Invalid, "invalid state");
            TestHarness.IsTrue(!s.AskSideValid, "ask side invalid");
        }

        private static void FlushIncrementsEpochAndClearsBook()
        {
            TestHarness.Begin("Zero-price FLUSH handled and increments epoch");
            var e = SeededEngine(BookMode.Mbo);
            long epoch = e.BookEpoch;
            var s = e.OnMarketEvent(NormalizedMarketEvent.Flush(1, T.AddSeconds(1), T.AddSeconds(1), "MNQ"));
            TestHarness.IsTrue(s.BookEpoch == epoch + 1, "epoch incremented");
            TestHarness.IsTrue(!s.HasTwoSidedBook, "book cleared");
            TestHarness.IsTrue(s.LifecycleState == BookLifecycleState.AwaitingSnapshot, "awaiting snapshot");
        }

        private static void ReconnectIncrementsEpoch()
        {
            TestHarness.Begin("Reconnect increments epoch");
            var e = SeededEngine(BookMode.Mbp);
            long epoch = e.BookEpoch;
            var s = e.MarkReconnect(T.AddSeconds(2), "test reconnect");
            TestHarness.IsTrue(s.BookEpoch == epoch + 1, "epoch incremented");
            TestHarness.IsTrue(s.LifecycleState == BookLifecycleState.Recovering, "recovering");
        }

        private static void BufferOverflowInvalidatesEpoch()
        {
            TestHarness.Begin("Buffer overflow invalidates epoch");
            var e = SeededEngine(BookMode.Mbp);
            long epoch = e.BookEpoch;
            var s = e.MarkBufferOverflow(T.AddSeconds(2), "overflow");
            TestHarness.IsTrue(s.BookEpoch == epoch + 1, "epoch incremented");
            TestHarness.IsTrue(s.LifecycleState == BookLifecycleState.Invalid, "invalid");
        }

        private static void MboToMbpClearsIncompatibleState()
        {
            TestHarness.Begin("MBO-to-MBP clears incompatible MBO state");
            var e = SeededEngine(BookMode.Mbo);
            var s = e.DowngradeToMbp(T.AddSeconds(1), "coverage");
            TestHarness.IsTrue(s.Mode == BookMode.Mbp, "mode MBP");
            TestHarness.AreEqual(0, s.MboOrders.Count, "MBO orders cleared");
            TestHarness.IsTrue(!s.HasTwoSidedBook, "book must be reseeded");
        }

        private static void MbpToMboDoesNotSynthesizeIdentities()
        {
            TestHarness.Begin("MBP-to-MBO does not synthesize order identities");
            var e = SeededEngine(BookMode.Mbp);
            e.BeginSnapshot(BookMode.Mbo, T.AddSeconds(1), "mode transition");
            e.ApplySnapshotLevel(T.AddSeconds(1), BookSide.Bid, 400, 10, orderId: null);
            e.ApplySnapshotLevel(T.AddSeconds(1), BookSide.Ask, 401, 10, orderId: null);
            var s = e.EndSnapshot(T.AddSeconds(1), T.AddSeconds(1));
            TestHarness.IsTrue(s.Mode == BookMode.Mbo, "mode MBO");
            TestHarness.IsTrue(s.MboIdentityCompleteness == 0, "no real identity coverage synthesized");
        }

        private static void LockedBookHandled()
        {
            TestHarness.Begin("Locked book handled");
            var e = Engine();
            e.StartSubscription("MNQ", BookMode.Mbp, T);
            e.BeginSnapshot(BookMode.Mbp, T, "test");
            e.ApplySnapshotLevel(T, BookSide.Bid, 400, 10);
            e.ApplySnapshotLevel(T, BookSide.Ask, 400, 10);
            var s = e.EndSnapshot(T, T);
            TestHarness.IsTrue(s.IsLocked, "locked flag");
            TestHarness.IsTrue(s.LifecycleState == BookLifecycleState.Valid, "locked is handled, not crossed invalid");
        }

        private static void CrossedBookInvalidated()
        {
            TestHarness.Begin("Crossed book invalidated");
            var e = Engine();
            e.StartSubscription("MNQ", BookMode.Mbp, T);
            e.BeginSnapshot(BookMode.Mbp, T, "test");
            e.ApplySnapshotLevel(T, BookSide.Bid, 402, 10);
            e.ApplySnapshotLevel(T, BookSide.Ask, 401, 10);
            var s = e.EndSnapshot(T, T);
            TestHarness.IsTrue(s.IsCrossed, "crossed flag");
            TestHarness.IsTrue(s.LifecycleState == BookLifecycleState.Invalid, "invalid state");
        }

        private static void SnapshotSeedExcludedFromLiveCounts()
        {
            TestHarness.Begin("Snapshot seed excluded from live activity metrics");
            var e = SeededEngine(BookMode.Mbp);
            var evt = NormalizedMarketEvent.BookLevel(1, T.AddSeconds(1), T.AddSeconds(1), "MNQ",
                BookSide.Bid, 400, 100, 11, null, false);
            var s = e.OnMarketEvent(evt);
            TestHarness.AreEqual(2, (int)s.Stats.SnapshotSeedEventCount, "two seed rows");
            TestHarness.AreEqual(1, (int)s.Stats.LiveBookEventCount, "one live row");
        }

        private static void StaleFeedProducesStaleQuality()
        {
            TestHarness.Begin("Stale feed produces stale quality");
            var e = SeededEngine(BookMode.Mbp);
            var s = e.Snapshot(T.AddSeconds(10));
            TestHarness.IsTrue(s.DataQuality == MetricQuality.Stale, "stale quality");
        }

        private static OrderBookEngine Engine() => new(new OrderBookEngineConfig
        {
            Symbol = "MNQ",
            TickSize = Tick,
            PreferredMode = BookMode.Mbo,
            TopDepth = 10,
            StaleTimeout = TimeSpan.FromSeconds(3)
        });

        private static OrderBookEngine SeededEngine(BookMode mode)
        {
            var e = Engine();
            e.StartSubscription("MNQ", mode, T);
            e.BeginSnapshot(mode, T, "seed");
            e.ApplySnapshotLevel(T, BookSide.Bid, 400, 10, mode == BookMode.Mbo ? "B" : null);
            e.ApplySnapshotLevel(T, BookSide.Ask, 401, 10, mode == BookMode.Mbo ? "A" : null);
            e.EndSnapshot(T, T);
            return e;
        }
    }
}
