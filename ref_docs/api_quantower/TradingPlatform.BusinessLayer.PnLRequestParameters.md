# Class PnLRequestParameters
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.PnLRequestParameters.html
> Fetched: 2026-04-10

---

# Class PnLRequestParameters

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class PnLRequestParameters : RequestParameters
```

### Constructors

#### PnLRequestParameters()

##### Declaration

```csharp
public PnLRequestParameters()
```

#### PnLRequestParameters(PnLRequestParameters)

##### Declaration

```csharp
public PnLRequestParameters(PnLRequestParameters original)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [PnLRequestParameters](TradingPlatform.BusinessLayer.PnLRequestParameters.html) | original |  |

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

#### ClosePrice

##### Declaration

```csharp
public double ClosePrice { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### OpenPrice

##### Declaration

```csharp
public double OpenPrice { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### PositionId

##### Declaration

```csharp
public string PositionId { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Quantity

##### Declaration

```csharp
public double Quantity { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Side

##### Declaration

```csharp
public Side Side { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Side](TradingPlatform.BusinessLayer.Side.html) |  |

#### Symbol

##### Declaration

```csharp
public Symbol Symbol { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Symbol](TradingPlatform.BusinessLayer.Symbol.html) |  |

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