# Class HistoryItemLast
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.HistoryItemLast.html
> Fetched: 2026-04-10

---

# Class HistoryItemLast

Represents historical data trade item

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public sealed class HistoryItemLast : HistoryItem
```

### Constructors

#### HistoryItemLast()

Creates HistoryItemLast instance

##### Declaration

```csharp
public HistoryItemLast()
```

#### HistoryItemLast(Last)

Represents historical data trade item

##### Declaration

```csharp
public HistoryItemLast(Last last)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Last](TradingPlatform.BusinessLayer.Last.html) | last |  |

### Properties

#### AggressorFlag

Defines trade operation side as aggressor flag

##### Declaration

```csharp
public AggressorFlag AggressorFlag { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [AggressorFlag](TradingPlatform.BusinessLayer.AggressorFlag.html) |  |

#### Buyer

Represents historical data trade item

##### Declaration

```csharp
public string Buyer { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### FundingRate

Represents historical data trade item

##### Declaration

```csharp
public double FundingRate { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### this[PriceType]

Gets price by indexing [PriceType](TradingPlatform.BusinessLayer.PriceType.html)

##### Declaration

```csharp
public override double this[PriceType priceType] { get; }
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType |  |

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

##### Overrides

[HistoryItem.this[PriceType]](TradingPlatform.BusinessLayer.PriceType.html)

#### OpenInterest

Represents historical data trade item

##### Declaration

```csharp
public double OpenInterest { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Price

Defines price value

##### Declaration

```csharp
public double Price { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### QuoteAssetVolume

Represents historical data trade item

##### Declaration

```csharp
public double QuoteAssetVolume { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Seller

Represents historical data trade item

##### Declaration

```csharp
public string Seller { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### TickDirection

Represents historical data trade item

##### Declaration

```csharp
public TickDirection TickDirection { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [TickDirection](TradingPlatform.BusinessLayer.TickDirection.html) |  |

#### Volume

Defines volume value

##### Declaration

```csharp
public double Volume { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |