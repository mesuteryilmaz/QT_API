# Class HistoryAggregationKagi
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.HistoryAggregationKagi.html
> Fetched: 2026-04-10

---

# Class HistoryAggregationKagi

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public sealed class HistoryAggregationKagi : HistoryAggregationTime, IHistoryAggregationHistoryTypeSupport
```

### Constructors

#### HistoryAggregationKagi(Period, HistoryType, int)

##### Declaration

```csharp
public HistoryAggregationKagi(Period period, HistoryType historyType, int reversal)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) | period |  |
| [HistoryType](TradingPlatform.BusinessLayer.HistoryType.html) | historyType |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | reversal |  |

### Fields

#### SETTINGS\_AGGREGATION\_REVERSAL

##### Declaration

```csharp
public const string SETTINGS_AGGREGATION_REVERSAL = "Reversal"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

### Properties

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
public int Reversal { get; set; }
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