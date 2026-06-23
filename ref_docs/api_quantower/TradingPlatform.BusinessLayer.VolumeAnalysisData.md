# Class VolumeAnalysisData
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.VolumeAnalysisData.html
> Fetched: 2026-04-10

---

# Class VolumeAnalysisData

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class VolumeAnalysisData
```

### Constructors

#### VolumeAnalysisData()

##### Declaration

```csharp
public VolumeAnalysisData()
```

### Properties

#### PriceLevels

Volume info for each price

##### Declaration

```csharp
public Dictionary<double, VolumeAnalysisItem> PriceLevels { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary-2)<[double](https://learn.microsoft.com/dotnet/api/system.double), [VolumeAnalysisItem](TradingPlatform.BusinessLayer.VolumeAnalysisItem.html)> |  |

#### TimeLeft

##### Declaration

```csharp
public DateTime TimeLeft { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

#### Total

Summary calculated Volume info

##### Declaration

```csharp
public VolumeAnalysisItem Total { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [VolumeAnalysisItem](TradingPlatform.BusinessLayer.VolumeAnalysisItem.html) |  |

### Methods

#### Calculate(double, double, AggressorFlag)

##### Declaration

```csharp
public void Calculate(double price, double size, AggressorFlag aggressorFlag)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | price |  |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | size |  |
| [AggressorFlag](TradingPlatform.BusinessLayer.AggressorFlag.html) | aggressorFlag |  |

#### Combine(VolumeAnalysisData)

##### Declaration

```csharp
public void Combine(VolumeAnalysisData data)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [VolumeAnalysisData](TradingPlatform.BusinessLayer.VolumeAnalysisData.html) | data |  |

#### CreateAggregatedSnapshot(double)

##### Declaration

```csharp
public VolumeAnalysisData CreateAggregatedSnapshot(double aggregationStep)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | aggregationStep |  |

##### Returns

| Type | Description |
| --- | --- |
| [VolumeAnalysisData](TradingPlatform.BusinessLayer.VolumeAnalysisData.html) |  |

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

### Events

#### ItemUpdated

Fire in case of price level was added or existing was updated

##### Declaration

```csharp
public event EventHandler<VolumeAnalysisDataEventArgs> ItemUpdated
```

##### Event Type

| Type | Description |
| --- | --- |
| [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler-1)<[VolumeAnalysisDataEventArgs](TradingPlatform.BusinessLayer.VolumeAnalysisDataEventArgs.html)> |  |