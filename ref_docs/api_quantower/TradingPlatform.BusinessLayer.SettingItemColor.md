# Class SettingItemColor
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.SettingItemColor.html
> Fetched: 2026-04-10

---

# Class SettingItemColor

Typecasts setting as Color item

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public sealed class SettingItemColor : SettingItem
```

### Constructors

#### SettingItemColor()

Typecasts setting as Color item

##### Declaration

```csharp
public SettingItemColor()
```

#### SettingItemColor(string, Color, int)

Typecasts setting as Color item

##### Declaration

```csharp
public SettingItemColor(string name, Color value, int sortIndex = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | name |  |
| [Color](https://learn.microsoft.com/dotnet/api/system.drawing.color) | value |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | sortIndex |  |

### Properties

#### AllowDisableColor

Typecasts setting as Color item

##### Declaration

```csharp
public bool AllowDisableColor { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### Checked

Typecasts setting as Color item

##### Declaration

```csharp
public bool Checked { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### ColorText

Typecasts setting as Color item

##### Declaration

```csharp
public string ColorText { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Type

Typecasts setting as Color item

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

#### WithCheckBox

Typecasts setting as Color item

##### Declaration

```csharp
public bool WithCheckBox { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |