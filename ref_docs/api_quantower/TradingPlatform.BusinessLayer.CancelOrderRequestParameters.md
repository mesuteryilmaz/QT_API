# Class CancelOrderRequestParameters
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.CancelOrderRequestParameters.html
> Fetched: 2026-04-10

---

# Class CancelOrderRequestParameters

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public sealed class CancelOrderRequestParameters : TradingRequestParameters
```

### Constructors

#### CancelOrderRequestParameters()

##### Declaration

```csharp
public CancelOrderRequestParameters()
```

#### CancelOrderRequestParameters(CancelOrderRequestParameters)

##### Declaration

```csharp
public CancelOrderRequestParameters(CancelOrderRequestParameters original)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [CancelOrderRequestParameters](TradingPlatform.BusinessLayer.CancelOrderRequestParameters.html) | original |  |

### Properties

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

#### Order

##### Declaration

```csharp
public IOrder Order { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IOrder](TradingPlatform.BusinessLayer.IOrder.html) |  |

#### OrderId

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