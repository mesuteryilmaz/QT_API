# Class Account
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Account.html
> Fetched: 2026-04-10

---

# Class Account

Contains all user's account information

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class Account : BusinessObject
```

### Properties

#### AccountCurrency

Gets base currency of account. Account CCY is always equal to the server CCY in AlgoStudio

##### Declaration

```csharp
public Asset AccountCurrency { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Asset](TradingPlatform.BusinessLayer.Asset.html) |  |

#### Balance

Gets current balance of the account.

##### Declaration

```csharp
public double Balance { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### ComplexId

Contains all user's account information

##### Declaration

```csharp
public AccountComplexIdentifier ComplexId { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [AccountComplexIdentifier](TradingPlatform.BusinessLayer.AccountComplexIdentifier.html) |  |

#### Id

Gets account unique code.

##### Declaration

```csharp
public string Id { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Name

Obtaining account name.

##### Declaration

```csharp
public string Name { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### NettingType

Contains all user's account information

##### Declaration

```csharp
public NettingType NettingType { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [NettingType](TradingPlatform.BusinessLayer.NettingType.html) |  |

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

#### CompareTo(Account)

Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.

##### Declaration

```csharp
public int CompareTo(Account other)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Account](TradingPlatform.BusinessLayer.Account.html) | other | An object to compare with this instance. |

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

#### Equals(Account)

Indicates whether the current object is equal to another object of the same type.

##### Declaration

```csharp
public bool Equals(Account other)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Account](TradingPlatform.BusinessLayer.Account.html) | other | An object to compare with this object. |

##### Returns

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | [true](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool) if the current object is equal to the `other` parameter; otherwise, [false](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool). |

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

### Events

#### Updated

Will be triggered on each account information updating

##### Declaration

```csharp
public event Action<Account> Updated
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[Account](TradingPlatform.BusinessLayer.Account.html)> |  |