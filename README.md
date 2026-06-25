# QT_API V2 Analytics

QT_API V2 is an analytics-only Quantower market-microstructure platform for CME futures order-flow research.

This version does not compile or activate the V1 Flow Ratio Strategy, does not submit live orders, and does not wire broker APIs into the runtime. `ExecutionCore` and broker abstractions are preserved only in `src/QT.Execution` as dormant future infrastructure with deterministic tests.

## Structure

- `DataAnalytics.csproj` - Quantower indicator host assembly (`MBO_Market_Data_Analytics.dll`).
- `src/QT.Core` - neutral primitives, metric quality, clocks, diagnostics.
- `src/QT.Market` - normalized market events, MBO/MBP books, lifecycle, epochs, snapshots.
- `src/QT.Features` - feature engine, market-state monitor, MBO floating-pair detector, optional aggregate lattice diagnostic.
- `src/QT.Storage` - versioned CSV recorder.
- `src/QT.Runtime` - single-writer analytics runtime.
- `src/QT.Simulation` - deterministic replay over the same runtime.
- `src/QT.Execution` - dormant execution infrastructure only.
- `tests` - deterministic console tests.

## Build

```powershell
dotnet build QT_API.sln -c Debug
dotnet build QT_API.sln -c Release
```

The Quantower reference is expected at:

```text
C:\Quantower\TradingPlatform\v1.146.13\bin\TradingPlatform.BusinessLayer.dll
```

The post-build step copies the analytics-only DLL to:

```text
C:\Quantower\Settings\Scripts\Indicators\MBO_Market_Data_Analytics.dll
C:\Quantower\Settings\Scripts\Strategies\MBO_Market_Data_Analytics.dll
```

The Strategies copy intentionally contains no `Strategy` implementation; it overwrites stale V1 strategy-capable binaries with the V2 analytics-only assembly.

## Tests

```powershell
dotnet run --project tests\DataAnalytics.Tests.csproj -c Release
dotnet run --project tests\QT.UnitTests\QT.UnitTests.csproj -c Release
```

Current verified result: `228 passed, 0 failed`.

## Quantower

Add the indicator named `QT API V2 Analytics`.

Primary settings:

- Preferred book mode: MBO.
- Snapshot depth.
- Panel refresh.
- Stale timeout.
- Versioned recorder enable/path.
- Optional aggregate lattice diagnostic.
- Floating-pair thresholds.

The panel shows feed/book health, market regime, spread/liquidity, activity/volatility, and MBO floating pairs. Unavailable, warming, stale, and invalid values are displayed as states, not numeric zero.

## Recording

Enable `Versioned Recorder` in the indicator settings. CSV outputs are written under the configured directory:

- `raw_events_v2.csv`
- `feature_snapshots_v2.csv`
- `regime_transitions_v2.csv`
- `floating_pair_events_v2.csv`
- `diagnostics_v2.csv`

Rows include schema version, source commit, build configuration, session id, configuration hash, symbol, timestamps, book epoch, lifecycle/mode where applicable, and metric quality.

## Replay

Use `QT.Simulation.Replay.DeterministicReplayRunner` with normalized `NormalizedMarketEvent` sequences. Replay feeds the same `AnalyticsRuntime`, `OrderBookEngine`, and `FeatureEngine` used by the Quantower host.

## Safety

V2 is not a trading strategy and makes no profitability claim. It has no active signal generation, no shadow/live promotion, no automatic flattening, no TP/SL logic, and no broker order submission path in the Quantower host.
