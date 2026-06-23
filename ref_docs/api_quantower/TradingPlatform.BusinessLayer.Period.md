# Struct Period
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Period.html
> Fetched: 2026-04-10

---

# Struct Period

Represents mechanism for supporting predefined and custom periods

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public struct Period
```

### Constructors

#### Period(BasePeriod, int)

Creates Period instance with [PeriodMultiplier](TradingPlatform.BusinessLayer.Period.html#TradingPlatform_BusinessLayer_Period_PeriodMultiplier) greater than 0

##### Declaration

```csharp
public Period(BasePeriod basePeriod, int periodMultiplier)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [BasePeriod](TradingPlatform.BusinessLayer.BasePeriod.html) | basePeriod |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | periodMultiplier |  |

### Properties

#### BasePeriod

Gets base period type

##### Declaration

```csharp
public readonly BasePeriod BasePeriod { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [BasePeriod](TradingPlatform.BusinessLayer.BasePeriod.html) |  |

#### DAY1

Predefined period

##### Declaration

```csharp
public static Period DAY1 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### Duration

Represents mechanism for supporting predefined and custom periods

##### Declaration

```csharp
public TimeSpan Duration { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan) |  |

#### HOUR1

Predefined period

##### Declaration

```csharp
public static Period HOUR1 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### HOUR12

Predefined period

##### Declaration

```csharp
public static Period HOUR12 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### HOUR2

Predefined period

##### Declaration

```csharp
public static Period HOUR2 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### HOUR3

Predefined period

##### Declaration

```csharp
public static Period HOUR3 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### HOUR4

Predefined period

##### Declaration

```csharp
public static Period HOUR4 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### HOUR6

Predefined period

##### Declaration

```csharp
public static Period HOUR6 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### HOUR8

Predefined period

##### Declaration

```csharp
public static Period HOUR8 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### MIN1

Predefined period

##### Declaration

```csharp
public static Period MIN1 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### MIN10

Predefined period

##### Declaration

```csharp
public static Period MIN10 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### MIN15

Predefined period

##### Declaration

```csharp
public static Period MIN15 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### MIN2

Predefined period

##### Declaration

```csharp
public static Period MIN2 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### MIN3

Predefined period

##### Declaration

```csharp
public static Period MIN3 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### MIN30

Predefined period

##### Declaration

```csharp
public static Period MIN30 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### MIN4

Predefined period

##### Declaration

```csharp
public static Period MIN4 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### MIN5

Predefined period

##### Declaration

```csharp
public static Period MIN5 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### MONTH1

Predefined period

##### Declaration

```csharp
public static Period MONTH1 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### PeriodMultiplier

Gets period multiplier

##### Declaration

```csharp
public readonly int PeriodMultiplier { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### SECOND1

Predefined period

##### Declaration

```csharp
public static Period SECOND1 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### SECOND10

Predefined period

##### Declaration

```csharp
public static Period SECOND10 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### SECOND15

Predefined period

##### Declaration

```csharp
public static Period SECOND15 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### SECOND30

Predefined period

##### Declaration

```csharp
public static Period SECOND30 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### SECOND5

Predefined period

##### Declaration

```csharp
public static Period SECOND5 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### TICK1

Predefined period

##### Declaration

```csharp
public static Period TICK1 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### Ticks

Gets ticks value as an result of base period [TicksInBasePeriod(BasePeriod)](TradingPlatform.BusinessLayer.Period.html#TradingPlatform_BusinessLayer_Period_TicksInBasePeriod_TradingPlatform_BusinessLayer_BasePeriod_) multiplicated by [PeriodMultiplier](TradingPlatform.BusinessLayer.Period.html#TradingPlatform_BusinessLayer_Period_PeriodMultiplier)

##### Declaration

```csharp
public long Ticks { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [long](https://learn.microsoft.com/dotnet/api/system.int64) |  |

#### WEEK1

Predefined period

##### Declaration

```csharp
public static Period WEEK1 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

#### YEAR1

Predefined period

##### Declaration

```csharp
public static Period YEAR1 { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) |  |

### Methods

#### BasePeriodToShortString(BasePeriod)

Returns shorted string according to base period type

##### Declaration

```csharp
public static string BasePeriodToShortString(BasePeriod basePeriod)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [BasePeriod](TradingPlatform.BusinessLayer.BasePeriod.html) | basePeriod |  |

##### Returns

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### TicksInBasePeriod(BasePeriod)

Returns value in ticks according to base period type

##### Declaration

```csharp
public static long TicksInBasePeriod(BasePeriod basePeriod)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [BasePeriod](TradingPlatform.BusinessLayer.BasePeriod.html) | basePeriod |  |

##### Returns

| Type | Description |
| --- | --- |
| [long](https://learn.microsoft.com/dotnet/api/system.int64) |  |

#### ToDatesRange(out DateTime, out DateTime)

Converts time gap into dates range

##### Declaration

```csharp
public void ToDatesRange(out DateTime from, out DateTime to)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) | from |  |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) | to |  |

#### TryParse(string, out Period)

Represents mechanism for supporting predefined and custom periods

##### Declaration

```csharp
public static bool TryParse(string value, out Period period)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | value |  |
| [Period](TradingPlatform.BusinessLayer.Period.html) | period |  |

##### Returns

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |