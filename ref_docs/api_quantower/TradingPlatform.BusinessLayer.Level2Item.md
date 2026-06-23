# Class Level2Item
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Level2Item.html
> Fetched: 2026-04-10

---

# Class Level2Item

Represent access to level2 item.

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class Level2Item
```

### Properties

#### Cumulative

Cumulative size

##### Declaration

```csharp
public double Cumulative { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### CumulativeCount

Cumulative orders count

##### Declaration

```csharp
public double CumulativeCount { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### CumulativeMoney

Cumulative money

##### Declaration

```csharp
public double CumulativeMoney { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### DetailedLevels

Represent access to level2 item.

##### Declaration

```csharp
public Level2Item[] DetailedLevels { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Level2Item](TradingPlatform.BusinessLayer.Level2Item.html)[] |  |

#### Id

Represent access to level2 item.

##### Declaration

```csharp
public string Id { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### ImbalancePercent

Imbalance Percent

##### Declaration

```csharp
public double ImbalancePercent { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### MMID

MMID

##### Declaration

```csharp
public string MMID { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### NumberOrders

Number orders

##### Declaration

```csharp
public int NumberOrders { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### Price

Price

##### Declaration

```csharp
public double Price { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Priority

Represent access to level2 item.

##### Declaration

```csharp
public long Priority { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [long](https://learn.microsoft.com/dotnet/api/system.int64) |  |

#### QuoteTime

Time

##### Declaration

```csharp
public DateTime QuoteTime { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

#### Size

Size

##### Declaration

```csharp
public double Size { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

### Methods

#### ToString()

Returns a string that represents the current object.

##### Declaration

```csharp
public override string ToString()
```

##### Returns

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | A string that represents the current object. |

##### Overrides

[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring)