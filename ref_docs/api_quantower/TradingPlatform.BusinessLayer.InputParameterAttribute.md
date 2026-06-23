# Class InputParameterAttribute
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.InputParameterAttribute.html
> Fetched: 2026-04-10

---

# Class InputParameterAttribute

Use this attribute to mark input parameters of your script. You will see them in the settings screen on adding

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class InputParameterAttribute : Attribute
```

### Constructors

#### InputParameterAttribute(string, int, double, double, double, int, object[])

Use this attribute to mark input parameters of your script. You will see them in the settings screen on adding

##### Declaration

```csharp
public InputParameterAttribute(string name = "", int sortIndex = 0, double minimum = -2147483648, double maximum = 2147483647, double increment = 0.01, int decimalPlaces = 2, object[] variants = null)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | name |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | sortIndex |  |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | minimum |  |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | maximum |  |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | increment |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | decimalPlaces |  |
| [object](https://learn.microsoft.com/dotnet/api/system.object)[] | variants |  |

### Properties

#### DecimalPlaces

Decimal palces for numeric input parameters

##### Declaration

```csharp
public int DecimalPlaces { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### Increment

Increment value for numeric input parameters

##### Declaration

```csharp
public double Increment { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Maximum

Maximal value for numeric input parameters

##### Declaration

```csharp
public double Maximum { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Minimum

Minimal value for numeric input parameters

##### Declaration

```csharp
public double Minimum { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Name

Displayed name of input parameter

##### Declaration

```csharp
public string Name { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### SortIndex

Sort index for input paramter

##### Declaration

```csharp
public int SortIndex { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### Variants

List of predefined values

##### Declaration

```csharp
public IComparable[] Variants { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IComparable](https://learn.microsoft.com/dotnet/api/system.icomparable)[] |  |