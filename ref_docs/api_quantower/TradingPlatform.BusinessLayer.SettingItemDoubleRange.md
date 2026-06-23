# Class SettingItemDoubleRange
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.SettingItemDoubleRange.html
> Fetched: 2026-04-10

---

# Class SettingItemDoubleRange

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class SettingItemDoubleRange : SettingItemNumberRange<double>
```

### Constructors

#### SettingItemDoubleRange()

##### Declaration

```csharp
public SettingItemDoubleRange()
```

#### SettingItemDoubleRange(string, NumberRange<double>, int)

##### Declaration

```csharp
public SettingItemDoubleRange(string name, NumberRange<double> value, int sortIndex = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | name |  |
| [NumberRange](TradingPlatform.BusinessLayer.Utils.NumberRange-1.html)<[double](https://learn.microsoft.com/dotnet/api/system.double)> | value |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | sortIndex |  |

### Properties

#### DecimalPlaces

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