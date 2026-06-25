# QT_API V2 Architecture

## Dependency Direction

```text
QT.Core
  -> QT.Market
      -> QT.Features
          -> QT.Storage / QT.Runtime / QT.Simulation
              -> DataAnalytics.csproj Quantower host

QT.Execution is dormant and not referenced by DataAnalytics.csproj.
```

`QT.Features`, `QT.Market`, `QT.Runtime`, and replay code do not reference Quantower APIs. Quantower-specific code exists only under `src/QT.Quantower` and the root `DataAnalytics.csproj`.

## Runtime Flow

```text
Quantower Last/Level2 events
  -> NormalizedMarketEvent
  -> AnalyticsRuntime serialized worker
  -> OrderBookEngine lifecycle + MBO/MBP book
  -> immutable BookSnapshot
  -> FeatureEngine
  -> immutable FeatureSnapshot
  -> CsvAnalyticsRecorder
  -> focused GDI panel view
```

Replay uses the same `AnalyticsRuntime`, `OrderBookEngine`, and `FeatureEngine`.

## Book Lifecycle

`OrderBookEngine` owns mode, lifecycle, validity, and epochs. Epoch increments occur on subscription start, snapshot restart, FLUSH, reconnect, buffer overflow, and MBO/MBP mode transition.

MBO and MBP books are physically separate:

- `MboOrderBook` tracks individual orders and aggregate levels derived from orders.
- `MbpOrderBook` tracks price-level quantities only.

Mode transitions clear incompatible state. The Quantower adapter does not seed an MBO epoch with an aggregate MBP snapshot.

## Feature Engine

`FeatureEngine` owns:

- `MarketStateMonitor`
- `MboFloatingPairDetector`
- `SymmetricAggregateLatticeDetector`

Feature values carry `MetricQuality`. Unavailable/warming/stale/invalid metrics are not encoded as meaningful numeric zero.

## Recorder / Replay Parity

`CsvAnalyticsRecorder` emits versioned raw events, feature snapshots, regime transitions, floating-pair break events, and diagnostics. `DeterministicReplayRunner` feeds normalized events into the same runtime path as the live adapter.

## Extension Points

Future strategy modules should consume immutable `FeatureSnapshot` objects from `QT.Runtime`. They must not mutate book state, duplicate feature formulas, or call broker APIs from analytics modules.
