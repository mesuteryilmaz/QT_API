# Class GetSymbolRequestParameters
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.GetSymbolRequestParameters.html
> Fetched: 2026-04-10

---

# Class GetSymbolRequestParameters

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class GetSymbolRequestParameters : CachedRequestParameters
```

### Constructors

#### GetSymbolRequestParameters()

##### Declaration

```csharp
public GetSymbolRequestParameters()
```

#### GetSymbolRequestParameters(GetSymbolRequestParameters)

##### Declaration

```csharp
public GetSymbolRequestParameters(GetSymbolRequestParameters origin)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [GetSymbolRequestParameters](TradingPlatform.BusinessLayer.GetSymbolRequestParameters.html) | origin |  |

### Properties

#### SymbolId

##### Declaration

```csharp
public string SymbolId { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Type

##### Declaration

```csharp
public override RequestType Type { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [RequestType](TradingPlatform.BusinessLayer.RequestType.html) |  |

##### Overrides

[RequestParameters.Type](TradingPlatform.BusinessLayer.RequestParameters.html#TradingPlatform_BusinessLayer_RequestParameters_Type)

### Methods

#### GetCacheKey()

##### Declaration

```csharp
public override int GetCacheKey()
```

##### Returns

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

##### Overrides

[CachedRequestParameters.GetCacheKey()](TradingPlatform.BusinessLayer.CachedRequestParameters.html#TradingPlatform_BusinessLayer_CachedRequestParameters_GetCacheKey)