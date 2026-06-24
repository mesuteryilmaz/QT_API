using System;
using System.Collections.Generic;
using TradingPlatform.BusinessLayer;

namespace MBO_Market_Data_Analytics.Tests
{
    // C-06 fill/position scenarios for ExecutionCore: position accounting, M-12 marginal price on
    // partial fills, realized PnL + round-trip callback, and the late-bracket-fill reversal that must
    // halt + flatten (broker truth) rather than "reject" an unrejectable fill.
    public static class ExecutionCoreFillTests
    {
        private static (ExecutionCore core, FakeBroker broker, List<double> roundTrips) Build(double pointCost = 1.0)
        {
            var broker = new FakeBroker();
            var sched = new FakeScheduler(); // auto-run
            var roundTrips = new List<double>();
            var core = new ExecutionCore(broker, sched, (_, __) => { }, new ExecConfig { PointCost = pointCost },
                onRoundTripClosed: pnl => roundTrips.Add(pnl),
                onEntryFilled: (_, __, ___) => { });
            core.SetRunning();
            return (core, broker, roundTrips);
        }

        private static TrackedOrderState Entry(string id, Side side, double price, double qty) => new TrackedOrderState
        {
            LocalId = Guid.NewGuid().ToString(),
            OrderId = id, Side = side, Price = price, Quantity = qty,
            Status = OrderStatus.Opened, Role = OrderRole.Entry
        };

        private static TrackedOrderState Bracket(string id, Side side, double price, double qty, bool isStop) => new TrackedOrderState
        {
            LocalId = Guid.NewGuid().ToString(),
            OrderId = id, Side = side, Price = price, Quantity = qty,
            Status = OrderStatus.Opened, Role = OrderRole.Bracket, IsStopHint = isStop
        };

        private static OrderSnapshot Snap(string id, Side side, OrderStatus status, double total, double filled, double avg)
            => new OrderSnapshot(id, side, status, total, filled, avg);

        public static void RunAll()
        {
            Console.WriteLine("ExecutionCore (C-06 fills/position):");
            EntryFillOpensPosition();
            PartialFillsUseMarginalPriceAndWeightedAverage();
            OppositeFillReducesAndRealizesPnL();
            LateBracketFillWhileFlatHaltsAndFlattens();
        }

        private static void EntryFillOpensPosition()
        {
            TestHarness.Begin("C-06: entry fill opens the position at the fill price");
            var (core, _, _) = Build();
            core.Track(Entry("E1", Side.Buy, 100.0, 2));
            core.ApplyOrderUpdate(Snap("E1", Side.Buy, OrderStatus.Filled, 2, 2, 100.0));
            TestHarness.AreEqual(2.0, core.PositionSize, 1e-9, "long 2 after buy fill");
            TestHarness.AreEqual(100.0, core.AverageEntryPrice, 1e-9, "average entry 100");
        }

        private static void PartialFillsUseMarginalPriceAndWeightedAverage()
        {
            TestHarness.Begin("M-12: partial fills use marginal price; weighted average is correct");
            var (core, _, _) = Build();
            core.Track(Entry("E1", Side.Buy, 100.0, 4));
            // First 2 lots at cumulative avg 100.
            core.ApplyOrderUpdate(Snap("E1", Side.Buy, OrderStatus.PartiallyFilled, 4, 2, 100.0));
            TestHarness.AreEqual(2.0, core.PositionSize, 1e-9, "long 2 after first partial");
            TestHarness.AreEqual(100.0, core.AverageEntryPrice, 1e-9, "avg 100 after first partial");
            // Next 2 lots: cumulative avg moves to 101 -> marginal price for this increment is 102.
            core.ApplyOrderUpdate(Snap("E1", Side.Buy, OrderStatus.Filled, 4, 4, 101.0));
            TestHarness.AreEqual(4.0, core.PositionSize, 1e-9, "long 4 after full fill");
            TestHarness.AreEqual(101.0, core.AverageEntryPrice, 1e-9, "weighted average 101");
        }

        private static void OppositeFillReducesAndRealizesPnL()
        {
            TestHarness.Begin("C-06: opposite fill flattens, realizes PnL, fires round-trip callback");
            var (core, _, roundTrips) = Build(pointCost: 1.0);
            core.Track(Entry("E1", Side.Buy, 100.0, 2));
            core.ApplyOrderUpdate(Snap("E1", Side.Buy, OrderStatus.Filled, 2, 2, 100.0)); // long 2 @ 100
            core.Track(Bracket("TP1", Side.Sell, 105.0, 2, isStop: false));
            core.ApplyOrderUpdate(Snap("TP1", Side.Sell, OrderStatus.Filled, 2, 2, 105.0)); // sell 2 @ 105
            TestHarness.AreEqual(0.0, core.PositionSize, 1e-9, "flat after TP fill");
            TestHarness.AreEqual(10.0, core.RealizedPnL, 1e-9, "realized +10 (5 ticks * 2 * point cost 1)");
            TestHarness.IsTrue(roundTrips.Count == 1 && Math.Abs(roundTrips[0] - 10.0) < 1e-9,
                "round-trip callback fired once with +10");
        }

        private static void LateBracketFillWhileFlatHaltsAndFlattens()
        {
            TestHarness.Begin("C-06: late bracket fill while flat -> apply broker truth, halt + flatten");
            var (core, broker, _) = Build();
            core.Track(Bracket("SL1", Side.Sell, 99.0, 1, isStop: true));
            broker.NetPosition = -1.0; // broker truth: the SL filled, account is now short 1
            core.ApplyOrderUpdate(Snap("SL1", Side.Sell, OrderStatus.Filled, 1, 1, 99.0));

            TestHarness.IsTrue(core.IsRiskHalted, "strategy halted on the reversal");
            TestHarness.AreEqual(-1.0, core.PositionSize, 1e-9, "core applied broker truth (short 1), not 'rejected'");
            TestHarness.IsTrue(core.Lifecycle == StrategyLifecycleState.Flattening, "lifecycle is Flattening");
            TestHarness.AreEqual(1, broker.MarketCloseCount, "one market close placed to unwind the reversal");
        }
    }
}
