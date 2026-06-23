# MBO Market Data Analytics (Quantower)

Real-time market-microstructure analytics for **CME futures** in
[Quantower](https://www.quantower.com/), built on **dxFeed** Level 2 / trade data
with optional automated execution via **Interactive Brokers**.

The solution builds a single assembly, `MBO_Market_Data_Analytics.dll`, that exposes
two Quantower scripts:

- **MBP Market Data Analytics** — an `Indicator` that overlays a live GDI+ dashboard
  (buyer/seller flow, VWAPs, delta, DOM imbalance, order arrivals/cancels, and
  microstructure event scores) on the chart.
- **Data Microstructure Analytics Strategy** — a `Strategy` that trades the rolling
  buyer/seller volume ratio with brackets, risk limits, and session/connection
  handling.

Both share one analytics engine (`DataAnalyticsCalculator` + `AnalyticsEngineHost`).

## Requirements

- Windows + **.NET 10 SDK**
- **Quantower** installed (the project references
  `…\TradingPlatform\v<version>\bin\TradingPlatform.BusinessLayer.dll`)
- A Level 2 market-data connection — **dxFeed** (CME direct) or **Interactive Brokers**
  (with the CME contract mapped in Quantower settings). MBO per-order depth is attempted
  on any connection; the engine falls back to price-level MBP automatically when the feed
  does not supply per-order quotes.
- For live trading: an **Interactive Brokers** connection with the symbol mapped to the
  IB CME contract in Quantower settings

## Build & install

```sh
dotnet build DataAnalytics.csproj -c Release
```

The post-build step copies the DLL into Quantower's script folders automatically:

```
C:\Quantower\Settings\Scripts\Indicators\
C:\Quantower\Settings\Scripts\Strategies\
```

If your Quantower install path or version differs, update the `Reference` `HintPath`
and the two `Copy` targets in [`DataAnalytics.csproj`](DataAnalytics.csproj).

Then in Quantower:

1. Restart Quantower (or use *Refresh scripts*) so it picks up the new DLL.
2. **Indicator** — open a chart on the mapped dxFeed symbol → add indicator
   **"MBP Market Data Analytics"**.
3. **Strategy** — open the *Strategies* panel → add **"Data Microstructure Analytics
   Strategy"** → set the `Symbol` and `Account`.
4. **Backtester (optional)** — add the **"MBP Replay Backtester"** indicator to a chart
   to replay historical trades through the same engine/strategy logic and get a
   performance report (places no orders). Use it to tune the adaptive settings.
5. **MBO Recorder (optional)** — add the **"MBP MBO Recorder"** indicator to capture the
   live order-level book + trades to disk (one CSV per day) and build your own historical
   depth corpus. The panel's "Granularity" row confirms whether per-order MBO is reaching
   the script. Requires a per-order/MBO depth subscription.

## Configuration

Key inputs (full tables in [`DataAnalytics.md`](DataAnalytics.md#configuration-reference)):

- **Execution Mode** (strategy) — *Shadow then promote* (default) paper-trades on the live
  tape and only routes real IB orders once the paper record clears a net-of-cost gate
  (demoting back to paper if live performance degrades); *Shadow only* never trades live;
  *Live only* is the legacy immediate-execution behavior. No historical data needed — the
  strategy validates itself forward ("on-the-job").
- **Parameter Mode** (strategy) — *Adaptive* self-tunes entry thresholds from rolling
  percentiles of order flow and sizes brackets from live ATR, standing aside during a
  short warmup; *Static* uses the fixed threshold/tick inputs (previous behavior).
- **Calibration Mode** — *Auto (MTR)* calibrates window sizes from recent volume;
  *Manual* uses the L2 window inputs directly.
- **Absorption Volume Threshold (5s contracts)** — minimum 5-second volume required
  to trigger an absorption event. Default `0` = auto-calibrate to 2× the typical 5s
  rate after MTR warmup. Set a positive value to lock the threshold across restarts
  (the calibrated value is printed in the log on each startup).
- **Update Frequency (ms)** — dashboard/snapshot cadence (default 250).
- **Enable Feature Store Export** — append a wide-format CSV row per sample for
  offline analysis / model training.
- Strategy risk inputs — Order Quantity, Take Profit / Stop Loss (ticks), Max
  Exposure, Max Daily Loss, ratio buy/sell thresholds (+ hysteresis reset band),
  Max Consecutive Failures, Order Cooldown.

> **Trading risk:** the strategy places live orders. Validate on a **paper / sim
> account** first. The build machine has no market feed, so behavior can only be
> verified inside Quantower.

## How it works

- **Single worker thread.** Market-data callbacks (`Symbol.NewLast`, `Symbol.NewLevel2`)
  only enqueue a `MarketEvent` into a bounded (10,000-event) `BlockingCollection`.
  A single background worker drains the queue, applies events to the calculator,
  publishes a snapshot at most every `UpdateFrequencyMs` (default 250 ms), and
  invokes the strategy's signal-evaluation hook. The UI thread reads only the
  immutable, volatile `AnalyticsSnapshot` — never the live calculator state.

- **Order book — SortedDictionary, O(log n + k) reads.**
  `bidBook` and `askBook` are `SortedDictionary<long, double>` keyed by price-tick
  (`long = Math.Round(price / tickSize)`). `bidBook` uses a reverse comparer so
  iteration always starts at the best bid (highest price); `askBook` uses the default
  ascending order so iteration starts at the best ask (lowest price). This replaces
  the previous Dictionary + per-snapshot LINQ sort (O(m log m)) with O(log n + k)
  top-N traversal. Best bid/ask fields (`bestBidTicks`, `bestBidSize`, etc.) are
  maintained incrementally by `UpdateBest()` on every event; `RescanBest()` calls
  `SortedDictionary.First()` (O(log n)) instead of a full linear scan when the best
  level is removed.

- **Microstructure event scores — O(1) running sums.**
  Iceberg, replenishment, and spoof detectors maintain six running sum fields
  (`_icebergBidSum`, `_icebergAskSum`, etc.). Each sum is incremented on enqueue and
  decremented when the event is pruned from its 60s window, making the score getters
  O(1) instead of the previous O(n) `.Where().Sum()` queue scan.

- **Event-time windows (replay-safe).** All rolling windows are driven by event
  timestamps, not wall-clock time. The host calls `AdvanceTime(now)` before each
  snapshot publish so windows decay even during quiet periods. This makes the engine
  usable for historical replay and backtesting without clock-skew issues.

- **MBO-first data mode.** The engine always attempts per-order (Market-By-Order)
  depth. `SeedBookSnapshot()` requests an MBO snapshot; if the feed returns one, per-order
  tracking begins immediately. If not, the engine downgrades transparently to price-level
  MBP for the session. `ProcessLevel2Mbo` similarly falls back to price-level keys when
  `Level2Quote.Id` is null or empty, so any L2 feed is handled correctly. The active
  data mode is displayed in the HUD footer as **"Data Mode: MBO (per-order)"** (green)
  or **"Data Mode: MBP (price-level)"** (orange).

- **Metric quality elevation with MBO.** When `MboMode` is active, metrics that were
  previously Heuristic or Derived are promoted:

  | Metric group | MBP quality | MBO quality |
  |---|---|---|
  | New order counts (bid/ask) | Derived | **Exact** |
  | New order volume (bid/ask) | Derived | **Exact** |
  | Cancel counts (bid/ask) | Derived | **Exact** |
  | Cancel ratio (count) | Derived | **Exact** |
  | Cancel volume (bid/ask) | Heuristic | **Derived** |
  | Cancel ratio (volume) | Heuristic | **Derived** |
  | Iceberg / Replenishment / Spoof scores | Heuristic | **Derived** |

- **Book-valid self-healing.** At startup, `SeedBookSnapshot()` pulls a depth
  snapshot — MBO first, falling back to MBP. If the L2 feed is not yet active and
  the snapshot returns null, the host logs the condition and the book is instead
  validated on the **first live L2 event** — the strategy is never stuck indefinitely
  waiting for a seed that will never arrive.

- **Strategy diagnostic logging.** Every early-return path in `EvaluateTradingSignal`
  now emits a log every 30–60 s explaining why no signal has been produced ("L2 book
  not yet seeded", "engine calibrating (MTR warmup)", adaptive warmup progress, etc.).
  Once the engine is fully active, a heartbeat logs the current ratio vs thresholds
  every 60 s. All log entries are visible in the Quantower Strategy log panel.

- **Metric quality tags.** Every `MetricValue` carries a `MetricQuality` label —
  **Exact / Derived / Heuristic / Unavailable** — colour-coded on the HUD. Ratios use
  a safe zero-denominator helper: `0/0` → `Unavailable`; `n/0` → saturation value
  (`999`), not zero, so downstream logic is not inverted.

- **Queue overflow recovery.** On overflow, the host drains the queue, clears L2
  state, re-seeds the book from a fresh depth snapshot, and keeps the book-valid flag
  false until the rebuild succeeds.

## Known behavior

- **DOM imbalance near-zero for NQ is expected.** The DOM imbalance formula is
  `(bidVol − askVol) / (bidVol + askVol)`. Full-size NQ futures have a thin order
  book (1–5 contracts per price level), so bid and ask sizes at the best level
  frequently coincide (e.g., 2 vs 2 → 0%). This is correct market behavior.
  Micro NQ (MNQ) has ~10× more contracts per level and shows more meaningful
  non-zero DOM signal.

- **Startup silent period.** The strategy logs status every 30–60 s but places no
  trades until (a) the L2 book is seeded (either from the snapshot or the first live
  event) and (b) the MTR calibration warmup has seen `CalibrationTrades` (default
  1,000) trades. On a quiet overnight session this can take several minutes.

- **MTR calibration on slow markets.** In Auto-MTR mode the engine needs 1,000 trades
  before `IsCalibrated` becomes true. During very quiet hours (e.g., 3 AM ET for NQ)
  this warmup can take 10+ minutes. The 30s status log shows the running count.

- **Absorption threshold varies between restarts.** The auto-calibrated absorption
  threshold (`2× typical 5s volume`) is computed once from the MTR calibration window,
  which depends on time-of-day conditions. To get a consistent value across sessions,
  run with auto-calibration once, note the logged threshold, then set
  `Absorption Volume Threshold` to that value in the indicator/strategy settings.

## Documentation

- [`DataAnalytics.md`](DataAnalytics.md) — full metric specification, implemented
  architecture, calibration, HUD layout, and configuration reference.
- [`CLAUDE.md`](CLAUDE.md) — project constraints and engineering conventions for
  contributors/agents.
- `ref_docs/` — vendored Quantower API reference material.

## Project layout

```
DataAnalyticsCalculator.cs   Pure analytics engine (aggregators, metrics, order book)
AnalyticsEngineHost.cs       Shared host: worker, queue, calibration, snapshots, book seed
AdaptiveParameters.cs        Phase 1 self-tuning thresholds + ATR brackets
ShadowSimulator.cs           Paper-fill sim + perf tracker (shadow→live gate)
ReplayBacktester.cs          Trade-replay backtester (pure; reuses live engine)
ReplayBacktestIndicator.cs   On-chart backtester launcher + report panel
MboOrderBook.cs              Order-keyed (MBO) book reconstruction + event model
MboRecorder.cs               Async daily-rotated CSV recorder for the MBO stream
MboRecorderIndicator.cs      Standalone MBO capture launcher + diagnostics panel
DataAnalyticsIndicator.cs    Indicator + GDI+ HUD; shared snapshot/event types
DataAnalyticsStrategy.cs     Strategy: execution, risk, brackets, reconciliation
DataAnalytics.csproj         net10.0-windows; references Quantower BusinessLayer
DataAnalytics.md             Specification & architecture
```
