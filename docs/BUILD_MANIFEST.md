# Build Manifest

Generated: `2026-06-25T16:28:28.4983362+03:00`

## Source

- Branch: `analytics-v2`
- HEAD: `3c93df668447ddbff94010c4b2302f910aff777b`
- Worktree: dirty, with V2 implementation changes and pre-existing untracked `.vscode/` plus `QT_API_V2_CODEX_IMPLEMENTATION_PROMPT.md`.
- Runtime source marker: `3c93df668447ddbff94010c4b2302f910aff777b-dirty-analytics-v2`

## Toolchain

- .NET SDK: `10.0.204`
- Quantower API reference: `C:\Quantower\TradingPlatform\v1.146.13\bin\TradingPlatform.BusinessLayer.dll`
- Target host assembly: `MBO_Market_Data_Analytics`
- Target framework: `net10.0-windows`
- Deployed support assemblies use V2-specific physical names (`QT_API.V2.*.dll`) so Quantower does not bind the V2 indicator to stale `QT.*` assemblies already loaded in the process.

## Build Commands

```powershell
dotnet build QT_API.sln -c Debug
dotnet build QT_API.sln -c Release
```

Results:

- Debug solution build: 0 warnings, 0 errors.
- Release solution build: 0 warnings, 0 errors.

## Test Commands

```powershell
dotnet run --project tests\DataAnalytics.Tests.csproj -c Release
dotnet run --project tests\QT.UnitTests\QT.UnitTests.csproj -c Release
```

Results:

- `DataAnalytics.Tests`: `230 passed, 0 failed`.
- `QT.UnitTests`: `230 passed, 0 failed`.

## DLL Hashes

SHA-256:

| Path | Hash |
|---|---|
| `C:\Users\mesuteryilmaz\Desktop\QT_API\bin\Debug\MBO_Market_Data_Analytics.dll` | `C91FAF43D911A3A6484EF86EDDEDDE87330E083476A55F0D092E344FA66C2EA3` |
| `C:\Users\mesuteryilmaz\Desktop\QT_API\bin\Release\MBO_Market_Data_Analytics.dll` | `06D5B1963680C96F9AB60645968FAA2757C860E7D7F4A002B0BBDA5BBAF44C67` |
| `C:\Quantower\Settings\Scripts\Indicators\MBO_Market_Data_Analytics.dll` | `06D5B1963680C96F9AB60645968FAA2757C860E7D7F4A002B0BBDA5BBAF44C67` |
| `C:\Quantower\Settings\Scripts\Indicators\QT_API.V2.Core.dll` | `2F6A7615B863E2454F6EF2A729E416D879FCEFB2BC11F498AF5CBA9FECBAEF6F` |
| `C:\Quantower\Settings\Scripts\Indicators\QT_API.V2.Market.dll` | `440B9B4190499BD6DA33713BE24CEE36E3E49A2F796381201B58D49C14E23D04` |
| `C:\Quantower\Settings\Scripts\Indicators\QT_API.V2.Features.dll` | `1D19092D38C0C56A783EDC6F937DAA59EAF06C2382B9F37DE7381702DC91FE8F` |
| `C:\Quantower\Settings\Scripts\Indicators\QT_API.V2.Storage.dll` | `AFDE4B25A8700E141925A039AED09AA21C03D903DCD0A51A9EAA7F3B6E5C90BD` |
| `C:\Quantower\Settings\Scripts\Indicators\QT_API.V2.Runtime.dll` | `979FCB192C8344E527840FF6E2CA7CD6D12ACA15E99D02CD62646C7C7BC6436A` |
| `C:\Quantower\Settings\Scripts\Strategies\MBO_Market_Data_Analytics.dll` | `06D5B1963680C96F9AB60645968FAA2757C860E7D7F4A002B0BBDA5BBAF44C67` |
| `C:\Quantower\Settings\Scripts\Strategies\QT_API.V2.Core.dll` | `2F6A7615B863E2454F6EF2A729E416D879FCEFB2BC11F498AF5CBA9FECBAEF6F` |
| `C:\Quantower\Settings\Scripts\Strategies\QT_API.V2.Market.dll` | `440B9B4190499BD6DA33713BE24CEE36E3E49A2F796381201B58D49C14E23D04` |
| `C:\Quantower\Settings\Scripts\Strategies\QT_API.V2.Features.dll` | `1D19092D38C0C56A783EDC6F937DAA59EAF06C2382B9F37DE7381702DC91FE8F` |
| `C:\Quantower\Settings\Scripts\Strategies\QT_API.V2.Storage.dll` | `AFDE4B25A8700E141925A039AED09AA21C03D903DCD0A51A9EAA7F3B6E5C90BD` |
| `C:\Quantower\Settings\Scripts\Strategies\QT_API.V2.Runtime.dll` | `979FCB192C8344E527840FF6E2CA7CD6D12ACA15E99D02CD62646C7C7BC6436A` |

The Strategies-path DLL is the same analytics-only Release binary and contains no compiled V1 `DataAnalyticsStrategy.cs`.

## Quantower Runtime Verification

Build and copy were verified. `C:\Quantower\Logs\Serilog\20260625.slog` was inspected after deployment.

Observed and addressed:

- `12:57:32Z` through `12:57:35Z`: Quantower could not load `QT.Runtime, Version=1.0.0.0` because only the main indicator DLL had been copied. The post-build copy now deploys all support assemblies.
- `13:08:03Z` and `13:11:33Z`: Quantower reported `Method not found` against `AnalyticsRuntimeConfig.set_PreferredBookMode`, caused by an old `QT.Runtime` assembly already loaded in the Quantower process. Support assemblies now use `QT_API.V2.*.dll` identities.
- `13:15:11Z`: Quantower logged `[QT_API V2] Analytics-only runtime started. No strategy or live-order path is active.`
- `13:15:12Z`: Quantower logged `[QT_API V2] MBO identity coverage confirmed (%100).`
- `13:25:00Z` through `13:25:30Z`: Quantower reported large Interactive Brokers `DataLatency` values while the previous per-event feature-publish build was active.
- Latency fix: the runtime now applies order-book mutations continuously but publishes full feature snapshots only on lifecycle changes or the configured feature cadence. Market-state event queues now use weighted samples instead of allocating one sample per book event. Quantower callbacks no longer read the runtime snapshot on every Level2 event.
- Regression coverage: `runtime throttles feature publication during event bursts` verifies that valid depth-update bursts inside the cadence window do not trigger per-event feature publication.
- `13:27:06Z`: Quantower logged `[QT_API V2] Analytics-only runtime started. No strategy or live-order path is active.` after the latency-fix package was deployed.

The log verifies that the indicator runtime started in Quantower after the packaging fix. Visual chart-panel rendering was not independently screenshot-verified.
