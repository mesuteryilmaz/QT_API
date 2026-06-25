# QT_API V1 Baseline

Baseline recorded on 2026-06-25 before the analytics-first V2 migration.

## Git

- Branch before migration: `master`
- Migration branch created: `analytics-v2`
- HEAD: `3c93df668447ddbff94010c4b2302f910aff777b`
- Untracked files present before migration:
  - `.vscode/`
  - `QT_API_V2_CODEX_IMPLEMENTATION_PROMPT.md`

## Environment

- .NET SDK: `10.0.204`
- MSBuild: `18.3.3+e7aa4b537`
- Quantower reference: `C:\Quantower\TradingPlatform\v1.146.13\bin\TradingPlatform.BusinessLayer.dll`
- Main project target: `net10.0-windows`
- Existing main assembly: `MBO_Market_Data_Analytics`

## Existing Projects

- `DataAnalytics.csproj`
  - Flat Quantower project with explicit compile items.
  - Builds the indicator and the active V1 strategy into one assembly.
  - Post-build copies the DLL into both Quantower `Indicators` and `Strategies` folders.
- `tests\DataAnalytics.Tests.csproj`
  - Dependency-free console test runner.
  - References `DataAnalytics.csproj`.

## V1 Runtime Entry Points

- `DataAnalyticsIndicator.cs`
  - Quantower indicator and GDI dashboard.
  - Owns the `AnalyticsSnapshot` type.
- `AnalyticsEngineHost.cs`
  - Quantower feed subscription, bounded queue, snapshot seeding, MBO coverage check, FLUSH/overflow recovery, and snapshot publication.
- `DataAnalyticsStrategy.cs`
  - Active Flow Ratio strategy path in V1.
  - Contains live/shadow promotion and execution behavior.
- `MboRecorderIndicator.cs`
  - Raw MBO recorder indicator.
- `ReplayBacktestIndicator.cs`
  - Strategy-coupled replay/backtest indicator.

## Important V1 Components

- `DataAnalyticsCalculator.cs`
  - Monolithic analytics engine, MBO/MBP book mutation, legacy lattice detector, first-cut market regime monitor, recorder output.
- `MboOrderBook.cs`
  - Order-keyed book reconstruction helper with deterministic tests.
- `PairedQuoteDetector.cs`
  - First-cut pure paired quote detector with deterministic tests.
- `ExecutionCore.cs`, `Broker.cs`, `ExecutionScheduler.cs`
  - Platform-free execution state machine plus Quantower broker adapter and scheduler seam.
  - Preserved only as dormant future infrastructure in V2.
- `AdaptiveParameters.cs`, `ShadowSimulator.cs`, `ReplayBacktester.cs`
  - Strategy-coupled V1 infrastructure.

## Baseline Build

Command:

```powershell
dotnet build DataAnalytics.csproj -c Debug
```

Result:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

Output DLL:

- `bin\Debug\MBO_Market_Data_Analytics.dll`

## Baseline Tests

Command:

```powershell
dotnet run --project tests/DataAnalytics.Tests.csproj
```

Result:

```text
==== 173 passed, 0 failed ====
```

Covered areas:

- `AdaptiveParameterController`
- `MboOrderBook`
- `FakeBroker` / `FakeScheduler`
- `ExecutionCore` flatten/fill/bracket/entry/reconcile behavior
- `PairedQuoteDetector`

## Known V1 Risks Carried Into Migration

- The Flow Ratio strategy has no actionable edge and must not remain active.
- Market-data, book state, feature logic, UI, replay, and strategy behavior are tightly coupled.
- MBO and MBP paths can be mixed after snapshot/recovery transitions.
- Existing CSV feature store has fragile stale-header behavior.
- The aggregate lattice detector is not an individual floating-pair detector.
- Existing Release/Quantower DLLs are not proof of a V2 build and must be overwritten by a verified V2 build.
