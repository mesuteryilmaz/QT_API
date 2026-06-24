using System;
using System.Linq;
using TradingPlatform.BusinessLayer;

namespace MBO_Market_Data_Analytics.Tests
{
    // H-12 entry-policy scenarios for ExecutionCore: at most one working entry across both sides,
    // opposite-side cancel/replace, exposure cap, and gating by lifecycle/risk.
    public static class ExecutionCoreEntryTests
    {
        private static (ExecutionCore core, FakeBroker broker) Build(int orderQty = 1, int maxExposure = 2, bool running = true)
        {
            var broker = new FakeBroker();
            var sched = new FakeScheduler(); // auto-run
            var core = new ExecutionCore(broker, sched, (_, __) => { },
                new ExecConfig { OrderQty = orderQty, MaxExposure = maxExposure });
            if (running) core.SetRunning();
            return (core, broker);
        }

        private static TrackedOrderState Entry(string id, Side side) => new TrackedOrderState
        {
            LocalId = Guid.NewGuid().ToString(),
            OrderId = id, Side = side, Price = 100.0, Quantity = 1,
            Status = OrderStatus.Opened, Role = OrderRole.Entry
        };

        public static void RunAll()
        {
            Console.WriteLine("ExecutionCore (H-12 entry policy):");
            FirstEntryIsPlaced();
            SameSideEntryIsBlocked();
            OppositeEntryIsCancelledNotStacked();
            ExposureCapBlocksEntry();
            BlockedWhenNotRunning();
        }

        private static void FirstEntryIsPlaced()
        {
            TestHarness.Begin("H-12: first entry is placed");
            var (core, broker) = Build();
            var r = core.TrySubmitEntry(Side.Buy, 100.0);
            TestHarness.IsTrue(r == EntrySubmitResult.Placed, "result is Placed");
            TestHarness.AreEqual(1, broker.Placed.Count(p => p.Kind == "EntryLimit"), "one entry placed");
            TestHarness.IsTrue(broker.Placed[0].Side == Side.Buy, "entry on buy side");
        }

        private static void SameSideEntryIsBlocked()
        {
            TestHarness.Begin("H-12: a second same-side entry does not stack");
            var (core, broker) = Build();
            core.Track(Entry("E1", Side.Buy)); // already working
            var r = core.TrySubmitEntry(Side.Buy, 100.0);
            TestHarness.IsTrue(r == EntrySubmitResult.SameSideExists, "result is SameSideExists");
            TestHarness.AreEqual(0, broker.Placed.Count(p => p.Kind == "EntryLimit"), "no new entry placed");
        }

        private static void OppositeEntryIsCancelledNotStacked()
        {
            TestHarness.Begin("H-12: an opposite-side working entry is cancelled, not stacked");
            var (core, broker) = Build();
            core.Track(Entry("E1", Side.Buy)); // working buy entry
            var r = core.TrySubmitEntry(Side.Sell, 99.0);
            TestHarness.IsTrue(r == EntrySubmitResult.OppositeCancelled, "result is OppositeCancelled");
            TestHarness.IsTrue(broker.Cancelled.Contains("E1"), "the opposite buy entry was cancelled");
            TestHarness.AreEqual(0, broker.Placed.Count(p => p.Kind == "EntryLimit"), "no new entry placed this tick");
        }

        private static void ExposureCapBlocksEntry()
        {
            TestHarness.Begin("H-12: exposure cap blocks an entry beyond MaxExposure");
            var (core, broker) = Build(orderQty: 2, maxExposure: 2);
            // Open a long 2 via a filled entry (no brackets: default getBracketTicks => (0,0)).
            core.Track(new TrackedOrderState
            {
                LocalId = Guid.NewGuid().ToString(), OrderId = "E0", Side = Side.Buy,
                Price = 100.0, Quantity = 2, Status = OrderStatus.Opened, Role = OrderRole.Entry
            });
            core.ApplyOrderUpdate(new OrderSnapshot("E0", Side.Buy, OrderStatus.Filled, 2, 2, 100.0));
            TestHarness.AreEqual(2.0, core.PositionSize, 1e-9, "long 2 established");

            var r = core.TrySubmitEntry(Side.Buy, 100.0); // 2 + 2 = 4 > MaxExposure 2
            TestHarness.IsTrue(r == EntrySubmitResult.ExposureExceeded, "result is ExposureExceeded");
            TestHarness.AreEqual(0, broker.Placed.Count(p => p.Kind == "EntryLimit"), "no entry placed beyond cap");
        }

        private static void BlockedWhenNotRunning()
        {
            TestHarness.Begin("H-12: entries are blocked when not Running");
            var (core, broker) = Build(running: false); // still Initializing
            var r = core.TrySubmitEntry(Side.Buy, 100.0);
            TestHarness.IsTrue(r == EntrySubmitResult.Blocked, "result is Blocked");
            TestHarness.AreEqual(0, broker.Placed.Count, "nothing placed");
        }
    }
}
