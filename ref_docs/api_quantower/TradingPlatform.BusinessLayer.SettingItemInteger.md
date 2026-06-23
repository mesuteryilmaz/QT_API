# Class SettingItemInteger
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.SettingItemInteger.html
> Fetched: 2026-04-10

---

# Class SettingItemInteger

Typecasts setting as NumericUpDown item

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public sealed class SettingItemInteger : SettingItemNumber<int>
```

### Constructors

#### SettingItemInteger()

Typecasts setting as NumericUpDown item

##### Declaration

```csharp
public SettingItemInteger()
```

#### SettingItemInteger(string, int, int)

Typecasts setting as NumericUpDown item

##### Declaration

```csharp
public SettingItemInteger(string name, int value, int sortIndex = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | name |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | value |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | sortIndex |  |

### Properties

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