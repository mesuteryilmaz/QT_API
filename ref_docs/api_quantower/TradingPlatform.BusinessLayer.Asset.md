# Class Asset
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Asset.html
> Fetched: 2026-04-10

---

# Class Asset

Defines asset entity

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class Asset : BusinessObject
```

### Properties

#### Description

Asset description

##### Declaration

```csharp
public string Description { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Id

Asset id bearer

##### Declaration

```csharp
public string Id { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### IsoCode

Gets asset ISO 4217 code

##### Declaration

```csharp
public string IsoCode { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### MinimumChange

Defines a number precision of the change value

##### Declaration

```csharp
public double MinimumChange { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Name

Asset name bearer

##### Declaration

```csharp
public string Name { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Precision

Gets precision value

##### Declaration

```csharp
public int Precision { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

### Methods

#### FormatPrice(double)

Formats price into precision normalized string

##### Declaration

```csharp
public string FormatPrice(double price)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | price |  |

##### Returns

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### FormatPriceWithCurrency(double, bool)

Formats price into concatenated string which contains the precision normalized value and Asset's name (optional)

##### Declaration

```csharp
public string FormatPriceWithCurrency(double price, bool withAssetName = true)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | price |  |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | withAssetName |  |

##### Returns

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### FormatWithCurrency(double)

Defines asset entity

##### Declaration

```csharp
public string FormatWithCurrency(double value)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | value |  |

##### Returns

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |