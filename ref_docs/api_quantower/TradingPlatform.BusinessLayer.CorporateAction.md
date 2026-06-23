# Class CorporateAction
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.CorporateAction.html
> Fetched: 2026-04-10

---

# Class CorporateAction

Represents information about corporate action.

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class CorporateAction : BusinessObject
```

### Constructors

#### CorporateAction(string)

Represents information about corporate action.

##### Declaration

```csharp
public CorporateAction(string connectionId)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | connectionId |  |

### Properties

#### CorporateActionType

Represents information about corporate action.

##### Declaration

```csharp
public CorporateActionType CorporateActionType { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [CorporateActionType](TradingPlatform.BusinessLayer.CorporateActionType.html) |  |

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

#### Details

Represents information about corporate action.

##### Declaration

```csharp
public string Details { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Id

Represents information about corporate action.

##### Declaration

```csharp
public string Id { get; protected set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Symbol

Represents information about corporate action.

##### Declaration

```csharp
public Symbol Symbol { get; protected set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Symbol](TradingPlatform.BusinessLayer.Symbol.html) |  |

### Events

#### Updated

Will be triggered on corporate action updating

##### Declaration

```csharp
public event Action Updated
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action) |  |