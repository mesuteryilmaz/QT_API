# Class HistoryItemBar
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.HistoryItemBar.html
> Fetched: 2026-04-10

---

# Class HistoryItemBar

Represents historical data bar item

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class HistoryItemBar : HistoryItem
```

### Constructors

#### HistoryItemBar()

Creates HistoryItemBar instance with default OHLC price = [DOUBLE\_UNDEFINED](TradingPlatform.BusinessLayer.Utils.Const.html#TradingPlatform_BusinessLayer_Utils_Const_DOUBLE_UNDEFINED)

##### Declaration

```csharp
public HistoryItemBar()
```

### Properties

#### Close

Defines Close price

##### Declaration

```csharp
public double Close { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### FundingRate

Represents historical data bar item

##### Declaration

```csharp
public double FundingRate { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### High

Defines High price

##### Declaration

```csharp
public double High { get; set; }
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

#### Low

Defines Low price

##### Declaration

```csharp
public double Low { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Median

Gets Median (High+Low)/2 price

##### Declaration

```csharp
public double Median { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Open

Defines Open price

##### Declaration

```csharp
public double Open { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### OpenInterest

Represents historical data bar item

##### Declaration

```csharp
public double OpenInterest { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### QuoteAssetVolume

Represents historical data bar item

##### Declaration

```csharp
public double QuoteAssetVolume { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Ticks

Defines ticks amount

##### Declaration

```csharp
public long Ticks { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [long](https://learn.microsoft.com/dotnet/api/system.int64) |  |

#### TicksRight

Defines bar's ticks count

##### Declaration

```csharp
public override long TicksRight { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [long](https://learn.microsoft.com/dotnet/api/system.int64) |  |

##### Overrides

[HistoryItem.TicksRight](TradingPlatform.BusinessLayer.HistoryItem.html#TradingPlatform_BusinessLayer_HistoryItem_TicksRight)

#### TimeRight

Gets bar's right time border

##### Declaration

```csharp
public DateTime TimeRight { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

#### Typical

Gets Typical (High+Low+Close)/3 price

##### Declaration

```csharp
public double Typical { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

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

#### Weighted

Gets Weighted (High+Low+Close+Close)/4 price

##### Declaration

```csharp
public double Weighted { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |