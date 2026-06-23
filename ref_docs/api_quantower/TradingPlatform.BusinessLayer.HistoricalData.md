# Class HistoricalData
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.HistoricalData.html
> Fetched: 2026-04-10

---

# Class HistoricalData

Represent access to historical data information and indicators control.

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class HistoricalData
```

### Constructors

#### HistoricalData(HistoryRequestParameters)

Represent access to historical data information and indicators control.

##### Declaration

```csharp
protected HistoricalData(HistoryRequestParameters historyRequestParameters)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [HistoryRequestParameters](TradingPlatform.BusinessLayer.HistoryRequestParameters.html) | historyRequestParameters |  |

### Fields

#### Indicators

Represent access to historical data information and indicators control.

##### Declaration

```csharp
protected readonly IndicatorsCollection Indicators
```

##### Field Value

| Type | Description |
| --- | --- |
| [IndicatorsCollection](TradingPlatform.BusinessLayer.IndicatorsCollection.html) |  |

#### Parameters

Represent access to historical data information and indicators control.

##### Declaration

```csharp
protected HistoryRequestParameters Parameters
```

##### Field Value

| Type | Description |
| --- | --- |
| [HistoryRequestParameters](TradingPlatform.BusinessLayer.HistoryRequestParameters.html) |  |

#### itemsLocker

Represent access to historical data information and indicators control.

##### Declaration

```csharp
protected readonly object itemsLocker
```

##### Field Value

| Type | Description |
| --- | --- |
| [object](https://learn.microsoft.com/dotnet/api/system.object) |  |

### Properties

#### Aggregation

Gets HistoricalData aggregation

##### Declaration

```csharp
public HistoryAggregation Aggregation { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [HistoryAggregation](TradingPlatform.BusinessLayer.HistoryAggregation.html) |  |

#### AttachedIndicators

Gets array of attached indicators

##### Declaration

```csharp
public Indicator[] AttachedIndicators { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html)[] |  |

#### BuiltInIndicators

Gets access to built-in indicators

##### Declaration

```csharp
public BuiltInIndicators BuiltInIndicators { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [BuiltInIndicators](TradingPlatform.BusinessLayer.BuiltInIndicators.html) |  |

#### Count

Gets HistoricalData items amount

##### Declaration

```csharp
public virtual int Count { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### FromTime

Gets HistoricalData left time boundary

##### Declaration

```csharp
public DateTime FromTime { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

#### IndicatorCalculationBehavior

Represent access to historical data information and indicators control.

##### Declaration

```csharp
public IndicatorCalculationBehavior IndicatorCalculationBehavior { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IndicatorCalculationBehavior](TradingPlatform.BusinessLayer.IndicatorCalculationBehavior.html) |  |

#### this[int, SeekOriginHistory]

Retrieves HistoricalData item by indexing offset and direction to find.

##### Declaration

```csharp
public virtual IHistoryItem this[int offset, SeekOriginHistory origin = SeekOriginHistory.End] { get; }
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | offset |  |
| [SeekOriginHistory](TradingPlatform.BusinessLayer.SeekOriginHistory.html) | origin |  |

##### Property Value

| Type | Description |
| --- | --- |
| [IHistoryItem](TradingPlatform.BusinessLayer.IHistoryItem.html) |  |

#### NeedSubscribe

Represent access to historical data information and indicators control.

##### Declaration

```csharp
protected virtual bool NeedSubscribe { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### Symbol

Gets HistoricalData symbol

##### Declaration

```csharp
public Symbol Symbol { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Symbol](TradingPlatform.BusinessLayer.Symbol.html) |  |

#### ToTime

Gets HistoricalData right time boundary

##### Declaration

```csharp
public DateTime ToTime { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

### Methods

#### AddIndicator(string, params SettingItem[])

Creates indicator by it's name and if it successfully created adds it to the HistoricalData

##### Declaration

```csharp
public Indicator AddIndicator(string indicatorName, params SettingItem[] settings)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | indicatorName |  |
| [SettingItem](TradingPlatform.BusinessLayer.SettingItem.html)[] | settings |  |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

#### AddIndicator(Indicator)

Adds indicator to the HistoricalData

##### Declaration

```csharp
public void AddIndicator(Indicator indicator)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) | indicator |  |

#### AddNewItem(IHistoryItem, bool, HistoryEventArgs)

Represent access to historical data information and indicators control.

##### Declaration

```csharp
protected virtual void AddNewItem(IHistoryItem historyItem, bool updateIndicators = true, HistoryEventArgs e = null)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [IHistoryItem](TradingPlatform.BusinessLayer.IHistoryItem.html) | historyItem |  |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | updateIndicators |  |
| [HistoryEventArgs](TradingPlatform.BusinessLayer.HistoryEventArgs.html) | e |  |

#### CalculateVolumeProfile(VolumeAnalysisCalculationParameters)

Represent access to historical data information and indicators control.

##### Declaration

```csharp
public IVolumeAnalysisCalculationProgress CalculateVolumeProfile(VolumeAnalysisCalculationParameters volumeAnalysisCalculationParameters)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [VolumeAnalysisCalculationParameters](TradingPlatform.BusinessLayer.VolumeAnalysisCalculationParameters.html) | volumeAnalysisCalculationParameters |  |

##### Returns

| Type | Description |
| --- | --- |
| [IVolumeAnalysisCalculationProgress](TradingPlatform.BusinessLayer.IVolumeAnalysisCalculationProgress.html) |  |

#### CreateHistoryProcessor()

Represent access to historical data information and indicators control.

##### Declaration

```csharp
protected virtual IHistoryProcessor CreateHistoryProcessor()
```

##### Returns

| Type | Description |
| --- | --- |
| [IHistoryProcessor](TradingPlatform.BusinessLayer.IHistoryProcessor.html) |  |

#### GetEnumerator()

Returns an enumerator that iterates through a collection.

##### Declaration

```csharp
public IEnumerator GetEnumerator()
```

##### Returns

| Type | Description |
| --- | --- |
| [IEnumerator](https://learn.microsoft.com/dotnet/api/system.collections.ienumerator) | An [IEnumerator](https://learn.microsoft.com/dotnet/api/system.collections.ienumerator) object that can be used to iterate through the collection. |

#### GetIndexByTime(long, SeekOriginHistory)

Gets index by time with counting on search direction

##### Declaration

```csharp
public double GetIndexByTime(long time, SeekOriginHistory origin = SeekOriginHistory.End)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [long](https://learn.microsoft.com/dotnet/api/system.int64) | time |  |
| [SeekOriginHistory](TradingPlatform.BusinessLayer.SeekOriginHistory.html) | origin |  |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### GetTimeToNextBar()

Represent access to historical data information and indicators control.

##### Declaration

```csharp
public string GetTimeToNextBar()
```

##### Returns

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### ProcessLast(Last)

Represent access to historical data information and indicators control.

##### Declaration

```csharp
protected virtual void ProcessLast(Last last)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Last](TradingPlatform.BusinessLayer.Last.html) | last |  |

#### ProcessLevel2Qute(MessageQuote)

Represent access to historical data information and indicators control.

##### Declaration

```csharp
protected virtual void ProcessLevel2Qute(MessageQuote quote)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [MessageQuote](TradingPlatform.BusinessLayer.MessageQuote.html) | quote |  |

#### ProcessMark(Mark)

Represent access to historical data information and indicators control.

##### Declaration

```csharp
protected virtual void ProcessMark(Mark mark)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Mark](TradingPlatform.BusinessLayer.Mark.html) | mark |  |

#### ProcessQuote(Quote)

Represent access to historical data information and indicators control.

##### Declaration

```csharp
protected virtual void ProcessQuote(Quote quote)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Quote](TradingPlatform.BusinessLayer.Quote.html) | quote |  |

#### Reload()

Reloads entire HistoricalData

##### Declaration

```csharp
public void Reload()
```

#### RemoveIndicator(Indicator)

Removes indicator from the HistoricalData

##### Declaration

```csharp
public void RemoveIndicator(Indicator indicator)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) | indicator |  |

#### SubscribeSymbol()

Represent access to historical data information and indicators control.

##### Declaration

```csharp
protected virtual void SubscribeSymbol()
```

#### Symbol\_NewLast(Symbol, Last)

Represent access to historical data information and indicators control.

##### Declaration

```csharp
protected void Symbol_NewLast(Symbol symbol, Last last)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Symbol](TradingPlatform.BusinessLayer.Symbol.html) | symbol |  |
| [Last](TradingPlatform.BusinessLayer.Last.html) | last |  |

#### Symbol\_NewQuote(Symbol, Quote)

Represent access to historical data information and indicators control.

##### Declaration

```csharp
protected void Symbol_NewQuote(Symbol symbol, Quote quote)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Symbol](TradingPlatform.BusinessLayer.Symbol.html) | symbol |  |
| [Quote](TradingPlatform.BusinessLayer.Quote.html) | quote |  |

#### UnSubscribeSymbol()

Represent access to historical data information and indicators control.

##### Declaration

```csharp
protected virtual void UnSubscribeSymbol()
```

### Events

#### HistoryItemUpdated

Will be triggered when current historical item changed or updated

##### Declaration

```csharp
public event HistoryEventHandler HistoryItemUpdated
```

##### Event Type

| Type | Description |
| --- | --- |
| [HistoryEventHandler](TradingPlatform.BusinessLayer.HistoryEventHandler.html) |  |

#### HistoryItemVolumeAnalysisUpdated

Will be triggered when volume analysis of current historical item changed or updated

##### Declaration

```csharp
public event Action HistoryItemVolumeAnalysisUpdated
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action) |  |

#### NewHistoryItem

Will be triggered when new historical item created

##### Declaration

```csharp
public event HistoryEventHandler NewHistoryItem
```

##### Event Type

| Type | Description |
| --- | --- |
| [HistoryEventHandler](TradingPlatform.BusinessLayer.HistoryEventHandler.html) |  |