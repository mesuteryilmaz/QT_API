# Class SettingItemPeriod
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.SettingItemPeriod.html
> Fetched: 2026-04-10

---

# Class SettingItemPeriod

Typecasts setting as Period item

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public sealed class SettingItemPeriod : SettingItem
```

### Constructors

#### SettingItemPeriod()

Typecasts setting as Period item

##### Declaration

```csharp
public SettingItemPeriod()
```

#### SettingItemPeriod(string, Period, int)

Typecasts setting as Period item

##### Declaration

```csharp
public SettingItemPeriod(string name, Period value, int sortIndex = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | name |  |
| [Period](TradingPlatform.BusinessLayer.Period.html) | value |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | sortIndex |  |

### Properties

#### ExcludedPeriods

Typecasts setting as Period item

##### Declaration

```csharp
public BasePeriod[] ExcludedPeriods { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [BasePeriod](TradingPlatform.BusinessLayer.BasePeriod.html)[] |  |

#### MultiplierMaximum

Typecasts setting as Period item

##### Declaration

```csharp
public int MultiplierMaximum { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### MultiplierMinimum

Typecasts setting as Period item

##### Declaration

```csharp
public int MultiplierMinimum { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### Type

Typecasts setting as Period item

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