# Class ModifyOrderRequestParameters
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.ModifyOrderRequestParameters.html
> Fetched: 2026-04-10

---

# Class ModifyOrderRequestParameters

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class ModifyOrderRequestParameters : OrderRequestParameters
```

### Constructors

#### ModifyOrderRequestParameters(IOrder)

##### Declaration

```csharp
public ModifyOrderRequestParameters(IOrder order)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [IOrder](TradingPlatform.BusinessLayer.IOrder.html) | order |  |

#### ModifyOrderRequestParameters(ModifyOrderRequestParameters)

##### Declaration

```csharp
public ModifyOrderRequestParameters(ModifyOrderRequestParameters original)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [ModifyOrderRequestParameters](TradingPlatform.BusinessLayer.ModifyOrderRequestParameters.html) | original |  |

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

#### OrderId

Id of the order

##### Declaration

```csharp
public string OrderId { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

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

[OrderRequestParameters.ToString()](TradingPlatform.BusinessLayer.OrderRequestParameters.html#TradingPlatform_BusinessLayer_OrderRequestParameters_ToString)