# Class SettingItemBoolean
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.SettingItemBoolean.html
> Fetched: 2026-04-10

---

# Class SettingItemBoolean

Typecasts setting as CheckBox item

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public sealed class SettingItemBoolean : SettingItem
```

### Constructors

#### SettingItemBoolean()

Typecasts setting as CheckBox item

##### Declaration

```csharp
public SettingItemBoolean()
```

#### SettingItemBoolean(string, bool, int)

Typecasts setting as CheckBox item

##### Declaration

```csharp
public SettingItemBoolean(string name, bool value, int sortIndex = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | name |  |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | value |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | sortIndex |  |

### Properties

#### Type

Typecasts setting as CheckBox item

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