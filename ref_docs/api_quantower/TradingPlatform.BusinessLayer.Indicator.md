# Class Indicator
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Indicator.html
> Fetched: 2026-04-10

---

# Class Indicator

Base class for all indicators.

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public abstract class Indicator : ExecutionEntity
```

### Constructors

#### Indicator()

Base class for all indicators.

##### Declaration

```csharp
protected Indicator()
```

### Properties

#### AllowFitAuto

Specified, whether indicator should participate into price auto scale system.

##### Declaration

```csharp
public bool AllowFitAuto { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### Count

Amount of items in internal buffers

##### Declaration

```csharp
public int Count { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### CurrentChart

Represent access to the chart, that created indicator

##### Declaration

```csharp
public IChart CurrentChart { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IChart](TradingPlatform.BusinessLayer.Chart.IChart.html) |  |

#### Digits

Precision amount for formatting price (the count of digits after decimal point); By default = -1, which means to use precision from indicator's symbol

##### Declaration

```csharp
public int Digits { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### HelpLink

Base class for all indicators.

##### Declaration

```csharp
public virtual string HelpLink { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### HistoricalData

Represent access to current used historical data.

##### Declaration

```csharp
public HistoricalData HistoricalData { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [HistoricalData](TradingPlatform.BusinessLayer.HistoricalData.html) |  |

#### IsUpdateTypesSupported

Base class for all indicators.

##### Declaration

```csharp
protected bool IsUpdateTypesSupported { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### Labels

Represent access indicator labels

##### Declaration

```csharp
public AdditionalInfoItemBasic[] Labels { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [AdditionalInfoItemBasic](TradingPlatform.BusinessLayer.AdditionalInfoItemBasic.html)[] |  |

#### LinesLevels

Base class for all indicators.

##### Declaration

```csharp
public LineLevel[] LinesLevels { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [LineLevel](TradingPlatform.BusinessLayer.LineLevel.html)[] |  |

#### LinesSeries

Represent access indicator series

##### Declaration

```csharp
public LineSeries[] LinesSeries { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [LineSeries](TradingPlatform.BusinessLayer.LineSeries.html)[] |  |

#### OnBackGround

Specified, whether indicator should draw on chart background by default.

##### Declaration

```csharp
public bool OnBackGround { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### SeparateWindow

Specified, whether indicator should use main or additional window on the chart

##### Declaration

```csharp
public bool SeparateWindow { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### Settings

Indicator's settings

##### Declaration

```csharp
public override IList<SettingItem> Settings { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)<[SettingItem](TradingPlatform.BusinessLayer.SettingItem.html)> |  |

##### Overrides

[ExecutionEntity.Settings](TradingPlatform.BusinessLayer.ExecutionEntity.html#TradingPlatform_BusinessLayer_ExecutionEntity_Settings)

#### ShortName

Short name of indicator

##### Declaration

```csharp
public virtual string ShortName { get; protected set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### SourceCodeLink

Base class for all indicators.

##### Declaration

```csharp
public virtual string SourceCodeLink { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Symbol

Access to current Symbol of indicator

##### Declaration

```csharp
public Symbol Symbol { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Symbol](TradingPlatform.BusinessLayer.Symbol.html) |  |

#### TFConfig

Base class for all indicators.

##### Declaration

```csharp
public TimeFrameConfig TFConfig { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [TimeFrameConfig](TradingPlatform.BusinessLayer.Utils.TimeFrameConfig.html) |  |

#### UpdateType

Base class for all indicators.

##### Declaration

```csharp
public IndicatorUpdateType UpdateType { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IndicatorUpdateType](TradingPlatform.BusinessLayer.IndicatorUpdateType.html) |  |

### Methods

#### AddIndicator(Indicator)

Base class for all indicators.

##### Declaration

```csharp
public void AddIndicator(Indicator indicator)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) | indicator |  |

#### AddLabel(string, ComparingType, string, IFormattingDescription)

Base class for all indicators.

##### Declaration

```csharp
public AdditionalInfoItemBasic AddLabel(string labelId, ComparingType type, string labelName, IFormattingDescription formattingDescription = null)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | labelId |  |
| [ComparingType](TradingPlatform.BusinessLayer.ComparingType.html) | type |  |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | labelName |  |
| [IFormattingDescription](TradingPlatform.BusinessLayer.Integration.IFormattingDescription.html) | formattingDescription |  |

##### Returns

| Type | Description |
| --- | --- |
| [AdditionalInfoItemBasic](TradingPlatform.BusinessLayer.AdditionalInfoItemBasic.html) |  |

#### AddLabel(AdditionalInfoItemBasic)

Base class for all indicators.

##### Declaration

```csharp
public void AddLabel(AdditionalInfoItemBasic label)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [AdditionalInfoItemBasic](TradingPlatform.BusinessLayer.AdditionalInfoItemBasic.html) | label |  |

#### AddLineLevel(double, string, Color, int, LineStyle)

Base class for all indicators.

##### Declaration

```csharp
public LineLevel AddLineLevel(double level, string lineName = "", Color lineColor = default, int lineWidth = 1, LineStyle lineStyle = LineStyle.Solid)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | level |  |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | lineName |  |
| [Color](https://learn.microsoft.com/dotnet/api/system.drawing.color) | lineColor |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | lineWidth |  |
| [LineStyle](TradingPlatform.BusinessLayer.LineStyle.html) | lineStyle |  |

##### Returns

| Type | Description |
| --- | --- |
| [LineLevel](TradingPlatform.BusinessLayer.LineLevel.html) |  |

#### AddLineLevel(LineLevel)

Base class for all indicators.

##### Declaration

```csharp
public void AddLineLevel(LineLevel lineLevel)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [LineLevel](TradingPlatform.BusinessLayer.LineLevel.html) | lineLevel |  |

#### AddLineSeries(string, Color, int, LineStyle)

Base class for all indicators.

##### Declaration

```csharp
public LineSeries AddLineSeries(string lineName = "", Color lineColor = default, int lineWidth = 1, LineStyle lineStyle = LineStyle.Solid)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | lineName |  |
| [Color](https://learn.microsoft.com/dotnet/api/system.drawing.color) | lineColor |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | lineWidth |  |
| [LineStyle](TradingPlatform.BusinessLayer.LineStyle.html) | lineStyle |  |

##### Returns

| Type | Description |
| --- | --- |
| [LineSeries](TradingPlatform.BusinessLayer.LineSeries.html) |  |

#### AddLineSeries(LineSeries)

Base class for all indicators.

##### Declaration

```csharp
public void AddLineSeries(LineSeries lineSeries)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [LineSeries](TradingPlatform.BusinessLayer.LineSeries.html) | lineSeries |  |

#### Ask(int)

Get Ask price

##### Declaration

```csharp
protected double Ask(int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset | Offset value |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### BeginCloud(int, int, Color, int)

Marks cloud begin between two line series with specific color

##### Declaration

```csharp
protected void BeginCloud(int line1Index, int line2Index, Color color, int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | line1Index | First line series index |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | line2Index | Second line series index |
| [Color](https://learn.microsoft.com/dotnet/api/system.drawing.color) | color | Cloud color |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset | Offset |

#### Bid(int)

Get Bid price

##### Declaration

```csharp
protected double Bid(int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset | Offset value |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Calculate(HistoricalData)

Base class for all indicators.

##### Declaration

```csharp
public void Calculate(HistoricalData historicalData)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [HistoricalData](TradingPlatform.BusinessLayer.HistoricalData.html) | historicalData |  |

#### Clear()

Base class for all indicators.

##### Declaration

```csharp
public void Clear()
```

#### Close(int)

Get Close price

##### Declaration

```csharp
protected double Close(int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset | Offset value |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### EndCloud(int, int, Color, int)

Marks cloud end between two line series with specific color

##### Declaration

```csharp
protected void EndCloud(int line1Index, int line2Index, Color color, int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | line1Index | First line series index |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | line2Index | Second line series index |
| [Color](https://learn.microsoft.com/dotnet/api/system.drawing.color) | color | Cloud color |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset | Offset |

#### FormatPrice(double)

Formatting price, using precision from assigned symbol or Digits value if specified

##### Declaration

```csharp
public string FormatPrice(double price)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | price | Price value |

##### Returns

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### FundingRate(int)

Get Funding rate

##### Declaration

```csharp
protected double FundingRate(int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset | Offset value |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### GetBarAppearance(int)

Base class for all indicators.

##### Declaration

```csharp
public IndicatorBarAppearance GetBarAppearance(int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset |  |

##### Returns

| Type | Description |
| --- | --- |
| [IndicatorBarAppearance](TradingPlatform.BusinessLayer.IndicatorBarAppearance.html) |  |

#### GetLabelValue(string)

Gets the indicator label value by unique Id

##### Declaration

```csharp
public ValueProvider GetLabelValue(string labelId)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | labelId | Unique label Id |

##### Returns

| Type | Description |
| --- | --- |
| [ValueProvider](TradingPlatform.BusinessLayer.ValueProvider.html) | [ValueProvider](TradingPlatform.BusinessLayer.ValueProvider.html) |

#### GetLineBreak(int, int, SeekOriginHistory)

Check if the point is a break point.

##### Declaration

```csharp
public bool GetLineBreak(int offset = 0, int lineIndex = 0, SeekOriginHistory origin = SeekOriginHistory.End)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset | Offset value |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | lineIndex | Index of indicator line |
| [SeekOriginHistory](TradingPlatform.BusinessLayer.SeekOriginHistory.html) | origin | Offset start point |

##### Returns

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### GetPrice(PriceType, int)

Gets the price from historical data

##### Declaration

```csharp
protected double GetPrice(PriceType priceType, int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset |  |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### GetValue(int, int, SeekOriginHistory)

Gets the value of indicator from internal buffer

##### Declaration

```csharp
public double GetValue(int offset = 0, int lineIndex = 0, SeekOriginHistory origin = SeekOriginHistory.End)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset | Offset value |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | lineIndex | Index of indicator line |
| [SeekOriginHistory](TradingPlatform.BusinessLayer.SeekOriginHistory.html) | origin | Offset start point |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### GetVolumeAnalysisData(int)

Base class for all indicators.

##### Declaration

```csharp
protected VolumeAnalysisData GetVolumeAnalysisData(int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset |  |

##### Returns

| Type | Description |
| --- | --- |
| [VolumeAnalysisData](TradingPlatform.BusinessLayer.VolumeAnalysisData.html) |  |

#### High(int)

Get High price

##### Declaration

```csharp
protected double High(int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset | Offset value |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Init()

Base class for all indicators.

##### Declaration

```csharp
public void Init()
```

#### Last(int)

Get Last price

##### Declaration

```csharp
protected double Last(int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset | Offset value |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Low(int)

Get Low price

##### Declaration

```csharp
protected double Low(int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset | Offset value |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Median(int)

Get Median price

##### Declaration

```csharp
protected double Median(int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset | Offset value |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### OnClear()

Base class for all indicators.

##### Declaration

```csharp
protected virtual void OnClear()
```

#### OnInit()

Base class for all indicators.

##### Declaration

```csharp
protected virtual void OnInit()
```

#### OnPaintChart(PaintChartEventArgs)

Base class for all indicators.

##### Declaration

```csharp
public virtual void OnPaintChart(PaintChartEventArgs args)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [PaintChartEventArgs](TradingPlatform.BusinessLayer.PaintChartEventArgs.html) | args |  |

#### OnSettingsUpdated()

Base class for all indicators.

##### Declaration

```csharp
protected override void OnSettingsUpdated()
```

##### Overrides

[ExecutionEntity.OnSettingsUpdated()](TradingPlatform.BusinessLayer.ExecutionEntity.html#TradingPlatform_BusinessLayer_ExecutionEntity_OnSettingsUpdated)

#### OnTryGetMinMax(int, int, out double, out double)

Base class for all indicators.

##### Declaration

```csharp
protected virtual bool OnTryGetMinMax(int fromOffset, int toOffset, out double min, out double max)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | fromOffset |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | toOffset |  |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | min |  |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | max |  |

##### Returns

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### OnUpdate(UpdateArgs)

Base class for all indicators.

##### Declaration

```csharp
protected virtual void OnUpdate(UpdateArgs args)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [UpdateArgs](TradingPlatform.BusinessLayer.UpdateArgs.html) | args |  |

#### Open(int)

Get Open price

##### Declaration

```csharp
protected double Open(int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset | Offset value |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### OpenInterest(int)

Get Open interest

##### Declaration

```csharp
protected double OpenInterest(int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset | Offset value |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### PaintChart(PaintChartEventArgs)

Base class for all indicators.

##### Declaration

```csharp
public void PaintChart(PaintChartEventArgs ev)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [PaintChartEventArgs](TradingPlatform.BusinessLayer.PaintChartEventArgs.html) | ev |  |

#### QuoteAssetVolume(int)

Get Volume in quoting asset

##### Declaration

```csharp
protected double QuoteAssetVolume(int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset | Offset value |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Refresh()

Recalculate indicator

##### Declaration

```csharp
public void Refresh()
```

#### RemoveIndicator(Indicator)

Base class for all indicators.

##### Declaration

```csharp
public void RemoveIndicator(Indicator indicator)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) | indicator |  |

#### RemoveLineBreak(int, int, SeekOriginHistory)

Remove line break point.

##### Declaration

```csharp
public void RemoveLineBreak(int offset = 0, int lineIndex = 0, SeekOriginHistory origin = SeekOriginHistory.End)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset | Offset value |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | lineIndex | Index of indicator line |
| [SeekOriginHistory](TradingPlatform.BusinessLayer.SeekOriginHistory.html) | origin | Offset start point |

#### SetBarAppearance(IndicatorBarAppearance, int)

Base class for all indicators.

##### Declaration

```csharp
public void SetBarAppearance(IndicatorBarAppearance barAppearance, int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [IndicatorBarAppearance](TradingPlatform.BusinessLayer.IndicatorBarAppearance.html) | barAppearance |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset |  |

#### SetBarColor(Color?, int)

Base class for all indicators.

##### Declaration

```csharp
public void SetBarColor(Color? color = null, int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Color](https://learn.microsoft.com/dotnet/api/system.drawing.color)? | color |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset |  |

#### SetLabelValue(string, bool)

Sets the indicator label by unique Id

##### Declaration

```csharp
public void SetLabelValue(string labelId, bool value)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | labelId | Unique label Id |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | value | Value |

#### SetLabelValue(string, DateTime)

Sets the indicator label by unique Id

##### Declaration

```csharp
public void SetLabelValue(string labelId, DateTime value)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | labelId | Unique label Id |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) | value | Value |

#### SetLabelValue(string, double)

Sets the indicator label by unique Id

##### Declaration

```csharp
public void SetLabelValue(string labelId, double value)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | labelId | Unique label Id |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | value | Value |

#### SetLabelValue(string, int)

Sets the indicator label by unique Id

##### Declaration

```csharp
public void SetLabelValue(string labelId, int value)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | labelId | Unique label Id |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | value | Value |

#### SetLabelValue(string, long)

Sets the indicator label by unique Id

##### Declaration

```csharp
public void SetLabelValue(string labelId, long value)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | labelId | Unique label Id |
| [long](https://learn.microsoft.com/dotnet/api/system.int64) | value | Value |

#### SetLabelValue(string, string)

Sets the indicator label by unique Id

##### Declaration

```csharp
public void SetLabelValue(string labelId, string value)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | labelId | Unique label Id |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | value | Value |

#### SetLineBreak(int, int, SeekOriginHistory)

Set line break point.

##### Declaration

```csharp
public void SetLineBreak(int offset = 0, int lineIndex = 0, SeekOriginHistory origin = SeekOriginHistory.End)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset | Offset value |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | lineIndex | Index of indicator line |
| [SeekOriginHistory](TradingPlatform.BusinessLayer.SeekOriginHistory.html) | origin | Offset start point |

#### SetValue(double, int, int)

Sets the value of indicator into internal buffer

##### Declaration

```csharp
public void SetValue(double value, int lineIndex = 0, int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | value | Value |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | lineIndex | Index of indicator line |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset | Offset value |

#### Ticks(int)

Get Ticks

##### Declaration

```csharp
protected double Ticks(int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset | Offset value |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Time(int)

Get Time

##### Declaration

```csharp
protected DateTime Time(int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset | Offset value |

##### Returns

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

#### TryGetMinMax(int, int, out double, out double)

Base class for all indicators.

##### Declaration

```csharp
public bool TryGetMinMax(int fromOffset, int toOffset, out double min, out double max)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | fromOffset |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | toOffset |  |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | min |  |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | max |  |

##### Returns

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### Typical(int)

Get Typical price

##### Declaration

```csharp
protected double Typical(int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset | Offset value |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Update(UpdateArgs)

Base class for all indicators.

##### Declaration

```csharp
public void Update(UpdateArgs args)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [UpdateArgs](TradingPlatform.BusinessLayer.UpdateArgs.html) | args |  |

#### Volume(int)

Get Volume

##### Declaration

```csharp
protected double Volume(int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset | Offset value |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Weighted(int)

Get Weighted price

##### Declaration

```csharp
protected double Weighted(int offset = 0)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset | Offset value |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |