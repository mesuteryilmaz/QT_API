using System;
using System.Linq;
using TradingPlatform.BusinessLayer;

namespace MBO_Market_Data_Analytics.Tests
{
    // C-07 bracket-placement + protection-invariant scenarios for ExecutionCore.
    public static class ExecutionCoreBracketTests
    {
        // ticks[0]=tp, ticks[1]=sl, mutable so a test can change the configured stop distance.
        private static (ExecutionCore core, FakeBroker broker, int[] ticks) Build(int tp, int sl)
        {
            var broker = new FakeBroker();
            var sched = new FakeScheduler(); // auto-run
            var ticks = new[] { tp, sl };
            var core = new ExecutionCore(broker, sched, (_, __) => { },
                new ExecConfig { TickSize = 0.25, MaxConsecutiveFailures = 5 },
                getBracketTicks: () => (ticks[0], ticks[1]));
            core.SetRunning();
            return (core, broker, ticks);
        }

        private static TrackedOrderState Entry(string id, Side side, double price, double qty) => new TrackedOrderState
        {
            LocalId = Guid.NewGuid().ToString(),
            OrderId = id, Side = side, Price = price, Quantity = qty,
            Status = OrderStatus.Opened, Role = OrderRole.Entry
        };

        private static OrderSnapshot Filled(string id, Side side, double qty, double avg)
            => new OrderSnapshot(id, side, OrderStatus.Filled, qty, qty, avg);

        public static void RunAll()
        {
            Console.WriteLine("ExecutionCore (C-07 brackets/protection):");
            EntryFillPlacesStopAndTakeProfit();
            NakedStopHaltsWhenNoStopType();
            StopPlacementFailureHalts();
            ProtectionInvariantHaltsWhenUncovered();
            ProtectionInvariantIgnoredWhenNoStopConfigured();
        }

        private static void EntryFillPlacesStopAndTakeProfit()
        {
            TestHarness.Begin("C-07: entry fill places a stop and a take-profit, invariant satisfied");
            var (core, broker, _) = Build(tp: 10, sl: 10);
            core.Track(Entry("E1", Side.Buy, 100.0, 2));
            core.ApplyOrderUpdate(Filled("E1", Side.Buy, 2, 100.0));

            var stops = broker.Placed.Where(p => p.Kind == "Stop").ToList();
            var limits = broker.Placed.Where(p => p.Kind == "Limit").ToList();
            TestHarness.AreEqual(1, stops.Count, "one protective stop placed");
            TestHarness.AreEqual(1, limits.Count, "one take-profit placed");
            TestHarness.IsTrue(stops[0].Side == Side.Sell && Math.Abs(stops[0].Price - 97.5) < 1e-9, "stop on sell at 97.5 (100 - 10*0.25)");
            TestHarness.IsTrue(limits[0].Side == Side.Sell && Math.Abs(limits[0].Price - 102.5) < 1e-9, "TP on sell at 102.5");
            TestHarness.IsTrue(!core.IsRiskHalted, "not halted — position is protected");
        }

        private static void NakedStopHaltsWhenNoStopType()
        {
            TestHarness.Begin("C-07: no stop order type -> halt + flatten (no naked position, no TP)");
            var (core, broker, _) = Build(tp: 10, sl: 10);
            broker.StopOrderTypeAvailable = false;
            broker.NetPosition = 2.0; // broker shows the just-opened long
            core.Track(Entry("E1", Side.Buy, 100.0, 2));
            core.ApplyOrderUpdate(Filled("E1", Side.Buy, 2, 100.0));

            TestHarness.IsTrue(core.IsRiskHalted, "halted with no stop type");
            TestHarness.AreEqual(0, broker.Placed.Count(p => p.Kind == "Limit"), "no take-profit placed");
            TestHarness.AreEqual(1, broker.MarketCloseCount, "flatten placed one close");
        }

        private static void StopPlacementFailureHalts()
        {
            TestHarness.Begin("C-07: stop placement failure -> halt + flatten");
            var (core, broker, _) = Build(tp: 10, sl: 10);
            broker.StopSucceeds = false;
            broker.NetPosition = 2.0;
            core.Track(Entry("E1", Side.Buy, 100.0, 2));
            core.ApplyOrderUpdate(Filled("E1", Side.Buy, 2, 100.0));

            TestHarness.IsTrue(core.IsRiskHalted, "halted when stop placement fails");
            TestHarness.AreEqual(0, broker.Placed.Count(p => p.Kind == "Limit"), "no TP after stop failure");
        }

        private static void ProtectionInvariantHaltsWhenUncovered()
        {
            TestHarness.Begin("C-07: protection invariant halts an open position with no working stop");
            // Open the position with NO stop configured (sl=0), then 'enable' a stop and re-check.
            var (core, broker, ticks) = Build(tp: 10, sl: 0);
            broker.NetPosition = 2.0; // broker truth: long 2 (so the flatten places a real close)
            core.Track(Entry("E1", Side.Buy, 100.0, 2));
            core.ApplyOrderUpdate(Filled("E1", Side.Buy, 2, 100.0)); // places only a TP; no stop expected yet
            TestHarness.IsTrue(!core.IsRiskHalted, "not halted while no stop is configured");

            ticks[1] = 10; // now a stop IS expected, but none is working
            core.VerifyProtectionInvariant("manual recheck");
            TestHarness.IsTrue(core.IsRiskHalted, "halted — position uncovered by a stop");
            TestHarness.IsTrue(core.Lifecycle == StrategyLifecycleState.Flattening, "flattening after invariant breach");
            TestHarness.AreEqual(1, broker.MarketCloseCount, "flatten placed one close");
        }

        private static void ProtectionInvariantIgnoredWhenNoStopConfigured()
        {
            TestHarness.Begin("C-07: protection invariant is a no-op when SL ticks <= 0");
            var (core, _, _) = Build(tp: 10, sl: 0);
            core.Track(Entry("E1", Side.Buy, 100.0, 2));
            core.ApplyOrderUpdate(Filled("E1", Side.Buy, 2, 100.0));
            core.VerifyProtectionInvariant("recheck with no stop configured");
            TestHarness.IsTrue(!core.IsRiskHalted, "not halted — operator opted out of stops");
        }
    }
}
