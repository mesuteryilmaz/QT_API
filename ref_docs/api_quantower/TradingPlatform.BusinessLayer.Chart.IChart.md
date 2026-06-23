# Interface IChart
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Chart.IChart.html
> Fetched: 2026-04-10

---

# Interface IChart

Access to the chart panel

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html).[Chart](TradingPlatform.BusinessLayer.Chart.html)

##### Syntax

```csharp
public interface IChart
```

### Properties

#### Account

Provides account of current chart.

##### Declaration

```csharp
Account Account { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Account](TradingPlatform.BusinessLayer.Account.html) |  |

#### BarsWidth

Current X scale value - width of the bar in pixels

##### Declaration

```csharp
int BarsWidth { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### CurrentSessionContainer

Provides custom sessions of current chart.

##### Declaration

```csharp
ISessionsContainer CurrentSessionContainer { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [ISessionsContainer](TradingPlatform.BusinessLayer.ISessionsContainer.html) |  |

#### CurrentTimeZone

Provides time zone of current chart.

##### Declaration

```csharp
TimeZone CurrentTimeZone { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [TimeZone](TradingPlatform.BusinessLayer.TimeZone.html) |  |

#### Drawings

Collection of chart drawingsCollection

##### Declaration

```csharp
IChartDrawingsCollection Drawings { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IChartDrawingsCollection](TradingPlatform.BusinessLayer.Chart.IChartDrawingsCollection.html) |  |

#### ID

Chart panel unique ID

##### Declaration

```csharp
string ID { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### MainWindow

Main window of the chart

##### Declaration

```csharp
IChartWindow MainWindow { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IChartWindow](TradingPlatform.BusinessLayer.Chart.IChartWindow.html) |  |

#### RightOffset

Current right offset value

##### Declaration

```csharp
int RightOffset { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### TickSize

Current tick size of the chart

##### Declaration

```csharp
double TickSize { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Windows

Collection of chart windows

##### Declaration

```csharp
IChartWindow[] Windows { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IChartWindow](TradingPlatform.BusinessLayer.Chart.IChartWindow.html)[] |  |

### Methods

#### RedrawBuffer()

Force chart redraw

##### Declaration

```csharp
void RedrawBuffer()
```

### Events

#### AccountChanged

The AccountChanged event occurs when the account was changed

##### Declaration

```csharp
event EventHandler<ChartEventArgs> AccountChanged
```

##### Event Type

| Type | Description |
| --- | --- |
| [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler-1)<[ChartEventArgs](TradingPlatform.BusinessLayer.Chart.ChartEventArgs.html)> |  |

#### MouseClick

The MouseClick event occurs when the mouse button is clicked

##### Declaration

```csharp
event EventHandler<ChartMouseNativeEventArgs> MouseClick
```

##### Event Type

| Type | Description |
| --- | --- |
| [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler-1)<[ChartMouseNativeEventArgs](TradingPlatform.BusinessLayer.Chart.ChartMouseNativeEventArgs.html)> |  |

#### MouseDown

The MouseDown event occurs when the mouse button is pressed down

##### Declaration

```csharp
event EventHandler<ChartMouseNativeEventArgs> MouseDown
```

##### Event Type

| Type | Description |
| --- | --- |
| [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler-1)<[ChartMouseNativeEventArgs](TradingPlatform.BusinessLayer.Chart.ChartMouseNativeEventArgs.html)> |  |

#### MouseEnter

The MouseDown event occurs when the mouse enter the chart

##### Declaration

```csharp
event EventHandler<ChartMouseNativeEventArgs> MouseEnter
```

##### Event Type

| Type | Description |
| --- | --- |
| [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler-1)<[ChartMouseNativeEventArgs](TradingPlatform.BusinessLayer.Chart.ChartMouseNativeEventArgs.html)> |  |

#### MouseLeave

The MouseDown event occurs when the mouse leave the chart

##### Declaration

```csharp
event EventHandler<ChartMouseNativeEventArgs> MouseLeave
```

##### Event Type

| Type | Description |
| --- | --- |
| [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler-1)<[ChartMouseNativeEventArgs](TradingPlatform.BusinessLayer.Chart.ChartMouseNativeEventArgs.html)> |  |

#### MouseMove

The MouseMove event occurs when the mouse moving over the chart

##### Declaration

```csharp
event EventHandler<ChartMouseNativeEventArgs> MouseMove
```

##### Event Type

| Type | Description |
| --- | --- |
| [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler-1)<[ChartMouseNativeEventArgs](TradingPlatform.BusinessLayer.Chart.ChartMouseNativeEventArgs.html)> |  |

#### MouseUp

The MouseUp event occurs when the mouse button is released

##### Declaration

```csharp
event EventHandler<ChartMouseNativeEventArgs> MouseUp
```

##### Event Type

| Type | Description |
| --- | --- |
| [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler-1)<[ChartMouseNativeEventArgs](TradingPlatform.BusinessLayer.Chart.ChartMouseNativeEventArgs.html)> |  |

#### MouseWheel

The MouseDown event occurs when the user scrolling mouse wheel

##### Declaration

```csharp
event EventHandler<ChartMouseNativeEventArgs> MouseWheel
```

##### Event Type

| Type | Description |
| --- | --- |
| [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler-1)<[ChartMouseNativeEventArgs](TradingPlatform.BusinessLayer.Chart.ChartMouseNativeEventArgs.html)> |  |

#### SettingsChanged

The SettingsChanged event occurs when any settings were changed

##### Declaration

```csharp
event EventHandler<ChartEventArgs> SettingsChanged
```

##### Event Type

| Type | Description |
| --- | --- |
| [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler-1)<[ChartEventArgs](TradingPlatform.BusinessLayer.Chart.ChartEventArgs.html)> |  |