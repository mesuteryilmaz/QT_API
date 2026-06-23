# Class VolumeAnalysisManager
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.VolumeAnalysisManager.html
> Fetched: 2026-04-10

---

# Class VolumeAnalysisManager

Volume Analysis calculations

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class VolumeAnalysisManager
```

### Methods

#### CalculateProfile(HistoricalData)

Calculate volume profile for each bar in History Data

##### Declaration

```csharp
public IVolumeAnalysisCalculationProgress CalculateProfile(HistoricalData historicalData)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [HistoricalData](TradingPlatform.BusinessLayer.HistoricalData.html) | historicalData |  |

##### Returns

| Type | Description |
| --- | --- |
| [IVolumeAnalysisCalculationProgress](TradingPlatform.BusinessLayer.IVolumeAnalysisCalculationProgress.html) |  |

#### CalculateProfile(HistoricalData, VolumeAnalysisCalculationParameters)

Calculate volume profile for each bar in History Data

##### Declaration

```csharp
public IVolumeAnalysisCalculationProgress CalculateProfile(HistoricalData historicalData, VolumeAnalysisCalculationParameters calculationParameters)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [HistoricalData](TradingPlatform.BusinessLayer.HistoricalData.html) | historicalData |  |
| [VolumeAnalysisCalculationParameters](TradingPlatform.BusinessLayer.VolumeAnalysisCalculationParameters.html) | calculationParameters |  |

##### Returns

| Type | Description |
| --- | --- |
| [IVolumeAnalysisCalculationProgress](TradingPlatform.BusinessLayer.IVolumeAnalysisCalculationProgress.html) |  |

#### CalculateProfile(Symbol, DateTime, DateTime)

Calculate volume profile for requested time range

##### Declaration

```csharp
public IVolumeAnalysisCalculationTask CalculateProfile(Symbol symbol, DateTime from, DateTime to)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Symbol](TradingPlatform.BusinessLayer.Symbol.html) | symbol |  |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) | from |  |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) | to |  |

##### Returns

| Type | Description |
| --- | --- |
| [IVolumeAnalysisCalculationTask](TradingPlatform.BusinessLayer.IVolumeAnalysisCalculationTask.html) |  |

#### CalculateProfile(VolumeAnalysisCalculationRequest)

Calculate volume profile for requested time range

##### Declaration

```csharp
public IVolumeAnalysisCalculationTask CalculateProfile(VolumeAnalysisCalculationRequest request)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [VolumeAnalysisCalculationRequest](TradingPlatform.BusinessLayer.VolumeAnalysisCalculationRequest.html) | request |  |

##### Returns

| Type | Description |
| --- | --- |
| [IVolumeAnalysisCalculationTask](TradingPlatform.BusinessLayer.IVolumeAnalysisCalculationTask.html) |  |