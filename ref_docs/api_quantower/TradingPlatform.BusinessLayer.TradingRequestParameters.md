# Class TradingRequestParameters
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.TradingRequestParameters.html
> Fetched: 2026-04-10

---

# Class TradingRequestParameters

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public abstract class TradingRequestParameters : RequestParameters
```

### Constructors

#### TradingRequestParameters()

##### Declaration

```csharp
public TradingRequestParameters()
```

#### TradingRequestParameters(TradingRequestParameters)

##### Declaration

```csharp
public TradingRequestParameters(TradingRequestParameters origin)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [TradingRequestParameters](TradingPlatform.BusinessLayer.TradingRequestParameters.html) | origin |  |

### Properties

#### ConnectionId

##### Declaration

```csharp
public abstract string ConnectionId { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Event

##### Declaration

```csharp
public abstract string Event { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Message

##### Declaration

```csharp
public abstract string Message { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### ParentOperation

##### Declaration

```csharp
public GroupTradingOperation ParentOperation { get; init; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [GroupTradingOperation](TradingPlatform.BusinessLayer.GroupTradingOperation.html) |  |

### Methods

#### GetAccount()

##### Declaration

```csharp
protected abstract Account GetAccount()
```

##### Returns

| Type | Description |
| --- | --- |
| [Account](TradingPlatform.BusinessLayer.Account.html) |  |