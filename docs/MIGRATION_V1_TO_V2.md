# Migration V1 to V2

## Retained

- Quantower indicator concept and post-build deployment.
- Feed subscription/adaptation ideas from `AnalyticsEngineHost.cs`.
- Deterministic console test harness.
- `ExecutionCore`, `IBroker`, scheduler seam, fakes, and execution tests as dormant future infrastructure in `src/QT.Execution`.
- The paired-quote detector concept, expanded and renamed as `MboFloatingPairDetector`.
- The aggregate lattice idea, renamed as `SymmetricAggregateLatticeDetector` and disabled by default.

## Replaced

- `DataAnalyticsCalculator.cs` monolith is no longer compiled into the Quantower host.
- V1 `AnalyticsSnapshot` was replaced by immutable `BookSnapshot`, `MarketStateSnapshot`, `FloatingPairSnapshot`, and `FeatureSnapshot`.
- V1 ad hoc feature CSV export was replaced by schema-versioned recorder files with configuration hash and session id.
- Strategy-coupled replay was replaced by `DeterministicReplayRunner` over the same runtime modules as live analytics.

## Disabled / Removed From Runtime

- `DataAnalyticsStrategy.cs`
- `ShadowSimulator.cs`
- `ReplayBacktester.cs`
- `ReplayBacktestIndicator.cs`
- `Broker.cs` and `ExecutionCore.cs` from the Quantower host compile path
- Live order placement, shadow/live promotion, TP/SL, flattening, and P/L logic

The files remain in the repository where useful for audit history, but the production `DataAnalytics.csproj` no longer compiles them.

## Compatibility Notes

- The Release build overwrites the Quantower Strategies DLL with an analytics-only assembly containing no Strategy class, preventing stale V1 strategy loading from that DLL path.
- `src/QT.Execution` still references Quantower types because dormant execution tests use Quantower enums such as `Side` and `OrderStatus`.
- Live Quantower feed behavior was not manually verified in this run; build and deterministic tests were verified.
