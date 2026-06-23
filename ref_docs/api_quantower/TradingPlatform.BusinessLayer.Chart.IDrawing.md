# Interface IDrawing
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Chart.IDrawing.html
> Fetched: 2026-04-10

---

# Interface IDrawing

Access to the chart drawing

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html).[Chart](TradingPlatform.BusinessLayer.Chart.html)

##### Syntax

```csharp
public interface IDrawing
```

### Properties

#### Availability

Determines, the availability of drawing - only current chart or all charts with same symbol

##### Declaration

```csharp
DrawingAvailability Availability { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DrawingAvailability](TradingPlatform.BusinessLayer.Chart.DrawingAvailability.html) |  |

#### CreationMode

Determines, the way how chart drawing was created: manually or programmatically

##### Declaration

```csharp
DrawingCreationMode CreationMode { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DrawingCreationMode](TradingPlatform.BusinessLayer.Chart.DrawingCreationMode.html) |  |

#### Id

The unique ID of the chart drawing

##### Declaration

```csharp
string Id { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### MoveToBackground

Determines, whether chart drawing draws above or below the main chart

##### Declaration

```csharp
bool MoveToBackground { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### State

Determines, state of the chart drawing: Locked or Unlocked

##### Declaration

```csharp
DrawingState State { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DrawingState](TradingPlatform.BusinessLayer.Chart.DrawingState.html) |  |

#### Type

Access to the chart drawing

##### Declaration

```csharp
DrawingType Type { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DrawingType](TradingPlatform.BusinessLayer.Chart.DrawingType.html) |  |

### Methods

#### GetPoint(int)

Get time and price of the particular point of the chart drawing

##### Declaration

```csharp
(DateTime, double) GetPoint(int pointIndex)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | pointIndex |  |

##### Returns

| Type | Description |
| --- | --- |
| ([DateTime](https://learn.microsoft.com/dotnet/api/system.datetime), [double](https://learn.microsoft.com/dotnet/api/system.double)) |  |

#### SetPoint(int, DateTime, double)

Set time and price value for particular point of the chart drawing

##### Declaration

```csharp
void SetPoint(int pointIndex, DateTime time, double price)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | pointIndex |  |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) | time |  |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | price |  |