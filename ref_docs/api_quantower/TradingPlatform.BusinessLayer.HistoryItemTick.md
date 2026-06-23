# Class HistoryItemTick
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.HistoryItemTick.html
> Fetched: 2026-04-10

---

# Class HistoryItemTick

Represents historical data tick item

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class HistoryItemTick : HistoryItem
```

### Constructors

#### HistoryItemTick()

Creates HistoryItemBar instance with default Ask/AskSize/Bid/BidSize = [DOUBLE\_UNDEFINED](TradingPlatform.BusinessLayer.Utils.Const.html#TradingPlatform_BusinessLayer_Utils_Const_DOUBLE_UNDEFINED)

##### Declaration

```csharp
public HistoryItemTick()
```

### Properties

#### Ask

Defines Ask price

##### Declaration

```csharp
public double Ask { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### AskSize

Defines Ask size

##### Declaration

```csharp
public double AskSize { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### AskTickDirection

Represents historical data tick item

##### Declaration

```csharp
public TickDirection AskTickDirection { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [TickDirection](TradingPlatform.BusinessLayer.TickDirection.html) |  |

#### Bid

Defines Bid price

##### Declaration

```csharp
public double Bid { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### BidSize

Defines Bid size

##### Declaration

```csharp
public double BidSize { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### BidTickDirection

Represents historical data tick item

##### Declaration

```csharp
public TickDirection BidTickDirection { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [TickDirection](TradingPlatform.BusinessLayer.TickDirection.html) |  |

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