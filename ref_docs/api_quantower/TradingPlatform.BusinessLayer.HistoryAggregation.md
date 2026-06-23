# Class HistoryAggregation
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.HistoryAggregation.html
> Fetched: 2026-04-10

---

# Class HistoryAggregation

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public abstract class HistoryAggregation
```

### Constructors

#### HistoryAggregation()

##### Declaration

```csharp
protected HistoryAggregation()
```

#### HistoryAggregation(HistoryAggregation)

##### Declaration

```csharp
protected HistoryAggregation(HistoryAggregation origin)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [HistoryAggregation](TradingPlatform.BusinessLayer.HistoryAggregation.html) | origin |  |

### Fields

#### DELTA\_BARS

##### Declaration

```csharp
public const string DELTA_BARS = "Delta bars"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### DOM\_AGGREGATED

##### Declaration

```csharp
public const string DOM_AGGREGATED = "Aggregated DOM"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### DOM\_BY\_TICKS\_COUNT

##### Declaration

```csharp
public const string DOM_BY_TICKS_COUNT = "DOM by ticks count"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### DOM\_BY\_TIME

##### Declaration

```csharp
public const string DOM_BY_TIME = "DOM by time"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### HEIKIN\_ASHI

##### Declaration

```csharp
public const string HEIKIN_ASHI = "Heikin Ashi"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### KAGI

##### Declaration

```csharp
public const string KAGI = "Kagi"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### LEVEL2

##### Declaration

```csharp
public const string LEVEL2 = "Level2"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### LINE\_BREAK

##### Declaration

```csharp
public const string LINE_BREAK = "Line Break"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### POINTS\_AND\_FIGURES

##### Declaration

```csharp
public const string POINTS_AND_FIGURES = "Points & Figures"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### POWER\_TRADES

##### Declaration

```csharp
public const string POWER_TRADES = "Power Trades"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### PRICE\_CHANGES\_COUNT\_BARS

##### Declaration

```csharp
public const string PRICE_CHANGES_COUNT_BARS = "Price changes count bars"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### RANGE\_BARS

##### Declaration

```csharp
public const string RANGE_BARS = "Range bars"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### RENKO

##### Declaration

```csharp
public const string RENKO = "Renko"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### REVERSAL

##### Declaration

```csharp
public const string REVERSAL = "Reversal"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### SETTINGS\_AGGREGATION\_HISTORY\_TYPE

##### Declaration

```csharp
public const string SETTINGS_AGGREGATION_HISTORY_TYPE = "History type"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### SETTINGS\_AGGREGATION\_PERIOD

##### Declaration

```csharp
public const string SETTINGS_AGGREGATION_PERIOD = "Period"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### SPY\_MONEY\_BARS

##### Declaration

```csharp
public const string SPY_MONEY_BARS = "Spy money bars"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### TICK

##### Declaration

```csharp
public const string TICK = "Tick"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### TICK\_BARS

##### Declaration

```csharp
public const string TICK_BARS = "Tick bars"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### TICK\_LAST\_AGGREGATED

##### Declaration

```csharp
public const string TICK_LAST_AGGREGATED = "Aggregated ticks (Last)"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### TIME

##### Declaration

```csharp
public const string TIME = "Time"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### TIME\_STATISTICS

##### Declaration

```csharp
public const string TIME_STATISTICS = "Time statistics"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### VOLUME

##### Declaration

```csharp
public const string VOLUME = "Volume"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### VOLUME\_PROFILE

##### Declaration

```csharp
public const string VOLUME_PROFILE = "Volume profile"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### VWAP

##### Declaration

```csharp
public const string VWAP = "VWAP"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

### Properties

#### DefaultRange

##### Declaration

```csharp
public virtual Period DefaultRange { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### IsWaitingFirstQuoteRequired

##### Declaration

```csharp
public virtual bool IsWaitingFirstQuoteRequired { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### Name

##### Declaration

```csharp
public abstract string Name { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### SessionsContainer

##### Declaration

```csharp
public ISessionsContainer SessionsContainer { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [ISessionsContainer](TradingPlatform.BusinessLayer.ISessionsContainer.html) |  |

#### Settings

##### Declaration

```csharp
public virtual IList<SettingItem> Settings { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)<[SettingItem](TradingPlatform.BusinessLayer.SettingItem.html)> |  |

#### Title

##### Declaration

```csharp
public virtual string Title { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

### Methods

#### Equals(object)

Determines whether the specified object is equal to the current object.

##### Declaration

```csharp
public override bool Equals(object obj)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [object](https://learn.microsoft.com/dotnet/api/system.object) | obj | The object to compare with the current object. |

##### Returns

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | [true](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool) if the specified object is equal to the current object; otherwise, [false](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool). |

##### Overrides

[object.Equals(object)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals(system-object))

#### Equals(HistoryAggregation)

Indicates whether the current object is equal to another object of the same type.

##### Declaration

```csharp
public bool Equals(HistoryAggregation other)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [HistoryAggregation](TradingPlatform.BusinessLayer.HistoryAggregation.html) | other | An object to compare with this object. |

##### Returns

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | [true](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool) if the current object is equal to the `other` parameter; otherwise, [false](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool). |

#### GetAggregationToDirectDownload(HistoryMetadata, ISessionsContainer, bool)

##### Declaration

```csharp
public abstract HistoryAggregation GetAggregationToDirectDownload(HistoryMetadata metadata, ISessionsContainer sessionsContainer, bool isResetOnSessionBoundaryEnabled)
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

#### GetHashCode()

Serves as the default hash function.

##### Declaration

```csharp
public override int GetHashCode()
```

##### Returns

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | A hash code for the current object. |

##### Overrides

[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)