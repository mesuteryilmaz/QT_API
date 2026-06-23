# Class SettingItemString
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.SettingItemString.html
> Fetched: 2026-04-10

---

# Class SettingItemString

Typecasts setting as TextBox item

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public sealed class SettingItemString : SettingItem
```

### Constructors

#### SettingItemString()

Typecasts setting as TextBox item

##### Declaration

```csharp
public SettingItemString()
```

#### SettingItemString(string, string, int)

Typecasts setting as TextBox item

##### Declaration

```csharp
public SettingItemString(string name, string value, int sortIndex = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | name |  |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | value |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | sortIndex |  |

### Properties

#### ApplyOnEachInput

Typecasts setting as TextBox item

##### Declaration

```csharp
public bool ApplyOnEachInput { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### Type

Typecasts setting as TextBox item

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