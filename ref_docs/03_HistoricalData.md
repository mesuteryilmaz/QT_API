# Class HistoricalData
> Namespace: `TradingPlatform.BusinessLayer`
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.HistoricalData.html

Provides access to historical bar/tick data and indicator control on that data.

```csharp
public class HistoricalData
```

Obtain via:
```csharp
var hd = symbol.GetHistory(new HistoryRequestParameters
{
    Symbol      = symbol,
    Aggregation = new HistoryAggregationTime(Period.MIN1),
    FromTime    = DateTime.UtcNow.AddDays(-30),
    ToTime      = DateTime.UtcNow
});
```

---

## Constructors

```csharp
protected HistoricalData(HistoryRequestParameters historyRequestParameters)
```

---

## Fields (protected)

| Field | Type | Description |
|---|---|---|
| `Indicators` | `IndicatorsCollection` | Attached indicators |
| `Parameters` | `HistoryRequestParameters` | Request parameters used |
| `itemsLocker` | `object` | Thread-safety lock object |

---

## Properties

| Property | Type | Description |
|---|---|---|
| `Count` | `int` | Number of history items |
| `Aggregation` | `HistoryAggregation` | Current aggregation (e.g., MIN1, TICK) |
| `Symbol` | `Symbol` | Associated symbol |
| `FromTime` | `DateTime` | Left time boundary |
| `ToTime` | `DateTime` | Right time boundary |
| `AttachedIndicators` | `Indicator[]` | All currently attached indicators |
| `BuiltInIndicators` | `BuiltInIndicators` | Access to built-in indicators |
| `IndicatorCalculationBehavior` | `IndicatorCalculationBehavior` | Calc behavior (get/set) |
| `this[int offset, SeekOriginHistory origin]` | `IHistoryItem` | Indexer: `hd[0]` = most recent bar |

---

## Methods

### Indicator Management

```csharp
// Add by name
Indicator AddIndicator(string indicatorName, params SettingItem[] settings)

// Add instance
void AddIndicator(Indicator indicator)

// Remove
void RemoveIndicator(Indicator indicator)
```

### Data Access

```csharp
// Iterate
IEnumerator GetEnumerator()

// Index → time mapping
double GetIndexByTime(long time, SeekOriginHistory origin = SeekOriginHistory.End)

// Time to next bar close
string GetTimeToNextBar()
```

### Volume Analysis

```csharp
IVolumeAnalysisCalculationProgress CalculateVolumeProfile(
    VolumeAnalysisCalculationParameters volumeAnalysisCalculationParameters)
```

### Lifecycle

```csharp
void Reload()   // Reload all data
```

### Protected Overridable

```csharp
protected virtual void AddNewItem(IHistoryItem item, bool updateIndicators = true,
                                   HistoryEventArgs e = null)
protected virtual void ProcessLast(Last last)
protected virtual void ProcessQuote(Quote quote)
protected virtual void ProcessLevel2Qute(MessageQuote quote)
protected virtual void ProcessMark(Mark mark)
protected virtual void SubscribeSymbol()
protected virtual void UnSubscribeSymbol()
protected virtual IHistoryProcessor CreateHistoryProcessor()
protected virtual bool NeedSubscribe { get; }
```

---

## Events

| Event | Type | Description |
|---|---|---|
| `NewHistoryItem` | `HistoryEventHandler` | New bar/tick added |
| `HistoryItemUpdated` | `HistoryEventHandler` | Current item updated (live tick) |
| `HistoryItemVolumeAnalysisUpdated` | `Action` | Volume analysis data updated |

---

## IHistoryItem — Key Members

```csharp
IHistoryItem bar = hd[0, SeekOriginHistory.End]; // Most recent

double open    = ((HistoryItemBar)bar).Open;
double high    = ((HistoryItemBar)bar).High;
double low     = ((HistoryItemBar)bar).Low;
double close   = ((HistoryItemBar)bar).Close;
double volume  = ((HistoryItemBar)bar).Volume;
DateTime time  = bar.TimeLeft;
```

---

## Aggregation Types

```csharp
// Time-based
new HistoryAggregationTime(Period.MIN1)
new HistoryAggregationTime(Period.TICK)

// Volume bars
new HistoryAggregationVolume(500)   // 500-volume bars

// Tick bars
new HistoryAggregationTick(100)     // 100-tick bars

// Range bars
new HistoryAggregationPriceRange(4) // 4-tick range bars
```

---

## Pattern — DOM Strategy (5-min + Tick)

```csharp
// In OnRun():
var hdTick = Symbol.GetHistory(new HistoryRequestParameters
{
    Symbol      = Symbol,
    Aggregation = new HistoryAggregationTime(Period.TICK),
    FromTime    = Core.Instance.TimeUtils.DateTimeUtcNow.AddHours(-2),
    ToTime      = Core.Instance.TimeUtils.DateTimeUtcNow
});

hdTick.NewHistoryItem += (hd, e) =>
{
    var tick = (HistoryItemLast)hd[0];
    ProcessTick(tick.Price, tick.Volume, tick.Time);
};
```
