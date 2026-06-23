# Class Exchange
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Exchange.html
> Fetched: 2026-04-10

---

# Class Exchange

Contains all information which belong to the given exchange

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public sealed class Exchange : BusinessObject
```

### Properties

#### ComplexId

Contains all information which belong to the given exchange

##### Declaration

```csharp
public ExchangeComplexIdentifier ComplexId { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [ExchangeComplexIdentifier](TradingPlatform.BusinessLayer.ExchangeComplexIdentifier.html) |  |

#### ExchangeName

Gets Exchange name

##### Declaration

```csharp
public string ExchangeName { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Id

Gets Exchange Id

##### Declaration

```csharp
public string Id { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### SortIndex

Used for the Exchanges comparing

##### Declaration

```csharp
public int SortIndex { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

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