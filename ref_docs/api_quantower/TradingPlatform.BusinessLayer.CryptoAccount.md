# Class CryptoAccount
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.CryptoAccount.html
> Fetched: 2026-04-10

---

# Class CryptoAccount

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class CryptoAccount : Account
```

### Properties

#### Balances

##### Declaration

```csharp
public CryptoAssetBalances[] Balances { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [CryptoAssetBalances](TradingPlatform.BusinessLayer.CryptoAssetBalances.html)[] |  |

### Methods

#### CreateInfo()

Creates a business object info with an Account data which can be used for the restoring/serialization process.

##### Declaration

```csharp
public override BusinessObjectInfo CreateInfo()
```

##### Returns

| Type | Description |
| --- | --- |
| [BusinessObjectInfo](TradingPlatform.BusinessLayer.BusinessObjectInfo.html) |  |

##### Overrides

[Account.CreateInfo()](TradingPlatform.BusinessLayer.Account.html#TradingPlatform_BusinessLayer_Account_CreateInfo)

#### TryGetAssetBalances(string, out CryptoAssetBalances)

##### Declaration

```csharp
public bool TryGetAssetBalances(string assetId, out CryptoAssetBalances cryptoAssetBalances)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | assetId |  |
| [CryptoAssetBalances](TradingPlatform.BusinessLayer.CryptoAssetBalances.html) | cryptoAssetBalances |  |

##### Returns

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

### Events

#### BalanceUpdated

##### Declaration

```csharp
public event EventHandler<CryptoAccountEventArgs> BalanceUpdated
```

##### Event Type

| Type | Description |
| --- | --- |
| [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler-1)<[CryptoAccountEventArgs](TradingPlatform.BusinessLayer.CryptoAccountEventArgs.html)> |  |