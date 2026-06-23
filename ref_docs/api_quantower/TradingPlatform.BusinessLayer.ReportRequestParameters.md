# Class ReportRequestParameters
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.ReportRequestParameters.html
> Fetched: 2026-04-10

---

# Class ReportRequestParameters

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class ReportRequestParameters : ProgressRequestParameters<float>
```

### Constructors

#### ReportRequestParameters()

##### Declaration

```csharp
public ReportRequestParameters()
```

#### ReportRequestParameters(ReportRequestParameters)

##### Declaration

```csharp
public ReportRequestParameters(ReportRequestParameters origin)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [ReportRequestParameters](TradingPlatform.BusinessLayer.ReportRequestParameters.html) | origin |  |

### Properties

#### ReportType

##### Declaration

```csharp
public ReportType ReportType { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [ReportType](TradingPlatform.BusinessLayer.ReportType.html) |  |

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