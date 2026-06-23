# Class OrderRequestParameters
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.OrderRequestParameters.html
> Fetched: 2026-04-10

---

# Class OrderRequestParameters

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public abstract class OrderRequestParameters : TradingRequestParameters
```

### Constructors

#### OrderRequestParameters()

##### Declaration

```csharp
protected OrderRequestParameters()
```

#### OrderRequestParameters(IOrder)

##### Declaration

```csharp
protected OrderRequestParameters(IOrder order)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [IOrder](TradingPlatform.BusinessLayer.IOrder.html) | order |  |

#### OrderRequestParameters(OrderRequestParameters)

##### Declaration

```csharp
protected OrderRequestParameters(OrderRequestParameters origin)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [OrderRequestParameters](TradingPlatform.BusinessLayer.OrderRequestParameters.html) | origin |  |

### Properties

#### Account

##### Declaration

```csharp
public Account Account { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Account](TradingPlatform.BusinessLayer.Account.html) |  |

#### AccountId

##### Declaration

```csharp
public string AccountId { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### AdditionalParameters

##### Declaration

```csharp
public IList<SettingItem> AdditionalParameters { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)<[SettingItem](TradingPlatform.BusinessLayer.SettingItem.html)> |  |

#### Comment

##### Declaration

```csharp
public string Comment { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### ConnectionId

##### Declaration

```csharp
public override string ConnectionId { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

##### Overrides

[TradingRequestParameters.ConnectionId](TradingPlatform.BusinessLayer.TradingRequestParameters.html#TradingPlatform_BusinessLayer_TradingRequestParameters_ConnectionId)

#### ExpirationTime

##### Declaration

```csharp
public DateTime ExpirationTime { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

#### GroupId

##### Declaration

```csharp
public string GroupId { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Message

##### Declaration

```csharp
public override string Message { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

##### Overrides

[TradingRequestParameters.Message](TradingPlatform.BusinessLayer.TradingRequestParameters.html#TradingPlatform_BusinessLayer_TradingRequestParameters_Message)

#### OrderType

##### Declaration

```csharp
public OrderType OrderType { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [OrderType](TradingPlatform.BusinessLayer.OrderType.html) |  |

#### OrderTypeId

##### Declaration

```csharp
public string OrderTypeId { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### PositionId

##### Declaration

```csharp
public string PositionId { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Price

##### Declaration

```csharp
public double Price { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Quantity

##### Declaration

```csharp
public double Quantity { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### QuantityDefinitionSettingName

##### Declaration

```csharp
public string QuantityDefinitionSettingName { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Side

##### Declaration

```csharp
public Side Side { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Side](TradingPlatform.BusinessLayer.Side.html) |  |

#### Slippage

##### Declaration

```csharp
public int Slippage { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### StopLoss

##### Declaration

```csharp
public SlTpHolder StopLoss { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [SlTpHolder](TradingPlatform.BusinessLayer.SlTpHolder.html) |  |

#### StopLossItems

##### Declaration

```csharp
public List<SlTpHolder> StopLossItems { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)<[SlTpHolder](TradingPlatform.BusinessLayer.SlTpHolder.html)> |  |

#### Symbol

##### Declaration

```csharp
public Symbol Symbol { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Symbol](TradingPlatform.BusinessLayer.Symbol.html) |  |

#### SymbolId

##### Declaration

```csharp
public string SymbolId { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### TakeProfit

##### Declaration

```csharp
public SlTpHolder TakeProfit { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [SlTpHolder](TradingPlatform.BusinessLayer.SlTpHolder.html) |  |

#### TakeProfitItems

##### Declaration

```csharp
public List<SlTpHolder> TakeProfitItems { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)<[SlTpHolder](TradingPlatform.BusinessLayer.SlTpHolder.html)> |  |

#### TimeInForce

##### Declaration

```csharp
public TimeInForce TimeInForce { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [TimeInForce](TradingPlatform.BusinessLayer.TimeInForce.html) |  |

#### Total

##### Declaration

```csharp
public double Total { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### TrailOffset

##### Declaration

```csharp
public double TrailOffset { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### TriggerPrice

##### Declaration

```csharp
public double TriggerPrice { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

### Methods

#### ApplyValuesFrom(OrderRequestParameters)

##### Declaration

```csharp
public void ApplyValuesFrom(OrderRequestParameters other)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [OrderRequestParameters](TradingPlatform.BusinessLayer.OrderRequestParameters.html) | other |  |

#### Clone()

Creates a new object that is a copy of the current instance.

##### Declaration

```csharp
public abstract object Clone()
```

##### Returns

| Type | Description |
| --- | --- |
| [object](https://learn.microsoft.com/dotnet/api/system.object) | A new object that is a copy of this instance. |

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

[RequestParameters.Equals(object)](TradingPlatform.BusinessLayer.RequestParameters.html#TradingPlatform_BusinessLayer_RequestParameters_Equals_System_Object_)

#### Equals(OrderRequestParameters)

Indicates whether the current object is equal to another object of the same type.

##### Declaration

```csharp
public bool Equals(OrderRequestParameters other)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [OrderRequestParameters](TradingPlatform.BusinessLayer.OrderRequestParameters.html) | other | An object to compare with this object. |

##### Returns

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | [true](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool) if the current object is equal to the `other` parameter; otherwise, [false](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool). |

#### FromXElement(XElement, DeserializationInfo)

##### Declaration

```csharp
public void FromXElement(XElement element, DeserializationInfo deserializationInfo)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [XElement](https://learn.microsoft.com/dotnet/api/system.xml.linq.xelement) | element |  |
| [DeserializationInfo](TradingPlatform.BusinessLayer.Serialization.DeserializationInfo.html) | deserializationInfo |  |

#### GetAccount()

##### Declaration

```csharp
protected override Account GetAccount()
```

##### Returns

| Type | Description |
| --- | --- |
| [Account](TradingPlatform.BusinessLayer.Account.html) |  |

##### Overrides

[TradingRequestParameters.GetAccount()](TradingPlatform.BusinessLayer.TradingRequestParameters.html#TradingPlatform_BusinessLayer_TradingRequestParameters_GetAccount)

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

[RequestParameters.GetHashCode()](TradingPlatform.BusinessLayer.RequestParameters.html#TradingPlatform_BusinessLayer_RequestParameters_GetHashCode)

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

[RequestParameters.ToString()](TradingPlatform.BusinessLayer.RequestParameters.html#TradingPlatform_BusinessLayer_RequestParameters_ToString)

#### ToXElement()

##### Declaration

```csharp
public XElement ToXElement()
```

##### Returns

| Type | Description |
| --- | --- |
| [XElement](https://learn.microsoft.com/dotnet/api/system.xml.linq.xelement) |  |

#### UpdateFrom(OrderRequestParameters)

##### Declaration

```csharp
public void UpdateFrom(OrderRequestParameters origin)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [OrderRequestParameters](TradingPlatform.BusinessLayer.OrderRequestParameters.html) | origin |  |