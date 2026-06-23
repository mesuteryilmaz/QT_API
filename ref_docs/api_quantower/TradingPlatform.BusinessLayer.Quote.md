# Class Quote
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Quote.html
> Fetched: 2026-04-10

---

# Class Quote

Represent access to quote information.

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class Quote : MessageQuote
```

### Properties

#### Ask

Ask price

##### Declaration

```csharp
public double Ask { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### AskSize

Ask size

##### Declaration

```csharp
public double AskSize { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### AskTickDirection

Shows the direction of ask price movement, comparing to previous value.

##### Declaration

```csharp
public TickDirection AskTickDirection { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [TickDirection](TradingPlatform.BusinessLayer.TickDirection.html) |  |

#### Bid

Bid price

##### Declaration

```csharp
public double Bid { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### BidSize

Bid size

##### Declaration

```csharp
public double BidSize { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### BidTickDirection

Shows the direction of bid price movement, comparing to previous value.

##### Declaration

```csharp
public TickDirection BidTickDirection { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [TickDirection](TradingPlatform.BusinessLayer.TickDirection.html) |  |