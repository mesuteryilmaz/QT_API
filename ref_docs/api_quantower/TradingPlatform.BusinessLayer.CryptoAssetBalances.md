# Class CryptoAssetBalances
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.CryptoAssetBalances.html
> Fetched: 2026-04-10

---

# Class CryptoAssetBalances

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class CryptoAssetBalances : BusinessObject
```

### Properties

#### Asset

##### Declaration

```csharp
public Asset Asset { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Asset](TradingPlatform.BusinessLayer.Asset.html) |  |

#### AssetId

##### Declaration

```csharp
public string AssetId { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### AvailableBalance

##### Declaration

```csharp
public double AvailableBalance { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### AvailableBalanceHandler

##### Declaration

```csharp
public GetAvailableBalanceHandler AvailableBalanceHandler { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [GetAvailableBalanceHandler](TradingPlatform.BusinessLayer.Integration.GetAvailableBalanceHandler.html) |  |

#### Debt

##### Declaration

```csharp
public double Debt { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Equity

##### Declaration

```csharp
public double Equity { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### EquityInBTC

##### Declaration

```csharp
public double EquityInBTC { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### LastUpdateTime

##### Declaration

```csharp
public DateTime LastUpdateTime { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

#### ReservedBalance

##### Declaration

```csharp
public double ReservedBalance { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### TotalBalance

##### Declaration

```csharp
public double TotalBalance { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### TotalInBTC

##### Declaration

```csharp
public double TotalInBTC { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### TotalInUSD

##### Declaration

```csharp
public double TotalInUSD { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

### Methods

#### BuildMessage()

##### Declaration

```csharp
public MessageCryptoAssetBalances BuildMessage()
```

##### Returns

| Type | Description |
| --- | --- |
| [MessageCryptoAssetBalances](TradingPlatform.BusinessLayer.Integration.MessageCryptoAssetBalances.html) |  |

#### GetAvailableBalance(OrderRequestParameters)

##### Declaration

```csharp
public double GetAvailableBalance(OrderRequestParameters requestParameters)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [OrderRequestParameters](TradingPlatform.BusinessLayer.OrderRequestParameters.html) | requestParameters |  |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

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