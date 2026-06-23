# Class ConnectionInfo
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.ConnectionInfo.html
> Fetched: 2026-04-10

---

# Class ConnectionInfo

Represents all needed parameters for the connection constructing process.

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public sealed class ConnectionInfo
```

### Properties

#### AllowCreateCustomConnections

Represents all needed parameters for the connection constructing process.

##### Declaration

```csharp
public bool AllowCreateCustomConnections { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### ConnectionId

Gets connection Id

##### Declaration

```csharp
public string ConnectionId { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### ConnectionLogoPath

Represents all needed parameters for the connection constructing process.

##### Declaration

```csharp
public string ConnectionLogoPath { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### ConnectionState

Gets ConnectionState

##### Declaration

```csharp
public ConnectionState ConnectionState { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [ConnectionState](TradingPlatform.BusinessLayer.ConnectionState.html) |  |

#### Copyrights

Represents all needed parameters for the connection constructing process.

##### Declaration

```csharp
public string Copyrights { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### CreationType

Specifies how connection was created: by default or by user

##### Declaration

```csharp
public ConnectionCreationType CreationType { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [ConnectionCreationType](TradingPlatform.BusinessLayer.ConnectionCreationType.html) |  |

#### Group

Gets connection group

##### Declaration

```csharp
public string Group { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### IsFavourite

Favorites one will be displayed in Control center toolbar

##### Declaration

```csharp
public bool IsFavourite { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### Links

Represents all needed parameters for the connection constructing process.

##### Declaration

```csharp
public List<ConnectionInfoLink> Links { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)<[ConnectionInfoLink](TradingPlatform.BusinessLayer.ConnectionInfoLink.html)> |  |

#### Name

Gets a user friendly name of the connection

##### Declaration

```csharp
public string Name { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Settings

[ICustomizable](TradingPlatform.BusinessLayer.ICustomizable.html) realization

##### Declaration

```csharp
public IList<SettingItem> Settings { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)<[SettingItem](TradingPlatform.BusinessLayer.SettingItem.html)> |  |

#### SyncMsgProcessing

Represents all needed parameters for the connection constructing process.

##### Declaration

```csharp
public bool SyncMsgProcessing { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### VendorInfo

Represents all needed parameters for the connection constructing process.

##### Declaration

```csharp
public VendorInfo VendorInfo { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [VendorInfo](TradingPlatform.BusinessLayer.Integration.VendorInfo.html) |  |

#### VendorName

Gets vendor's name

##### Declaration

```csharp
public string VendorName { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### VendorSettings

Gets vendor's settings

##### Declaration

```csharp
public IList<SettingItem> VendorSettings { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)<[SettingItem](TradingPlatform.BusinessLayer.SettingItem.html)> |  |

### Methods

#### CompareTo(object)

Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.

##### Declaration

```csharp
public int CompareTo(object obj)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [object](https://learn.microsoft.com/dotnet/api/system.object) | obj | An object to compare with this instance. |

##### Returns

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | A value that indicates the relative order of the objects being compared. The return value has these meanings:   | Value | Meaning | | --- | --- | | Less than zero | This instance precedes `obj` in the sort order. | | Zero | This instance occurs in the same position in the sort order as `obj`. | | Greater than zero | This instance follows `obj` in the sort order. | |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception) | `obj` is not the same type as this instance. |

#### CompareTo(ConnectionInfo)

Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.

##### Declaration

```csharp
public int CompareTo(ConnectionInfo other)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [ConnectionInfo](TradingPlatform.BusinessLayer.ConnectionInfo.html) | other | An object to compare with this instance. |

##### Returns

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | A value that indicates the relative order of the objects being compared. The return value has these meanings:   | Value | Meaning | | --- | --- | | Less than zero | This instance precedes `other` in the sort order. | | Zero | This instance occurs in the same position in the sort order as `other`. | | Greater than zero | This instance follows `other` in the sort order. | |

#### Equals(object)

Determines whether the specified object is equal to the current object.

##### Declaration

```csharp
public override bool Equals(object obj)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [object](https://learn.microsoft.com/dotnet/api/system.object) | obj | The object to compare with the current object. |

##### Returns

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | [true](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool) if the specified object is equal to the current object; otherwise, [false](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool). |

##### Overrides

[object.Equals(object)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals(system-object))

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

[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)