# Class ClosePositionRequestParameters
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.ClosePositionRequestParameters.html
> Fetched: 2026-04-10

---

# Class ClosePositionRequestParameters

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class ClosePositionRequestParameters : TradingRequestParameters
```

### Constructors

#### ClosePositionRequestParameters()

##### Declaration

```csharp
public ClosePositionRequestParameters()
```

#### ClosePositionRequestParameters(ClosePositionRequestParameters)

##### Declaration

```csharp
public ClosePositionRequestParameters(ClosePositionRequestParameters origin)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [ClosePositionRequestParameters](TradingPlatform.BusinessLayer.ClosePositionRequestParameters.html) | origin |  |

### Properties

#### CloseQuantity

##### Declaration

```csharp
public double CloseQuantity { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

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

#### Position

##### Declaration

```csharp
public Position Position { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Position](TradingPlatform.BusinessLayer.Position.html) |  |

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