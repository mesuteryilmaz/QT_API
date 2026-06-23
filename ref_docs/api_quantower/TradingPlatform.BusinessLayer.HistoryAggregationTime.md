# Class HistoryAggregationTime
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.HistoryAggregationTime.html
> Fetched: 2026-04-10

---

# Class HistoryAggregationTime

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class HistoryAggregationTime : HistoryAggregation, IHistoryAggregationHistoryTypeSupport
```

### Constructors

#### HistoryAggregationTime(HistoryAggregationTime)

##### Declaration

```csharp
protected HistoryAggregationTime(HistoryAggregationTime aggregation)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [HistoryAggregationTime](TradingPlatform.BusinessLayer.HistoryAggregationTime.html) | aggregation |  |

#### HistoryAggregationTime(Period, HistoryType)

##### Declaration

```csharp
public HistoryAggregationTime(Period period, HistoryType historyType)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) | period |  |
| [HistoryType](TradingPlatform.BusinessLayer.HistoryType.html) | historyType |  |

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

#### Period

##### Declaration

```csharp
public Period Period { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

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