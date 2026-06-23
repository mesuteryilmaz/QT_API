# Class SettingItemAction
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.SettingItemAction.html
> Fetched: 2026-04-10

---

# Class SettingItemAction

Typecasts setting as Button item

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public sealed class SettingItemAction : SettingItem
```

### Constructors

#### SettingItemAction()

Typecasts setting as Button item

##### Declaration

```csharp
public SettingItemAction()
```

#### SettingItemAction(string, SettingItemActionDelegate, int)

Typecasts setting as Button item

##### Declaration

```csharp
public SettingItemAction(string name, SettingItemActionDelegate value, int sortIndex = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | name |  |
| [SettingItemActionDelegate](TradingPlatform.BusinessLayer.SettingItemActionDelegate.html) | value |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | sortIndex |  |

#### SettingItemAction(SettingItemAction)

Typecasts setting as Button item

##### Declaration

```csharp
public SettingItemAction(SettingItemAction settingItem)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [SettingItemAction](TradingPlatform.BusinessLayer.SettingItemAction.html) | settingItem |  |

### Properties

#### LabelText

Typecasts setting as Button item

##### Declaration

```csharp
public string LabelText { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Type

Typecasts setting as Button item

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

#### ValueFromXElement(XElement, DeserializationInfo)

Typecasts setting as Button item

##### Declaration

```csharp
protected override void ValueFromXElement(XElement element, DeserializationInfo deserializationInfo)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [XElement](https://learn.microsoft.com/dotnet/api/system.xml.linq.xelement) | element |  |
| [DeserializationInfo](TradingPlatform.BusinessLayer.Serialization.DeserializationInfo.html) | deserializationInfo |  |

##### Overrides

[SettingItem.ValueFromXElement(XElement, DeserializationInfo)](TradingPlatform.BusinessLayer.SettingItem.html#TradingPlatform_BusinessLayer_SettingItem_ValueFromXElement_System_Xml_Linq_XElement_TradingPlatform_BusinessLayer_Serialization_DeserializationInfo_)