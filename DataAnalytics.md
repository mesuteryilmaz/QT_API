# Data Analytics Specification

---

## Common Fields

All analytics messages share the following fields:

| Field | Tag | Type | Description |
| ----- | --- | ---- | ----------- |
| Instrument ID | i | Id | Instrument identifier from the market data system |
| Source System | s | Integer | Data source identifier |
| Calculation Time | t | Time | Analytics calculation timestamp |

Some fields may be transmitted as NULL values.

An `Of` (OrderbookFlush) flag indicates that all previously received values must be reset and recalculated.

---

## 1. Buyer Seller Analytics

```text
DABSRm
```

Provides trade statistics classified by aggressor side.

---

### Fields — Buyer Seller Analytics

| Tag | Description | Window |
| --- | ----------- | ------ |
| DABTCf | Buyer-initiated trade count | Last 60 seconds |
| DASTCf | Seller-initiated trade count | Last 60 seconds |
| DABTQf | Buyer-initiated trade quantity | Last 60 seconds |
| DASTQf | Seller-initiated trade quantity | Last 60 seconds |
| DABSCRf | Buyer/Seller trade count ratio | Last 60 seconds |
| DABSQRf | Buyer/Seller quantity ratio | Last 60 seconds |
| DATBSCRf | Cumulative Buyer/Seller trade count ratio | Session cumulative |
| DATBSQRf | Cumulative Buyer/Seller quantity ratio | Session cumulative |
| Of | Orderbook Flush | Reset signal |

---

### Definitions — Buyer Seller Analytics

### Buyer-Initiated Trade

Incoming buy order matches resting sell order(s).

### Seller-Initiated Trade

Incoming sell order matches resting buy order(s).

---

## 2. Volume Weighted Average Price Analytics (VWAP)

Message Type:

```text
DAVWAPm
```

Provides VWAP-based analytics.

---

### Fields — VWAP Analytics

| Tag | Description | Window |
| --- | ----------- | ------ |
| DAWTf | VWAP of trades | Last 5 minutes |
| DAWATf | VWAP of all trades | Session cumulative |
| DAWBTf | VWAP of buyer-initiated trades | Last 5 minutes |
| DAWSTf | VWAP of seller-initiated trades | Last 5 minutes |
| Of | Orderbook Flush | Reset signal |

---

### Definitions — VWAP Analytics

### DAWTf

Volume-weighted average price of all trades executed during the previous 5 minutes.

### DAWATf

Volume-weighted average price of all trades since session start.

### DAWBTf

VWAP of trades initiated by incoming buy orders.

### DAWSTf

VWAP of trades initiated by incoming sell orders.

---

## 3. Order Arrival Analytics

Message Type:

```text
DAARRm
```

Provides statistics regarding incoming orders.

Modified orders are included.

---

### Fields — Order Arrival Analytics

| Tag | Description | Window |
| --- | ----------- | ------ |
| DAOCf | Number of arrived orders | Last 60 seconds |
| DATOCf | Cumulative arrived orders | Session cumulative |
| DAOQf | Quantity of arrived orders | Last 60 seconds |
| DATOQf | Cumulative arrived quantity | Session cumulative |
| DABOCf | Number of arrived buy orders | Last 60 seconds |
| DASOCf | Number of arrived sell orders | Last 60 seconds |
| DABOQf | Quantity of arrived buy orders | Last 60 seconds |
| DASOQf | Quantity of arrived sell orders | Last 60 seconds |
| DAFAKCf | Number of Fill-And-Kill orders | Last 60 seconds |
| Of | Orderbook Flush | Reset signal |

---

## 4. Order Flow Analytics

Message Type:

```text
DAORDFm
```

Provides order size averages and volatility statistics.

Modified orders are included.

---

### Fields — Order Flow Analytics

| Tag | Description | Window |
| --- | ----------- | ------ |
| DAABQf | Average buy order quantity | Last 5 minutes |
| DAASQf | Average sell order quantity | Last 5 minutes |
| DAVBQf | Buy order quantity volatility (standard deviation) | Last 5 minutes |
| DAVSQf | Sell order quantity volatility (standard deviation) | Last 5 minutes |
| Of | Orderbook Flush | Reset signal |

---

### Definitions — Order Flow Analytics

### DAABQf

Average size of incoming buy orders during the last 5 minutes.

### DAASQf

Average size of incoming sell orders during the last 5 minutes.

### DAVBQf

Standard deviation of incoming buy order sizes during the last 5 minutes.

### DAVSQf

Standard deviation of incoming sell order sizes during the last 5 minutes.

---

## 5. Order Cancellation Analytics

Message Type:

```text
DACXRm
```

Provides cancellation statistics.

Includes:

- Explicit cancellations
- Modified orders
- Unfilled portions of Fill-And-Kill orders

---

### Fields — Order Cancellation Analytics

| Tag | Description | Window |
| --- | ----------- | ------ |
| DACXCf | Cancelled order count | Last 60 seconds |
| DACXQf | Cancelled order quantity | Last 60 seconds |
| DACXBCf | Cancelled buy order count | Last 60 seconds |
| DACXSCf | Cancelled sell order count | Last 60 seconds |
| DACXBQf | Cancelled buy order quantity | Last 60 seconds |
| DACXSQf | Cancelled sell order quantity | Last 60 seconds |
| DATCXCf | Cumulative cancelled order count | Session cumulative |
| DAWCXf | VWAP of cancelled orders | Session cumulative |
| DAWCXBf | VWAP of cancelled buy orders | Session cumulative |
| DAWCXSf | VWAP of cancelled sell orders | Session cumulative |
| DACXCRf | Cancel/Order Ratio (Count) | Last 60 seconds |
| DACXQRf | Cancel/Order Ratio (Quantity) | Last 60 seconds |
| DATCXCRf | Cumulative Cancel/Order Ratio (Count) | Session cumulative |
| DATCXQRf | Cumulative Cancel/Order Ratio (Quantity) | Session cumulative |
| Of | Orderbook Flush | Reset signal |

---

## Formula Reference

## Buyer/Seller Ratios

```text
DABSCRf = BuyerInitiatedTradeCount
           / SellerInitiatedTradeCount
```

```text
DABSQRf = BuyerInitiatedTradeQty
           / SellerInitiatedTradeQty
```

---

## Cancel/Order Ratios

```text
DACXCRf = CancelledOrderCount
           / ArrivedOrderCount
```

```text
DACXQRf = CancelledOrderQty
           / ArrivedOrderQty
```

---

## VWAP

```text
VWAP = Σ(Price × Quantity)
       / Σ(Quantity)
```

---

## Analytics Windows Summary

| Analytics Group | Time Window |
| --------------- | ----------- |
| Buyer/Seller Analytics | 60 seconds |
| Order Arrival Analytics | 60 seconds |
| Order Cancellation Analytics | 60 seconds |
| VWAP Analytics | 5 minutes |
| Order Flow Analytics | 5 minutes |
| Cumulative Metrics | Entire trading session |

---

## Mapping to Historical Order Book Reconstruction

For order-level datasets:

| Analytics | Can Be Rebuilt From Order Data |
| --------- | ------------------------------ |
| Buyer/Seller Analytics | Yes |
| VWAP Analytics | Yes |
| Order Arrival Analytics | Yes |
| Order Flow Analytics | Yes |
| Order Cancellation Analytics | Yes |

All analytics can be reconstructed from order-level and trade-level event data when order lifecycle events and executions are available.

---

## Implemented System Architecture

The project targets **.NET 10.0 (Windows)** and builds a single assembly,
`MBO_Market_Data_Analytics.dll`, which is copied on build into both the Quantower
`Indicators` and `Strategies` script folders.

### Source layout

| File | Responsibility |
| ---- | -------------- |
| `DataAnalyticsCalculator.cs` | Pure analytics engine. Ingests trades + L2, maintains incremental sliding-window aggregators and the sorted order book, exposes all metric getters. No platform/UI/threading concerns. |
| `AnalyticsEngineHost.cs` | Shared host (composition). Owns the background worker thread, bounded event queue with overflow recovery, capability probing, auto-MTR warmup, initial book seed with self-healing fallback, CME session-rollover detection, snapshot publishing, and optional feature-store export. |
| `DataAnalyticsIndicator.cs` | Quantower `Indicator`. Owns an `AnalyticsEngineHost`, renders the GDI+ HUD from the double-buffered snapshot. Also defines the shared `MarketEvent`, `AnalyticsSnapshot`, `CalibrationMode`, and `MetricValue` types. |
| `DataAnalyticsStrategy.cs` | Quantower `Strategy`. Owns an `AnalyticsEngineHost`, runs execution/risk/bracket logic via the host's per-event hook. |
| `AdaptiveParameters.cs` | Phase 1–3 adaptive stack: rolling-percentile thresholds + ATR brackets (Phase 1); lag-1 ACF polarity selection — Momentum / MeanReversion (Phase 2); regime classifier — volatility spike, extreme ratio, thin-queue stand-aside gates (Phase 3). |
| `ShadowSimulator.cs` | Paper-fill simulator + performance tracker for the shadow→live execution envelope. |
| `MboOrderBook.cs` | Order-keyed (MBO) book reconstruction + normalized `MboEvent` model. |
| `MboRecorder.cs` | Async, batched, daily-rotated CSV recorder for the normalized MBO stream. |
| `MboRecorderIndicator.cs` | Standalone launcher: subscribes to order-level depth + trades, reconstructs the book, records to disk. |
| `ReplayBacktester.cs` | Pure trade-replay backtester. Reuses the live calculator + adaptive controller; produces a performance report. |
| `ReplayBacktestIndicator.cs` | On-chart launcher: pulls tick history, runs the backtester, logs/draws the report, optionally exports a trades CSV. |

Because Quantower's `Indicator` and `Strategy` base classes cannot share a common
base, the indicator and strategy each **compose** an `AnalyticsEngineHost` rather
than inheriting shared plumbing.

### Threading model

- Market-data callbacks (`Symbol.NewLast`, `Symbol.NewLevel2`) only enqueue a
  `MarketEvent` into a bounded (10,000) `BlockingCollection`. They never compute.
- A single background worker drains the queue, applies events to the calculator,
  publishes a snapshot at most every `UpdateFrequencyMs` (default 250 ms), and
  invokes the optional per-event hook (the strategy's signal evaluation).
- The UI thread (`OnPaintChart`) reads only the immutable, volatile
  `AnalyticsSnapshot` — never the live calculator state (double buffering).
- On queue overflow the host drains the queue, clears L2 state, re-seeds the book
  from a fresh depth snapshot, and flags the book invalid until rebuilt.

### Event-time windows (replay-safe)

All rolling windows are driven by **event timestamps**, not wall-clock time. The
calculator advances an internal event-time clock on every event and decays every
window incrementally. The host also calls `AdvanceTime(now)` before each publish so
windows still age out during quiet periods. Passing event timestamps instead of the
wall clock makes the engine usable for **historical replay / backtesting** and
immune to clock skew.

### Incremental aggregation (O(1) reads)

Sliding-window statistics are maintained incrementally rather than recomputed on
each read:

- **`TradeWindowAggregator`** — one time-ordered node store with a head cursor and
  running sums per window for **{1s, 2s, 5s, 30s, 60s, 5m, 15m}** (total/buy/sell
  qty & count, ΣP·Q overall and per side). `Add` is O(1) amortized; every getter is
  O(1). Absorption scans only the small 5s slice for the price range.
- **`L2WindowAggregator`** — running add/cancel counts, quantities, and Σqty² per
  book side over **{60s, 900s}** (the Σqty² feeds the order-size standard-deviation
  metrics).
- **Microstructure event running sums** — six fields (`_icebergBidSum`,
  `_icebergAskSum`, `_replenishBidSum`, `_replenishAskSum`, `_spoofBidSum`,
  `_spoofAskSum`) replace per-read `.Where().Sum()` O(n) queue scans. Each sum is
  incremented when an iceberg/replenishment/spoof event is enqueued into its 60s
  window and decremented when the event is pruned on expiry. The getter methods
  `SumIcebergEvents(bool)`, `SumReplenishmentEvents(bool)`, `SumSpoofEvents(bool)`
  return the running sum in O(1). All six fields are reset in `Reset()` and
  `ResetSessionState()`.

### Order book storage

The price-level book is stored in two `SortedDictionary<long, double>` instances
keyed by **price-tick** (`long = (long)Math.Round(price / symbol.TickSize)`):

- **`bidBook`** — custom reverse comparer `(a, b) => b.CompareTo(a)`, so the
  dictionary's natural iteration order is highest price-tick first (best bid first).
- **`askBook`** — default ascending comparer, so iteration is lowest price-tick first
  (best ask first).

This replaces the previous `Dictionary<long, double>` which required
`OrderByDescending` / `OrderBy` LINQ sorts (O(m log m)) on every 250 ms snapshot
publish. With `SortedDictionary`, iterating the top-N levels in
`CalculateImbalance(N)` and `GetBookPressure()` costs O(log n + k) — the tree
traversal to the first node plus k steps through the iterator.

In MBP mode, `ProcessLevel2(time, id, price, size, isBid, closed)` applies each
feed update directly to the appropriate book dictionary, handling add, reduce, and
remove semantics. In MBO mode, `ProcessLevel2Mbo` maintains an order-keyed map and
derives aggregated price-level sizes, keeping `bidBook`/`askBook` consistent with
the order-level view.

### Best-quote tracking

Six fields track the best bid and ask incrementally:
`hasBestBid`, `hasBestAsk`, `bestBidTicks`, `bestAskTicks`, `bestBidSize`,
`bestAskSize`.

**`UpdateBest(isBid, tick, newTotal)`** is called after every book update:
- If `newTotal > 0` and the tick improves the best (higher for bid, lower for ask),
  or equals the current best, the fields are updated in place — O(1).
- If `newTotal <= 0` and the tick was the current best, `RescanBest` is called.

**`RescanBest(isBid)`** reads `SortedDictionary.First()` — O(log n) — to find the
new best entry without scanning the whole book. Because the SortedDictionary
maintains sorted order, `First()` always yields the best remaining level.

The `GetQueueImbalance()` (1-level DOM imbalance) path reads `bestBidSize` /
`bestAskSize` directly, requiring no book traversal at all.

### MBO-first data mode

The engine always attempts per-order (Market-By-Order) depth. No vendor-specific
detection is performed — MBO is enabled unconditionally, and fallback is handled at
the data level:

- **`ProcessLevel2Mbo`** — if `Level2Quote.Id` is null or empty, falls back to using
  the price as a synthetic key, providing MBP-equivalent behavior on any L2 feed.
- **`SeedBookSnapshot`** — requests an MBO snapshot via
  `AggregateMethod.None + GetMBOItems = true`. If the feed returns a valid snapshot,
  per-order tracking begins. If null, the engine logs a downgrade notice and falls
  through to the standard MBP seed (`AggregateMethod.Level2`).

`Calculator.MboMode` is set to `true` unconditionally at startup. `SeedBookSnapshot`
downgrades it to `false` only when the MBO snapshot returns null. The active data mode
is surfaced in `AnalyticsSnapshot.IsMboActive` and displayed in the HUD footer.

### Book-valid startup resilience

At startup, `AnalyticsEngineHost.Start()` calls `SeedBookSnapshot()`, which first
tries an MBO depth snapshot, then falls back to MBP if the MBO snapshot is null.
If the L2 feed is not yet active at all and both attempts return null, the host
logs the condition and the book is seeded from the live stream instead:

- The host logs: *"Initial L2 snapshot returned null (L2 feed not yet active). Book
  will be seeded from the live stream."*
- `isBookValid` remains `false`.
- On the **first live L2 event** processed by the worker, `isBookValid` is set to
  `true` and the host logs: *"L2 book validated from live stream (initial snapshot
  was unavailable at startup)."*

This ensures the strategy is never permanently silent because of a missed startup
seed. Without this, a null seed left `isBookValid = false` forever and the strategy's
`!engine.IsBookValid` guard prevented all signal evaluation indefinitely.

### Calibration

1. **Volume windows (startup, AutoMTR mode)** — median volume of 1-minute bars over
   the last 2 days, filtered to RTH (09:30–16:00 EST), rounded to the nearest 10,
   becomes the **Short Trade Volume Window**; **Long = Short × 5**. (In Manual mode
   the operator supplies the L2 windows directly.)
2. **Message-to-Trade Ratio (MTR) warmup** — over the first `CalibrationTrades`
   trades (default 1,000) the calculator computes
   `MTR = L2 message count / traded volume`
   and sets `OrderCountWindowShort = max(100, ShortVol × MTR)` and
   `OrderCountWindowLong = max(500, LongVol × MTR)`. If volume is zero it falls back
   to `MTR = 5`. `IsCalibrated` becomes `true` only after the warmup completes. On
   a quiet overnight session (e.g., 3 AM ET for NQ) accumulating 1,000 trades can
   take 10+ minutes; the strategy's 30s status log shows the running count.
3. **Absorption threshold (auto mode)** — immediately after MTR warmup, if
   `AbsorptionVolumeThreshold` is still `0` (not pinned by the operator), it is set to
   `max(10, (TradeVolumeWindowShort / 12) × 2)` — i.e. 2× the typical 5s traded volume
   (the short window divided into 12 five-second buckets). This prevents the threshold
   from being calibrated for full NQ volumes when running on MNQ (or vice versa). The
   calibrated value is logged as `"Absorption threshold: X contracts/5s"`. To get a
   stable value across restarts, note this number and set
   `Absorption Volume Threshold (5s contracts)` in the indicator/strategy inputs.

### Metric quality and ratio safety

Every `MetricValue` carries a `MetricQuality` tag — **Exact / Derived / Heuristic /
Unavailable** — colour-coded on the HUD. Ratios are produced through a single
helper with safe zero-denominator handling:

- `0 / 0` → `Unavailable` (rendered `-`; consumers must not act on it).
- `n / 0` (one-sided extreme, e.g. all buys, no sells) → a saturation value
  (`999`), **not** `0`, so downstream signal logic is not inverted.

#### MBO quality elevation

When `MboMode` is active (per-order depth is confirmed), several quality labels are
promoted relative to their MBP baseline:

| Metric | MBP quality | MBO quality | Reason |
|--------|-------------|-------------|--------|
| New order counts (bid / ask) | Derived | **Exact** | Each L2 event corresponds to a real order add |
| New order volume (bid / ask) | Derived | **Exact** | Same — size is the true order quantity |
| Cancel counts (bid / ask) | Derived | **Exact** | Cancel events map 1:1 to order removals |
| Cancel ratio (count) | Derived | **Exact** | Both numerator and denominator are now Exact |
| Cancel volume (bid / ask) | Heuristic | **Derived** | Volume known per event; timing heuristic remains |
| Cancel ratio (volume) | Heuristic | **Derived** | Same rationale |
| Iceberg score (bid / ask) | Heuristic | **Derived** | Order identity lets the detector confirm refills |
| Replenishment score (bid / ask) | Heuristic | **Derived** | Same |
| Spoof score (bid / ask) | Heuristic | **Derived** | Add+cancel pairs are traceable per order ID |

MBP mode retains all the original quality labels — the engine falls back gracefully and
no metric is removed.

### Strategy diagnostic logging

`EvaluateTradingSignal` previously had four silent early-return paths that produced
no log output, making it impossible to distinguish "running but warming up" from
"not running at all." All paths now emit periodic log entries:

| Condition | Interval | Message |
| --------- | -------- | ------- |
| `!IsBookValid` | 30 s | "L2 book not yet seeded — no signals yet." |
| `!IsCalibrated` | 30 s | "engine calibrating (MTR warmup) — no signals yet." |
| Adaptive controller warming up | 30 s | Sample count and warmup progress |
| Adaptive controller first ready | once | Buy/sell thresholds, ATR, TP/SL, sample count, initial polarity |
| Polarity transition | on change | `Momentum → MeanReversion (ACF=−0.234). Signal state reset.` |
| Regime transition | on change | `Normal → StandAside: thin queue (1 orders at best)` |
| Ratio not usable | 60 s | IsWarm and Quality values |
| Active evaluation | 60 s | Current ratio vs thresholds, polarity, shadow trade summary |

Log entries appear in the Quantower Strategy log panel and the daily `.slog` file
under `Scripts\ScriptsData\<strategy-instance>\logs\`.

### DOM imbalance behavior

The imbalance formula is `(bidVol − askVol) / (bidVol + askVol)`, displayed with
one decimal percentage (P1 format, so values below ±0.05% show as "0.0%"). For
**full-size NQ futures**, the order book is typically very thin — 1–5 contracts per
price level. Because sizes are small integers, the bid and ask sizes at the best
level frequently coincide (e.g., 2 vs 2 → exactly 0%). The same balancing effect
applies at 3, 5, and 10 levels as the volumes average out. This is correct market
behavior: CME NQ is an efficient, liquid market where passive order books are often
symmetric.

**Micro NQ (MNQ)** has approximately 10× more contracts per price level, making
equal bid/ask counts rare and producing meaningful non-zero DOM imbalance readings.
The DOM imbalance metrics are therefore more informative when applied to MNQ than to
full-size NQ.

If the DOM imbalance is needed as a signal for NQ, consider using MNQ as a proxy
for passive order book pressure while routing execution on NQ, or reducing the P1
format to P2 to surface sub-0.1% differences.

### Excluded metrics (data limitations)

- **Fill-and-Kill (FAK) order count** is not reported. Even with MBO, the Quantower
  `Level2Quote` does not expose a FAK/IOC flag, so isolating immediate-or-cancel
  orders from regular limit-order removals is not possible.
- **Arrival/cancel counts in MBP mode** are level-change event counts (Derived quality),
  not true per-order counts. In MBO mode these become Exact — each event maps to one
  real order add or remove. See the MBO quality elevation table above.

### Feature-store export

When enabled, the calculator appends a **wide-format CSV** (one row per sample, one
column per metric, header written automatically, invariant-culture numbers) to
`FeatureStorePath`. Suitable for offline analysis / model training.

### HUD layout (GDI+ overlay)

The dashboard overlay renders two columns plus a diagnostics footer:

- **Column 1**
  - **1. Buyer/Seller (60s & Session)** — trade count B/S, trade volume B/S,
    B/S ratio (count & volume), cumulative volume B/S, cumulative delta,
    cumulative trade count.
  - **2. VWAP** — rolling VWAP 1m/5m/15m, session VWAP & deviation,
    VWAP distance (ticks), aggressor VWAP buy/sell.
  - **3. Delta & Velocity** — delta 1s/5s/30s/60s, delta velocity.
- **Column 2**
  - **4. DOM Imbalances & Pressure** — queue imbalance (best bid/ask), 3/5/10-level
    imbalance, order-book pressure (distance-weighted); **Best Bid / Best Ask order
    count** (MBO exact; Unavailable in MBP). All imbalance values near-zero for
    full-size NQ; more active on MNQ. See DOM imbalance behavior note above.
  - **5. Order Arrivals & Cancels (60s)** — new orders B/A count & volume,
    cancel orders B/A count & volume, cancel ratio (count & volume).
  - **6. Microstructure Events** — absorption buy/sell (5s), iceberg, replenishment
    and spoofing scores per side (60s). Scores use O(1) running sums.
- **Footer** — two-row status bar at the bottom of the panel:
  - Row 0: engine status (CALIBRATED / CALIBRATING with MTR value and overflow counter)
    on the left; **Data Mode** (MBO per-order in green, or MBP price-level in orange)
    on the right.
  - Row 1: active L2 and trade windows on the left; combined **L2 Depth / Update (ms)**
    (`N levels / X ms`) and feature-store status on the right, plus the last-update
    timestamp.

---

## 6. Order Execution Strategy (DataAnalyticsStrategy)

A production-grade execution strategy class, `DataAnalyticsStrategy`, runs in the
Quantower Strategy Runner and trades on the microstructure signals produced by the
shared engine.

### Key capabilities

- **Mapped symbol & account selection** — `Symbol` / `Account` inputs bind to mapped
  CME contracts (dxFeed CME data mapped to Interactive Brokers CME execution); both
  are re-resolved to the active mapped instances on start.
- **Shared analytics engine** — uses the same `AnalyticsEngineHost` as the indicator;
  the trading signal is evaluated on the worker thread via the host's
  `OnEventProcessed` hook (no separate plumbing).
- **Signal logic with hysteresis** — compares the rolling 60s buyer/seller **volume
  ratio** against configurable buy/sell thresholds; a separate pair of reset
  thresholds (hysteresis band) re-arms each side so a single sustained signal does
  not fire repeatedly. `Unavailable`/not-warm ratios are skipped.
- **Entry cooldown** — `Order Cooldown (Seconds)` throttles entry attempts.
- **Comprehensive diagnostic logging** — every early-return path in
  `EvaluateTradingSignal` logs its reason every 30–60 s. When the engine is active,
  a heartbeat logs ratio vs thresholds every 60 s. Logging starts immediately on
  strategy start and continues through all states. See "Strategy diagnostic logging"
  above for the full matrix.
- **Prioritized async task queue** — all broker calls are serialized through a
  priority queue: cancellations (0) before modifications (1) before placements (2).
- **Interactive Brokers safety** — never sends `REDUCE_ONLY` / `POST_ONLY`.
- **Brackets with restart-safe roles** — each entry fill places a Stop-loss and a
  Take-profit. Every order is tagged in its `Comment` (`MBO:ENTRY` / `MBO:BRK`) so
  that, after a restart or reconnect, reconciliation recovers each order's exact role
  from the tag. Only true entries spawn brackets, which prevents a bracket fill after
  a reconnect from cascading into new orders. When the position goes flat, sibling
  brackets are cancelled; on partial reductions, excess bracket quantity is trimmed.
- **Order-state reconciliation** — orders tracked in a `ConcurrentDictionary`, updated
  via `Order.Updated`, reconciled on `Core.OrderAdded/OrderRemoved/PositionAdded/PositionRemoved`.
- **Partial-fill position rebuild** — average entry price and signed position size are
  recomputed on every partial fill; realized PnL accrues on reductions.
- **Risk limits & halting**
  - **Max Exposure** caps net open size.
  - **Max Daily Loss** — evaluated on each event from *realized + open* PnL. The open
    PnL uses the signed position size directly (correct for both longs and shorts). On
    breach the strategy flattens and hard-halts.
  - **Consecutive-failure halt** — repeated order rejects/exceptions
    (`Max Consecutive Failures`) flatten and halt.
- **CME session rollover** — at 17:00 Chicago the engine resets session metrics and
  signals the strategy, which flattens open positions and resets daily realized PnL.
- **Disconnect / reconnect recovery** — pauses on disconnect; on reconnect performs a
  full broker state query to rebuild positions and the order map.
- **Order-placement concurrency gate** — a volatile `isOrderPlacementPending` flag
  blocks duplicate placements while a request is in flight under high-frequency L2.
- **Concurrency safety** — all position/PnL/signal state is guarded by a single lock;
  cross-thread flags (`isRiskHalted`, `isConnectionActive`) are `volatile`.
- **Log handle release workaround** — `GC.Collect()` + `GC.WaitForPendingFinalizers()`
  in `OnStop()` / `OnClear()` releases Quantower's Serilog file handles to avoid
  `.slog` sharing violations when the script is removed.

### Adaptive parameters — three-phase self-tuning stack

`Parameter Mode` selects between **Static** (the fixed operator inputs) and
**Adaptive** (self-tuning), implemented by `AdaptiveParameterController`
(`AdaptiveParameters.cs`). In Adaptive mode the controller runs three cooperating
phases on the same 2-second recalc cycle:

#### Phase 1 — data-driven entry thresholds + ATR-scaled brackets

- **Entry thresholds** — instead of the fixed 1.5 / 0.67 ratio cut-offs, the
  controller samples the live buyer/seller volume ratio at 250 ms intervals and uses
  rolling **percentiles** of that distribution (upper = `Adaptive Entry Percentile`,
  lower = `1 −` that). The hysteresis re-arm level is the running **median**.
- **ATR-scaled brackets** — TP/SL distances are `round(ATR_ticks × multiplier)`,
  clamped between `MinBracketTicks` and `MaxBracketTicks`, so risk is consistent
  across volatility regimes. Two ATRs are maintained via Wilder smoothing: a
  **baseline** (14-min period, seeded from history) and a **fast** (5-min period,
  seeded to the same initial value and allowed to diverge as live bars arrive).
- **Warmup gate** — until both an ATR estimate and enough ratio samples (~2 min) are
  available, the controller reports *not ready* and the strategy stands aside.
  Progress is logged every 30 s.

#### Phase 2 — polarity selection (momentum vs mean-reversion)

Each recalc cycle computes the **lag-1 Pearson autocorrelation** of the most recent
60 ratio samples (~15 s). The autocorrelation measures whether the ratio tends to
persist (positive → momentum) or revert (negative → mean-reversion):

- `ACF > +0.10` → `Polarity = Momentum` — buy when buyers dominate, sell when sellers
  dominate.
- `ACF < −0.10` → `Polarity = MeanReversion` — buy when sellers dominate (expecting a
  bounce), sell when buyers dominate (expecting a fade). Entry and reset thresholds are
  the same values; only the signal direction is inverted.
- `ACF ∈ [−0.10, +0.10]` → current polarity is held (hysteresis band prevents
  rapid flip-flopping in ambiguous conditions).

On a polarity transition the strategy logs `Momentum → MeanReversion (ACF=−0.234)`,
resets both signal flags, and immediately re-evaluates on the next tick.

#### Phase 3 — regime classifier (stand-aside gate)

Three gates are evaluated each recalc cycle. Any gate firing sets `Regime = StandAside`
and the strategy skips all new signal entries (exits still process normally):

| Gate | Condition | Tunable input |
|------|-----------|---------------|
| **Volatility spike** | `fastATR / baselineATR > VolatilityGateRatio` | `Volatility Gate Ratio` (default 1.5) |
| **Extreme ratio** | Last ratio > 97th or < 3rd percentile of distribution | Config only (`ExtremeUpperPercentile` / `ExtremeLowerPercentile`) |
| **Thin queue** (MBO only, per-tick) | `bestBidOrders + bestAskOrders ≤ threshold` | `Thin Queue Stand-Aside` (default 2; 0 = disabled) |

The thin-queue gate only fires when best-level order count quality is `Exact` (MBO
mode); it is automatically disabled in MBP mode. Regime transitions are logged with
the triggering reason (`thin queue (1 orders at best)` / `voltRatio=2.1 extreme`).

Static mode reproduces the previous fixed-parameter behavior exactly and is fully
backward-compatible.

### Execution envelope — shadow (paper) → promotion gate

`Execution Mode` wraps the strategy in a "prove it before risking capital" loop
(`ShadowSimulator.cs`). This is the **on-the-job training** path: it requires no historical
depth — the strategy validates itself forward on the live tape.

- **Shadow then promote (default).** Signals are paper-traded against the live tape — no
  IB orders — with net-of-cost PnL tracked. Only when the paper record clears the gate does
  the strategy switch to **live IB execution**. If the live record later degrades (or a hard
  risk halt fires) it **demotes** back to paper and re-validates on a fresh record.
- **Shadow only.** Pure forward paper test; never routes to IB.
- **Live only.** Routes to IB immediately — the legacy behavior, no gate.

**Paper fill model.** One bracketed position at a time. Entries normally fill
*aggressively at the far touch* (buy at ask / sell at bid, + slippage) — pessimistic
and reproducible. **Queue-aware exception:** when MBO mode is active and the total
order count at the best level is ≤ 2 (thin queue), the entry fills at the **mid-price**
instead, reflecting the higher probability of getting a fill near mid when there is
minimal resting interest. Stop exits fill at the stop (+ slippage), take-profit at the
limit. Costs = commission per contract (both sides) + the spread paid on entry.

**The gate has teeth (not just "made money lately").** Promotion requires: trades ≥
`Promotion Min Trades`, mean PnL ≥ `Promotion Min Expectancy`, **and** a positive one-sided
lower confidence bound `mean − Z·(std/√n) > 0`. The LCB demands *more* trades when per-trade
variance is high, which is the cheap, luck-resistant stand-in for the anytime-valid test until
that lands. Demotion uses the same LCB on the live record (`Demotion Min Trades`). On
promotion the live tracker resets; on demotion the paper record resets — so each verdict is
evaluated on data gathered *after* the last transition, not reused.

**Caveats (carry over from the earlier discussion):** forward-only validation is *slow*
(real-time; weeks of paper to reach significance on a thin edge), it is a single realized
path (less statistical leverage than a historical corpus), and the aggressive-entry paper
fill is deliberately conservative (live limit entries may do better or may not fill at all —
a queue-aware fill model arrives with MBO ingestion).

---

## Configuration Reference

### Indicator inputs

| # | Input | Default | Notes |
|---|-------|---------|-------|
| 1 | Calibration Mode | Auto (MTR) | Auto vs Manual L2 windows |
| 2 | Short / Long Trade Volume Window | 1000 / 5000 | Auto mode overrides Short from RTH median |
| 3 | Manual Short / Long L2 Window | 2000 / 10000 | Used only in Manual mode |
| 4 | MTR Calibration Trades Count | 1000 | Warmup sample size |
| 5 | Initial Snapshot Depth Levels | 360 | Depth pulled for the book seed |
| 6 | Analytics Update Frequency (ms) | 250 | Snapshot/publish cadence |
| 7 | Symbol | — | Target instrument |
| 8 | Absorption Volume Threshold (5s contracts) | 0 | 0 = auto-calibrate to 2× typical 5s rate after MTR warmup; set a positive value to pin the threshold across restarts |
| 9 | Enable Feature Store Export | false | Wide-format CSV append |
| 10 | Feature Store Path | `…\ScriptsData\microstructure_features.csv` | Output file |

### Strategy inputs (additional)

| Input | Default | Notes |
| ----- | ------- | ----- |
| Order Quantity | 1 | Contracts per entry |
| Take Profit / Stop Loss (Ticks) | 10 / 10 | 0 disables that bracket leg |
| Max Exposure (Contracts) | 2 | Net position cap |
| Max Daily Loss ($) | 500 | Realized + open PnL halt |
| Buyer/Seller Ratio Buy / Sell Threshold | 1.5 / 0.67 | Entry triggers |
| Ratio Buy / Sell Reset Threshold | 1.1 / 0.9 | Hysteresis re-arm band |
| Max Consecutive Failures | 5 | Halt after N rejects |
| Order Cooldown (Seconds) | 5 | Min spacing between entries |
| Parameter Mode | Adaptive | Adaptive (self-tuning) or Static (fixed inputs) |
| Adaptive Entry Percentile | 0.85 | Upper percentile for entries; lower = 1 − this |
| Adaptive ATR Period (Minutes) | 14 | Wilder ATR period for bracket sizing |
| Adaptive TP / SL ATR Multiplier | 1.0 / 1.0 | Bracket distance = ATR × multiplier (clamped) |
| Execution Mode | Shadow then promote | Paper→promote, paper-only, or live-only |
| Commission per Contract ($, per side) | 0.0 | Net-of-cost model for the gate |
| Slippage (Ticks) | 0 | Adverse ticks on paper entry + stop exits |
| Promotion Min Trades | 30 | Min paper trades before promotion |
| Promotion/Demotion Confidence Z | 1.64 | LCB z (≈95% one-sided) |
| Promotion Min Expectancy ($/trade) | 0.0 | Min mean net PnL per trade to promote |
| Demotion Min Trades | 20 | Min live trades before demotion can trigger |
| Volatility Gate Ratio (fastATR/ATR) | 1.5 | Phase 3: regime stand-aside when `fastATR / baselineATR` exceeds this (Adaptive mode only) |
| Thin Queue Stand-Aside (MBO orders) | 2 | Phase 3: stand aside when `bestBidOrders + bestAskOrders ≤` this (MBO only; 0 = disabled) |

> In **Static** mode the ratio thresholds and TP/SL tick inputs above are used
> directly. In **Adaptive** mode they are superseded by the controller (the static
> values still apply as the fallback before warmup completes for bracket sizing).
> Phase 2 (polarity) and Phase 3 (regime classifier) are only active in Adaptive mode.

---

## Replay Backtester

`ReplayBacktestIndicator` ("MBP Replay Backtester") validates the signal and the
Phase-1 adaptive parameters against historical data **without placing orders**. Add it
to a chart on the mapped symbol; it pulls trade-tick history, replays it, logs a
report, draws a summary panel, and can export a per-trade CSV.

**How it works.** It reuses the same `DataAnalyticsCalculator` and
`AdaptiveParameterController` as the live strategy (via `ReplayBacktester`), so results
reflect live behavior. Each historical trade is applied in event time; the signal,
thresholds, ATR brackets and warmup gate behave exactly as they would live.

**Scope & limitations (be aware):**

- **Trade-only.** Historical Level 2 is not generally available, so order-book metrics
  are not exercised. The backtested signal is the trade-flow buyer/seller volume ratio
  — which is what the strategy trades on — but DOM/cancel/spoof inputs are absent.
- **Aggressor side.** Uses the feed's historical aggressor flag when present; otherwise
  falls back to the **tick rule** (toggle: *Infer Aggressor*). Check the reported
  *Aggressor coverage* — low coverage means the signal is approximate.
- **Fill model.** One bracketed position at a time (no pyramiding). Entry fills at the
  signal trade price (+ slippage); stop exits at the stop price (+ slippage); take
  profit at the limit price. If both bracket legs fall inside one trade, the stop is
  assumed first (conservative). Limit-entry queue dynamics are not modeled.
- **Tick-history depth** depends on the data provider (often only recent days). If no
  data is returned, the panel says so.

**Report:** trade count, win rate, net/gross PnL, commission, profit factor,
expectancy, average win/loss, max drawdown, and max consecutive losses. Tune
`Adaptive Entry Percentile`, `ATR Period`, and the TP/SL ATR multipliers here, then
mirror the chosen values into the strategy.

---

## MBO Ingestion & Recorder

`MboRecorderIndicator` ("MBP MBO Recorder") captures the **live order-level (Market-By-Order)
book + trades** to disk and reconstructs an order-keyed book. It is the forward-capture answer
to "no historical depth": you manufacture your own research corpus from the live feed, in the
exact format the live engine sees (train == serve, same vendor, same code path).

It is **independent of the analytics engine and strategy** — run it purely for data capture
with zero impact on the trading path.

**How it works.** Quantower exposes per-order depth via `AggregateMethod.None` +
`GetLevel2ItemsParameters.GetMBOItems`; the live `Level2Quote` carries `Id` (order id),
`Priority` (queue priority), `NumberOrders`, `Price`, `Size`, `PriceType`, `Closed`. The
indicator seeds from an MBO snapshot, then folds each live quote (and each trade, with aggressor
side) into `MboOrderBook`, which resolves the raw quote into a normalized `MboEvent`
(Add / Update / Remove / Snapshot / Trade) and maintains per-price size & order-count
aggregates plus best-effort queue-ahead size.

**Recording.** `MboRecorder` runs on its own thread — producers only enqueue — with a bounded
queue (overflows are dropped-and-counted so capture can never stall the feed), batched flushes,
and one CSV per UTC day: `mbo_<symbol>_<yyyyMMdd>.csv` with columns
`seq,timeUtc,action,side,price,size,priority,numOrders,orderId`. Timestamps are receive-time
UTC (consistent with the rest of the engine).

**Panel diagnostics** (verify you're actually getting MBO): rows written / dropped, events/sec,
live order count, bid/ask levels, best bid/ask with size, and a **granularity check** — the EMA
of `NumberOrders` per quote: ≈1 ⇒ true per-order MBO, >1 ⇒ the feed is price-aggregated.

**Caveats:**
- **MBO must be flowing to the script.** If your Quantower depth subscription is aggregated, the
  panel's granularity row will say `AGGREGATED` — fix the subscription to per-order/MBO depth.
- **Timestamps are receive-time,** not exchange time (exchange timestamps aren't reliably
  exposed here). Fine for the event-time engine; note it for latency-sensitive research.
- **CSV** is the first format (simple, inspectable). At sustained very high MBO rates a binary
  format would be smaller/faster — a future optimization; the async + overflow-drop design
  protects the feed in the meantime.
- This records and reconstructs the book. Order-level features are now wired into the live
  calculator: per-order spoof detection, best-level order counts, and the queue-aware fill
  model all use the MBO stream. The recorder remains the tool for building an offline
  research corpus from the live feed.

> **Note:** the rolling-window definitions in this document's earlier sections
> (60-second / 5-minute) describe the original specification. The implemented engine
> uses the activity-/event-time windows documented above (1s–15m for trades, 60s/900s
> for L2) and exposes the original spec metrics through compatibility wrappers.
