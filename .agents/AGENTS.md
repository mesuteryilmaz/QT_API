# Quantower CME Futures Strategy Rules & Project Context

## Project Constraints
* **Platform**: Quantower (C# API)
* **Brokerage / Execution**: Interactive Brokers (IB)
* **Market Data Provider**: dxFeed (market depth subscription)
* **Symbol Mapping**: IB CME Futures Data is mapped to dxFeed CME Futures Data in the platform settings.

## Execution Rules & Guidelines
1. **Interactive Brokers Limitations**:
   * Omit `REDUCE_ONLY` and `POST_ONLY` parameters from `PlaceOrderRequestParameters` / `ModifyOrderRequestParameters` as they are rejected or unsupported by Interactive Brokers for CME futures contracts.
2. **Data Speed & Processing**:
   * High-frequency updates from dxFeed Level 2 and ticks must not block the main trading execution thread. Use the prioritized `AsyncTaskQueue` pattern for order management.
3. **Symbol Mapping Considerations**:
   * Quantower automatically maps symbols when mapped in the UI. Ensure symbols retrieved inside the code via `Core.Instance.GetSymbol` or parameters refer to the active mapped symbols.

## Architecture (current)

| File | Responsibility |
| ---- | -------------- |
| `DataAnalyticsCalculator.cs` | Pure analytics engine. Incremental sliding-window aggregators (`TradeWindowAggregator`, `L2WindowAggregator`); event-time driven; O(1) getters. No threading/UI. |
| `AnalyticsEngineHost.cs` | Shared host (composition): worker thread, bounded queue + overflow recovery, capability probe, auto-MTR warmup, book seed, session rollover, snapshot publish, feature store. |
| `DataAnalyticsIndicator.cs` | `Indicator` + GDI+ HUD. Defines `MarketEvent`, `AnalyticsSnapshot`, `CalibrationMode`. |
| `AdaptiveParameters.cs` | Phase 1 self-tuning controller: rolling-percentile entry thresholds + ATR-scaled brackets, with a warmup-ready gate. |
| `ShadowSimulator.cs` | Paper-fill simulator + `TradePerformanceTracker` for the shadow→live execution envelope (promotion/demotion gate). |
| `MboOrderBook.cs` / `MboRecorder.cs` / `MboRecorderIndicator.cs` | MBO ingestion: order-keyed book reconstruction, async daily-rotated CSV recorder, standalone capture indicator. |
| `ReplayBacktester.cs` / `ReplayBacktestIndicator.cs` | Trade-replay backtester (pure) + on-chart launcher. Reuses the live calculator + adaptive controller. |
| `DataAnalyticsStrategy.cs` | `Strategy`: execution, risk, brackets, reconciliation. |

Indicator and strategy each **compose** an `AnalyticsEngineHost` (Quantower base
classes can't share a base). Put any shared lifecycle/plumbing in the host, not in
both files.

## Conventions (do not regress)

* **Event-time, not wall-clock**: window cutoffs use the calculator's event-time
  clock (`AdvanceTo`/`AdvanceTime`). Keep it that way so replay/backtest works.
* **Incremental aggregators**: do not reintroduce per-read LINQ scans over trade/L2
  history. Add new windowed metrics by extending the aggregators (window index +
  running sum), not by scanning.
* **Ratio safety**: build ratios via `MakeRatio` — `0/0` → `Unavailable`, `n/0` →
  saturation (not `0`). Signal code must skip non-warm / `Unavailable` ratios.
* **PnL sign**: signed position size already encodes direction; open PnL is
  `size * (last - avgEntry) * pointCost` with no extra sign factor.
* **Order roles**: tag orders via `Comment` (`MBO:ENTRY` / `MBO:BRK`); reconciliation
  recovers role via `RoleFromComment`. Only `Entry` fills spawn brackets.
* **Thread-safety**: position/PnL/signal state is guarded by `stateLock`; cross-thread
  flags are `volatile`. Never hold `stateLock` across a blocking broker call (enqueue
  to the prioritized queue instead). The `AdaptiveParameterController` and
  `TradePerformanceTracker` have their own locks; `ShadowSimulator` is worker-thread-only.
* **Execution envelope**: default is shadow (paper) until the promotion gate clears, then
  live, with demotion back to paper. The gate is net-of-cost with a positive lower-confidence
  bound (`mean − z·sem > 0`), not just "made money lately" — keep that teeth. Promotion resets
  the live tracker; demotion resets the shadow record (verdicts use post-transition data only).

## Regime roadmap

* **Phase 1 (done)**: `AdaptiveParameterController` — percentile entry thresholds +
  ATR brackets, warmup gate. Momentum polarity unchanged; only magnitudes adapt.
* **Phase 2**: `RegimeClassifier` (volatility / trend-vs-range / liquidity) with
  stand-aside gating in extreme/illiquid regimes.
* **Phase 3**: regime-driven **polarity** (momentum vs mean-reversion — "let the regime
  decide") and a per-regime parameter policy.
* **Phase 4 (done, trade-replay)**: `ReplayBacktester` drives the live calculator +
  adaptive controller from historical ticks. Trade-only (no historical L2); keep new
  backtest logic reusing the live engine, not a reimplementation.
* Feed regime inputs from existing calculator analytics; do not add per-read scans.

## MBO / data capture

* Per-order depth: subscribe via `AggregateMethod.None` + `GetMBOItems`; live `Level2Quote`
  carries `Id` (order id), `Priority` (queue), `NumberOrders`, `Closed`.
* The recorder is the forward-capture corpus (no historical depth exists from QT/dxFeed).
  Keep it off the data thread (own queue + writer thread, overflow drop-and-count).
* Recorded format = exactly what the live engine sees (train == serve). If you change the
  normalized `MboEvent` shape, version the CSV header.
* Next step (not yet done): wire order-level features (queue position, per-order cancellation)
  into the calculator behind a toggle, keeping the current price-aggregated path intact.

## Build & verify

* Build: `dotnet build DataAnalytics.csproj -c Release` (references the Quantower
  `TradingPlatform.BusinessLayer.dll`; PostBuild copies the DLL into the Quantower
  `Indicators` and `Strategies` script folders).
* The build machine has no live feed — runtime behavior must be validated on a
  sim/paper session in Quantower. The calculator is pure and unit-testable.
