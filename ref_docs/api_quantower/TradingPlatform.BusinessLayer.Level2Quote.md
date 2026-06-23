# Class Level2Quote
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Level2Quote.html
> Fetched: 2026-04-10

---

# Class Level2Quote

Represent access to Level2 quote.

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class Level2Quote : MessageQuote
```

### Properties

#### Broker

Broker identifier that send level2 quote

##### Declaration

```csharp
public string Broker { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Closed

Shows, whether Level2 quote is using only for removing from depth

##### Declaration

```csharp
public bool Closed { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### Id

Unique ID of Level2 quote

##### Declaration

```csharp
public string Id { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### ImpliedSize

specifies the implied quantity associated with the price for the quote. Subtracting this amount from the Size yields the outright quantity for the price level. A value of zero indicates that the implied size is not available/defined or that it is actually zero.

##### Declaration

```csharp
public double ImpliedSize { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### NumberOrders

Number orders of Level2 quote

##### Declaration

```csharp
public int NumberOrders { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### Price

Price of Level2 quote

##### Declaration

```csharp
public double Price { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### PriceType

Price type of Level2 quote: Bid or Ask

##### Declaration

```csharp
public QuotePriceType PriceType { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [QuotePriceType](TradingPlatform.BusinessLayer.Integration.QuotePriceType.html) |  |

#### Priority

Represent access to Level2 quote.

##### Declaration

```csharp
public long Priority { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [long](https://learn.microsoft.com/dotnet/api/system.int64) |  |

#### Size

Size of Level2 quote

##### Declaration

```csharp
public double Size { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |