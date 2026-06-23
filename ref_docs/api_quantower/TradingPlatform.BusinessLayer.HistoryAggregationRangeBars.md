# Class HistoryAggregationRangeBars
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.HistoryAggregationRangeBars.html
> Fetched: 2026-04-10

---

# Class HistoryAggregationRangeBars

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public sealed class HistoryAggregationRangeBars : HistoryAggregation, IHistoryAggregationHistoryTypeSupport
```

### Constructors

#### HistoryAggregationRangeBars(int, HistoryType)

##### Declaration

```csharp
public HistoryAggregationRangeBars(int rangeBars, HistoryType historyType)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | rangeBars |  |
| [HistoryType](TradingPlatform.BusinessLayer.HistoryType.html) | historyType |  |

### Fields

#### SETTINGS\_AGGREGATION\_RANGE\_BARS

##### Declaration

```csharp
public const string SETTINGS_AGGREGATION_RANGE_BARS = "Range bars"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

### Properties

#### GetPeriod

##### Declaration

```csharp
public override Period GetPeriod { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

##### Overrides

[HistoryAggregation.GetPeriod](TradingPlatform.BusinessLayer.HistoryAggregation.html#TradingPlatform_BusinessLayer_HistoryAggregation_GetPeriod)

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

#### RangeBars

##### Declaration

```csharp
public int RangeBars { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

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