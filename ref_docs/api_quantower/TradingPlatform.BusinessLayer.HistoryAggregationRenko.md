# Class HistoryAggregationRenko
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.HistoryAggregationRenko.html
> Fetched: 2026-04-10

---

# Class HistoryAggregationRenko

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public sealed class HistoryAggregationRenko : HistoryAggregationTime, IHistoryAggregationHistoryTypeSupport
```

### Constructors

#### HistoryAggregationRenko(Period, HistoryType, int, RenkoStyle, int, int, bool, bool)

##### Declaration

```csharp
public HistoryAggregationRenko(Period period, HistoryType historyType, int brickSize, RenkoStyle renkoStyle, int extension = 100, int inversion = 100, bool showWicks = false, bool buildCurrentBar = true)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) | period |  |
| [HistoryType](TradingPlatform.BusinessLayer.HistoryType.html) | historyType |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | brickSize |  |
| [RenkoStyle](TradingPlatform.BusinessLayer.RenkoStyle.html) | renkoStyle |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | extension |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | inversion |  |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | showWicks |  |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | buildCurrentBar |  |

### Fields

#### SETTINGS\_AGGREGATION\_RENKO\_BRICK\_SIZE

##### Declaration

```csharp
public const string SETTINGS_AGGREGATION_RENKO_BRICK_SIZE = "Brick size"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### SETTINGS\_AGGREGATION\_RENKO\_BUILD\_CURRENT\_BAR

##### Declaration

```csharp
public const string SETTINGS_AGGREGATION_RENKO_BUILD_CURRENT_BAR = "Build current bar"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### SETTINGS\_AGGREGATION\_RENKO\_EXTENSION

##### Declaration

```csharp
public const string SETTINGS_AGGREGATION_RENKO_EXTENSION = "Extension, %"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### SETTINGS\_AGGREGATION\_RENKO\_INVERSION

##### Declaration

```csharp
public const string SETTINGS_AGGREGATION_RENKO_INVERSION = "Inversion, %"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### SETTINGS\_AGGREGATION\_RENKO\_SHOW\_WICKS

##### Declaration

```csharp
public const string SETTINGS_AGGREGATION_RENKO_SHOW_WICKS = "Show wicks"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### SETTINGS\_AGGREGATION\_RENKO\_STYLE

##### Declaration

```csharp
public const string SETTINGS_AGGREGATION_RENKO_STYLE = "Style"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

### Properties

#### BrickSize

##### Declaration

```csharp
public int BrickSize { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### BuildCurrentBar

##### Declaration

```csharp
public bool BuildCurrentBar { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### Extension

##### Declaration

```csharp
public int Extension { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### Inversion

##### Declaration

```csharp
public int Inversion { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### Name

##### Declaration

```csharp
public override string Name { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

##### Overrides

[HistoryAggregationTime.Name](TradingPlatform.BusinessLayer.HistoryAggregationTime.html#TradingPlatform_BusinessLayer_HistoryAggregationTime_Name)

#### RenkoStyle

##### Declaration

```csharp
public RenkoStyle RenkoStyle { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [RenkoStyle](TradingPlatform.BusinessLayer.RenkoStyle.html) |  |

#### Settings

##### Declaration

```csharp
public override IList<SettingItem> Settings { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)<[SettingItem](TradingPlatform.BusinessLayer.SettingItem.html)> |  |

##### Overrides

[HistoryAggregationTime.Settings](TradingPlatform.BusinessLayer.HistoryAggregationTime.html#TradingPlatform_BusinessLayer_HistoryAggregationTime_Settings)

#### ShowWicks

##### Declaration

```csharp
public bool ShowWicks { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

### Methods

#### GetAggregationToDirectDownload(HistoryMetadata, ISessionsContainer, bool)

##### Declaration

```csharp
public override HistoryAggregation GetAggregationToDirectDownload(HistoryMetadata metadata, ISessionsContainer sessionsContainer, bool isResetOnSessionBoundaryEnabled)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [HistoryMetadata](TradingPlatform.BusinessLayer.Integration.HistoryMetadata.html) | metadata |  |
| [ISessionsContainer](TradingPlatform.BusinessLayer.ISessionsContainer.html) | sessionsContainer |  |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | isResetOnSessionBoundaryEnabled |  |

##### Returns

| Type | Description |
| --- | --- |
| [HistoryAggregation](TradingPlatform.BusinessLayer.HistoryAggregation.html) |  |

##### Overrides

[HistoryAggregationTime.GetAggregationToDirectDownload(HistoryMetadata, ISessionsContainer, bool)](TradingPlatform.BusinessLayer.HistoryAggregationTime.html#TradingPlatform_BusinessLayer_HistoryAggregationTime_GetAggregationToDirectDownload_TradingPlatform_BusinessLayer_Integration_HistoryMetadata_TradingPlatform_BusinessLayer_ISessionsContainer_System_Boolean_)

#### GetBaseAggregation()

##### Declaration

```csharp
public HistoryAggregation GetBaseAggregation()
```

##### Returns

| Type | Description |
| --- | --- |
| [HistoryAggregation](TradingPlatform.BusinessLayer.HistoryAggregation.html) |  |

### Implements

[IHistoryAggregationHistoryTypeSupport](TradingPlatform.BusinessLayer.IHistoryAggregationHistoryTypeSupport.html)