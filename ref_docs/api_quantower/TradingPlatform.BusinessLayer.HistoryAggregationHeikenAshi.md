# Class HistoryAggregationHeikenAshi
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.HistoryAggregationHeikenAshi.html
> Fetched: 2026-04-10

---

# Class HistoryAggregationHeikenAshi

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public sealed class HistoryAggregationHeikenAshi : HistoryAggregation, IHistoryAggregationHistoryTypeSupport
```

### Constructors

#### HistoryAggregationHeikenAshi(HeikenAshiSource, int, HistoryType, bool, HeikenAshiSmoosedType, HeikenAshiSmoosedType, int)

##### Declaration

```csharp
public HistoryAggregationHeikenAshi(HeikenAshiSource source, int value, HistoryType historyType, bool smoothing, HeikenAshiSmoosedType smoothingType1, HeikenAshiSmoosedType smoothingType2, int smoothingPeriod)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [HeikenAshiSource](TradingPlatform.BusinessLayer.HeikenAshiSource.html) | source |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | value |  |
| [HistoryType](TradingPlatform.BusinessLayer.HistoryType.html) | historyType |  |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | smoothing |  |
| [HeikenAshiSmoosedType](TradingPlatform.BusinessLayer.HeikenAshiSmoosedType.html) | smoothingType1 |  |
| [HeikenAshiSmoosedType](TradingPlatform.BusinessLayer.HeikenAshiSmoosedType.html) | smoothingType2 |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | smoothingPeriod |  |

#### HistoryAggregationHeikenAshi(HistoryAggregationHeikenAshi)

##### Declaration

```csharp
protected HistoryAggregationHeikenAshi(HistoryAggregationHeikenAshi aggregation)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [HistoryAggregationHeikenAshi](TradingPlatform.BusinessLayer.HistoryAggregationHeikenAshi.html) | aggregation |  |

### Properties

#### HistoryType

##### Declaration

```csharp
public HistoryType HistoryType { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [HistoryType](TradingPlatform.BusinessLayer.HistoryType.html) |  |

#### Name

##### Declaration

```csharp
public override string Name { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

##### Overrides

[HistoryAggregation.Name](TradingPlatform.BusinessLayer.HistoryAggregation.html#TradingPlatform_BusinessLayer_HistoryAggregation_Name)

#### Settings

##### Declaration

```csharp
public override IList<SettingItem> Settings { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)<[SettingItem](TradingPlatform.BusinessLayer.SettingItem.html)> |  |

##### Overrides

[HistoryAggregation.Settings](TradingPlatform.BusinessLayer.HistoryAggregation.html#TradingPlatform_BusinessLayer_HistoryAggregation_Settings)

#### Smoothing

##### Declaration

```csharp
public bool Smoothing { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### SmoothingPeriod

##### Declaration

```csharp
public int SmoothingPeriod { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### SmoothingType1

##### Declaration

```csharp
public HeikenAshiSmoosedType SmoothingType1 { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [HeikenAshiSmoosedType](TradingPlatform.BusinessLayer.HeikenAshiSmoosedType.html) |  |

#### SmoothingType2

##### Declaration

```csharp
public HeikenAshiSmoosedType SmoothingType2 { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [HeikenAshiSmoosedType](TradingPlatform.BusinessLayer.HeikenAshiSmoosedType.html) |  |

#### Source

##### Declaration

```csharp
public HeikenAshiSource Source { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [HeikenAshiSource](TradingPlatform.BusinessLayer.HeikenAshiSource.html) |  |

#### Value

##### Declaration

```csharp
public int Value { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

### Methods

#### GetAggregationToDirectDownload(HistoryMetadata, ISessionsContainer, bool)

##### Declaration

```csharp
public override HistoryAggregation GetAggregationToDirectDownload(HistoryMetadata metadata, ISessionsContainer sessionsContainer, bool isResetOnSessionBoundaryEnabled)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [HistoryMetadata](TradingPlatform.BusinessLayer.Integration.HistoryMetadata.html) | metadata |  |
| [ISessionsContainer](TradingPlatform.BusinessLayer.ISessionsContainer.html) | sessionsContainer |  |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | isResetOnSessionBoundaryEnabled |  |

##### Returns

| Type | Description |
| --- | --- |
| [HistoryAggregation](TradingPlatform.BusinessLayer.HistoryAggregation.html) |  |

##### Overrides

[HistoryAggregation.GetAggregationToDirectDownload(HistoryMetadata, ISessionsContainer, bool)](TradingPlatform.BusinessLayer.HistoryAggregation.html#TradingPlatform_BusinessLayer_HistoryAggregation_GetAggregationToDirectDownload_TradingPlatform_BusinessLayer_Integration_HistoryMetadata_TradingPlatform_BusinessLayer_ISessionsContainer_System_Boolean_)

### Implements

[IHistoryAggregationHistoryTypeSupport](TradingPlatform.BusinessLayer.IHistoryAggregationHistoryTypeSupport.html)