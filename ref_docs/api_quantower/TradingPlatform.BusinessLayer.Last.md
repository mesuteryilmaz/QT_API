# Class Last
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Last.html
> Fetched: 2026-04-10

---

# Class Last

Represent access to trade information.

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class Last : MessageQuote
```

### Properties

#### AggressorFlag

Information about operation side of the trade

##### Declaration

```csharp
public AggressorFlag AggressorFlag { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [AggressorFlag](TradingPlatform.BusinessLayer.AggressorFlag.html) |  |

#### Buyer

Represent access to trade information.

##### Declaration

```csharp
public string Buyer { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### OpenInterest

Represent access to trade information.

##### Declaration

```csharp
public double OpenInterest { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Price

Price at which trade occured

##### Declaration

```csharp
public double Price { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### QuoteAssetVolume

Represent access to trade information.

##### Declaration

```csharp
public double QuoteAssetVolume { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Seller

Represent access to trade information.

##### Declaration

```csharp
public string Seller { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Size

Size of the trade

##### Declaration

```csharp
public double Size { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### TickDirection

Shows the direction of price movement, comparing to previous value.

##### Declaration

```csharp
public TickDirection TickDirection { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [TickDirection](TradingPlatform.BusinessLayer.TickDirection.html) |  |

#### TradeId

Represent access to trade information.

##### Declaration

```csharp
public string TradeId { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |