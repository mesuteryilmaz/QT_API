# Class GetLevel2ItemsParameters
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.GetLevel2ItemsParameters.html
> Fetched: 2026-04-10

---

# Class GetLevel2ItemsParameters

Represent parameters of request for Leve2Item collection

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class GetLevel2ItemsParameters
```

### Properties

#### AggregateMethod

Aggregation method

##### Declaration

```csharp
public AggregateMethod AggregateMethod { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [AggregateMethod](TradingPlatform.BusinessLayer.AggregateMethod.html) |  |

#### CalculateCumulative

Calculate cumulative size

##### Declaration

```csharp
public bool CalculateCumulative { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### CustomTickSize

Use custom tick size

##### Declaration

```csharp
public double CustomTickSize { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### GetMBOItems

Represent parameters of request for Leve2Item collection

##### Declaration

```csharp
public bool GetMBOItems { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### ImplicitOrderBookType

Represent parameters of request for Leve2Item collection

##### Declaration

```csharp
public ImplicitOrderBookType ImplicitOrderBookType { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [ImplicitOrderBookType](TradingPlatform.BusinessLayer.ImplicitOrderBookType.html) |  |

#### LevelsCount

Required amount of level2

##### Declaration

```csharp
public int LevelsCount { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

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

#### operator ==(GetLevel2ItemsParameters, GetLevel2ItemsParameters)

Represent parameters of request for Leve2Item collection

##### Declaration

```csharp
public static bool operator ==(GetLevel2ItemsParameters p1, GetLevel2ItemsParameters p2)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [GetLevel2ItemsParameters](TradingPlatform.BusinessLayer.GetLevel2ItemsParameters.html) | p1 |  |
| [GetLevel2ItemsParameters](TradingPlatform.BusinessLayer.GetLevel2ItemsParameters.html) | p2 |  |

##### Returns

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### operator !=(GetLevel2ItemsParameters, GetLevel2ItemsParameters)

Represent parameters of request for Leve2Item collection

##### Declaration

```csharp
public static bool operator !=(GetLevel2ItemsParameters p1, GetLevel2ItemsParameters p2)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [GetLevel2ItemsParameters](TradingPlatform.BusinessLayer.GetLevel2ItemsParameters.html) | p1 |  |
| [GetLevel2ItemsParameters](TradingPlatform.BusinessLayer.GetLevel2ItemsParameters.html) | p2 |  |

##### Returns

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |