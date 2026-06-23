# Class HistoryAggregationPointsAndFigures
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.HistoryAggregationPointsAndFigures.html
> Fetched: 2026-04-10

---

# Class HistoryAggregationPointsAndFigures

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public sealed class HistoryAggregationPointsAndFigures : HistoryAggregationTime, IHistoryAggregationHistoryTypeSupport
```

### Constructors

#### HistoryAggregationPointsAndFigures(HistoryAggregationPointsAndFigures)

##### Declaration

```csharp
protected HistoryAggregationPointsAndFigures(HistoryAggregationPointsAndFigures aggregation)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [HistoryAggregationPointsAndFigures](TradingPlatform.BusinessLayer.HistoryAggregationPointsAndFigures.html) | aggregation |  |

#### HistoryAggregationPointsAndFigures(Period, HistoryType, int, int, PointsAndFiguresStyle)

##### Declaration

```csharp
public HistoryAggregationPointsAndFigures(Period period, HistoryType historyType, int boxSize, int reversal, PointsAndFiguresStyle style)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) | period |  |
| [HistoryType](TradingPlatform.BusinessLayer.HistoryType.html) | historyType |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | boxSize |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | reversal |  |
| [PointsAndFiguresStyle](TradingPlatform.BusinessLayer.PointsAndFiguresStyle.html) | style |  |

### Properties

#### BoxSize

##### Declaration

```csharp
public int BoxSize { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

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

[HistoryAggregationTime.Name](TradingPlatform.BusinessLayer.HistoryAggregationTime.html#TradingPlatform_BusinessLayer_HistoryAggregationTime_Name)

#### Reversal

##### Declaration

```csharp
public int Reversal { get; }
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

[HistoryAggregationTime.Settings](TradingPlatform.BusinessLayer.HistoryAggregationTime.html#TradingPlatform_BusinessLayer_HistoryAggregationTime_Settings)

#### Style

##### Declaration

```csharp
public PointsAndFiguresStyle Style { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [PointsAndFiguresStyle](TradingPlatform.BusinessLayer.PointsAndFiguresStyle.html) |  |

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

[HistoryAggregationTime.GetAggregationToDirectDownload(HistoryMetadata, ISessionsContainer, bool)](TradingPlatform.BusinessLayer.HistoryAggregationTime.html#TradingPlatform_BusinessLayer_HistoryAggregationTime_GetAggregationToDirectDownload_TradingPlatform_BusinessLayer_Integration_HistoryMetadata_TradingPlatform_BusinessLayer_ISessionsContainer_System_Boolean_)

#### GetBaseAggregation()

##### Declaration

```csharp
public HistoryAggregation GetBaseAggregation()
```

##### Returns

| Type | Description |
| --- | --- |
| [HistoryAggregation](TradingPlatform.BusinessLayer.HistoryAggregation.html) |  |

### Implements

[IHistoryAggregationHistoryTypeSupport](TradingPlatform.BusinessLayer.IHistoryAggregationHistoryTypeSupport.html)