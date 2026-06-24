using System;
using TradingPlatform.BusinessLayer;

namespace MBO_Market_Data_Analytics.Tests
{
    // C-08 race scenarios for the lifecycle + idempotent flatten slice of ExecutionCore, driven by
    // FakeBroker/FakeScheduler. These are the regression net for the extraction.
    public static class ExecutionCoreFlattenTests
    {
        private static (ExecutionCore core, FakeBroker broker, FakeScheduler sched) Build(double netPosition)
        {
            var broker = new FakeBroker { NetPosition = netPosition };
            var sched = new FakeScheduler(); // auto-run: posted work executes immediately
            var core = new ExecutionCore(broker, sched, (_, __) => { }, new ExecConfig());
            core.SetRunning();
            return (core, broker, sched);
        }

        private static TrackedOrderState WorkingBracket(string id, Side side) => new TrackedOrderState
        {
            LocalId = Guid.NewGuid().ToString(),
            OrderId = id,
            Side = side,
            Price = 99.0,
            Quantity = 2,
            Status = OrderStatus.Opened,
            Role = OrderRole.Bracket,
            IsStopHint = true
        };

        public static void RunAll()
        {
            Console.WriteLine("ExecutionCore (C-08 flatten/lifecycle):");
            RepeatedFlattenPlacesExactlyOneClose();
            FlattenCancelsWorkingOrders();
            FlattenWhenAlreadyFlatPlacesNoCloseAndConfirms();
            RejectedCloseReleasesGuardForRetry();
            ConfirmFlatAdvancesToFlatVerifiedAfterHalt();
        }

        private static void RepeatedFlattenPlacesExactlyOneClose()
        {
            TestHarness.Begin("C-08: repeated FlattenPosition places exactly one market close");
            var (core, broker, _) = Build(netPosition: 2.0); // long 2
            core.FlattenPosition("first");
            core.FlattenPosition("second"); // duplicate while in flight -> no-op
            core.FlattenPosition("third");
            TestHarness.AreEqual(1, broker.MarketCloseCount, "exactly one market close despite three calls");
            TestHarness.IsTrue(core.FlattenInFlight, "flatten still in flight (fill not simulated)");
        }

        private static void FlattenCancelsWorkingOrders()
        {
            TestHarness.Begin("C-08: flatten cancels working tracked orders before closing");
            var (core, broker, _) = Build(netPosition: 1.0);
            core.Track(WorkingBracket("SL-1", Side.Sell));
            core.Track(WorkingBracket("TP-1", Side.Sell));
            core.FlattenPosition("flatten");
            TestHarness.IsTrue(broker.Cancelled.Contains("SL-1") && broker.Cancelled.Contains("TP-1"),
                "both working orders cancelled");
            TestHarness.AreEqual(1, broker.MarketCloseCount, "one market close");
        }

        private static void FlattenWhenAlreadyFlatPlacesNoCloseAndConfirms()
        {
            TestHarness.Begin("C-08: flatten when already flat places no close and clears the guard");
            var (core, broker, _) = Build(netPosition: 0.0); // already flat
            core.FlattenPosition("flatten");
            TestHarness.AreEqual(0, broker.MarketCloseCount, "no market close when already flat");
            TestHarness.IsTrue(!core.FlattenInFlight, "guard released after confirming flat");
        }

        private static void RejectedCloseReleasesGuardForRetry()
        {
            TestHarness.Begin("C-08: a rejected close releases the in-flight guard so a retry can run");
            var (core, broker, _) = Build(netPosition: 2.0);
            broker.MarketCloseSucceeds = false;
            core.FlattenPosition("attempt-1");
            TestHarness.IsTrue(!core.FlattenInFlight, "guard released after rejected close");
            // Now allow it and retry — a second flatten should place a close.
            broker.MarketCloseSucceeds = true;
            core.FlattenPosition("attempt-2");
            TestHarness.AreEqual(1, broker.MarketCloseCount, "retry placed exactly one close");
        }

        private static void ConfirmFlatAdvancesToFlatVerifiedAfterHalt()
        {
            TestHarness.Begin("C-08: Halt -> Flattening, then confirm-flat advances to FlatVerified");
            var (core, broker, sched) = Build(netPosition: 2.0);
            core.Halt("daily loss");
            TestHarness.IsTrue(core.IsRiskHalted, "halt set the risk flag");
            TestHarness.IsTrue(core.Lifecycle == StrategyLifecycleState.Flattening, "lifecycle is Flattening");
            TestHarness.AreEqual(1, broker.MarketCloseCount, "one close on halt");

            // Simulate the close filling: broker now flat, position-removed confirms it.
            broker.NetPosition = 0.0;
            core.OnFlattenConfirmedFlat("position removed");
            TestHarness.IsTrue(!core.FlattenInFlight, "guard cleared on confirm");
            TestHarness.IsTrue(core.Lifecycle == StrategyLifecycleState.FlatVerified, "lifecycle advanced to FlatVerified");
        }
    }
}
