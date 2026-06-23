# Class SettingItemGroup
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.SettingItemGroup.html
> Fetched: 2026-04-10

---

# Class SettingItemGroup

Typecasts setting as TabControl item

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public sealed class SettingItemGroup : SettingItemList
```

### Constructors

#### SettingItemGroup()

Typecasts setting as TabControl item

##### Declaration

```csharp
public SettingItemGroup()
```

#### SettingItemGroup(string, IList<SettingItem>, int)

Typecasts setting as TabControl item

##### Declaration

```csharp
public SettingItemGroup(string name, IList<SettingItem> items, int sortIndex = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | name |  |
| [IList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)<[SettingItem](TradingPlatform.BusinessLayer.SettingItem.html)> | items |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | sortIndex |  |

### Fields

#### AllowCreateEmptyGroup

Typecasts setting as TabControl item

##### Declaration

```csharp
public bool AllowCreateEmptyGroup
```

##### Field Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

### Properties

#### FirstActionInfo

Typecasts setting as TabControl item

##### Declaration

```csharp
public GroupActionInfo FirstActionInfo { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [GroupActionInfo](TradingPlatform.BusinessLayer.GroupActionInfo.html) |  |

#### Items

Typecasts setting as TabControl item

##### Declaration

```csharp
protected override List<SettingItem> Items { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)<[SettingItem](TradingPlatform.BusinessLayer.SettingItem.html)> |  |

##### Overrides

[SettingItemList.Items](TradingPlatform.BusinessLayer.SettingItemList.html#TradingPlatform_BusinessLayer_SettingItemList_Items)

#### SecondActionInfo

Typecasts setting as TabControl item

##### Declaration

```csharp
public GroupActionInfo SecondActionInfo { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [GroupActionInfo](TradingPlatform.BusinessLayer.GroupActionInfo.html) |  |

#### Type

Typecasts setting as TabControl item

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

#### AddItem(SettingItem)

Typecasts setting as TabControl item

##### Declaration

```csharp
public void AddItem(SettingItem item)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [SettingItem](TradingPlatform.BusinessLayer.SettingItem.html) | item |  |

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

[SettingItem.GetHashCode()](TradingPlatform.BusinessLayer.SettingItem.html#TradingPlatform_BusinessLayer_SettingItem_GetHashCode)