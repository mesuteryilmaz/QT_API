# V2 Test Plan

## Order Book Lifecycle

Covered by deterministic tests:

- Initial two-sided snapshot becomes valid.
- Empty and one-sided snapshots are invalid.
- Zero-price FLUSH increments epoch and clears book.
- Reconnect and buffer overflow increment epochs.
- MBO-to-MBP clears incompatible state.
- MBP-to-MBO does not synthesize identities.
- Locked book is flagged.
- Crossed book is invalid.
- Snapshot seed rows are excluded from live event counts.
- Stale feed produces stale quality.

## Market State

Covered:

- Constant one-tick spread.
- Time-weighted mean spread.
- Time at one tick.
- Spread widening episode and recovery.
- BBO price update versus size-only update.
- Cancellation pressure.
- QuietTight, VolatileDislocated, stale, and invalid behavior.

## Floating Pairs

Covered:

- 20/20 candidate.
- 200/200 very-large tier.
- Exact-size mismatch rejection.
- Individual order preserved despite same-price aggregate quantity.
- Static pair persistent but not confirmed.
- Two coordinated moves confirm.
- One-leg-only and late-counterpart failures.
- Distance gating.
- One-to-one matching.
- Same-ID repricing.
- Strict cancel/re-entry stitching.
- Partial fill break.
- Leg removal break.
- MBO downgrade unavailable.
- FLUSH/reconnect reset.
- Epoch mismatch prevents stitching.

## Recorder / Replay / Architecture

Covered:

- Schema version and config hash included.
- Unavailable values do not serialize as misleading numeric zero.
- Replay/live runtime parity for sample sequence.
- Feature projects do not reference Quantower.
- Runtime does not reference broker order methods.
- Quantower host does not compile active strategy, shadow simulator, strategy replay, broker, or monolithic calculator.

## Preserved Execution Tests

Existing deterministic `ExecutionCore` tests still run against dormant `QT.Execution`.

## Verified Commands

```powershell
dotnet build QT_API.sln -c Debug
dotnet build QT_API.sln -c Release
dotnet run --project tests\DataAnalytics.Tests.csproj -c Release
dotnet run --project tests\QT.UnitTests\QT.UnitTests.csproj -c Release
```
