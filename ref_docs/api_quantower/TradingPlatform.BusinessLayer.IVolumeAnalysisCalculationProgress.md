# Interface IVolumeAnalysisCalculationProgress
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.IVolumeAnalysisCalculationProgress.html
> Fetched: 2026-04-10

---

# Interface IVolumeAnalysisCalculationProgress

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public interface IVolumeAnalysisCalculationProgress
```

### Properties

#### CalculationParameters

##### Declaration

```csharp
VolumeAnalysisCalculationParameters CalculationParameters { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [VolumeAnalysisCalculationParameters](TradingPlatform.BusinessLayer.VolumeAnalysisCalculationParameters.html) |  |

#### IsAborted

##### Declaration

```csharp
bool IsAborted { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### ProgressBarIndex

##### Declaration

```csharp
int ProgressBarIndex { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### ProgressPercent

##### Declaration

```csharp
int ProgressPercent { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### State

##### Declaration

```csharp
VolumeAnalysisCalculationState State { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [VolumeAnalysisCalculationState](TradingPlatform.BusinessLayer.VolumeAnalysisCalculationState.html) |  |

### Methods

#### AbortLoading()

##### Declaration

```csharp
void AbortLoading()
```

#### Wait(CancellationToken)

##### Declaration

```csharp
void Wait(CancellationToken token = default)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) | token |  |

### Events

#### ProgressChanged

##### Declaration

```csharp
event EventHandler<VolumeAnalysisTaskEventArgs> ProgressChanged
```

##### Event Type

| Type | Description |
| --- | --- |
| [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler-1)<[VolumeAnalysisTaskEventArgs](TradingPlatform.BusinessLayer.VolumeAnalysisTaskEventArgs.html)> |  |

#### StateChanged

##### Declaration

```csharp
event EventHandler<VolumeAnalysisTaskEventArgs> StateChanged
```

##### Event Type

| Type | Description |
| --- | --- |
| [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler-1)<[VolumeAnalysisTaskEventArgs](TradingPlatform.BusinessLayer.VolumeAnalysisTaskEventArgs.html)> |  |