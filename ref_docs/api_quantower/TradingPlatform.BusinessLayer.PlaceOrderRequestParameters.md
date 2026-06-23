# Class PlaceOrderRequestParameters
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.PlaceOrderRequestParameters.html
> Fetched: 2026-04-10

---

# Class PlaceOrderRequestParameters

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class PlaceOrderRequestParameters : OrderRequestParameters
```

### Constructors

#### PlaceOrderRequestParameters()

##### Declaration

```csharp
public PlaceOrderRequestParameters()
```

#### PlaceOrderRequestParameters(IOrder)

##### Declaration

```csharp
public PlaceOrderRequestParameters(IOrder order)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [IOrder](TradingPlatform.BusinessLayer.IOrder.html) | order |  |

#### PlaceOrderRequestParameters(OrderRequestParameters)

##### Declaration

```csharp
public PlaceOrderRequestParameters(OrderRequestParameters original)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [OrderRequestParameters](TradingPlatform.BusinessLayer.OrderRequestParameters.html) | original |  |

### Properties

#### Event

##### Declaration

```csharp
public override string Event { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

##### Overrides

[TradingRequestParameters.Event](TradingPlatform.BusinessLayer.TradingRequestParameters.html#TradingPlatform_BusinessLayer_TradingRequestParameters_Event)

#### Type

##### Declaration

```csharp
public override RequestType Type { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [RequestType](TradingPlatform.BusinessLayer.RequestType.html) |  |

##### Overrides

[RequestParameters.Type](TradingPlatform.BusinessLayer.RequestParameters.html#TradingPlatform_BusinessLayer_RequestParameters_Type)

### Methods

#### Clone()

Creates a new object that is a copy of the current instance.

##### Declaration

```csharp
public override object Clone()
```

##### Returns

| Type | Description |
| --- | --- |
| [object](https://learn.microsoft.com/dotnet/api/system.object) | A new object that is a copy of this instance. |

##### Overrides

[OrderRequestParameters.Clone()](TradingPlatform.BusinessLayer.OrderRequestParameters.html#TradingPlatform_BusinessLayer_OrderRequestParameters_Clone)