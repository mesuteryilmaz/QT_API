using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradingPlatform.BusinessLayer;

namespace MBO_Market_Data_Analytics.Tests
{
    // Validates the test doubles themselves (FakeBroker / FakeScheduler) so the upcoming
    // ExecutionCore race tests build on a trusted, deterministic foundation.
    public static class FakesSmokeTests
    {
        public static void RunAll()
        {
            Console.WriteLine("Test doubles (FakeBroker / FakeScheduler):");
            BrokerRecordsAndAssignsIds();
            BrokerFailureFlags();
            SchedulerImmediateMode();
            SchedulerManualDrainOrdersByPriority();
            SchedulerCancelPendingEntries();
            SchedulerStopAcceptingBlocksPost();
        }

        private static void BrokerRecordsAndAssignsIds()
        {
            TestHarness.Begin("FakeBroker records operations and assigns stable ids");
            var b = new FakeBroker();
            var r1 = b.PlaceEntryLimit(Side.Buy, 100.0, 1, "c1");
            var r2 = b.PlaceProtectiveStop(Side.Sell, 99.0, 1, "c2");
            TestHarness.IsTrue(r1.Success && r2.Success, "both placements succeed by default");
            TestHarness.IsTrue(r1.OrderId == "FAKE-1", "first id is FAKE-1");
            TestHarness.IsTrue(r2.OrderId == "FAKE-2", "second id is FAKE-2");
            TestHarness.AreEqual(2, b.Placed.Count, "two placements recorded");
            TestHarness.IsTrue(b.Placed[1].Kind == "Stop", "second placement is a Stop");

            b.Cancel("FAKE-1");
            TestHarness.AreEqual(1, b.Cancelled.Count, "cancel recorded");
            b.ModifyQuantity("FAKE-2", 3);
            TestHarness.IsTrue(b.Modified.Count == 1 && b.Modified[0].Qty == 3, "modify recorded");

            b.NetPosition = -3.0;
            TestHarness.AreEqual(-3.0, b.GetNetPositionSize(), 1e-9, "net position returned");

            b.PlaceMarketClose(Side.Buy, 3, "");
            TestHarness.AreEqual(1, b.MarketCloseCount, "market close counted");
        }

        private static void BrokerFailureFlags()
        {
            TestHarness.Begin("FakeBroker failure flags drive results");
            var b = new FakeBroker { StopOrderTypeAvailable = false };
            TestHarness.IsTrue(!b.HasStopOrderType, "no stop type reported");
            TestHarness.IsTrue(!b.PlaceProtectiveStop(Side.Sell, 99, 1, "").Success, "stop fails with no stop type");

            var b2 = new FakeBroker { EntrySucceeds = false };
            TestHarness.IsTrue(!b2.PlaceEntryLimit(Side.Buy, 100, 1, "").Success, "entry fails when flagged");

            var b3 = new FakeBroker { MarketCloseSucceeds = false };
            TestHarness.IsTrue(!b3.PlaceMarketClose(Side.Sell, 1, "").Success, "market close fails when flagged");
            TestHarness.AreEqual(0, b3.MarketCloseCount, "failed close is not counted");
        }

        private static void SchedulerImmediateMode()
        {
            TestHarness.Begin("FakeScheduler auto-run executes posted work immediately");
            var s = new FakeScheduler();
            int ran = 0;
            s.Post(() => { ran++; return Task.CompletedTask; }, 2, TaskCategory.EntryPlacement);
            TestHarness.AreEqual(1, ran, "work ran on post");
            TestHarness.AreEqual(1, s.PostedCount, "posted count incremented");
        }

        private static void SchedulerManualDrainOrdersByPriority()
        {
            TestHarness.Begin("FakeScheduler manual mode drains in priority order");
            var s = new FakeScheduler { AutoRun = false };
            var order = new List<int>();
            s.Post(() => { order.Add(2); return Task.CompletedTask; }, 2, TaskCategory.General);
            s.Post(() => { order.Add(0); return Task.CompletedTask; }, 0, TaskCategory.Flatten);
            s.Post(() => { order.Add(1); return Task.CompletedTask; }, 1, TaskCategory.Protection);
            TestHarness.AreEqual(0, order.Count, "nothing runs before drain");
            s.Drain();
            TestHarness.IsTrue(order.Count == 3 && order[0] == 0 && order[1] == 1 && order[2] == 2,
                "drained in priority order (0,1,2)");
        }

        private static void SchedulerCancelPendingEntries()
        {
            TestHarness.Begin("FakeScheduler CancelPendingEntries drops only entry-placement work");
            var s = new FakeScheduler { AutoRun = false };
            s.Post(() => Task.CompletedTask, 2, TaskCategory.EntryPlacement);
            s.Post(() => Task.CompletedTask, 1, TaskCategory.Protection);
            s.Post(() => Task.CompletedTask, 2, TaskCategory.EntryPlacement);
            int removed = s.CancelPendingEntries();
            TestHarness.AreEqual(2, removed, "two entry tasks cancelled");
            TestHarness.AreEqual(1, s.Pending.Count, "protective task remains");
        }

        private static void SchedulerStopAcceptingBlocksPost()
        {
            TestHarness.Begin("FakeScheduler StopAccepting blocks further posts");
            var s = new FakeScheduler { AutoRun = false };
            s.StopAccepting();
            s.Post(() => Task.CompletedTask, 0, TaskCategory.General);
            TestHarness.AreEqual(0, s.PostedCount, "post rejected after StopAccepting");
            TestHarness.IsTrue(!s.Accepting, "scheduler no longer accepting");
        }
    }
}
