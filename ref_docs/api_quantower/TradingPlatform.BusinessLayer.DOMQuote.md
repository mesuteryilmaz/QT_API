# Class DOMQuote
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.DOMQuote.html
> Fetched: 2026-04-10

---

# Class DOMQuote

Represent access to DOM2 quote, which contains Bids and Asks.

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class DOMQuote : MessageQuote
```

### Properties

#### Asks

Collection of Asks quotes

##### Declaration

```csharp
public List<Level2Quote> Asks { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)<[Level2Quote](TradingPlatform.BusinessLayer.Level2Quote.html)> |  |

#### Bids

Collection of Bids quotes

##### Declaration

```csharp
public List<Level2Quote> Bids { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)<[Level2Quote](TradingPlatform.BusinessLayer.Level2Quote.html)> |  |