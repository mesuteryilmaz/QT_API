using System;
using System.Collections.Generic;
using System.Linq;
using TradingPlatform.BusinessLayer;

namespace MBO_Market_Data_Analytics.Tests
{
    // Reconcile / teardown API used by the strategy adapter: BeginRun reset, SetPosition,
    // foreign-exposure gate, ClearOrdersNotIn, and Shutdown teardown.
    public static class ExecutionCoreReconcileTests
    {
        private static (ExecutionCore core, FakeBroker broker, FakeScheduler sched) Build()
        {
            var broker = new FakeBroker();
            var sched = new FakeScheduler();
            var core = new ExecutionCore(broker, sched, (_, __) => { }, new ExecConfig());
            core.SetRunning();
            return (core, broker, sched);
        }

        private static TrackedOrderState Order(string id, OrderRole role) => new TrackedOrderState
        {
            LocalId = Guid.NewGuid().ToString(),
            OrderId = id, Side = Side.Sell, Price = 100.0, Quantity = 1,
            Status = OrderStatus.Opened, Role = role, IsStopHint = role == OrderRole.Bracket ? true : (bool?)null
        };

        public static void RunAll()
        {
            Console.WriteLine("ExecutionCore (reconcile / teardown API):");
            BeginRunResetsState();
            SetPositionRestoresOwnedPosition();
            ForeignExposureGate();
            ClearOrdersNotInDropsTerminated();
            ShutdownCancelsAndFlattens();
        }

        private static void BeginRunResetsState()
        {
            TestHarness.Begin("BeginRun resets state and stamps the run id");
            var (core, broker, _) = Build();
            core.Track(Order("X1", OrderRole.Bracket));
            core.SetPosition(2.0, 100.0);
            core.SetForeignExposure(true);

            core.BeginRun("run-abc");
            TestHarness.IsTrue(core.RunId == "run-abc", "run id stamped");
            TestHarness.AreEqual(0.0, core.PositionSize, 1e-9, "position cleared");
            TestHarness.IsTrue(!core.ForeignExposureDetected, "foreign exposure cleared");
            TestHarness.IsTrue(!core.IsTracked("X1"), "tracked orders cleared");
            TestHarness.IsTrue(core.Lifecycle == StrategyLifecycleState.Initializing, "lifecycle back to Initializing");
        }

        private static void SetPositionRestoresOwnedPosition()
        {
            TestHarness.Begin("SetPosition restores owned position (flat zeroes the avg)");
            var (core, _, _) = Build();
            core.SetPosition(-3.0, 99.5);
            TestHarness.AreEqual(-3.0, core.PositionSize, 1e-9, "short 3 restored");
            TestHarness.AreEqual(99.5, core.AverageEntryPrice, 1e-9, "avg restored");
            core.SetPosition(0.0, 99.5);
            TestHarness.AreEqual(0.0, core.AverageEntryPrice, 1e-9, "flat zeroes the average");
        }

        private static void ForeignExposureGate()
        {
            TestHarness.Begin("Foreign-exposure flag round-trips");
            var (core, _, _) = Build();
            TestHarness.IsTrue(!core.ForeignExposureDetected, "default false");
            core.SetForeignExposure(true);
            TestHarness.IsTrue(core.ForeignExposureDetected, "set true");
            core.SetForeignExposure(false);
            TestHarness.IsTrue(!core.ForeignExposureDetected, "set false");
        }

        private static void ClearOrdersNotInDropsTerminated()
        {
            TestHarness.Begin("ClearOrdersNotIn drops tracked orders absent from the live set");
            var (core, _, _) = Build();
            core.Track(Order("A", OrderRole.Entry));
            core.Track(Order("B", OrderRole.Bracket));
            core.Track(Order("C", OrderRole.Bracket));
            core.ClearOrdersNotIn(new HashSet<string> { "B" }); // only B is still live
            TestHarness.IsTrue(!core.IsTracked("A") && core.IsTracked("B") && !core.IsTracked("C"),
                "A and C dropped, B kept");
        }

        private static void ShutdownCancelsAndFlattens()
        {
            TestHarness.Begin("Shutdown cancels working orders, flattens, and ends Stopped");
            var (core, broker, sched) = Build();
            core.Track(Order("SL1", OrderRole.Bracket));
            broker.NetPosition = 1.0; // open long at teardown

            core.Shutdown();

            TestHarness.IsTrue(broker.Cancelled.Contains("SL1"), "working order cancelled");
            TestHarness.AreEqual(1, broker.MarketCloseCount, "open position flattened");
            TestHarness.IsTrue(sched.ShutdownCalled, "scheduler shut down");
            TestHarness.IsTrue(core.Lifecycle == StrategyLifecycleState.Stopped, "lifecycle Stopped");
        }
    }
}
