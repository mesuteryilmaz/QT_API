# Class HistoryRequestParameters
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.HistoryRequestParameters.html
> Fetched: 2026-04-10

---

# Class HistoryRequestParameters

Resolves a history request parameters per symbol

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public sealed class HistoryRequestParameters : RequestParameters
```

### Constructors

#### HistoryRequestParameters()

Resolves a history request parameters per symbol

##### Declaration

```csharp
public HistoryRequestParameters()
```

#### HistoryRequestParameters(HistoryRequestParameters)

Resolves a history request parameters per symbol

##### Declaration

```csharp
public HistoryRequestParameters(HistoryRequestParameters original)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [HistoryRequestParameters](TradingPlatform.BusinessLayer.HistoryRequestParameters.html) | original |  |

### Properties

#### Aggregation

Resolves a history request parameters per symbol

##### Declaration

```csharp
public HistoryAggregation Aggregation { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [HistoryAggregation](TradingPlatform.BusinessLayer.HistoryAggregation.html) |  |

#### Copy

Resolves a history request parameters per symbol

##### Declaration

```csharp
public HistoryRequestParameters Copy { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [HistoryRequestParameters](TradingPlatform.BusinessLayer.HistoryRequestParameters.html) |  |

#### ExcludeOutOfSession

Resolves a history request parameters per symbol

##### Declaration

```csharp
public bool ExcludeOutOfSession { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### ForceReload

Resolves a history request parameters per symbol

##### Declaration

```csharp
public bool ForceReload { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### FromTime

Resolves a history request parameters per symbol

##### Declaration

```csharp
public DateTime FromTime { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

#### HistoryRequestType

Resolves a history request parameters per symbol

##### Declaration

```csharp
public HistoryRequestType HistoryRequestType { get; init; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [HistoryRequestType](TradingPlatform.BusinessLayer.HistoryRequestType.html) |  |

#### Interval

Resolves a history request parameters per symbol

##### Declaration

```csharp
public Interval<DateTime> Interval { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Interval](TradingPlatform.BusinessLayer.Utils.Interval-1.html)<[DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)> |  |

#### IsResetOnSessionBoundaryEnabled

Resolves a history request parameters per symbol

##### Declaration

```csharp
public bool IsResetOnSessionBoundaryEnabled { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### ProgressInfo

Resolves a history request parameters per symbol

##### Declaration

```csharp
public IProgress<float> ProgressInfo { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IProgress](https://learn.microsoft.com/dotnet/api/system.iprogress-1)<[float](https://learn.microsoft.com/dotnet/api/system.single)> |  |

#### SessionsContainer

Resolves a history request parameters per symbol

##### Declaration

```csharp
public ISessionsContainer SessionsContainer { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [ISessionsContainer](TradingPlatform.BusinessLayer.ISessionsContainer.html) |  |

#### Symbol

Resolves a history request parameters per symbol

##### Declaration

```csharp
public Symbol Symbol { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Symbol](TradingPlatform.BusinessLayer.Symbol.html) |  |

#### SymbolId

Resolves a history request parameters per symbol

##### Declaration

```csharp
public string SymbolId { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### ToTime

Resolves a history request parameters per symbol

##### Declaration

```csharp
public DateTime ToTime { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

#### Type

Resolves a history request parameters per symbol

##### Declaration

```csharp
public override RequestType Type { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [RequestType](TradingPlatform.BusinessLayer.RequestType.html) |  |

##### Overrides

[RequestParameters.Type](TradingPlatform.BusinessLayer.RequestParameters.html#TradingPlatform_BusinessLayer_RequestParameters_Type)

### Methods

#### ToDescription()

Resolves a history request parameters per symbol

##### Declaration

```csharp
public HistoryDescription ToDescription()
```

##### Returns

| Type | Description |
| --- | --- |
| [HistoryDescription](TradingPlatform.BusinessLayer.History.Storage.HistoryDescription.html) |  |