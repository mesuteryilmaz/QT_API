# Class Position
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Position.html
> Fetched: 2026-04-10

---

# Class Position

Represents trading information about related position

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class Position : TradingObject
```

### Properties

#### CurrentPrice

The market price obtainable from your broker.

##### Declaration

```csharp
public double CurrentPrice { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Fee

Gets fee amount for the position.

##### Declaration

```csharp
public PnLItem Fee { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [PnLItem](TradingPlatform.BusinessLayer.PnLItem.html) |  |

#### GrossPnL

Gets Profit/loss (without swaps or commissions) all calculated based on the current broker's price. For open position it shows the profit/loss you would make if you close the position at the current price. If position closed, this parameter show profit/loss what trader have after closing this position.

##### Declaration

```csharp
public PnLItem GrossPnL { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [PnLItem](TradingPlatform.BusinessLayer.PnLItem.html) |  |

#### GrossPnLTicks

Returns ticks amount between open and current price.

##### Declaration

```csharp
public double GrossPnLTicks { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### LiquidationPrice

Represents trading information about related position

##### Declaration

```csharp
public double LiquidationPrice { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### NetPnL

Gets Profit/loss calculated based on the current broker's price. For open position it shows the profit/loss you would make if you close the position at the current price. If position closed, this parameter show profit/loss what trader have after closing this position.

##### Declaration

```csharp
public PnLItem NetPnL { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [PnLItem](TradingPlatform.BusinessLayer.PnLItem.html) |  |

#### OpenPrice

Gets position open order price

##### Declaration

```csharp
public double OpenPrice { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### OpenTime

Gets position openning time

##### Declaration

```csharp
public DateTime OpenTime { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

#### Quantity

Gets position quantity value

##### Declaration

```csharp
public double Quantity { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### StopLoss

Gets StopLoss order which belongs to the position

##### Declaration

```csharp
public Order StopLoss { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Order](TradingPlatform.BusinessLayer.Order.html) |  |

#### Swaps

Gets PnL swaps

##### Declaration

```csharp
public PnLItem Swaps { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [PnLItem](TradingPlatform.BusinessLayer.PnLItem.html) |  |

#### TakeProfit

Gets TakeProfit order which belongs to the position

##### Declaration

```csharp
public Order TakeProfit { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Order](TradingPlatform.BusinessLayer.Order.html) |  |

### Methods

#### BuildMessage()

Represents trading information about related position

##### Declaration

```csharp
public MessageOpenPosition BuildMessage()
```

##### Returns

| Type | Description |
| --- | --- |
| [MessageOpenPosition](TradingPlatform.BusinessLayer.Integration.MessageOpenPosition.html) |  |

#### Close(double)

Closes position if quantity is not specified else - uses partial closing operation.

##### Declaration

```csharp
public virtual TradingOperationResult Close(double closeQuantity = -1)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | closeQuantity |  |

##### Returns

| Type | Description |
| --- | --- |
| [TradingOperationResult](TradingPlatform.BusinessLayer.TradingOperationResult.html) |  |

#### ForceRecalculatePnl()

Represents trading information about related position

##### Declaration

```csharp
public void ForceRecalculatePnl()
```

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

Will be triggered on each [UpdateByMessage(MessageOpenPosition)](TradingPlatform.BusinessLayer.Position.html#TradingPlatform_BusinessLayer_Position_UpdateByMessage_TradingPlatform_BusinessLayer_Integration_MessageOpenPosition_) and [UpdatePnl(PnL)](TradingPlatform.BusinessLayer.Position.html#TradingPlatform_BusinessLayer_Position_UpdatePnl_TradingPlatform_BusinessLayer_PnL_) invocation

##### Declaration

```csharp
public event Action<Position> Updated
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[Position](TradingPlatform.BusinessLayer.Position.html)> |  |