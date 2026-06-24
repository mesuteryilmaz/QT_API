using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TradingPlatform.BusinessLayer;

[assembly: InternalsVisibleTo("DataAnalytics.Tests")]

namespace MBO_Market_Data_Analytics
{
    /// <summary>Operator/derived configuration the execution core needs (platform-neutral).</summary>
    public sealed class ExecConfig
    {
        public double TickSize = 1.0;
        public int OrderQty = 1;
        public int MaxExposure = 2;
        public int MaxConsecutiveFailures = 5;
        public double PointCost = 1.0;
        public bool RequireDedicatedAccount = true;
    }

    /// <summary>
    /// Platform-free execution state machine. Owns the order/position/lifecycle state and drives the
    /// broker through <see cref="IBroker"/> + <see cref="IExecScheduler"/>, logging through an injected
    /// delegate. It has no dependency on the Quantower Strategy/Core singletons, so it can be driven by
    /// a fake broker + scheduler in tests.
    ///
    /// Grown test-first: each audit race scenario (C-06 late fill, C-07 naked stop, C-08 repeated
    /// flatten, …) is added with a fake-broker test before the live strategy delegates to it. Today it
    /// carries the C-08 lifecycle + idempotent-flatten slice.
    /// </summary>
    internal sealed class ExecutionCore
    {
        private readonly IBroker broker;
        private readonly IExecScheduler scheduler;
        private readonly Action<string, StrategyLoggingLevel> log;
        private readonly ExecConfig cfg;

        private readonly ConcurrentDictionary<string, TrackedOrderState> trackedOrders = new();
        private readonly object lifecycleLock = new();

        // Position/PnL state arrives with the fills slice; today's slice is lifecycle + flatten only.

        private volatile StrategyLifecycleState lifecycle = StrategyLifecycleState.Initializing;
        private int flattenGeneration;
        private volatile bool flattenInFlight;
        private volatile bool isRiskHalted;

        public ExecutionCore(IBroker broker, IExecScheduler scheduler,
                             Action<string, StrategyLoggingLevel> log, ExecConfig cfg)
        {
            this.broker = broker ?? throw new ArgumentNullException(nameof(broker));
            this.scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            this.log = log ?? ((_, __) => { });
            this.cfg = cfg ?? new ExecConfig();
        }

        // ---- read-only state (consumed by the strategy's signal layer and by tests) ----
        public StrategyLifecycleState Lifecycle => lifecycle;
        public bool IsRiskHalted => isRiskHalted;
        public bool FlattenInFlight => flattenInFlight;
        public int FlattenGeneration => flattenGeneration;

        public int WorkingOrderCount
        {
            get
            {
                int n = 0;
                foreach (var o in trackedOrders.Values)
                    if (o.Status == OrderStatus.Opened || o.Status == OrderStatus.PartiallyFilled) n++;
                return n;
            }
        }

        // ---- setup / reconcile helpers ----
        public void SetRunning() => TransitionTo(StrategyLifecycleState.Running, "running");
        public void Track(TrackedOrderState s) { if (s.OrderId != null) trackedOrders[s.OrderId] = s; }

        // ===================== C-08: lifecycle + idempotent flatten =====================

        /// <summary>
        /// Drives a hard halt: stops new entries (lifecycle + isRiskHalted), drops queued
        /// entry-placement work, and starts one idempotent flatten. Call OUTSIDE stateLock.
        /// </summary>
        public void Halt(string reason)
        {
            isRiskHalted = true;
            TransitionTo(StrategyLifecycleState.Halting, reason);
            int dropped = scheduler.CancelPendingEntries();
            if (dropped > 0)
                log($"[Lifecycle] Cancelled {dropped} pending entry-placement task(s) on halt.", StrategyLoggingLevel.Info);
            FlattenPosition(reason);
        }

        /// <summary>Single guarded lifecycle transition with logging. Stopped is terminal.</summary>
        public void TransitionTo(StrategyLifecycleState next, string reason)
        {
            StrategyLifecycleState prev;
            lock (lifecycleLock)
            {
                prev = lifecycle;
                if (prev == next || prev == StrategyLifecycleState.Stopped) return;
                lifecycle = next;
            }
            log($"[Lifecycle] {prev} -> {next}: {reason}", StrategyLoggingLevel.Info);
        }

        /// <summary>
        /// Called when the broker position is observed flat (position-removed or a flat reconcile).
        /// Clears the in-flight guard and, if we were flattening on a hard halt, advances to FlatVerified.
        /// </summary>
        public void OnFlattenConfirmedFlat(string source)
        {
            lock (lifecycleLock)
            {
                if (!flattenInFlight) return;
                if (broker.GetNetPositionSize() != 0.0) return; // not actually flat yet
                flattenInFlight = false;
                if (lifecycle == StrategyLifecycleState.Flattening)
                    lifecycle = StrategyLifecycleState.FlatVerified;
            }
            log($"[Flatten] Generation {flattenGeneration} confirmed flat ({source}).", StrategyLoggingLevel.Info);
        }

        /// <summary>
        /// Idempotent flatten. If a flatten is already in flight, a duplicate request is a no-op —
        /// preventing the prior behaviour where each call enqueued another full-size market close
        /// (over-closing / reversing the account). Continues running for non-halt flattens; only a
        /// hard Halt advances the lifecycle to FlatVerified.
        /// </summary>
        public void FlattenPosition(string reason = "")
        {
            lock (lifecycleLock)
            {
                if (flattenInFlight)
                {
                    log($"[Flatten] Already in flight (generation {flattenGeneration}); ignoring duplicate request{(string.IsNullOrEmpty(reason) ? "" : $" ({reason})")}.", StrategyLoggingLevel.Info);
                    return;
                }
                flattenInFlight = true;
                flattenGeneration++;
                if (lifecycle == StrategyLifecycleState.Halting)
                    lifecycle = StrategyLifecycleState.Flattening;
            }

            int gen = flattenGeneration;
            log($"[Flatten] Generation {gen} starting{(string.IsNullOrEmpty(reason) ? "" : $" ({reason})")}: cancelling working orders and market-closing position.", StrategyLoggingLevel.Info);

            // 1. Cancel all working orders (priority 0).
            foreach (var trackedOrder in trackedOrders.Values)
            {
                if (trackedOrder.OrderId != null &&
                    (trackedOrder.Status == OrderStatus.Opened || trackedOrder.Status == OrderStatus.PartiallyFilled))
                {
                    string oid = trackedOrder.OrderId;
                    scheduler.Post(() => { broker.Cancel(oid); return Task.CompletedTask; }, 0, TaskCategory.Flatten);
                }
            }

            // 2. One market close (priority 1).
            scheduler.Post(() =>
            {
                double posSize = broker.GetNetPositionSize();
                if (posSize == 0.0)
                {
                    // Already flat by the time the close runs (e.g. brackets closed it first).
                    OnFlattenConfirmedFlat($"already flat at flatten gen {gen}");
                    return Task.CompletedTask;
                }

                Side flatSide = posSize > 0.0 ? Side.Sell : Side.Buy;
                double qty = Math.Abs(posSize);
                log($"[Flatten Placement] Generation {gen}: Market {flatSide} for {qty} contracts.", StrategyLoggingLevel.Info);

                var flatResult = broker.PlaceMarketClose(flatSide, qty, "");
                if (!flatResult.Success)
                {
                    // C-08: the close was not accepted — release the in-flight guard so a subsequent
                    // event can retry, rather than leaving the position stuck with flattenInFlight=true.
                    log($"[Flatten Placement] Generation {gen} market close REJECTED: {flatResult.Message}. Releasing flatten guard for retry.", StrategyLoggingLevel.Error);
                    lock (lifecycleLock) { flattenInFlight = false; }
                }
                // On success leave flattenInFlight=true; confirmation clears it when broker reports flat.
                return Task.CompletedTask;
            }, 1, TaskCategory.Flatten);
        }
    }
}
