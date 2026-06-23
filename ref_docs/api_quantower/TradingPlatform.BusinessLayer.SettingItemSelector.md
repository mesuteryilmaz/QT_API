# Class SettingItemSelector
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.SettingItemSelector.html
> Fetched: 2026-04-10

---

# Class SettingItemSelector

Typecasts setting as ComboBox item

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public sealed class SettingItemSelector : SettingItem
```

### Constructors

#### SettingItemSelector()

Typecasts setting as ComboBox item

##### Declaration

```csharp
public SettingItemSelector()
```

#### SettingItemSelector(string, string, IEnumerable<string>, int)

Typecasts setting as ComboBox item

##### Declaration

```csharp
public SettingItemSelector(string name, string value, IEnumerable<string> items, int sortIndex = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | name |  |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | value |  |
| [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)<[string](https://learn.microsoft.com/dotnet/api/system.string)> | items |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | sortIndex |  |

### Properties

#### Items

Typecasts setting as ComboBox item

##### Declaration

```csharp
[Bindable("items")]
public IEnumerable<string> Items { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)<[string](https://learn.microsoft.com/dotnet/api/system.string)> |  |

#### Type

Typecasts setting as ComboBox item

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