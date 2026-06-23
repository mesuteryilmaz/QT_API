# Interface IChartWindow
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Chart.IChartWindow.html
> Fetched: 2026-04-10

---

# Interface IChartWindow

Access to the particular window from chart panel

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html).[Chart](TradingPlatform.BusinessLayer.Chart.html)

##### Syntax

```csharp
public interface IChartWindow
```

### Properties

#### ClientRectangle

Client rectangle of the chart window

##### Declaration

```csharp
Rectangle ClientRectangle { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Rectangle](https://learn.microsoft.com/dotnet/api/system.drawing.rectangle) |  |

#### CoordinatesConverter

Special object, allows you to convert values from x/y scale to Time/Price and back

##### Declaration

```csharp
IChartWindowCoordinatesConverter CoordinatesConverter { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IChartWindowCoordinatesConverter](TradingPlatform.BusinessLayer.Chart.IChartWindowCoordinatesConverter.html) |  |

#### IsMainWindow

Determines, whether this window is the main window of the chart

##### Declaration

```csharp
bool IsMainWindow { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### WindowNumber

Chart window number

##### Declaration

```csharp
int WindowNumber { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### YScaleFactor

Access to the particular window from chart panel

##### Declaration

```csharp
double YScaleFactor { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |