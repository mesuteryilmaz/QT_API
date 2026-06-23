# Class SettingItemIntegerRange
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.SettingItemIntegerRange.html
> Fetched: 2026-04-10

---

# Class SettingItemIntegerRange

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class SettingItemIntegerRange : SettingItemNumberRange<int>
```

### Constructors

#### SettingItemIntegerRange()

##### Declaration

```csharp
public SettingItemIntegerRange()
```

#### SettingItemIntegerRange(string, NumberRange<int>, int)

##### Declaration

```csharp
public SettingItemIntegerRange(string name, NumberRange<int> value, int sortIndex = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | name |  |
| [NumberRange](TradingPlatform.BusinessLayer.Utils.NumberRange-1.html)<[int](https://learn.microsoft.com/dotnet/api/system.int32)> | value |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | sortIndex |  |

### Properties

#### Type

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