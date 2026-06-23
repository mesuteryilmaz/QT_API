# Class Trade
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Trade.html
> Fetched: 2026-04-10

---

# Class Trade

Represents information about trade.

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class Trade : TradingObject
```

### Constructors

#### Trade(string)

Represents information about trade.

##### Declaration

```csharp
public Trade(string connectionId)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | connectionId |  |

### Properties

#### DateTime

Get the date and time when trade was executed

##### Declaration

```csharp
public DateTime DateTime { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

#### Fee

Get the fee value that was charged for this trade

##### Declaration

```csharp
public PnLItem Fee { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [PnLItem](TradingPlatform.BusinessLayer.PnLItem.html) |  |

#### GrossPnl

Get the trade Gross P&L

##### Declaration

```csharp
public PnLItem GrossPnl { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [PnLItem](TradingPlatform.BusinessLayer.PnLItem.html) |  |

#### NetPnl

Get the trade Net P&L

##### Declaration

```csharp
public PnLItem NetPnl { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [PnLItem](TradingPlatform.BusinessLayer.PnLItem.html) |  |

#### OrderId

Gets the unique identifier of the order initiating the trade.

##### Declaration

```csharp
public string OrderId { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### OrderTypeId

Get the trade order type

##### Declaration

```csharp
public string OrderTypeId { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### PositionId

Gets a unique identifier of the position, which is related to this trade.

##### Declaration

```csharp
public string PositionId { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### PositionImpactType

Represents information about trade.

##### Declaration

```csharp
public PositionImpactType PositionImpactType { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [PositionImpactType](TradingPlatform.BusinessLayer.PositionImpactType.html) |  |

#### Price

Get the price where trade was executed

##### Declaration

```csharp
public double Price { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Quantity

Get the trade quantity

##### Declaration

```csharp
public double Quantity { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

### Methods

#### BuildMessage()

Represents information about trade.

##### Declaration

```csharp
public MessageTrade BuildMessage()
```

##### Returns

| Type | Description |
| --- | --- |
| [MessageTrade](TradingPlatform.BusinessLayer.Integration.MessageTrade.html) |  |

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

### Events

#### Updated

Will be triggered on trade updating

##### Declaration

```csharp
public event Action Updated
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action) |  |