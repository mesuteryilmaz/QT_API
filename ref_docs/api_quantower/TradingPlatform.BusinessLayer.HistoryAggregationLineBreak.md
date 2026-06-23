# Class HistoryAggregationLineBreak
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.HistoryAggregationLineBreak.html
> Fetched: 2026-04-10

---

# Class HistoryAggregationLineBreak

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public sealed class HistoryAggregationLineBreak : HistoryAggregationTime, IHistoryAggregationHistoryTypeSupport
```

### Constructors

#### HistoryAggregationLineBreak(Period, HistoryType, int)

##### Declaration

```csharp
public HistoryAggregationLineBreak(Period period, HistoryType historyType, int lineBreak)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) | period |  |
| [HistoryType](TradingPlatform.BusinessLayer.HistoryType.html) | historyType |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | lineBreak |  |

### Fields

#### SETTINGS\_AGGREGATION\_LINE\_BREAK

##### Declaration

```csharp
public const string SETTINGS_AGGREGATION_LINE_BREAK = "Line break"
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

[HistoryAggregationTime.GetPeriod](TradingPlatform.BusinessLayer.HistoryAggregationTime.html#TradingPlatform_BusinessLayer_HistoryAggregationTime_GetPeriod)

#### LineBreak

##### Declaration

```csharp
public int LineBreak { get; set; }
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

### Methods

#### Clone()

Creates a new object that is a copy of the current instance.

##### Declaration

```csharp
public override object Clone()
```

##### Returns

| Type | Description |
| --- | --- |
| [object](https://learn.microsoft.com/dotnet/api/system.object) | A new object that is a copy of this instance. |

##### Overrides

[HistoryAggregationTime.Clone()](TradingPlatform.BusinessLayer.HistoryAggregationTime.html#TradingPlatform_BusinessLayer_HistoryAggregationTime_Clone)

### Implements

[IHistoryAggregationHistoryTypeSupport](TradingPlatform.BusinessLayer.IHistoryAggregationHistoryTypeSupport.html)