# Class Order
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Order.html
> Fetched: 2026-04-10

---

# Class Order

Represents trading information about pending order

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class Order : TradingObject
```

### Properties

#### AverageFillPrice

Represents trading information about pending order

##### Declaration

```csharp
public double AverageFillPrice { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### ExpirationTime

Gets orders expiration time

##### Declaration

```csharp
public DateTime ExpirationTime { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

#### FilledQuantity

Filled quantity of the order

##### Declaration

```csharp
public double FilledQuantity { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### GroupId

The ID of the order group. This group created when trades done by the MAM account.

##### Declaration

```csharp
public string GroupId { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### LastUpdateTime

Gets orders last update time

##### Declaration

```csharp
public DateTime LastUpdateTime { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

#### OrderType

Gets OrderType

##### Declaration

```csharp
public OrderType OrderType { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [OrderType](TradingPlatform.BusinessLayer.OrderType.html) |  |

#### OrderTypeId

Orders Type Id. It is used for the orders type comparing.

##### Declaration

```csharp
public string OrderTypeId { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### OriginalStatus

Gets open order original status

##### Declaration

```csharp
public string OriginalStatus { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### PositionId

Gets Position Id.

##### Declaration

```csharp
public string PositionId { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Price

Gets order price value

##### Declaration

```csharp
public double Price { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### RemainingQuantity

Remaining quantity of the order

##### Declaration

```csharp
public double RemainingQuantity { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Status

Gets orders current status

##### Declaration

```csharp
public OrderStatus Status { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [OrderStatus](TradingPlatform.BusinessLayer.OrderStatus.html) |  |

#### StopLoss

Gets StopLoss holder for given order

##### Declaration

```csharp
public SlTpHolder StopLoss { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [SlTpHolder](TradingPlatform.BusinessLayer.SlTpHolder.html) |  |

#### StopLossItems

Represents trading information about pending order

##### Declaration

```csharp
public SlTpHolder[] StopLossItems { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [SlTpHolder](TradingPlatform.BusinessLayer.SlTpHolder.html)[] |  |

#### TakeProfit

Gets TakeProfit holder for given order

##### Declaration

```csharp
public SlTpHolder TakeProfit { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [SlTpHolder](TradingPlatform.BusinessLayer.SlTpHolder.html) |  |

#### TakeProfitItems

Represents trading information about pending order

##### Declaration

```csharp
public SlTpHolder[] TakeProfitItems { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [SlTpHolder](TradingPlatform.BusinessLayer.SlTpHolder.html)[] |  |

#### TimeInForce

Gets order TIF(Time-In-Force) type

##### Declaration

```csharp
public TimeInForce TimeInForce { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [TimeInForce](TradingPlatform.BusinessLayer.TimeInForce.html) |  |

#### TotalQuantity

Total quantity of the order

##### Declaration

```csharp
public double TotalQuantity { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### TrailOffset

Gets order trailing offset value

##### Declaration

```csharp
public double TrailOffset { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### TriggerPrice

Gets order trigger price value

##### Declaration

```csharp
public double TriggerPrice { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

### Methods

#### BuildMessage()

Represents trading information about pending order

##### Declaration

```csharp
public MessageOpenOrder BuildMessage()
```

##### Returns

| Type | Description |
| --- | --- |
| [MessageOpenOrder](TradingPlatform.BusinessLayer.Integration.MessageOpenOrder.html) |  |

#### Cancel(string)

Cancels pending order

##### Declaration

```csharp
public TradingOperationResult Cancel(string sendingSource = null)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | sendingSource |  |

##### Returns

| Type | Description |
| --- | --- |
| [TradingOperationResult](TradingPlatform.BusinessLayer.TradingOperationResult.html) |  |

#### Equals(object)

Determines whether the specified object is equal to the current object.

##### Declaration

```csharp
public override bool Equals(object obj)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [object](https://learn.microsoft.com/dotnet/api/system.object) | obj | The object to compare with the current object. |

##### Returns

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | [true](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool) if the specified object is equal to the current object; otherwise, [false](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool). |

##### Overrides

[object.Equals(object)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals(system-object))

#### Equals(Order)

Indicates whether the current object is equal to another object of the same type.

##### Declaration

```csharp
public bool Equals(Order other)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Order](TradingPlatform.BusinessLayer.Order.html) | other | An object to compare with this object. |

##### Returns

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | [true](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool) if the current object is equal to the `other` parameter; otherwise, [false](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool). |

#### GetHashCode()

Serves as the default hash function.

##### Declaration

```csharp
public override int GetHashCode()
```

##### Returns

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | A hash code for the current object. |

##### Overrides

[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

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

Will be triggered on each [UpdateByMessage(MessageOpenOrder)](TradingPlatform.BusinessLayer.Order.html#TradingPlatform_BusinessLayer_Order_UpdateByMessage_TradingPlatform_BusinessLayer_Integration_MessageOpenOrder_) invocation

##### Declaration

```csharp
public event Action<IOrder> Updated
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[IOrder](TradingPlatform.BusinessLayer.IOrder.html)> |  |