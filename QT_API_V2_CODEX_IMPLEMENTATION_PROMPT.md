# CODEX IMPLEMENTATION PROMPT — QT_API V2 ANALYTICS-FIRST REARCHITECTURE

You are acting as a senior C#/.NET systems architect, quantitative market-microstructure engineer, and test engineer. Work directly on the existing QT_API repository and implement a new, organized, analytics-first version of the project.

The current repository is expected to be located at:

`C:\Users\mesuteryilmaz\Desktop\QT_API`

Use the actual repository root you are given. Do not assume every existing class or filename is exactly as described below; inspect the repository first, map the current implementation, and adapt the migration accordingly.

Do not stop after producing a plan or scaffolding. Implement the new architecture, wire it into the runtime, add tests, build the solution, run the tests, and document the result. Avoid placeholder-only modules, dead interfaces, duplicate production paths, and TODOs for core functionality.

---

## 0. CURRENT V1 BASELINE & CONTINUITY (read before Section 2)

This section states the actual repository state as of the last commit, so you do not rediscover it from scratch or treat already-built work as greenfield. Verify it against the live repo (Section 2 still applies), but start from these facts.

### 0.1 Commit, build, and test baseline

- **Branch / HEAD:** `master` at commit `3c93df6` ("Add market regime monitor + MBO paired floating-quote detector (analytics)"). The previously audited HEAD was `69f1b1a`; audits 4a/4b were written against `69f1b1a`, so commit `3c93df6` is one step ahead of them (see 0.4).
- **Solution shape:** there is **no `.sln` and no `src/` tree yet** — the repo is a flat directory with a single main project `DataAnalytics.csproj` (`AssemblyName = MBO_Market_Data_Analytics`, `net10.0-windows`, `UseWindowsForms`, `Nullable=enable`, `EnableDefaultCompileItems=false` so every file is listed explicitly in an `<ItemGroup>`). The target solution structure in Section 5 is the destination, not the current state.
- **Quantower reference:** `TradingPlatform.BusinessLayer.dll` from `C:\Quantower\TradingPlatform\v1.146.13\bin\` (`Private=False`).
- **Build:** `dotnet build DataAnalytics.csproj -c Debug` → `bin\Debug\MBO_Market_Data_Analytics.dll`. A `PostBuild` target copies that DLL into `C:\Quantower\Settings\Scripts\Indicators\` and `...\Strategies\`. Current build is **0 warnings / 0 errors**; preserve that bar.
- **Tests:** `dotnet run --project tests/DataAnalytics.Tests.csproj` — a dependency-free console runner (no NuGet test framework, exit code 0 = pass). Currently **173 assertions, all green**. Covers `AdaptiveParameterController`, `MboOrderBook`, `ExecutionCore` (flatten/fill/bracket/entry/reconcile), and `PairedQuoteDetector`. This is the harness to extend; the new `QT.*Tests` projects in Section 15 are the destination.
- **Instrument context:** CME MNQ/NQ futures. MNQ tick = 0.25 pt, $0.50/tick, $2/pt. Prices are stored internally as integer ticks already (`(long)Math.Round(price / tickSize)`).

### 0.2 Current files and their disposition

| File | Role today | V2 disposition |
|---|---|---|
| `DataAnalyticsIndicator.cs` | Quantower `Indicator` + `AnalyticsSnapshot` record + HUD rendering (3 columns, sections 1–9) | Split: thin host/panel (Section 13); `AnalyticsSnapshot` becomes the immutable feature snapshot |
| `AnalyticsEngineHost.cs` | Owns the calculator, `PublishSnapshot`, resync/overflow handling, worker queue | Becomes `AnalyticsRuntime` (Section 11) |
| `DataAnalyticsCalculator.cs` | **Monolithic** feature calc, ~98 getters, incremental O(1) windows, MBO/MBP book, lattice + regime + paired wiring | Decompose into feature modules (Section 8); wrap as a legacy module first, then migrate formulas |
| `DataAnalyticsStrategy.cs` | **Flow Ratio strategy** (the only active strategy) | **Remove from runtime** (Section 4.3). No edge — see 0.4 |
| `ShadowSimulator.cs`, `ReplayBacktester.cs`, `ReplayBacktestIndicator.cs` | Strategy-coupled shadow/replay | Remove strategy-specific paths; replay concept folds into the unified replay runner (Section 12) |
| `MboOrderBook.cs` | Order-keyed book reconstruction (has tests) | Keep/refactor as `MboOrderBook` (Section 7.3) |
| `MboRecorder.cs`, `MboRecorderIndicator.cs` | Raw MBO capture/replay unit (`MboEvent`/`MboAction`) | Keep; fold into versioned recorder (Section 12) |
| `ExecutionCore.cs`, `Broker.cs`, `ExecutionScheduler.cs` | Platform-free execution state machine + `IBroker`/scheduler seams + deterministic tests | **Keep dormant** (Section 4.1 / 15.6); do not wire into analytics runtime |
| `AdaptiveParameters.cs` | Adaptive thresholds/regime/polarity for the strategy | Strategy-coupled; not part of analytics V2 runtime |
| `PairedQuoteDetector.cs` | **NEW pure module** (see 0.3) | Refactor/extend into `MboFloatingPairDetector` (Section 10) — do not rewrite |
| `research/` | `edge_study.py`, `signal_scan.py`, `FINDINGS.md` | Keep as research evidence |

### 0.3 The two "new" Section 9 / Section 10 modules already have first cuts — refactor, don't reinvent

Both primary V2 features were started in commit `3c93df6`. Treat the existing code as the starting point and bring it up to the full spec in Sections 9 and 10.

- **Market State / Regime (Section 9):** a first-cut regime monitor already exists **inside `DataAnalyticsCalculator`** (it is *not* yet a standalone module). Method `UpdateRegimeMonitor()` runs on the throttled ~250 ms scan; getters: `GetBidAskSpreadTicks`, `GetAvgSpreadTicks`, `GetSpreadVolatility`, `GetMidRealizedVol`, `GetOrderFlowSpeed`, `GetRegimeStress`, `GetMarketRegime` (ordinal 0 Calm / 1 Normal / 2 Active / 3 Stressed), `GetSecondsSinceRegimeChange`. It uses time-decayed EWMAs of spread / mid-realized-vol / event-rate normalized against the session's own long baselines. It is a simplified subset of Section 9 — **extract it into the standalone `MarketStateMonitor` and expand** it to the full spread-episode / score / regime / transition spec.
- **MBO Floating Pairs (Section 10):** `PairedQuoteDetector.cs` is already a **pure, platform-free, unit-tested module** (`tests/PairedQuoteDetectorTests.cs`, 12 cases) that closely matches the Section 10 architecture. Already implemented: per-order identity (real IDs, integer ticks/sizes), exact-equal-size pairing, size tiers (Large 20–199 / VeryLarge ≥200), distance gate, `Candidate → Persistent → FloatingConfirmed → Broken` states, follow-confirmation via per-leg touch-offset stability vs the pair's reference band, greedy one-to-one assignment, `Reset(epoch)` on book-epoch change, and `Unavailable` (never zero) in MBP mode. **Refactor/extend it into `MboFloatingPairDetector`** rather than starting over; see the continuity note in Section 10.

### 0.4 Audit lineage and the standing "analytics-only" decision

- This rearchitecture is the response to **two audit-4 reports** (`QT_API_4_PROFITABILITY_ARCHITECTURE_AUDIT.md`, `QT_API_4_REAUDIT_AND_PAIRED_QUOTE_DETECTOR.md`, both dated 2026-06-25), which follow three earlier audits already worked through on `master`.
- **4a (profitability):** the strategy consumes only 3 of ~98 metrics; "adaptive" never learns from realized P/L; momentum/mean-reversion polarity comes from the ratio's own ~0.996 autocorrelation (a 60 s window-overlap artifact); live P/L is gross while shadow is net; shadow/replay/live are not the same strategy. Verdict: keep live disabled.
- **Independent edge study** (`research/FINDINGS.md`, 2026-06-24): the Flow Ratio signal has **no actionable edge** — short-window flow IC ≈ 0; only long-window signals show IC at long horizons, identified as trend-autocorrelation, regime-unstable, net-negative after cost. This is *why* V2 is analytics-only and does not carry the strategy forward.
- **4b (paired detector):** the V1 aggregate symmetric-lattice detector solves the wrong problem; the intended object is the individual floating pair of Section 10. The lattice detector (`ScanSymmetricLattice`/`FindLadder` in `DataAnalyticsCalculator`, HUD section 7) is the "`SymmetricAggregateLatticeDetector`" referenced in Section 10.11 — retain it as an optional/diagnostic structural metric, not the primary pair detector.

### 0.5 Known caveat to fix in V2

The V1 CSV feature store appends columns to a fixed header that is only written when the file does not yet exist, so a pre-existing `microstructure_features.csv` keeps a stale header while newer rows carry extra columns. The Section 12 versioned recorder (schema version + config hash per file) **supersedes and must replace** this fragile single-file approach.

---

## 1. PRIMARY OBJECTIVE

Transform the current QT_API prototype into a sound, organized, extensible market-microstructure analytics platform with room to add and test many strategies later.

For this version:

- Do not implement a new trading strategy.
- Do not use or preserve the current Flow Ratio Strategy as an active strategy.
- Do not place real orders.
- Do not automatically promote anything to live trading.
- Keep useful execution-related code such as `ExecutionCore`, broker abstractions, and deterministic tests in an isolated dormant project for future use, but do not wire them into the analytics runtime.
- The delivered runtime must be analytics-only by default and must be unable to submit live orders unless a future, explicit execution host is added.

The new version must focus on:

1. Reliable market-data ingestion.
2. Correct MBO/MBP order-book lifecycle and validity.
3. Modular feature calculation.
4. A new Market State, Spread & Liquidity Regime Monitor.
5. A new MBO Paired Floating Quote Detector.
6. Versioned recording and deterministic replay.
7. A clear Quantower indicator/panel showing useful, interpretable states.
8. Strong data-quality, reset, reconnect, and test behavior.
9. A structure that can later accept independent strategy modules without modifying market data, feature, recorder, or UI infrastructure.

The resulting project is an analytics and research platform, not a claimed profitable trading system.

---

## 2. FIRST ACTION: INSPECT AND BASELINE THE EXISTING REPOSITORY

Before changing code:

1. Inspect the full solution, projects, Git status, current branch, current commit, build configuration, references, and existing tests.
2. Identify:
   - Quantower host/indicator/strategy classes.
   - dxFeed or other market-data adapters.
   - MBO and MBP reconstruction code.
   - `DataAnalyticsCalculator`.
   - `AnalyticsEngineHost`.
   - `AnalyticsSnapshot`.
   - Recorder and replay code.
   - Current grid/lattice detector.
   - Current shadow/replay/live strategy logic.
   - `IBroker`, scheduler abstractions, `ExecutionCore`, and execution tests.
3. Build and test the current source before the migration if the environment permits.
4. Record baseline results in `docs/BASELINE_V1.md`:
   - Commit hash.
   - Build command.
   - Test command and results.
   - Existing projects.
   - Existing runtime entry points.
   - Known stale binaries or mismatches.
5. Preserve the old implementation while migrating:
   - Prefer a Git branch named `analytics-v2`.
   - Do not delete useful code before replacement behavior is tested.
   - Move obsolete runtime code to a `Legacy` namespace/folder only when necessary.
   - Do not allow legacy and V2 production paths to run simultaneously.

If the repository has uncommitted user work, preserve it. Do not reset, clean, or overwrite unrelated changes.

---

## 3. KNOWN ARCHITECTURAL PROBLEMS TO ELIMINATE

The current project has useful components but is too monolithic and has duplicated behavior. The new version must eliminate these patterns:

- One large Quantower class owning market data, book state, analytics, strategy, risk, execution, P/L, replay, and UI.
- Separate implementations of the same logic for live, shadow, and replay.
- Strategy logic embedded in the Quantower host.
- Direct broker calls from analytics or future strategy code.
- Aggregate MBP snapshots coexisting with later MBO per-order increments in the same book state.
- Snapshot and incremental events combined without an explicit lifecycle/cutover state.
- Treating unavailable or warming metrics as numeric zero.
- Ignoring zero-price `FLUSH` events.
- Counting snapshot seed rows as live order-flow activity.
- Accepting empty or one-sided order books as valid.
- Continuing to use stale book/features after reconnect.
- A grid detector that claims to detect paired floating orders but actually detects aggregate multi-level lattices.
- Dashboard sections containing many metrics that are not interpretable or connected to a clear research purpose.
- Release DLLs that cannot be traced to the exact source commit and test result.

---

## 4. KEEP, REFACTOR, REMOVE, AND ADD

### 4.1 Keep and reuse where sound

Retain useful existing work, subject to tests and cleanup:

- Quantower and dxFeed market-data subscriptions/adapters.
- Trade, quote, depth, and MBO event parsing.
- MBO/MBP book data structures and formulas that can be validated.
- Existing buyer/seller, delta, VWAP, OFI, imbalance, pressure, cancellation, absorption, iceberg/replenishment, spoof-related, and volatility calculations.
- Raw and feature recorders.
- Broker interface abstractions.
- `ExecutionCore` concept and deterministic execution tests.
- Existing test utilities and representative replay data.
- Existing configuration/settings framework where it is not coupled to a strategy.

### 4.2 Refactor

Refactor these components behind clean interfaces:

- Order-book reconstruction and lifecycle.
- Feature calculation.
- Analytics snapshot publication.
- Recorder schemas.
- Quantower panel rendering.
- Reset/reconnect/session transitions.
- Existing aggregate grid/lattice detector.

### 4.3 Remove or disable from active runtime

- Flow Ratio Strategy.
- Current signal generation.
- Current shadow/live promotion and demotion.
- Live order placement.
- Strategy-specific replay logic.
- Strategy-specific shadow simulator.
- Duplicate execution logic inside the Quantower host.
- Any automatic flatten, TP, SL, P/L, or position logic from the analytics-only host.
- Any panel wording that implies the analytics platform is a proven trading strategy.

### 4.4 Add

- Explicit book lifecycle and epochs.
- Immutable market and feature snapshots.
- Typed feature registry.
- Market State, Spread & Liquidity Regime Monitor.
- MBO Paired Floating Quote Detector.
- Data-quality and warm-up metadata.
- Versioned feature recording.
- Deterministic replay runner that uses the same book and feature modules as live analytics.
- Focused panel sections.
- Comprehensive unit, scenario, reset, replay-parity, and regression tests.
- Architecture and feature documentation.

---

## 5. PRACTICAL TARGET SOLUTION STRUCTURE

Do not over-engineer into too many assemblies immediately. Prefer the following practical structure, adapting names to the current solution:

```text
QT_API.sln
│
├── src/
│   ├── QT.Core/
│   │   ├── Time/
│   │   ├── Primitives/
│   │   ├── Quality/
│   │   ├── Events/
│   │   └── Diagnostics/
│   │
│   ├── QT.Market/
│   │   ├── Adapters/
│   │   ├── Events/
│   │   ├── OrderBook/
│   │   ├── Lifecycle/
│   │   └── Snapshots/
│   │
│   ├── QT.Features/
│   │   ├── Contracts/
│   │   ├── Registry/
│   │   ├── Engine/
│   │   ├── LegacyAnalytics/
│   │   ├── MarketState/
│   │   ├── FloatingPairs/
│   │   └── AggregateLattice/
│   │
│   ├── QT.Storage/
│   │   ├── RawEvents/
│   │   ├── Features/
│   │   ├── Transitions/
│   │   └── Schemas/
│   │
│   ├── QT.Simulation/
│   │   ├── Replay/
│   │   └── DeterministicClock/
│   │
│   ├── QT.Execution/
│   │   ├── ExecutionCore/
│   │   ├── BrokerAbstractions/
│   │   └── DormantForFuture/
│   │
│   ├── QT.Runtime/
│   │   ├── AnalyticsRuntime/
│   │   ├── Health/
│   │   └── Coordination/
│   │
│   └── QT.Quantower/
│       ├── Host/
│       ├── Adapters/
│       ├── Settings/
│       └── Panel/
│
├── tests/
│   ├── QT.UnitTests/
│   ├── QT.ScenarioTests/
│   ├── QT.ReplayTests/
│   └── QT.RegressionTests/
│
└── docs/
```

If current Quantower SDK constraints make multiple projects awkward, use fewer projects but preserve these namespace and dependency boundaries.

Required dependency direction:

```text
QT.Core
  ↑
QT.Market
  ↑
QT.Features
  ↑
QT.Storage / QT.Simulation / QT.Runtime
  ↑
QT.Quantower
```

`QT.Features` must not reference Quantower APIs.

`QT.Market` must not reference UI, strategy, broker, or execution code.

`QT.Quantower` may adapt Quantower events into neutral domain events, but neutral code must not depend on Quantower types.

`QT.Execution` must compile but remain disconnected from the analytics-only runtime.

---

## 6. CORE DOMAIN TYPES

Create neutral immutable types. Adapt exact numeric types to the existing project, but use integer price ticks internally wherever practical.

### 6.1 Market event envelope

```csharp
public readonly record struct MarketEventEnvelope(
    long LocalSequence,
    DateTime EventTimeUtc,
    DateTime ReceiveTimeUtc,
    string Symbol,
    long BookEpoch,
    MarketEventKind Kind,
    MarketDataQuality Quality,
    object Payload);
```

Prefer strongly typed generic or discriminated event handling over raw `object` if practical.

### 6.2 Book state

```csharp
public enum BookLifecycleState
{
    Disconnected,
    AwaitingSnapshot,
    ApplyingSnapshot,
    ReplayingBufferedEvents,
    WarmingUp,
    Valid,
    Invalid,
    Recovering,
    DowngradedToMbp
}

public enum BookMode
{
    Unknown,
    Mbp,
    Mbo
}
```

Every published book snapshot must include:

- Symbol.
- Event time.
- Local receive time.
- Book epoch.
- Mode.
- Lifecycle state.
- Bid-side validity.
- Ask-side validity.
- MBO identity completeness.
- Last event age.
- Crossed/locked status.
- Best bid/ask.
- Top-N depth.
- Optional MBO order collection or a read-only view for MBO-only features.

### 6.3 Data quality

```csharp
public enum MetricQuality
{
    Unavailable,
    WarmingUp,
    Derived,
    Exact,
    Stale,
    Invalid
}
```

Each feature value must carry at least:

```csharp
public readonly record struct MetricValue<T>(
    T Value,
    MetricQuality Quality,
    DateTime EventTimeUtc,
    string? Reason);
```

Do not encode unavailable, invalid, or warming values as zero.

---

## 7. ORDER-BOOK LIFECYCLE REQUIREMENTS

Implement an explicit order-book lifecycle.

### 7.1 Epoch rules

Increment the book epoch on:

- Initial subscription.
- Reconnect.
- `FLUSH`.
- Snapshot restart.
- MBO-to-MBP or MBP-to-MBO mode transition.
- Buffer overflow or unrecoverable sequence uncertainty.
- Symbol remap/rebind.

All feature modules that depend on book state must reset or start a new epoch when the book epoch changes.

### 7.2 Snapshot and incremental cutover

Use the strongest sequencing/cutover information available from the actual feed or Quantower API.

If an authoritative source sequence exists:

- Record snapshot boundary sequence.
- Apply only increments after the boundary.
- Detect gaps and regressions.

If an authoritative sequence does not exist:

- Make the limitation explicit.
- Use a conservative local snapshot state machine.
- Buffer increments during snapshot.
- Establish a deterministic local cutover.
- Mark the book `WarmingUp` or `Derived`, not `Exact`, until continuity has been observed.
- Never mix an aggregate MBP seed with individual MBO increments.

### 7.3 MBO/MBP separation

Maintain separate book representations:

- `MboOrderBook` for individual orders.
- `MbpOrderBook` for aggregated price levels.

Do not use aggregate MBP quantities as synthetic MBO orders.

MBO-only features must report `Unavailable` when true order identity is not reliable.

### 7.4 Validity rules

A book cannot become `Valid` unless:

- Both bid and ask sides are present.
- Best bid is less than or equal to best ask.
- A crossed state is either resolved or explicitly classified invalid.
- Prices align with tick size.
- Required snapshot/recovery conditions are complete.
- Event staleness is within configured limits.
- MBO identity requirements are satisfied for MBO-only modules.

Locked market:

- Spread = 0.
- Mark as locked.
- Do not treat as a normal positive spread.

Crossed market:

- Spread < 0.
- Mark book invalid.
- Suspend normal spread/regime calculations.
- Emit a diagnostic event.

### 7.5 FLUSH and reconnect

Handle `FLUSH` before price validation, including zero-price flush events.

On FLUSH/reconnect:

- Increment epoch.
- Clear book-dependent features.
- Clear floating-pair state.
- Clear spread episode state.
- Report `Unknown`/`WarmingUp`, never zeros.
- Require a fresh valid two-sided baseline.

Snapshot seed events must not count as live add/cancel/reprice activity.

---

## 8. FEATURE ENGINE

Create a modular feature engine.

```csharp
public interface IFeatureModule
{
    string Id { get; }
    FeatureRequirements Requirements { get; }

    void Reset(in FeatureResetContext context);
    void OnBookSnapshot(in BookSnapshot snapshot);
    void OnBookEvent(in NormalizedBookEvent bookEvent, in BookSnapshot snapshot);
    void OnTrade(in NormalizedTrade trade, in BookSnapshot snapshot);
    void AdvanceTime(DateTime eventTimeUtc);
    void Publish(FeatureSnapshotBuilder builder);
}
```

Requirements should declare:

- Needs trades.
- Needs BBO.
- Needs depth.
- Needs exact MBO.
- Minimum warm-up time.
- Reset on book epoch.
- Compatible modes.

Build a typed feature registry rather than continuing to expand one giant class indefinitely.

Example:

```csharp
public readonly record struct FeatureKey<T>(string Name);

public static class FeatureKeys
{
    public static readonly FeatureKey<double> SpreadTicksCurrent =
        new("market.spread.ticks.current");

    public static readonly FeatureKey<double> BookEventRate1s =
        new("market.activity.book_events.rate_1s");

    public static readonly FeatureKey<int> FloatingPairsLarge =
        new("mbo.floating_pairs.large.count");
}
```

The existing `DataAnalyticsCalculator` may initially be wrapped as a legacy feature module. Gradually move formulas into focused modules, preserving output parity with tests.

---

## 9. NEW PRIMARY MODULE: MARKET STATE, SPREAD & LIQUIDITY REGIME MONITOR

Implement this as a standalone feature module, not as more methods added to the existing monolithic calculator.

> **Continuity (see 0.3):** a first-cut version already exists *inside* `DataAnalyticsCalculator` (`UpdateRegimeMonitor()` + the `GetBidAskSpreadTicks` / `GetAvgSpreadTicks` / `GetSpreadVolatility` / `GetMidRealizedVol` / `GetOrderFlowSpeed` / `GetRegimeStress` / `GetMarketRegime` / `GetSecondsSinceRegimeChange` getters). It is a simplified EWMA subset of this section. **Extract it into the standalone module and expand to the full spec below** (time-weighted spread stats, spread episodes, separate Activity/Volatility/LiquidityStress scores, the richer regime enum, and the transition state machine). Do not leave the regime logic embedded in the calculator.

Suggested class:

`MarketStateMonitor`

Its purpose is to measure and interpret:

- Best bid/ask spread.
- Spread stability, widening, narrowing, and recovery.
- BBO and book event speed.
- Trade and aggressive-volume speed.
- Mid-price movement and realized volatility.
- Visible liquidity supply and withdrawal.
- Cancellation pressure.
- Activity, volatility, and liquidity-stress scores.
- Market regime.
- Risk-environment proxy.
- Regime transitions and reasons.

It must not place orders or alter risk settings.

### 9.1 Configurable windows

Default windows:

- Fast: 1 second.
- Short: 5 seconds.
- Baseline: 60 seconds.
- Slow baseline: 300 seconds.

Make them configurable.

Use event time, not wall-clock timing, in deterministic replay.

### 9.2 Spread metrics

For every valid two-sided BBO:

```text
SpreadTicks  = BestAskTicks - BestBidTicks
SpreadPoints = BestAskPrice - BestBidPrice
MidPrice     = (BestBidPrice + BestAskPrice) / 2
SpreadBps    = SpreadPoints / MidPrice * 10,000
```

Calculate:

- Current spread ticks, points, and bps.
- Time-weighted mean spread over 5, 30, and 60 seconds.
- Time-weighted median where practical.
- Minimum and maximum spread over 5, 30, and 60 seconds.
- 50th and 90th percentile spread.
- Percentage of time at 1 tick.
- Percentage of time at 2 ticks.
- Percentage of time at 3 or more ticks.
- Current spread-state duration.
- Widening rate.
- Narrowing rate.
- Spread changes per second.

Time-weighting is mandatory. Do not calculate spread statistics by merely averaging quote messages.

### 9.3 Spread shock

Use a robust baseline:

```text
RobustScale = max(1 tick, 1.4826 * RollingMAD)
SpreadShockZ = (CurrentSpread - RollingMedianSpread) / RobustScale
```

Also expose a normalized `SpreadStressScore` from 0 to 100.

### 9.4 Spread episodes

Track:

```text
Normal
Widening
Wide
Recovering
```

Default episode start:

```text
CurrentSpread >= max(2 ticks, rolling P90)
```

Default recovery completion:

```text
CurrentSpread <= rolling median
```

Track:

- Episode age.
- Initial spread.
- Peak spread.
- Maximum widening.
- Time to first narrowing.
- Full recovery time.
- Failed-recovery count.
- Number of widening episodes.

Use hysteresis.

### 9.5 BBO and order-flow activity

Track separately:

BBO activity:

- Best-bid price changes per second.
- Best-ask price changes per second.
- Any BBO price change per second.
- BBO size-only updates per second.
- Bid-touch quantity changes per second.
- Ask-touch quantity changes per second.

Book activity:

- Total book events per second.
- Add events per second.
- Remove/cancel events per second.
- Reprice events per second.
- Size updates per second.
- Added quantity per second.
- Removed quantity per second.

Trade activity:

- Trades per second.
- Traded quantity per second.
- Buyer-initiated trades and quantity per second.
- Seller-initiated trades and quantity per second.

Timing:

- Median interarrival time.
- P90 interarrival time.
- Minimum interarrival time.
- Longest event silence.
- Time since last book event.
- Time since last trade.

Burst ratios:

```text
BookEventBurst = Rate1s / max(Rate30s, epsilon)
TradeBurst     = TradeRate1s / max(TradeRate30s, epsilon)
VolumeBurst    = VolumeRate1s / max(VolumeRate30s, epsilon)
```

### 9.6 Mid-price volatility

Use midpoint changes, not only trade-price standard deviation.

For each valid midpoint change:

```text
DeltaMidTicks = (CurrentMid - PreviousMid) / TickSize
```

Calculate:

- Midpoint change count per second.
- Absolute midpoint movement per second.
- Midpoint range over 1, 5, 30, and 60 seconds.
- Tick realized volatility over 1, 5, 30, and 60 seconds.
- Basis-point realized volatility where useful.
- Two-tick-or-larger jump count/rate.
- Maximum single midpoint jump.
- Up-move and down-move rates.
- Quote reversal rate.

Tick realized volatility:

```text
MidRVTicksWindow = sqrt(sum(DeltaMidTicks^2))
```

Normalized:

```text
MidRVPerSqrtSecond = MidRVTicksWindow / sqrt(WindowSeconds)
```

### 9.7 Liquidity supply and withdrawal

Calculate:

- Best-bid size.
- Best-ask size.
- Combined touch depth.
- Top-3 combined depth.
- Top-5 combined depth.
- Current depth versus rolling median.
- Touch and top-N withdrawal percentage.
- Added versus removed quantity.
- Cancellation pressure.
- Replenishment rate.
- Spread recovery speed.

Example:

```text
DepthWithdrawal =
    clamp(1 - CurrentTouchDepth / max(RollingMedianTouchDepth, epsilon), 0, 1)
```

Cancellation pressure:

```text
CancelPressure =
    RemovedQty / max(AddedQty + RemovedQty, epsilon)
```

Do not claim direct measurement of participant risk appetite. Label the interpretation as a liquidity-provision/risk-environment proxy.

### 9.8 Robust score normalization

Create three separate scores, each 0–100:

- ActivityScore.
- VolatilityScore.
- LiquidityStressScore.

Use robust rolling baselines and percentile/rank or robust-z transformations. Avoid raw fixed thresholds where possible.

A practical initial implementation may convert each component into a 0–100 percentile score using the 5-minute rolling baseline and then combine them.

Suggested default LiquidityStressScore weights:

```text
30% spread stress
25% midpoint volatility
20% depth withdrawal
15% cancellation pressure
10% order-event burst
```

Make weights configurable and record them in output metadata.

Do not combine everything into one unexplained number. Always publish component scores.

### 9.9 Market risk environment

```csharp
public enum MarketRiskEnvironment
{
    Unknown,
    Normal,
    Elevated,
    Defensive,
    Critical
}
```

Default mapping from LiquidityStressScore:

- 0–34 Normal.
- 35–54 Elevated.
- 55–74 Defensive.
- 75–100 Critical.

This is descriptive analytics only.

### 9.10 Market regimes

```csharp
public enum MarketRegime
{
    Unknown,
    WarmingUp,
    QuietTight,
    ActiveLiquid,
    FastOrderly,
    ThinFragile,
    VolatileDislocated,
    Recovering
}
```

Implement an interpretable deterministic classifier using normalized scores and liquidity conditions.

Suggested starting rules:

#### QuietTight

- ActivityScore < 35.
- VolatilityScore < 30.
- LiquidityStressScore < 30.
- Spread at or below rolling median/P50.
- No stale-feed condition.

#### ActiveLiquid

- ActivityScore >= 55.
- VolatilityScore < 60.
- LiquidityStressScore < 40.
- Spread at or below P75.
- Touch depth at least 75% of baseline.

#### FastOrderly

- ActivityScore >= 60.
- VolatilityScore >= 55.
- LiquidityStressScore < 60.
- Spread not persistently above P90.
- Touch depth at least 60% of baseline.
- Spread recovery remains relatively fast.

#### ThinFragile

- LiquidityStressScore >= 50.
- Depth withdrawal >= 35% or spread above P90.
- VolatilityScore below severe-dislocation threshold, or the market is vulnerable before volatility fully expands.

#### VolatileDislocated

- LiquidityStressScore >= 75 and VolatilityScore >= 65.
- Or severe multi-tick jumps plus abnormal spread and depth withdrawal.
- Do not use feed corruption/crossed books as a market regime; those are invalid data states.

#### Recovering

Only from a previous `ThinFragile` or `VolatileDislocated` state:

- Spread is narrowing.
- Depth is recovering.
- Cancellation pressure is declining.
- Stress score is falling.
- Volatility may remain elevated but is declining.

### 9.11 Transition state machine

Do not switch regimes on every update.

Track:

- Current state.
- Candidate state.
- Candidate start time.
- Current-state entry time.
- Confidence.
- Transition reasons.

Defaults:

- Candidate persistence: 1 second.
- Minimum normal dwell: 2 seconds.
- Separate entry and exit thresholds.
- Immediate entry allowed for critical dislocation.
- Recovery must be explicit.

Transition reasons:

```csharp
[Flags]
public enum RegimeTransitionReason
{
    None = 0,
    SpreadWidening = 1,
    SpreadRecovery = 2,
    VolatilityIncrease = 4,
    VolatilityDecrease = 8,
    DepthWithdrawal = 16,
    DepthRecovery = 32,
    CancellationSurge = 64,
    EventBurst = 128,
    TradeBurst = 256,
    QuoteJump = 512,
    FeedStale = 1024
}
```

Publish each transition as a structured record.

### 9.12 Warm-up and quality

- 0–5 seconds: raw metrics only; regime `WarmingUp`.
- 5–60 seconds: provisional scores; quality `Derived` or `WarmingUp`.
- 60–300 seconds: usable short baseline.
- After 300 seconds: full rolling baseline quality.
- If feed is stale, report `Stale`, not QuietTight.
- If book is invalid, regime is `Unknown`.
- If BBO is locked/crossed, expose that state explicitly.

---

## 10. NEW MBO-ONLY MODULE: PAIRED FLOATING QUOTE DETECTOR

Implement the user’s intended detector separately from the current aggregate grid/lattice detector.

> **Continuity (see 0.3):** `PairedQuoteDetector.cs` already implements most of this as a pure, unit-tested module (`tests/PairedQuoteDetectorTests.cs`, 12 cases). **Refactor/extend it into `MboFloatingPairDetector`; do not rewrite from zero.**
> - *Already done:* per-order identity, exact-equal-size pairing, size tiers (Large 20–199 / VeryLarge ≥200), distance gate, `Candidate → Persistent → FloatingConfirmed → Broken` states, follow-confirmation via per-leg touch-offset stability vs reference band, greedy one-to-one assignment, `Reset(epoch)`, and `Unavailable` (never zero) in MBP mode.
> - *Still to implement here (the gaps):* cancel/replace stitching across new order IDs (§10.8 — V1 is same-ID only); a true 100–500 ms synchronization window (§10.7 — V1 judges follows at ~250 ms sample granularity); raise default max offset 100 → 250 ticks (§10.5); weighted one-to-one assignment cost instead of greedy nearest-offset (§10.5); pair-center / pair-width co-movement tracking (§10.7); top-3 active-pair summaries and last-break-reason reporting (§10.10); replay/live parity (§10 tests).

Suggested class:

`MboFloatingPairDetector`

### 10.1 Purpose

Detect individual resting MBO bid and ask orders that:

- Have exactly the same remaining size.
- Rest on opposite sides of the book.
- Are at approximately matching distances from the current best bid and best ask.
- Persist over time.
- Reprice or cancel/re-enter in a coordinated way as the BBO moves.
- Are especially relevant in large-size tiers.

This is an analytics detector, not a trading signal.

### 10.2 MBO-only requirement

Use individual MBO orders and stable order IDs.

In MBP mode or incomplete MBO identity:

- Return `Unavailable`.
- Do not fall back to aggregate price-level volume.
- Do not report zero.

### 10.3 Size tiers

Default:

- Large: `20 <= size < 200`.
- VeryLarge: `size >= 200`.
- Also publish combined `size >= 20`.

Make thresholds configurable.

### 10.4 Candidate extraction

Track per MBO order:

- Order ID.
- Side.
- Price ticks.
- Remaining quantity.
- First seen.
- Last seen.
- Last price.
- Last update.
- Book epoch.
- Same-ID reprice history.
- Cancel/re-entry stitching status and confidence.

Ignore invalid, zero, market, or non-resting orders.

### 10.5 Pair matching

For each exact integer size, match bids and asks one-to-one.

Offsets:

```text
BidOffsetTicks = BestBidTicks - BidOrderPriceTicks
AskOffsetTicks = AskOrderPriceTicks - BestAskTicks
```

Candidate rule:

```text
BidSize == AskSize
abs(BidOffsetTicks - AskOffsetTicks) <= OffsetToleranceTicks
```

Defaults:

- Offset tolerance: 1 tick.
- Maximum offset: configurable, initially 250 ticks rather than the prior restrictive 100.
- Pairing must be stable over time.

When multiple same-size orders exist:

- Prefer a previously tracked pair.
- Then prefer minimum offset mismatch.
- Then minimum age mismatch.
- Use deterministic one-to-one assignment.
- Never assign one order to multiple pairs.

### 10.6 Pair lifecycle

```csharp
public enum FloatingPairState
{
    Candidate,
    Persistent,
    FloatingConfirmed,
    Broken
}
```

Default confirmation:

- Pair age >= 1 second.
- At least 2 coordinated market-following relocations.
- Follow ratio >= 80%.
- Mean offset error <= 1–2 ticks.
- Exact equal remaining size remains intact.

A static pair may become `Persistent` but not `FloatingConfirmed` until actual market movement opportunities occur.

### 10.7 Coordinated movement

Track BBO movement and pair movement.

Useful values:

```text
MarketCenter2 = BestBidTicks + BestAskTicks
PairCenter2   = BidOrderTicks + AskOrderTicks
PairWidthTicks = AskOrderTicks - BidOrderTicks
```

A pair follows the market when:

- Change in PairCenter2 approximately matches change in MarketCenter2.
- Bid and ask offsets remain within tolerance.
- Pair width remains approximately stable.
- Both legs update within the synchronization window.

Default synchronization window:

- Configurable 100–500 ms.
- Start with 300 ms.

Track:

- Market move opportunities.
- Both-leg synchronized follows.
- One-leg-only moves.
- Late counterpart moves.
- Missed follows.
- Follow ratio.
- Mean and max synchronization delay.

### 10.8 Cancel-and-replace stitching

Prefer same-ID repricing.

Allow strict heuristic stitching only when:

- Same side.
- Same exact remaining quantity.
- New price is close to the expected location.
- Arrival is within a configured replacement window.
- No competing candidate is a better match.
- Book epoch is unchanged.

Mark heuristic stitching with lower confidence.

### 10.9 Break conditions

Break a pair on:

- Size mismatch after partial fill.
- One leg disappears beyond replacement window.
- Offset divergence beyond tolerance.
- Book epoch change.
- FLUSH/reconnect.
- MBO identity downgrade.
- Maximum inactivity.
- Reassignment ambiguity.

### 10.10 Published features

Publish:

- Detector status and quality.
- Eligible large bid/ask order counts.
- Eligible very-large bid/ask order counts.
- Exact-size pair candidate count.
- Persistent pair count.
- Floating-confirmed pair count.
- Counts by size tier.
- Top pair size.
- Top pair bid and ask prices.
- Top pair offsets.
- Top pair age.
- Top pair synchronized moves/opportunities.
- Top pair follow ratio.
- Top pair mean sync delay.
- Top pair confidence.
- Last pair-break event and reason.
- Top three active pair summaries.

### 10.11 Existing grid detector

Retain the existing detector only if its formulas are useful.

Rename it accurately:

`SymmetricAggregateLatticeDetector`

It detects aggregate repeated multi-level ladders, not individual floating pairs.

Hide it from the primary panel by default. Keep it as an optional diagnostics/experimental module and recorder output.

---

## 11. ANALYTICS RUNTIME

Create an `AnalyticsRuntime` that coordinates:

```text
Normalized market events
    → Book lifecycle and book reconstruction
    → Immutable BookSnapshot
    → FeatureEngine
    → Immutable FeatureSnapshot
    → Recorder
    → Panel view model
```

The runtime must:

- Be single-writer or otherwise deterministically serialized for book and feature state.
- Avoid concurrent mutation of rolling windows.
- Use one event-time context per update.
- Throttle panel rendering separately from feature processing.
- Support a deterministic replay clock.
- Publish health information.
- Never call broker order APIs.

Suggested interface:

```csharp
public interface IAnalyticsRuntime
{
    void Start();
    void Stop();

    void OnMarketEvent(in NormalizedMarketEvent marketEvent);
    void AdvanceTime(DateTime eventTimeUtc);

    AnalyticsRuntimeSnapshot Current { get; }
}
```

---

## 12. RECORDER AND REPLAY

### 12.1 Recorder outputs

Implement versioned outputs:

1. Raw normalized market events.
2. Book-health/epoch events.
3. Feature snapshots.
4. Regime transitions.
5. Floating-pair lifecycle events.
6. Diagnostics and reset events.

CSV is acceptable for the first implementation to match the current workflow. Design an interface that allows Parquet later.

Every row/file must include where applicable:

- Schema version.
- Source commit.
- Build configuration.
- Symbol.
- Event time UTC.
- Receive time UTC.
- Book epoch.
- Book mode.
- Book lifecycle state.
- Data quality.
- Feature configuration hash.
- Runtime session ID.

Feature snapshot cadence:

- Configurable.
- Default 250 ms.
- Also emit immediately on regime transition, book reset, and pair lifecycle event.

### 12.2 Replay

Create a deterministic replay runner that feeds the same normalized event types into the same book and feature modules.

Replay must not contain alternate formulas.

Given the same event sequence and configuration, live-host analytics and replay must generate equivalent:

- Book snapshots.
- Feature values.
- Regime states.
- Transition events.
- Floating-pair states.

Use tolerances only for unavoidable floating-point differences.

---

## 13. QUANTOWER HOST AND PANEL

Reduce the Quantower host to:

- Settings.
- Subscription/adaptation.
- Starting/stopping `AnalyticsRuntime`.
- Rendering a view model.
- Recorder controls.
- Health and diagnostics.

It must not contain feature formulas, order-book mutation logic, strategy rules, execution logic, or P/L logic.

### 13.1 Primary panel layout

Create a clear panel with these sections.

#### A. Feed & Book Health

Show:

- Connection state.
- Book mode: MBO/MBP.
- Book lifecycle state.
- Book epoch.
- Data age.
- Book validity.
- MBO identity quality.
- Locked/crossed warning.
- Recorder status.

#### B. Market State & Regime

Show:

- Current regime.
- Risk environment.
- Activity score.
- Volatility score.
- Liquidity stress score.
- Regime confidence.
- Previous regime.
- Last transition.
- Transition age.
- Transition reasons.

#### C. Spread & Liquidity

Show:

- Current spread ticks.
- Time-weighted mean 30s.
- P90 and max 60s.
- Time at 1 tick.
- Widen/narrow rates.
- Current spread episode.
- Recovery time.
- Touch depth versus baseline.
- Top-5 depth versus baseline.
- Cancellation pressure.

#### D. Activity & Volatility

Show:

- BBO price changes/sec.
- Book events/sec.
- Book event burst.
- Trades/sec.
- Volume/sec.
- Trade burst.
- Mid RV 5s/30s.
- Mid range 5s/30s.
- Multi-tick jump rate.

#### E. MBO Floating Pairs

Show:

- Detector status.
- Eligible large bid/ask orders.
- Eligible very-large bid/ask orders.
- Candidate pairs.
- Persistent pairs.
- Confirmed floating pairs.
- Top pair size and tier.
- Bid/ask offsets.
- Pair age.
- Synchronized moves/opportunities.
- Follow ratio.
- Confidence.

#### F. Optional Legacy Analytics

Provide collapsible/optional sections for validated legacy analytics. Do not display every available value by default.

### 13.2 Display rules

- `Unavailable`, `Warming`, `Stale`, and `Invalid` must be textual states, not `0`.
- Use restrained color coding:
  - Normal/valid.
  - Warning.
  - Critical/invalid.
- Do not flash on every 250 ms update.
- Default panel refresh: 4 Hz.
- Feature processing may be faster than UI refresh.
- Include tooltips or concise descriptions for non-obvious metrics.
- Do not label any analytics value as guaranteed profit, buy/sell, or proven edge.

---

## 14. CONFIGURATION

Create grouped configuration classes and expose relevant settings in Quantower.

Required groups:

### Book

- Preferred mode.
- Top-N depth.
- Snapshot timeout.
- Stale timeout.
- Buffer limit.
- Recovery policy.

### Market State

- Rolling windows.
- Score weights.
- Warm-up durations.
- Regime thresholds.
- Candidate persistence.
- Minimum dwell.
- Emergency transition thresholds.

### Floating Pairs

- Large threshold.
- Very-large threshold.
- Offset tolerance.
- Maximum offset.
- Persistence time.
- Synchronization window.
- Required coordinated moves.
- Minimum follow ratio.
- Replacement stitching window.
- Maximum inactivity.

### Recorder

- Enabled.
- Output path.
- Raw events.
- Feature cadence.
- Transition output.
- Flush interval.
- Schema version.

Validate settings at startup. Reject nonsensical values with explicit messages.

Record a stable configuration hash in all output files.

---

## 15. TEST REQUIREMENTS

Do not consider the migration complete without tests.

### 15.1 Order-book lifecycle tests

- Initial snapshot to valid two-sided book.
- Empty snapshot rejected.
- One-sided snapshot rejected.
- Zero-price FLUSH handled.
- Reconnect increments epoch.
- Buffer overflow invalidates epoch.
- MBO-to-MBP clears incompatible state.
- MBP-to-MBO does not synthesize order identities.
- Locked book handled.
- Crossed book invalidated.
- Snapshot seed excluded from live event counts.
- Stale feed produces stale quality.

### 15.2 Market-state tests

- Constant one-tick spread.
- Time-weighted mean spread.
- Time at one tick.
- One-to-three-tick widening.
- Spread episode age and peak.
- Full recovery duration.
- Repeated widening episodes.
- BBO price update versus size-only update.
- Book-event rate.
- Add/remove/reprice rates.
- Trade and volume rates.
- Burst ratio.
- Interarrival metrics.
- Midpoint RV.
- Midpoint range.
- Multi-tick jump.
- Depth withdrawal.
- Cancellation pressure.
- QuietTight regime.
- ActiveLiquid regime.
- FastOrderly regime.
- ThinFragile regime.
- VolatileDislocated regime.
- Recovering regime.
- Candidate persistence.
- Minimum dwell.
- Hysteresis.
- Emergency transition.
- Stale feed is not QuietTight.
- Invalid book is Unknown.
- Replay/live parity for metrics.

### 15.3 Floating-pair tests

- One 20/20 pair becomes Candidate.
- One 200/200 pair is VeryLarge.
- 20/21 rejected.
- Individual 20-lot order remains visible when other orders share the same price.
- Static pair becomes Persistent but not FloatingConfirmed.
- Two coordinated moves confirm the pair.
- One-leg-only move fails.
- Late counterpart beyond synchronization window fails.
- Stable one-to-one matching with multiple equal-size orders.
- Same-ID repricing preserves pair.
- Strict cancel/re-entry stitching.
- Partial fill breaks pair.
- FLUSH clears all pair state.
- Reconnect clears all pair state.
- MBO downgrade returns Unavailable.
- Book epoch mismatch prevents stitching.
- Replay/live parity for pair state.

### 15.4 Recorder tests

- Schema version included.
- Configuration hash included.
- Epoch included.
- Transition emitted immediately.
- Pair lifecycle emitted immediately.
- Unavailable values are not serialized as misleading numeric zero.
- Deterministic timestamps in replay.

### 15.5 Architecture tests

Where practical, add tests or analyzers ensuring:

- Feature projects do not reference Quantower.
- Analytics runtime does not reference broker order methods.
- Quantower host does not contain duplicated feature formulas.
- Only one production book writer exists.
- Only one implementation of the new modules is used by live host and replay.

### 15.6 Preserve execution tests

Keep existing deterministic `ExecutionCore` tests compiling and passing, but do not activate execution in this version.

---

## 16. PERFORMANCE AND CONCURRENCY

The project processes high-frequency market data. Implement carefully:

- Prefer one serialized event-processing loop per symbol.
- Avoid locks inside hot paths where single-writer ownership suffices.
- Use bounded queues and explicit overflow behavior.
- Do not silently drop events.
- If events are dropped or ordering is uncertain:
  - Invalidate the book epoch.
  - Reset dependent features.
  - Request/rebuild from snapshot.
- Use ring buffers/deques for rolling windows.
- Avoid repeated LINQ allocations in hot paths.
- Avoid rebuilding full collections every 250 ms.
- Use integer tick prices internally.
- Keep UI work off the event-processing hot path.
- Add basic performance counters:
  - Queue depth.
  - Processing latency.
  - Dropped/rejected events.
  - Snapshot duration.
  - Feature calculation duration.
  - Panel render duration.

Do not compromise correctness to optimize prematurely. Add a simple benchmark or replay throughput report.

---

## 17. LOGGING AND DIAGNOSTICS

Use structured diagnostics.

Log important events:

- Subscription start/stop.
- Book epoch changes.
- Snapshot start/completion/failure.
- Buffer overflow.
- FLUSH.
- Reconnect.
- Mode downgrade.
- Invalid/crossed book.
- Regime transition.
- Floating-pair creation, confirmation, and break.
- Recorder failure.
- Runtime queue health.

Avoid logging every market event at normal verbosity.

Expose a diagnostics snapshot for the panel and recorder.

---

## 18. BUILD, VERSIONING, AND REPRODUCIBILITY

A successful implementation must:

1. Build Debug and Release.
2. Run all tests.
3. Produce no unexplained compiler warnings.
4. Identify the exact Git commit.
5. Record:
   - .NET SDK/compiler version.
   - Quantower API/reference version.
   - Build timestamp.
   - Configuration.
   - Test result.
   - Output DLL hash.
6. Generate `docs/BUILD_MANIFEST.md`.
7. Ensure the shipped DLL is built from the audited source commit, not an older binary.

Do not treat existing DLLs as proof of a successful build.

---

## 19. REQUIRED DOCUMENTATION

Create or update:

### `README.md`

- Purpose.
- Analytics-only status.
- How to build.
- How to run in Quantower.
- How to enable recording.
- How to run replay.
- Safety statement: no live trading in V2.

### `docs/ARCHITECTURE_V2.md`

- Dependency diagram.
- Runtime data flow.
- Book lifecycle.
- Feature engine.
- Recorder/replay parity.
- Extension points.

### `docs/MIGRATION_V1_TO_V2.md`

- Which old classes were retained.
- Which were wrapped.
- Which were replaced.
- Which were removed from runtime.
- Known compatibility notes.

### `docs/FEATURE_CATALOG.md`

For every feature:

- Name/key.
- Formula.
- Units.
- Windows.
- Required source mode.
- Warm-up.
- Reset behavior.
- Quality semantics.
- Interpretation limitations.

### `docs/MARKET_STATE_REGIMES.md`

- Score formulas.
- Default thresholds.
- State definitions.
- Transition/hysteresis rules.
- Examples.

### `docs/FLOATING_PAIR_DETECTOR.md`

- Exact intended behavior.
- Pairing rules.
- Movement confirmation.
- Stitching.
- Break conditions.
- Panel fields.
- Limitations.

### `docs/TEST_PLAN_V2.md`

- Test categories.
- Scenario descriptions.
- Replay parity criteria.

### `docs/BUILD_MANIFEST.md`

- Reproducible build information.

---

## 20. MIGRATION SEQUENCE

Implement in controlled phases.

### Phase 0 — Baseline

- Inspect and document current repository.
- Build/test current source.
- Preserve existing behavior and test data.

### Phase 1 — Domain and lifecycle

- Add neutral events, snapshots, quality types, book mode, lifecycle state, and epochs.
- Adapt current market-data input into neutral events.
- Implement valid book publication.

### Phase 2 — Feature engine

- Add feature contracts and registry.
- Wrap existing analytics as a legacy module where needed.
- Ensure no Quantower dependency.

### Phase 3 — Market-state module

- Implement spread, event-rate, volatility, liquidity, scores, regimes, and transitions.
- Add all tests.

### Phase 4 — Floating-pair module

- Implement individual MBO order tracking and paired floating quote detection.
- Rename/retain the old aggregate lattice detector separately.
- Add all tests.

### Phase 5 — Recorder and replay

- Add versioned feature, transition, pair, and diagnostic output.
- Make replay use the same runtime modules.
- Add parity tests.

### Phase 6 — Quantower host and panel

- Replace monolithic behavior with a thin adapter/view host.
- Add focused panel sections.
- Remove active strategy and execution wiring.

### Phase 7 — Cleanup and build

- Remove unreachable duplicate paths.
- Keep necessary legacy formulas behind modules.
- Build Debug/Release.
- Run tests.
- Generate docs and build manifest.

Do not delete legacy code until replacement tests pass. After migration, legacy runtime code must not be reachable from the V2 host.

---

## 21. DEFINITION OF DONE

The work is complete only when all of the following are true:

- The solution builds in Debug and Release.
- All tests pass.
- The Quantower V2 host starts without active strategy or live order capability.
- The book lifecycle exposes explicit state, mode, quality, and epoch.
- FLUSH/reconnect/reset behavior is deterministic.
- MBP and MBO states are not mixed.
- Snapshot seeds are excluded from live activity metrics.
- Market-state metrics update correctly.
- Regime transitions are stable and explainable.
- Stale/invalid states are not displayed as zero.
- The floating-pair detector works on individual MBO orders.
- One-bid/one-ask exact-size pairs can be detected.
- Large and very-large tiers are shown.
- Static pairs are not falsely labeled floating.
- Coordinated market-following movement is measured.
- Replay and live-host analytics use the same modules.
- Recorder output is versioned and reproducible.
- The primary panel is organized and understandable.
- The old Flow Ratio Strategy is not active.
- No order is submitted by the analytics V2 runtime.
- `ExecutionCore` and its tests are preserved but dormant.
- Documentation describes the architecture and formulas.
- A build manifest ties the binary to the exact commit and tests.

---

## 22. FINAL RESPONSE REQUIRED FROM CODEX

After implementation, provide a concise but complete report containing:

1. Final architecture summary.
2. New projects/folders created.
3. Existing components retained.
4. Components removed or disabled.
5. Important changed files.
6. Market-state module behavior.
7. Floating-pair detector behavior.
8. Book lifecycle and reset behavior.
9. Panel changes.
10. Recorder/replay changes.
11. Build commands and results.
12. Test commands and results.
13. Exact commit hash.
14. Output DLL paths and hashes.
15. Remaining limitations or items that could not be completed.
16. Any manual Quantower installation or verification steps.

Be transparent. Do not claim a successful build, test, feed behavior, MBO completeness, or Quantower runtime verification unless it was actually performed.

---

## 23. IMPLEMENTATION PRINCIPLES

Use these principles throughout:

- Correctness before optimization.
- One source of truth for book state.
- One source of truth for each feature formula.
- One runtime path for live analytics and replay.
- Immutable snapshots at module boundaries.
- Explicit unavailable/warming/stale/invalid states.
- No hidden MBP-to-MBO assumptions.
- No strategy or trading claims in analytics code.
- No duplicate production state machines.
- No unexplained magic thresholds; configure and document them.
- No silent event loss.
- No stale binary delivery.
- Preserve useful work, but do not preserve architectural mistakes merely for compatibility.

Begin by inspecting the repository and creating the baseline document, then implement the migration through the phases above.
