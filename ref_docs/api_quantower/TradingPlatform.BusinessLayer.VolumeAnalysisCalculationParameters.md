# Class VolumeAnalysisCalculationParameters
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.VolumeAnalysisCalculationParameters.html
> Fetched: 2026-04-10

---

# Class VolumeAnalysisCalculationParameters

Provides VA calculation parameters

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class VolumeAnalysisCalculationParameters
```

### Constructors

#### VolumeAnalysisCalculationParameters()

Provides VA calculation parameters

##### Declaration

```csharp
public VolumeAnalysisCalculationParameters()
```

### Properties

#### CalculatePriceLevels

Provides VA calculation parameters

##### Declaration

```csharp
public bool CalculatePriceLevels { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### CumulativeDeltaReset

Provides VA calculation parameters

##### Declaration

```csharp
public CumulativeDeltaReset CumulativeDeltaReset { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [CumulativeDeltaReset](TradingPlatform.BusinessLayer.CumulativeDeltaReset.html) |  |

#### CustomStep

Provides VA calculation parameters

##### Declaration

```csharp
public int CustomStep { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### CustomTickSize

Provides VA calculation parameters

##### Declaration

```csharp
public double CustomTickSize { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### DeltaCalculationType

Provides VA calculation parameters

##### Declaration

```csharp
public DeltaCalculationType DeltaCalculationType { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DeltaCalculationType](TradingPlatform.BusinessLayer.DeltaCalculationType.html) |  |

#### FilteredVolume

Provides VA calculation parameters

##### Declaration

```csharp
public double FilteredVolume { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### ForceReload

Provides VA calculation parameters

##### Declaration

```csharp
public bool ForceReload { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### ForceUsingTickData

Provides VA calculation parameters

##### Declaration

```csharp
public bool ForceUsingTickData { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### SessionsContainer

Provides VA calculation parameters

##### Declaration

```csharp
public ISessionsContainer SessionsContainer { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [ISessionsContainer](TradingPlatform.BusinessLayer.ISessionsContainer.html) |  |

#### TimeZone

Provides VA calculation parameters

##### Declaration

```csharp
public TimeZone TimeZone { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [TimeZone](TradingPlatform.BusinessLayer.TimeZone.html) |  |