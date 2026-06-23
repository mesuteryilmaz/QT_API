# Class RequestParameters
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.RequestParameters.html
> Fetched: 2026-04-10

---

# Class RequestParameters

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public abstract class RequestParameters
```

### Constructors

#### RequestParameters()

##### Declaration

```csharp
protected RequestParameters()
```

#### RequestParameters(RequestParameters)

##### Declaration

```csharp
protected RequestParameters(RequestParameters origin)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [RequestParameters](TradingPlatform.BusinessLayer.RequestParameters.html) | origin |  |

### Properties

#### CancellationToken

##### Declaration

```csharp
public CancellationToken CancellationToken { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) |  |

#### RequestId

##### Declaration

```csharp
public long RequestId { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [long](https://learn.microsoft.com/dotnet/api/system.int64) |  |

#### SendingSource

##### Declaration

```csharp
public string SendingSource { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Type

##### Declaration

```csharp
public abstract RequestType Type { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [RequestType](TradingPlatform.BusinessLayer.RequestType.html) |  |

### Methods

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

#### Equals(RequestParameters)

Indicates whether the current object is equal to another object of the same type.

##### Declaration

```csharp
public bool Equals(RequestParameters other)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [RequestParameters](TradingPlatform.BusinessLayer.RequestParameters.html) | other | An object to compare with this object. |

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

#### ToString()

Returns a string that represents the current object.

##### Declaration

```csharp
public override string ToString()
```

##### Returns

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | A string that represents the current object. |

##### Overrides

[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring)