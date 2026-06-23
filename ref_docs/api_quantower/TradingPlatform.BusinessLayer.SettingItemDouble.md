# Class SettingItemDouble
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.SettingItemDouble.html
> Fetched: 2026-04-10

---

# Class SettingItemDouble

Typecasts setting as NumericUpDown item

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class SettingItemDouble : SettingItemNumber<double>
```

### Constructors

#### SettingItemDouble()

Typecasts setting as NumericUpDown item

##### Declaration

```csharp
public SettingItemDouble()
```

#### SettingItemDouble(string, double, int)

Typecasts setting as NumericUpDown item

##### Declaration

```csharp
public SettingItemDouble(string name, double value, int sortIndex = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | name |  |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | value |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | sortIndex |  |

### Properties

#### DecimalPlaces

Typecasts setting as NumericUpDown item

##### Declaration

```csharp
[Bindable("decimalPlaces")]
public int DecimalPlaces { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### Type

Typecasts setting as NumericUpDown item

##### Declaration

```csharp
public override SettingItemType Type { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [SettingItemType](TradingPlatform.BusinessLayer.SettingItemType.html) |  |

##### Overrides

[SettingItem.Type](TradingPlatform.BusinessLayer.SettingItem.html#TradingPlatform_BusinessLayer_SettingItem_Type)

### Methods

#### Equals(SettingItem)

Indicates whether the current object is equal to another object of the same type.

##### Declaration

```csharp
public override bool Equals(SettingItem other)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [SettingItem](TradingPlatform.BusinessLayer.SettingItem.html) | other | An object to compare with this object. |

##### Returns

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | [true](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool) if the current object is equal to the `other` parameter; otherwise, [false](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool). |

##### Overrides

[SettingItemNumber<double>.Equals(SettingItem)](TradingPlatform.BusinessLayer.SettingItemNumber-1.html#TradingPlatform_BusinessLayer_SettingItemNumber_1_Equals_TradingPlatform_BusinessLayer_SettingItem_)

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

[SettingItemNumber<double>.GetHashCode()](TradingPlatform.BusinessLayer.SettingItemNumber-1.html#TradingPlatform_BusinessLayer_SettingItemNumber_1_GetHashCode)