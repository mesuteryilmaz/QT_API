using System;

namespace MBO_Market_Data_Analytics.Tests
{
    // Validates the order-keyed book reconstruction: add/update/remove semantics, M-01 (remove
    // callbacks emit the STORED order's fields even when the feed sends empty/zero), M-02 coverage
    // counter reset, synthetic-key fallback, best-bid/ask selection, and queue-ahead.
    public static class MboOrderBookTests
    {
        private const double Tick = 0.25;
        private static readonly DateTime T = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        public static void RunAll()
        {
            Console.WriteLine("MboOrderBook:");
            AddCreatesOrder();
            UpdateChangesSize();
            RemoveByClosedEmitsStoredFields_M01();
            RemoveBySizeZeroEmitsStoredFields_M01();
            CoverageCountersRealVsSynthetic();
            ResetCoverageCounters_M02();
            SyntheticKeyFallback();
            BestBidAskSelection();
            QueueAheadSize();
        }

        private static void AddCreatesOrder()
        {
            TestHarness.Begin("Add creates a tracked order");
            var ob = new MboOrderBook(Tick);
            var ev = ob.Apply(T, "A", isBid: true, price: 100.0, size: 5, priority: 1, numberOrders: 1, closed: false);
            TestHarness.IsTrue(ev.Action == MboAction.Add, "action is Add");
            TestHarness.AreEqual(1, ob.OrderCount, "order count");
            TestHarness.AreEqual(1, ob.BidLevels, "bid levels");
        }

        private static void UpdateChangesSize()
        {
            TestHarness.Begin("Re-applying an existing id updates it");
            var ob = new MboOrderBook(Tick);
            ob.Apply(T, "A", true, 100.0, 5, 1, 1, false);
            var ev = ob.Apply(T, "A", true, 100.0, 9, 1, 1, false);
            TestHarness.IsTrue(ev.Action == MboAction.Update, "action is Update");
            TestHarness.AreEqual(1, ob.OrderCount, "still one order");
            ob.TryGetBestBid(out _, out double size);
            TestHarness.AreEqual(9.0, size, 1e-9, "level size updated");
        }

        private static void RemoveByClosedEmitsStoredFields_M01()
        {
            TestHarness.Begin("M-01: Remove(closed) emits stored order fields when feed sends empty/zero");
            var ob = new MboOrderBook(Tick);
            ob.Apply(T, "A", isBid: true, price: 100.0, size: 5, priority: 7, numberOrders: 1, closed: false);
            // Feed sends a closed callback with the WRONG side and zero price/size — common in practice.
            var ev = ob.Apply(T, "A", isBid: false, price: 0.0, size: 0, priority: 0, numberOrders: 0, closed: true);
            TestHarness.IsTrue(ev.Action == MboAction.Remove, "action is Remove");
            TestHarness.IsTrue(ev.IsBid, "emitted side is the stored bid side");
            TestHarness.AreEqual(100.0, ev.Price, 1e-9, "emitted price is the stored price");
            TestHarness.AreEqual(5, ev.Size, 1e-9, "emitted size is the stored size");
            TestHarness.IsTrue(ev.Priority == 7, "emitted priority is the stored priority");
            TestHarness.AreEqual(0, ob.OrderCount, "order removed");
        }

        private static void RemoveBySizeZeroEmitsStoredFields_M01()
        {
            TestHarness.Begin("M-01: size<=0 (not flagged closed) is also a Remove with stored fields");
            var ob = new MboOrderBook(Tick);
            ob.Apply(T, "B", isBid: false, price: 102.0, size: 4, priority: 3, numberOrders: 1, closed: false);
            var ev = ob.Apply(T, "B", isBid: false, price: 102.0, size: 0, priority: 0, numberOrders: 0, closed: false);
            TestHarness.IsTrue(ev.Action == MboAction.Remove, "action is Remove");
            TestHarness.AreEqual(4, ev.Size, 1e-9, "emitted size is the stored size");
            TestHarness.AreEqual(102.0, ev.Price, 1e-9, "emitted price is the stored price");
            TestHarness.AreEqual(0, ob.OrderCount, "order removed");
        }

        private static void CoverageCountersRealVsSynthetic()
        {
            TestHarness.Begin("Coverage counters separate real ids from synthetic");
            var ob = new MboOrderBook(Tick);
            ob.Apply(T, "real-1", true, 100.0, 1, 0, 0, false);
            ob.Apply(T, "real-2", true, 100.25, 1, 0, 0, false);
            ob.Apply(T, null, true, 99.75, 1, 0, 0, false);   // synthetic
            TestHarness.AreEqual(2, ob.RealIdEvents, "real id events");
            TestHarness.AreEqual(1, ob.SyntheticIdEvents, "synthetic id events");
            TestHarness.AreEqual(2.0 / 3.0, ob.RealIdCoverage, 1e-9, "coverage fraction");
        }

        private static void ResetCoverageCounters_M02()
        {
            TestHarness.Begin("M-02: ResetCoverageCounters zeroes the coverage stats");
            var ob = new MboOrderBook(Tick);
            ob.Apply(T, "real-1", true, 100.0, 1, 0, 0, false);
            ob.Apply(T, null, true, 99.75, 1, 0, 0, false);
            ob.ResetCoverageCounters();
            TestHarness.AreEqual(0, ob.RealIdEvents, "real id events reset");
            TestHarness.AreEqual(0, ob.SyntheticIdEvents, "synthetic id events reset");
            TestHarness.AreEqual(0.0, ob.RealIdCoverage, 1e-9, "coverage reset to 0");
        }

        private static void SyntheticKeyFallback()
        {
            TestHarness.Begin("Synthetic key: same price+side without id collapses to one order");
            var ob = new MboOrderBook(Tick);
            ob.Apply(T, null, true, 100.0, 5, 0, 0, false);  // add synthetic B:400
            var ev = ob.Apply(T, "", true, 100.0, 3, 0, 0, false); // same synthetic key -> update
            TestHarness.IsTrue(ev.Action == MboAction.Update, "second event is Update (same key)");
            TestHarness.AreEqual(1, ob.OrderCount, "still one synthetic order");
            // Different price -> different synthetic key -> separate order.
            ob.Apply(T, null, true, 99.75, 2, 0, 0, false);
            TestHarness.AreEqual(2, ob.OrderCount, "different price is a distinct synthetic order");
        }

        private static void BestBidAskSelection()
        {
            TestHarness.Begin("Best bid is the highest bid tick; best ask the lowest ask tick");
            var ob = new MboOrderBook(Tick);
            ob.Apply(T, "b1", true, 100.0, 5, 0, 0, false);
            ob.Apply(T, "b2", true, 101.0, 3, 0, 0, false);
            ob.Apply(T, "a1", false, 103.0, 4, 0, 0, false);
            ob.Apply(T, "a2", false, 102.0, 6, 0, 0, false);
            TestHarness.IsTrue(ob.TryGetBestBid(out double bidPx, out double bidSz), "has best bid");
            TestHarness.AreEqual(101.0, bidPx, 1e-9, "best bid price");
            TestHarness.AreEqual(3, bidSz, 1e-9, "best bid size");
            TestHarness.IsTrue(ob.TryGetBestAsk(out double askPx, out double askSz), "has best ask");
            TestHarness.AreEqual(102.0, askPx, 1e-9, "best ask price");
            TestHarness.AreEqual(6, askSz, 1e-9, "best ask size");
        }

        private static void QueueAheadSize()
        {
            TestHarness.Begin("Queue-ahead sums same price+side size with better (lower) priority");
            var ob = new MboOrderBook(Tick);
            ob.Apply(T, "P1", true, 100.0, 5, priority: 1, numberOrders: 0, closed: false);
            ob.Apply(T, "P2", true, 100.0, 3, priority: 2, numberOrders: 0, closed: false);
            ob.Apply(T, "P3", true, 100.0, 7, priority: 3, numberOrders: 0, closed: false);
            TestHarness.AreEqual(0.0, ob.QueueAheadSize("P1"), 1e-9, "front of queue has nothing ahead");
            TestHarness.AreEqual(5.0, ob.QueueAheadSize("P2"), 1e-9, "P2 has P1 ahead");
            TestHarness.AreEqual(8.0, ob.QueueAheadSize("P3"), 1e-9, "P3 has P1+P2 ahead");
        }
    }
}
