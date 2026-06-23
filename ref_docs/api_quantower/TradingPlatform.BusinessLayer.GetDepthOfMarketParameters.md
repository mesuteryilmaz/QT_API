# Class GetDepthOfMarketParameters
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.GetDepthOfMarketParameters.html
> Fetched: 2026-04-10

---

# Class GetDepthOfMarketParameters

Represent parameters of DepthOfMarket

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class GetDepthOfMarketParameters
```

### Constructors

#### GetDepthOfMarketParameters()

Represent parameters of DepthOfMarket

##### Declaration

```csharp
public GetDepthOfMarketParameters()
```

### Properties

#### CalculateImbalancePercent

Represent parameters of DepthOfMarket

##### Declaration

```csharp
public bool CalculateImbalancePercent { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### GetLevel2ItemsParameters

Represent parameters of DepthOfMarket

##### Declaration

```csharp
public GetLevel2ItemsParameters GetLevel2ItemsParameters { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [GetLevel2ItemsParameters](TradingPlatform.BusinessLayer.GetLevel2ItemsParameters.html) |  |

### Methods

#### Equals(object)

Determines whether the specified object is equal to the current object.

##### Declaration

```csharp
public override bool Equals(object obj)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [object](https://learn.microsoft.com/dotnet/api/system.object) | obj | The object to compare with the current object. |

##### Returns

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | [true](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool) if the specified object is equal to the current object; otherwise, [false](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool). |

##### Overrides

[object.Equals(object)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals(system-object))

#### GetHashCode()

Serves as the default hash function.

##### Declaration

```csharp
public override int GetHashCode()
```

##### Returns

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | A hash code for the current object. |

##### Overrides

[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

### Operators

#### operator ==(GetDepthOfMarketParameters, GetDepthOfMarketParameters)

Represent parameters of DepthOfMarket

##### Declaration

```csharp
public static bool operator ==(GetDepthOfMarketParameters p1, GetDepthOfMarketParameters p2)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [GetDepthOfMarketParameters](TradingPlatform.BusinessLayer.GetDepthOfMarketParameters.html) | p1 |  |
| [GetDepthOfMarketParameters](TradingPlatform.BusinessLayer.GetDepthOfMarketParameters.html) | p2 |  |

##### Returns

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### operator !=(GetDepthOfMarketParameters, GetDepthOfMarketParameters)

Represent parameters of DepthOfMarket

##### Declaration

```csharp
public static bool operator !=(GetDepthOfMarketParameters p1, GetDepthOfMarketParameters p2)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [GetDepthOfMarketParameters](TradingPlatform.BusinessLayer.GetDepthOfMarketParameters.html) | p1 |  |
| [GetDepthOfMarketParameters](TradingPlatform.BusinessLayer.GetDepthOfMarketParameters.html) | p2 |  |

##### Returns

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |