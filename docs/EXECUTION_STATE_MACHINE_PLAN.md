# Execution State-Machine Design Plan (Audit-3 Phase 3)

**Status:** design only — no code written yet. This is the plan to be executed in a
dedicated work cycle, per the audit's directive: *"Do not spend the next cycle adding
more indicators, classifiers, or grid features. The strategy logic is not yet the
bottleneck; the trading infrastructure is."*

**Covers:** C-06, C-07, C-08, C-09 (Critical) plus the tightly-coupled H-08, H-09,
H-11, H-12. These cannot be fixed as isolated surgical edits — they share root causes
in ownership, lifecycle, and broker-truth reconciliation, so they are designed together.

**Scope guard:** until this plan is implemented and passes the deterministic broker
tests in §7, the strategy must run in `ShadowOnly`. Live mode stays disabled.

---

## 1. Root-cause summary

| Finding | Symptom in code | Root cause |
|---------|-----------------|------------|
| C-06 | `OnOrderUpdated` (`~1063-1073`) "rejects" a late bracket fill and calls `Cancel()` on an order the broker already filled. | Local position state treated as authoritative over a final broker fill. |
| C-07 | `ManageBracketsForFill` (`~1203-1240`) skips the entire SL block when `stopOrderType == null`; reconciliation only trims *excess* bracket qty; freshly placed brackets have `OrderInstance == null`; brackets are placed at priority 2 (same as entries). | No explicit "position is protected" invariant enforced from broker state. |
| C-08 | `FlattenPosition` (`924-970`) enqueues a fresh full-size market close on every call with no pending-guard; `OnStop` (`327-403`) cancels without awaiting acks, market-flattens without checking the result, never drains the queue. | No lifecycle state; flatten is not idempotent; shutdown races queued work. |
| C-09 | `trackedOrders` imports every order for the symbol/account; `EntryTag`/`BracketTag` (`183-184`) carry no per-run identity; `FlattenPosition` closes the **net** broker position. | No strategy-instance ownership; cannot distinguish own orders from foreign/manual. |
| H-08 | `async void ProcessQueueLoop` (`1561`) as thread entry; `Shutdown` (`1555`) only cancels; no drain/join; enqueue still accepted after shutdown. | Queue is not a properly owned, awaitable consumer. |
| H-09 | `TrackedOrderState` mutable fields written from platform callbacks + queue threads without a common lock (`ConcurrentDictionary` only guards the map, not the values). | Order state is mutable and shared. |
| H-11 | Bracket modify (`~1308-1355`) writes a *remaining* qty into `ModifyOrderRequestParameters.Quantity` (a *total*), never refreshes `TrackedOrderState`, ignores the result. | Confuses total vs remaining; no post-modify verification. |
| H-12 | Side-specific duplicate checks let a working buy entry and a working sell entry coexist, unlinked. | No global single-entry policy or OCO. |

---

## 2. Target design: one owned execution state machine

Replace the ad-hoc flags (`isOrderPlacementPending`, `isRiskHalted`, scattered
`FlattenPosition` calls) with a single explicit lifecycle, owned by one actor.

### 2.1 Strategy lifecycle states

```
Initializing → Running → Halting → Flattening → FlatVerified → Stopped
                  │                                    ▲
                  └────────────── (risk/halt) ─────────┘
```

- **Initializing** — subscriptions, reconciliation, ownership probe. No entries.
- **Running** — normal; entries allowed only here (and only when Layer-3 analytics gate passes).
- **Halting** — stop accepting *new* entries; cancel queued-but-unsent entries. Triggered by risk halt, demotion, session rollover, `OnStop`, data/exec loss.
- **Flattening** — one idempotent flatten in flight (see §4).
- **FlatVerified** — broker position confirmed 0 and no working owned orders.
- **Stopped** — terminal; teardown complete.

State transitions are the *only* place flatten/cancel/halt are decided. `FlattenPosition`
becomes a private transition action, not a method callable from five places.

### 2.2 Single ownership actor

All order-state mutation and broker calls move behind one serialized consumer (replaces
the `PrioritizedTaskQueue` + scattered `lock (stateLock)`). Options, in preference order:

1. `System.Threading.Channels.Channel<Command>` (unbounded-with-cap) + one long-running
   `Task` consumer. Commands are records: `PlaceEntry`, `PlaceBracket`, `ModifyBracket`,
   `CancelOrder`, `Flatten`, `Reconcile`, `Halt`. The consumer owns all `trackedOrders`
   mutation, so H-09 disappears (no shared mutable state across threads).
2. If staying with the existing queue, convert `ProcessQueueLoop` from `async void` to a
   retained `Task`, add `StopAccepting()` / `CancelPendingEntries()` / `Drain()` /
   awaited `ShutdownAsync()` (fixes H-08).

Platform callbacks (`OnOrderUpdated`, `OnCore*`) **post commands** to the actor instead of
mutating state inline. `TrackedOrderState` becomes an immutable record replaced atomically
by the actor (fixes H-09).

---

## 3. Ownership model (C-09)

- Generate one `runId` (GUID) per strategy start. Persist it (see §6) so a restart can
  re-adopt its own orders.
- Every placed order carries `Comment = $"{Tag}|{runId}"` (entry/bracket tag + run id).
- `trackedOrders` only adopts orders whose comment contains **this** `runId`. Orders from
  another run, another strategy, or manual trading are **observed but never managed**
  (never cancelled, never counted into our exposure, never flattened).
- Flatten/халt operate on **owned lots only**, not the net broker position. Maintain a
  signed `ownedPositionQty` derived purely from fills of owned orders. If the account net
  position diverges from `ownedPositionQty` by foreign activity, that delta is left alone.
- **Refuse to run live** if, at `Initializing`, foreign working orders or a foreign net
  position exist on the same account/symbol **and** the operator has not set a dedicated
  account flag. Surface this as an explicit input: `RequireDedicatedAccount = true`.

> Note: Quantower native OCO via `LinkOCORequestParameters` (see the bundled API examples)
> should be used where the connector supports it, so sibling brackets are linked at the
> broker and the local OCO guard becomes a backstop, not the primary mechanism.

---

## 4. Broker-truth reconciliation & idempotent flatten

### 4.1 C-06 — accept the fill, never "reject" it

A broker-reported fill is final and cannot be cancelled. Replace the reject branch with:

1. Apply the fill to `ownedPositionQty` (broker truth wins).
2. Cancel any remaining owned siblings (OCO backstop).
3. If the fill produced a **reversed** owned position, post a single idempotent `Flatten`.
4. Transition to `Halting` and require `FlatVerified` before any new entry.

### 4.2 C-08 — idempotent flatten with a generation guard

```
flattenGeneration : int        // incremented each time we ENTER Flattening
flattenInFlight    : bool       // set when a close order is working, cleared on confirm
```

- `Flatten` is a no-op if `flattenInFlight` for the current generation.
- The flatten command reads broker position **at execution time**, sizes the close to the
  **owned** quantity, submits **one** market close, and records the close order id.
- `FlatVerified` is reached only when (a) the close order is `Filled` **and** (b) a re-read
  of broker position shows owned qty == 0. Otherwise retry within the same generation
  (bounded retries, then escalate to a logged hard error).

### 4.3 C-07 — protection invariant

Define and continuously enforce:

```
working owned stop remaining qty  ==  |owned position qty|
```

Checked after every fill, reconnect, and order update. If it cannot be restored
immediately (no stop type available, stop placement failed, stop qty < position):
**cancel entries → one market flatten → Halting**. Brackets are placed/repaired at an
**emergency priority above entries** (entries are the lowest trading priority). The
`stopOrderType == null` path must flatten, not silently continue to TP.

### 4.4 H-11 — bracket modify uses totals + verification

- Compute the connector's required **total** quantity (`filled + desiredRemaining`), not the
  remaining alone, for `ModifyOrderRequestParameters.Quantity`.
- After modify, refresh from `Order.TotalQuantity` / `RemainingQuantity` and replace the
  immutable `TrackedOrderState`. Validate the result status; on failure, re-enter the
  protection-invariant check.

### 4.5 H-10 (related) — reconcile from executions, not just position

On reconnect/restart, walk broker **executions/order history** since a persisted fill
cursor, not just the current position, so realized PnL, `pnlAtLastFlat`, and promotion
stats are correct. Position alone cannot reconstruct PnL or which brackets are ours.

---

## 5. Entry policy (H-12)

Pick one explicitly and enforce globally (not per-side):

- **Directional (current intent):** at most **one** working owned entry across both sides.
  Before submitting a new-signal entry, cancel/replace any opposite working owned entry.
- (Alternative, out of scope: a true two-sided quoting strategy with inventory logic.)

The duplicate/exposure check in `EnterSignal` becomes a single global predicate evaluated
by the actor against owned working entries, removing the side-specific blind spot.

---

## 6. Persistence (supports C-09, H-10)

A small durable checkpoint (single JSON file keyed by symbol+account):

- `runId`
- last processed execution/fill cursor
- `ownedPositionQty`, `averageEntryPrice`, `strategyRealizedPnL`, `pnlAtLastFlat`

Written on each owned fill and on clean stop; read at `Initializing` to re-adopt owned
orders and resume PnL/promotion accounting across restarts.

---

## 7. Deterministic test suite (H-19 — gate before any live use)

Build a fake Quantower broker/event adapter and property/unit tests for:

1. Late sibling bracket fill after local-flat → no reversal, ends flat, halted.
2. SL placement returns null type / fails → flatten + halt, never naked.
3. Repeated `FlattenPosition` calls before first close fills → exactly one close, no reverse.
4. `OnStop` with open position + queued entries → entries dropped, one flatten, flat verified.
5. Restart with open position → owned orders re-adopted, PnL cursor resumed.
6. Foreign order/position on same account → never cancelled/flattened.
7. Partial fills → protection invariant holds at each step.
8. Reconnect/FLUSH/overflow → book invalid, entries paused, rebuild, resume only when valid.
9. Session rollover with late pre-rollover event → no double reset (also H-07).

"Build clean" is **not** evidence of state-machine correctness; these tests are the gate.

---

## 8. Suggested implementation order

1. Ownership (`runId` in comments + adopt-only-own) — smallest change, unblocks C-09 and
   makes flatten/cancel safe to touch.
2. Actor + immutable `TrackedOrderState` (H-08, H-09) — the substrate everything else needs.
3. Lifecycle state machine + idempotent flatten (C-08).
4. Broker-truth fill handling (C-06) + protection invariant (C-07) + modify fix (H-11).
5. Entry policy (H-12).
6. Persistence + execution-cursor reconciliation (H-10).
7. Fake-broker test suite (H-19) — written alongside, not after.

Each step lands behind `ShadowOnly`; live is enabled only after step 7 is green.
