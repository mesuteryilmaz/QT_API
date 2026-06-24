using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        // Round-trip realized PnL callback (for the strategy's promotion tracker) and the effective
        // bracket distances (TP/SL ticks), which the strategy derives from its adaptive controller.
        private readonly Action<double> onRoundTripClosed;
        private readonly Func<(int tp, int sl)> getBracketTicks;

        private readonly ConcurrentDictionary<string, TrackedOrderState> trackedOrders = new();
        private readonly object lifecycleLock = new();
        private readonly object stateLock = new();

        // Owned position/PnL state (our fills only).
        private double currentPositionSize;
        private double averageEntryPrice;
        private double strategyRealizedPnL;
        private double pnlAtLastFlat;

        private volatile StrategyLifecycleState lifecycle = StrategyLifecycleState.Initializing;
        private int flattenGeneration;
        private volatile bool flattenInFlight;
        private volatile bool isRiskHalted;
        private int consecutiveOrderFailures;

        /// <summary>Per-run id embedded in placed-order comments (set by the strategy adapter).</summary>
        public string RunId = "";

        public ExecutionCore(IBroker broker, IExecScheduler scheduler,
                             Action<string, StrategyLoggingLevel> log, ExecConfig cfg,
                             Action<double>? onRoundTripClosed = null,
                             Func<(int tp, int sl)>? getBracketTicks = null)
        {
            this.broker = broker ?? throw new ArgumentNullException(nameof(broker));
            this.scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            this.log = log ?? ((_, __) => { });
            this.cfg = cfg ?? new ExecConfig();
            this.onRoundTripClosed = onRoundTripClosed ?? (_ => { });
            this.getBracketTicks = getBracketTicks ?? (() => (0, 0));
        }

        private string BracketComment() => $"MBO:BRK|{RunId}";

        // ---- read-only state (consumed by the strategy's signal layer and by tests) ----
        public StrategyLifecycleState Lifecycle => lifecycle;
        public bool IsRiskHalted => isRiskHalted;
        public bool FlattenInFlight => flattenInFlight;
        public int FlattenGeneration => flattenGeneration;
        public double PositionSize { get { lock (stateLock) return currentPositionSize; } }
        public double AverageEntryPrice { get { lock (stateLock) return averageEntryPrice; } }
        public double RealizedPnL { get { lock (stateLock) return strategyRealizedPnL; } }

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

        // ===================== C-06: fills + position accounting =====================

        /// <summary>
        /// Platform-free fill/status handler. Returns true if the order reached a terminal status so
        /// the caller can unsubscribe. Applies broker truth (a fill is final, never "rejected"); a
        /// protective fill that arrives while already flat reversed the position, so it halts + flattens.
        /// </summary>
        public bool ApplyOrderUpdate(OrderSnapshot snap)
        {
            if (!trackedOrders.TryGetValue(snap.OrderId, out var state))
                return false;

            var newStatus = snap.Status;
            var oldFilled = state.FilledQuantity;
            var newFilled = snap.FilledQuantity;
            double oldCumAvg = state.CumAverageFillPrice;
            double newCumAvg = snap.AverageFillPrice > 0 ? snap.AverageFillPrice : state.Price;

            // H-09: atomically replace the immutable state so cross-thread readers never see a torn mix.
            trackedOrders[snap.OrderId] = state with
            {
                Status = newStatus,
                FilledQuantity = newFilled,
                CumAverageFillPrice = newCumAvg
            };

            if (newFilled > oldFilled)
            {
                double filledDiff = newFilled - oldFilled;
                log($"[Fill Alert] Order {snap.OrderId} filled: {oldFilled} -> {newFilled} (Diff: {filledDiff})", StrategyLoggingLevel.Trading);

                // M-12: AverageFillPrice is cumulative; derive the marginal price for this increment.
                double marginalPrice = (oldFilled <= 0 || oldCumAvg <= 0)
                    ? newCumAvg
                    : (newFilled * newCumAvg - oldFilled * oldCumAvg) / filledDiff;
                if (marginalPrice <= 0) marginalPrice = state.Price;

                // C-06: a broker fill is FINAL — apply it (broker truth), then handle consequences.
                bool wasFlatBefore;
                lock (stateLock) { wasFlatBefore = currentPositionSize == 0.0; }

                ProcessExecutionFill(snap.Side, marginalPrice, filledDiff);

                if (state.Role == OrderRole.Entry)
                {
                    // Our own signal entry — attach protective brackets.
                    ManageBracketsForFill(snap.Side, marginalPrice, filledDiff);
                }
                else if (state.Role == OrderRole.Bracket && wasFlatBefore)
                {
                    // C-06: a protective bracket filled while already flat — its sibling had closed the
                    // position first, so this final fill opened a REVERSED position. Halt + flatten
                    // (which cancels the sibling) instead of "rejecting" an unrejectable fill.
                    log($"[OCO] Bracket {snap.OrderId} filled while already flat — broker opened a reversed position. Halting and flattening.", StrategyLoggingLevel.Error);
                    Halt($"late bracket fill reversed position (order {snap.OrderId})");
                }
            }

            bool terminal = newStatus == OrderStatus.Filled || newStatus == OrderStatus.Cancelled || newStatus == OrderStatus.Refused;
            if (terminal)
            {
                log($"[Order Finished] Order {snap.OrderId} finished with status: {newStatus}", StrategyLoggingLevel.Trading);
                trackedOrders.TryRemove(snap.OrderId, out _);
            }
            return terminal;
        }

        private void ProcessExecutionFill(Side fillSide, double fillPrice, double fillQty)
        {
          lock (stateLock)
          {
            double pointCost = cfg.PointCost;
            double fillQtySigned = fillSide == Side.Buy ? fillQty : -fillQty;

            if (currentPositionSize == 0.0)
            {
                averageEntryPrice = fillPrice;
                currentPositionSize = fillQtySigned;
            }
            else if (Math.Sign(currentPositionSize) == Math.Sign(fillQtySigned))
            {
                double existingAbs = Math.Abs(currentPositionSize);
                averageEntryPrice = ((averageEntryPrice * existingAbs) + (fillPrice * fillQty)) / (existingAbs + fillQty);
                currentPositionSize += fillQtySigned;
            }
            else
            {
                double existingAbs = Math.Abs(currentPositionSize);
                double reductionQty = Math.Min(existingAbs, fillQty);
                double remainingFillQty = fillQty - reductionQty;

                double pnlCoeff = currentPositionSize > 0 ? 1.0 : -1.0;
                double realized = (fillPrice - averageEntryPrice) * reductionQty * pointCost * pnlCoeff;
                strategyRealizedPnL += realized;

                log($"[PnL Update] Realized {realized:C2} on reduction of {reductionQty} contracts. Total realized PnL: {strategyRealizedPnL:C2}.", StrategyLoggingLevel.Info);

                currentPositionSize += (fillSide == Side.Buy ? reductionQty : -reductionQty);

                if (remainingFillQty > 0)
                {
                    averageEntryPrice = fillPrice;
                    currentPositionSize = fillSide == Side.Buy ? remainingFillQty : -remainingFillQty;
                }
                else if (currentPositionSize == 0.0)
                {
                    averageEntryPrice = 0.0;
                }
            }

            log($"[Local Position] Size: {currentPositionSize}, Average Price: {averageEntryPrice}.", StrategyLoggingLevel.Info);

            if (currentPositionSize == 0.0)
            {
                // Round-trip complete: report realized PnL since the last flat to the promotion tracker.
                double roundTripPnL = strategyRealizedPnL - pnlAtLastFlat;
                pnlAtLastFlat = strategyRealizedPnL;
                onRoundTripClosed(roundTripPnL);

                // Position went flat — cancel all working bracket orders.
                foreach (var trackedOrder in trackedOrders.Values)
                {
                    if (trackedOrder.Role == OrderRole.Bracket && trackedOrder.OrderId != null &&
                        (trackedOrder.Status == OrderStatus.Opened || trackedOrder.Status == OrderStatus.PartiallyFilled))
                    {
                        string oid = trackedOrder.OrderId;
                        log($"[OCO Action] Position flat. Cancelling bracket order {oid}.", StrategyLoggingLevel.Trading);
                        scheduler.Post(() => { broker.Cancel(oid); return Task.CompletedTask; }, 0, TaskCategory.Protection);
                    }
                }
            }
            else
            {
                ReconcileBracketQuantities();
            }
          }
        }

        // C-07: classify a tracked bracket via the placement hint (the strategy sets it on every
        // adopted bracket too), so no platform Order-type lookup is needed in the core.
        private static bool IsWorkingStop(TrackedOrderState s) => s.IsStopHint == true;
        private static bool IsWorkingLimit(TrackedOrderState s) => s.IsStopHint == false;

        private void ReconcileBracketQuantities()
        {
            double absPosition = Math.Abs(currentPositionSize);

            var activeSLs = trackedOrders.Values.Where(o => o.Role == OrderRole.Bracket &&
                                                            (o.Status == OrderStatus.Opened || o.Status == OrderStatus.PartiallyFilled) &&
                                                            IsWorkingStop(o)).ToList();
            var activeTPs = trackedOrders.Values.Where(o => o.Role == OrderRole.Bracket &&
                                                            (o.Status == OrderStatus.Opened || o.Status == OrderStatus.PartiallyFilled) &&
                                                            IsWorkingLimit(o)).ToList();

            AdjustBracketGroup(activeSLs, absPosition);
            AdjustBracketGroup(activeTPs, absPosition);
        }

        private void AdjustBracketGroup(List<TrackedOrderState> brackets, double targetQty)
        {
            if (brackets.Count == 0) return;
            double totalQty = brackets.Sum(o => o.Quantity - o.FilledQuantity);
            if (totalQty <= targetQty) return;

            double excess = totalQty - targetQty;
            log($"[Bracket Adjust] Excess qty detected: {totalQty} > {targetQty}. Adjusting...", StrategyLoggingLevel.Trading);

            foreach (var state in brackets)
            {
                if (excess <= 0) break;
                if (state.OrderId == null) continue;

                double remainingOrderQty = state.Quantity - state.FilledQuantity;
                if (remainingOrderQty <= excess)
                {
                    string oid = state.OrderId;
                    log($"[Bracket Adjust] Cancelling bracket order {oid}.", StrategyLoggingLevel.Trading);
                    scheduler.Post(() => { broker.Cancel(oid); return Task.CompletedTask; }, 0, TaskCategory.Protection);
                    excess -= remainingOrderQty;
                }
                else
                {
                    // H-11: modify quantity is the order TOTAL, not the remaining. For a partially
                    // filled order set total = filled + desired remaining.
                    double desiredRemaining = remainingOrderQty - excess;
                    double newTotal = state.FilledQuantity + desiredRemaining;
                    string ordId = state.OrderId;
                    log($"[Bracket Adjust] Modifying bracket {ordId}: remaining {remainingOrderQty} -> {desiredRemaining} (total qty -> {newTotal}).", StrategyLoggingLevel.Trading);
                    scheduler.Post(() =>
                    {
                        var modResult = broker.ModifyQuantity(ordId, newTotal);
                        if (modResult.Success)
                        {
                            if (trackedOrders.TryGetValue(ordId, out var cur))
                                trackedOrders[ordId] = cur with { Quantity = newTotal };
                        }
                        else
                        {
                            log($"[Bracket Adjust] Modify of {ordId} failed: {modResult.Message}.", StrategyLoggingLevel.Error);
                        }
                        return Task.CompletedTask;
                    }, 1, TaskCategory.Protection);
                    excess = 0;
                }
            }
        }

        // ===================== C-07: bracket placement + protection invariant =====================

        private void ManageBracketsForFill(Side entrySide, double fillPrice, double qty)
        {
            var (tpTicks, slTicks) = getBracketTicks();
            if (slTicks <= 0 && tpTicks <= 0) return;

            Side bracketSide = entrySide == Side.Buy ? Side.Sell : Side.Buy;
            double tickSize = cfg.TickSize;

            scheduler.Post(() =>
            {
                if (slTicks > 0)
                {
                    double slPrice = entrySide == Side.Buy
                        ? fillPrice - (slTicks * tickSize)
                        : fillPrice + (slTicks * tickSize);
                    slPrice = Math.Round(slPrice / tickSize) * tickSize;

                    if (!broker.HasStopOrderType)
                    {
                        // C-07: no stop order type means the position would be NAKED. Do not fall
                        // through to TP placement — halt and flatten.
                        log($"[Risk Limit] No Stop order type available. Cannot protect position — halting and flattening.", StrategyLoggingLevel.Error);
                        Halt("no stop order type available (naked position)");
                        return Task.CompletedTask;
                    }

                    log($"[Bracket SL] Scheduling SL Stop on {bracketSide} at {slPrice} for {qty}.", StrategyLoggingLevel.Trading);
                    var slResult = broker.PlaceProtectiveStop(bracketSide, slPrice, qty, BracketComment());
                    if (slResult.Success)
                    {
                        Track(new TrackedOrderState
                        {
                            LocalId = Guid.NewGuid().ToString(),
                            OrderId = slResult.OrderId,
                            Side = bracketSide,
                            Price = slPrice,
                            Quantity = qty,
                            Status = OrderStatus.Opened,
                            Role = OrderRole.Bracket,
                            IsStopHint = true
                        });
                    }
                    else
                    {
                        log($"[Risk Limit] SL placement failed: {slResult.Message}. Flattening — position has no stop.", StrategyLoggingLevel.Error);
                        Halt($"stop-loss placement failed ({slResult.Message})");
                        return Task.CompletedTask; // no point protecting upside without a stop
                    }
                }

                if (tpTicks > 0)
                {
                    double tpPrice = entrySide == Side.Buy
                        ? fillPrice + (tpTicks * tickSize)
                        : fillPrice - (tpTicks * tickSize);
                    tpPrice = Math.Round(tpPrice / tickSize) * tickSize;

                    log($"[Bracket TP] Scheduling TP Limit on {bracketSide} at {tpPrice} for {qty}.", StrategyLoggingLevel.Trading);
                    var tpResult = broker.PlaceProtectiveLimit(bracketSide, tpPrice, qty, BracketComment());
                    if (tpResult.Success)
                    {
                        Track(new TrackedOrderState
                        {
                            LocalId = Guid.NewGuid().ToString(),
                            OrderId = tpResult.OrderId,
                            Side = bracketSide,
                            Price = tpPrice,
                            Quantity = qty,
                            Status = OrderStatus.Opened,
                            Role = OrderRole.Bracket,
                            IsStopHint = false
                        });
                    }
                    else
                    {
                        log($"[Bracket TP] Placement failed: {tpResult.Message}", StrategyLoggingLevel.Error);
                        consecutiveOrderFailures++;
                        if (consecutiveOrderFailures >= cfg.MaxConsecutiveFailures)
                        {
                            log($"[Risk Limit] Halted due to bracket TP placement failure.", StrategyLoggingLevel.Error);
                            Halt("bracket TP placement failure");
                        }
                    }
                }

                // C-07: confirm the position is now covered by a working stop before finishing.
                VerifyProtectionInvariant($"after brackets for {entrySide} fill");
                return Task.CompletedTask;
            }, 1, TaskCategory.Protection);
        }

        /// <summary>
        /// C-07: enforce "working stop remaining qty >= |position|". Call after fills/bracket placement
        /// and after reconnect. If an open position is not covered by a working stop, halt + flatten.
        /// Gated on slTicks &gt; 0 so a deliberately stop-less configuration is unaffected.
        /// </summary>
        public void VerifyProtectionInvariant(string context)
        {
            var (_, slTicks) = getBracketTicks();
            if (slTicks <= 0) return;

            double pos;
            lock (stateLock) { pos = currentPositionSize; }
            double absPos = Math.Abs(pos);
            if (absPos <= 1e-9) return;

            Side closeSide = pos > 0 ? Side.Sell : Side.Buy;
            double stopCover = trackedOrders.Values
                .Where(o => o.Role == OrderRole.Bracket && o.Side == closeSide && IsWorkingStop(o) &&
                            (o.Status == OrderStatus.Opened || o.Status == OrderStatus.PartiallyFilled))
                .Sum(o => o.Quantity - o.FilledQuantity);

            if (stopCover + 1e-9 < absPos)
            {
                log($"[Protection] Invariant violated ({context}): working stop cover {stopCover} < position {absPos}. Halting and flattening.", StrategyLoggingLevel.Error);
                Halt($"stop cover {stopCover} < position {absPos}");
            }
        }

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
