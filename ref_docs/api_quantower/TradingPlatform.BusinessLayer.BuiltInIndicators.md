# Class BuiltInIndicators
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.BuiltInIndicators.html
> Fetched: 2026-04-10

---

# Class BuiltInIndicators

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public sealed class BuiltInIndicators
```

### Methods

#### AC()

Returns an instance of the Acceleration/Deceleration Oscillator (AC).

AC measures the acceleration and deceleration of the current momentum.

##### Declaration

```csharp
public Indicator AC()
```

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### ADX(int, MaMode, IndicatorCalculationType)

Returns an instance of the Average Directional Index (ADX) indicator.

The ADX determines the strength of a prevailing trend.

##### Declaration

```csharp
public Indicator ADX(int period, MaMode mode, IndicatorCalculationType calculationType = IndicatorCalculationType.AllAvailableData)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Period |
| [MaMode](TradingPlatform.BusinessLayer.MaMode.html) | mode | Type of Moving Average |
| [IndicatorCalculationType](TradingPlatform.BusinessLayer.IndicatorCalculationType.html) | calculationType | Calculation type |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### AFIRMA(int, PriceType, AfirmaMode, bool)

Gets the AFIRMA indicator

Autoregressive finite impulse response moving average. A digital filter accurately shows the price movement as powered with least square method to minimise time lag

##### Declaration

```csharp
public Indicator AFIRMA(int period, PriceType priceType, AfirmaMode afirmaMode, bool least_squares_method)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Moving average period |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType | Type of the price |
| [AfirmaMode](TradingPlatform.BusinessLayer.AfirmaMode.html) | afirmaMode | Afirma mode |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | least\_squares\_method | with least squares method overlapping if true |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### AO()

Gets the AO (Awesome Oscillator) indicator.

The 'AO' indicator determines market momentum.

##### Declaration

```csharp
public Indicator AO()
```

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

#### AROON(int)

Gets the Aroon indicator.

Reveals the beginning of a new trend and determines how strong it is

##### Declaration

```csharp
public Indicator AROON(int period)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Aroons period |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### ATR(int, MaMode, IndicatorCalculationType)

Gets the Average True Range (ATR) indicator.

The ATR measures of market volatility.

##### Declaration

```csharp
public Indicator ATR(int period, MaMode mode, IndicatorCalculationType calculationType = IndicatorCalculationType.AllAvailableData)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Period of Moving Average. |
| [MaMode](TradingPlatform.BusinessLayer.MaMode.html) | mode | Type of Moving Average |
| [IndicatorCalculationType](TradingPlatform.BusinessLayer.IndicatorCalculationType.html) | calculationType | Calculation type |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### Alligator(MaMode, PriceType, int, int, MaMode, PriceType, int, int, MaMode, PriceType, int, int)

Gets the Alligator.

Three moving averages with different colors, periods and calculation methods.

##### Declaration

```csharp
public Indicator Alligator(MaMode JawMAType, PriceType JawSourcePrice, int JawMAPeiod, int JawMAShift, MaMode TeethMAType, PriceType TeethSourcePrice, int TeethMAPeiod, int TeethMAShift, MaMode LipsMAType, PriceType LipsSourcePrice, int LipsMAPeiod, int LipsMAShift)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [MaMode](TradingPlatform.BusinessLayer.MaMode.html) | JawMAType | Type of Jaw Moving Average. |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | JawSourcePrice | SourcePrice of Jaw Moving Average. |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | JawMAPeiod | Period of Jaw Moving Average. |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | JawMAShift | Shift of Jaw Moving Average. |
| [MaMode](TradingPlatform.BusinessLayer.MaMode.html) | TeethMAType | Period of Moving Average. |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | TeethSourcePrice | Type of Moving Average. |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | TeethMAPeiod | Period of Moving Average. |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | TeethMAShift | Type of Moving Average. |
| [MaMode](TradingPlatform.BusinessLayer.MaMode.html) | LipsMAType | Period of Moving Average. |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | LipsSourcePrice | Type of Moving Average. |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | LipsMAPeiod | Period of Moving Average. |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | LipsMAShift | Type of Moving Average. |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### BB(int, double, PriceType, MaMode, IndicatorCalculationType)

Gets the BB(Bollinger Bands) indicator.

The 'BB' indicator provides a relative definition of high and low based on standard deviation and a simple moving average.

##### Declaration

```csharp
public Indicator BB(int period, double coefficient, PriceType priceType, MaMode maMode, IndicatorCalculationType calculationType = IndicatorCalculationType.AllAvailableData)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Period of MA for envelopes. |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | coefficient | Value of confidence interval. |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType | Sources prices for MA. |
| [MaMode](TradingPlatform.BusinessLayer.MaMode.html) | maMode | Type of moving average. |
| [IndicatorCalculationType](TradingPlatform.BusinessLayer.IndicatorCalculationType.html) | calculationType | Calculation type |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### BBF(int, double, PriceType, MaMode, IndicatorCalculationType)

Returns an instance of the Bollinger Bands Flat (BBF) indicator.

The BBF provides the same data as BB, but drawn in separate field and easier to recognize whether price is in or out of the band.

##### Declaration

```csharp
public Indicator BBF(int period, double deviation, PriceType priceType, MaMode mode, IndicatorCalculationType calculationType = IndicatorCalculationType.AllAvailableData)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Period |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | deviation | Deviation |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType | Sources prices for MA |
| [MaMode](TradingPlatform.BusinessLayer.MaMode.html) | mode | Type of Moving Average |
| [IndicatorCalculationType](TradingPlatform.BusinessLayer.IndicatorCalculationType.html) | calculationType | Calculation type |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### CCI(int, PriceType, MaMode, IndicatorCalculationType)

Gets the Commodity Channel Index.

Measures the position of price in relation to its moving average.

##### Declaration

```csharp
public Indicator CCI(int maPeriod, PriceType priceType, MaMode maMode, IndicatorCalculationType calculationType = IndicatorCalculationType.AllAvailableData)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | maPeriod | Period for CCI MA |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType | Sources prices for CCI |
| [MaMode](TradingPlatform.BusinessLayer.MaMode.html) | maMode | MA mode for CCI |
| [IndicatorCalculationType](TradingPlatform.BusinessLayer.IndicatorCalculationType.html) | calculationType | Calculation type |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### CMO(int, PriceType)

Gets the CMO (Chande Momentum Oscillator) indicator.

The CMO calculates the dividing of difference between the sum of all recent gains and the sum of all recent losses by the sum of all price movement over the period.

##### Declaration

```csharp
public Indicator CMO(int period, PriceType priceType)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Period of MA for envelopes. |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType | Sources prices for MA. |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### Channel(int)

Gets the Channel (Price Channel) indicator.

The 'Channel' indicator is based on measurement of min and max prices for the definite number of periods.

##### Declaration

```csharp
public Indicator Channel(int period)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Period of price channel |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### DMI(int, MaMode, IndicatorCalculationType)

Gets the Directional Movement Index(DMI) indicator.

The DMI Ñdentifies whether there is a definable trend in the market.

##### Declaration

```csharp
public Indicator DMI(int period, MaMode mode, IndicatorCalculationType calculationType = IndicatorCalculationType.AllAvailableData)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Period of Moving Average. |
| [MaMode](TradingPlatform.BusinessLayer.MaMode.html) | mode | Type of Moving Average. |
| [IndicatorCalculationType](TradingPlatform.BusinessLayer.IndicatorCalculationType.html) | calculationType | Calculation type |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### EMA(int, PriceType, IndicatorCalculationType)

Returns an instance of the Exponential Moving Average (EMA) indicator.

EMA provides a weighted price calculation for the last N periods.

##### Declaration

```csharp
public Indicator EMA(int maPeriod, PriceType priceType, IndicatorCalculationType calculationType = IndicatorCalculationType.AllAvailableData)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | maPeriod | Period of Exponential Moving Average |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType | Sources prices for MA |
| [IndicatorCalculationType](TradingPlatform.BusinessLayer.IndicatorCalculationType.html) | calculationType | Calculation type |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

#### HV(int, int, PriceType, HVSheduleMode)

##### Declaration

```csharp
public Indicator HV(int stdPeriod, int volatilityPeriod, PriceType priceType, HVSheduleMode hvMode)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | stdPeriod |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | volatilityPeriod |  |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType |  |
| [HVSheduleMode](TradingPlatform.BusinessLayer.HVSheduleMode.html) | hvMode |  |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

#### HV(int, int, PriceType, HVSheduleMode, int)

##### Declaration

```csharp
public Indicator HV(int stdPeriod, int volatilityPeriod, PriceType priceType, HVSheduleMode hvMode, int percentilePeriod)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | stdPeriod |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | volatilityPeriod |  |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType |  |
| [HVSheduleMode](TradingPlatform.BusinessLayer.HVSheduleMode.html) | hvMode |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | percentilePeriod |  |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

#### ICH(int, int, int)

Gets the Ichimoku.

Enables to quickly discern and filter 'at a glance' the low-probability trading setups from those of higher probability.

##### Declaration

```csharp
public Indicator ICH(int TenkanPeriod, int KijunPeriod, int SenkouSpanB)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | TenkanPeriod | Tenkan Period |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | KijunPeriod | Kijun Period |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | SenkouSpanB | Senkou Span B |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### KAMA(int, double, double, PriceType)

Returns an instance of the Kaufman Adaptive Moving Average (KAMA) indicator.

KAMA is an exponential style average with a smoothing that varies according to recent data.

##### Declaration

```csharp
public Indicator KAMA(int period, double fast, double slow, PriceType priceType)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Period |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | fast | Fast factor |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | slow | Slow factor |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType | Sources prices for MA |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### KRI(int)

Returns an instance of the Kairi Relative Index (KRI) indicator.

KRI calculates deviation of the current price from its simple moving average as a percent of the moving average.

##### Declaration

```csharp
public Indicator KRI(int period)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period |  |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### Keltner(int, double, PriceType, MaMode, IndicatorCalculationType)

Returns an instance of the Keltner Channel indicator.

Keltner Channels are volatility-based envelopes set above and below an exponential moving average.

##### Declaration

```csharp
public Indicator Keltner(int period, double offset, PriceType priceType, MaMode mode, IndicatorCalculationType calculationType = IndicatorCalculationType.AllAvailableData)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Period of MA for Keltner's Channel |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | offset | Coefficient of channel's width |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType | Sources prices for MA |
| [MaMode](TradingPlatform.BusinessLayer.MaMode.html) | mode | Type of Moving Average |
| [IndicatorCalculationType](TradingPlatform.BusinessLayer.IndicatorCalculationType.html) | calculationType | Calculation type |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### LWMA(int, PriceType)

Gets the Linearly Weighted Moving Average

Linear Weighted Moving Average makes the most recent bar more important unlike SMA.

##### Declaration

```csharp
public Indicator LWMA(int maPeriod, PriceType priceType)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | maPeriod | Moving average period |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType | Type of the price |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### MA(int, PriceType, MaMode, IndicatorCalculationType)

Gets the specific MA indicator, according to selected 'MaMode'.

##### Declaration

```csharp
public Indicator MA(int period, PriceType priceType, MaMode maMode, IndicatorCalculationType calculationType = IndicatorCalculationType.AllAvailableData)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Period of moving average. |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType | Type of price. |
| [MaMode](TradingPlatform.BusinessLayer.MaMode.html) | maMode | MA mode. |
| [IndicatorCalculationType](TradingPlatform.BusinessLayer.IndicatorCalculationType.html) | calculationType | Calculation type |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### MACD(int, int, int, IndicatorCalculationType)

Gets the MACD (Moving Average Convergence/Divergence) indicator.

The MACD is a trend-following momentum indicator that shows the relationship between two moving averages of prices.

##### Declaration

```csharp
public Indicator MACD(int fastEMA, int slowEMA, int signalEMA, IndicatorCalculationType calculationType = IndicatorCalculationType.AllAvailableData)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | fastEMA | Period of fast EMA. |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | slowEMA | Period of slow EMA. |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | signalEMA | Period of signal EMA. |
| [IndicatorCalculationType](TradingPlatform.BusinessLayer.IndicatorCalculationType.html) | calculationType | Calculation type |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### MAE(int, PriceType, MaMode, double, double, IndicatorCalculationType)

Gets the MAE (Moving Average Envelope) indicator.

The 'MAE' indicator demonstrates a range of the prices discrepancy from a Moving Average.

##### Declaration

```csharp
public Indicator MAE(int period, PriceType priceType, MaMode maMode, double upShift, double downShift, IndicatorCalculationType calculationType = IndicatorCalculationType.AllAvailableData)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Period of MA for envelopes. |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType | Sources prices for MA. |
| [MaMode](TradingPlatform.BusinessLayer.MaMode.html) | maMode | Type of moving average. |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | upShift | Upband deviation in %. |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | downShift | Downband deviation in %. |
| [IndicatorCalculationType](TradingPlatform.BusinessLayer.IndicatorCalculationType.html) | calculationType | Calculation type |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### MAS3(int, int, int, int)

Gets the MAS3 (3MASignal) indicator.

The 'MAS3' indicator offers buy and sell signals according to intersections of three moving averages.

##### Declaration

```csharp
public Indicator MAS3(int shortPeriod, int middlePeriod, int longPeriod, int barsInterval)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | shortPeriod | Short moving average period. |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | middlePeriod | Middle moving average period. |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | longPeriod | Long moving average period. |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | barsInterval | The count of bars. The trend will be determined on this interval. |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### MD(int, int, PriceType, IndicatorCalculationType)

Returns an instance of the McGinley Dynamic indicator.

McGinley Dynamic avoids of most whipsaws and it rapidly moves up or down according to a quickly changing market. It needs no adjusting because it is dynamic and it adjusts itself.

##### Declaration

```csharp
public Indicator MD(int period, int trackingFactor, PriceType priceType, IndicatorCalculationType calculationType = IndicatorCalculationType.AllAvailableData)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Period of exponential moving average |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | trackingFactor | Dynamic tracking factor |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType | Source price type |
| [IndicatorCalculationType](TradingPlatform.BusinessLayer.IndicatorCalculationType.html) | calculationType | Calculation type |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### MFI(int)

Gets the MFI(Money Flow Index) indicator.

The MFI(Money Flow Index) is an oscillator that uses both price and volume to measure buying and selling pressure.

##### Declaration

```csharp
public Indicator MFI(int period)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Period of MFI. |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### MMA(int, PriceType)

Returns an instance of the Modified Moving Average (MMA) indicator.

MMA comprises a sloping factor to help it overtake with the growing or declining value of the trading price of the currency.

##### Declaration

```csharp
public Indicator MMA(int maPeriod, PriceType priceType)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | maPeriod | Period of Modified Moving Average |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType | Sources prices for MA |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### Momentum(int, PriceType)

Gets the Momentum indicator.

Momentum compares where the current price is in relation to where the price was in the past.

##### Declaration

```csharp
public Indicator Momentum(int period, PriceType priceType)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Period for Momentum |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType | Sources prices for Momentum |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### OBV(PriceType)

Gets On Balance Volume.

On Balance Volume (OBV) measures buying and selling pressure as a cumulative indicator that adds volume on up days and subtracts volume on down days.

##### Declaration

```csharp
public Indicator OBV(PriceType priceType)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType | Sources prices for OBV |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### OsMA(int, int, int, IndicatorCalculationType)

Gets the OsMA (Moving Average of Oscillator) indicator.

The OsMA reflects the difference between an oscillator (MACD) and its moving average (signal line).

##### Declaration

```csharp
public Indicator OsMA(int fastEMA, int slowEMA, int signalEMA, IndicatorCalculationType calculationType = IndicatorCalculationType.AllAvailableData)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | fastEMA | Period of fast EMA. |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | slowEMA | Period of slow EMA. |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | signalEMA | Period of signal EMA. |
| [IndicatorCalculationType](TradingPlatform.BusinessLayer.IndicatorCalculationType.html) | calculationType | Calculation type |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### PO(int, int, PriceType, MaMode, IndicatorCalculationType)

Returns an instance of the Price Oscillator (PO) indicator.

PO calculates the variation between price moving averages.

##### Declaration

```csharp
public Indicator PO(int period1, int period2, PriceType priceType, MaMode mode, IndicatorCalculationType calculationType = IndicatorCalculationType.AllAvailableData)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period1 | Period of MA1 |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period2 | Period of MA2 |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType | Sources prices for MA |
| [MaMode](TradingPlatform.BusinessLayer.MaMode.html) | mode | Type of Moving Average |
| [IndicatorCalculationType](TradingPlatform.BusinessLayer.IndicatorCalculationType.html) | calculationType | Calculation type |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### PPMA(int)

Gets the PPMA(Pivot Point Moving Average) indicator.

The 'PPMA' indicator uses the pivot point calculation as the input a simple moving average.

##### Declaration

```csharp
public Indicator PPMA(int period)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Period of PPMA indicator |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### PPO(int, int, int, IndicatorCalculationType)

Returns an instance of the Percentage Price Oscillator (PPO).

Percentage Price Oscillator is a momentum indicator. Signal line is EMA of PPO. Formula: (FastEMA-SlowEMA)/SlowEMA.

##### Declaration

```csharp
public Indicator PPO(int fastPeriod, int slowPeriod, int signalPeriod, IndicatorCalculationType calculationType = IndicatorCalculationType.AllAvailableData)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | fastPeriod | Fast EMA Period |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | slowPeriod | Slow EMA Period |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | signalPeriod | Signal EMA Period |
| [IndicatorCalculationType](TradingPlatform.BusinessLayer.IndicatorCalculationType.html) | calculationType | Calculation type |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### PVI(PriceType)

Returns an instance of the Positive Volume Index (PVI) indicator.

The PVI value changes on the periods in which value of volume has increased in comparison with the previous period.

##### Declaration

```csharp
public Indicator PVI(PriceType priceType)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType |  |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### Qstick(int, MaMode, IndicatorCalculationType)

Returns an instance of the Qstick indicator.

The Qstick is a moving average that shows the difference between the prices at which an issue opens and closes.

##### Declaration

```csharp
public Indicator Qstick(int period, MaMode mode, IndicatorCalculationType calculationType = IndicatorCalculationType.AllAvailableData)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period |  |
| [MaMode](TradingPlatform.BusinessLayer.MaMode.html) | mode |  |
| [IndicatorCalculationType](TradingPlatform.BusinessLayer.IndicatorCalculationType.html) | calculationType | Calculation type |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### RLW(int)

Gets the %R Larry Williams.

Uses Stochastic to determine overbought and oversold levels.

##### Declaration

```csharp
public Indicator RLW(int period)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Period for Momentum |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### ROC(int)

Gets the ROC (Rate of Change) indicator.

The ROC shows the speed at which price is changing.

##### Declaration

```csharp
public Indicator ROC(int period)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Period of momentum. |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### RSI(int, PriceType, RSIMode, MaMode, int, IndicatorCalculationType)

Gets the RSI indicator.

Relative Strength Index (RSI) is a momentum oscillator that measures the speed and change of price movements.

##### Declaration

```csharp
public Indicator RSI(int period, PriceType priceType, RSIMode rsiMode, MaMode maMode, int maperiod, IndicatorCalculationType calculationType = IndicatorCalculationType.AllAvailableData)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | RSI Period |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType | Price Type |
| [RSIMode](TradingPlatform.BusinessLayer.RSIMode.html) | rsiMode | RSI Mode (Simple or Exponential) |
| [MaMode](TradingPlatform.BusinessLayer.MaMode.html) | maMode | MA Mode for smooth data |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | maperiod | MA period for smooth data |
| [IndicatorCalculationType](TradingPlatform.BusinessLayer.IndicatorCalculationType.html) | calculationType | Calculation type |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### Regression(int, PriceType)

Gets the Regression indicator

The Linear Regression Indicator plots the ending value of a Linear Regression Line for a specified number of bars; showing, statistically, where the price is expected to be.

##### Declaration

```csharp
public Indicator Regression(int period, PriceType priceType)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Moving average period |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType | Type of the price |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### SAR(double, double)

Returns an instance of the Parabolic Time/Price System (SAR) indicator.

SAR indicator helps to define the direction of the prevailing trend and the moment to close positions opened during the reversal.

##### Declaration

```csharp
public Indicator SAR(double step, double maximum)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | step | Step of parabolic SAR system |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | maximum | Maximum value for the acceleration factor |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### SD(int, PriceType, MaMode, IndicatorCalculationType)

Returns an instance of the Standart Deviation (SD) indicator.

The SD shows the difference of the volatility value from the average one.

##### Declaration

```csharp
public Indicator SD(int period, PriceType priceType, MaMode mode, IndicatorCalculationType calculationType = IndicatorCalculationType.AllAvailableData)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Period of indicator |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType | Sources prices for MA |
| [MaMode](TradingPlatform.BusinessLayer.MaMode.html) | mode | Type of Moving Average |
| [IndicatorCalculationType](TradingPlatform.BusinessLayer.IndicatorCalculationType.html) | calculationType | Calculation type |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### SI(double)

Get the Swing Index (SI) indicator.

The SI is used to confirm trend line breakouts on price charts.

##### Declaration

```csharp
public Indicator SI(double divider)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | divider | The divider. |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### SMA(int, PriceType)

Gets the SMA(Simple Moving Average) indicator.

The 'SMA' indicator provides an average price for the last N periods.

##### Declaration

```csharp
public Indicator SMA(int period, PriceType priceType)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Period of simple moving average. |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType | Sources prices for MA. |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### SMMA(int, PriceType, IndicatorCalculationType)

Returns an instance of the Smoothed Moving Average (SMMA) indicator.

SMMA indicator provides a smoothed average price for the last N periods.

##### Declaration

```csharp
public Indicator SMMA(int period, PriceType priceType, IndicatorCalculationType calculationType = IndicatorCalculationType.AllAvailableData)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Moving average period |
| [PriceType](TradingPlatform.BusinessLayer.PriceType.html) | priceType | Type of the price |
| [IndicatorCalculationType](TradingPlatform.BusinessLayer.IndicatorCalculationType.html) | calculationType | Calculation type |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### Stochastic(int, int, int, MaMode, IndicatorCalculationType)

Gets the Stochastic Slow.

Shows the location of the current close relative to the high/low range over a set number of periods (Slow).

##### Declaration

```csharp
public Indicator Stochastic(int period, int smooth, int doubleSmooth, MaMode MaType, IndicatorCalculationType calculationType = IndicatorCalculationType.AllAvailableData)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | period | Period |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | smooth | Smoothing |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | doubleSmooth | Double smoothing |
| [MaMode](TradingPlatform.BusinessLayer.MaMode.html) | MaType | Moving type |
| [IndicatorCalculationType](TradingPlatform.BusinessLayer.IndicatorCalculationType.html) | calculationType | Calculation type |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### StochasticxRSI(int, int, int)

Gets the Stochastic x Relative Strength Index.

StochRSI is an oscillator that measures the level of RSI relative to its range.

##### Declaration

```csharp
public Indicator StochasticxRSI(int rsiPeriod, int kPeriod, int dPeriod)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | rsiPeriod | Period |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | kPeriod | Smoothing |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | dPeriod | Double smoothing |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### TSI(int, int, IndicatorCalculationType)

Get the True Strength Index (TSI) indicator.

The TSI is a variation of the Relative Strength Indicator which uses a doubly-smoothed
EMA of price momentum to eliminate choppy price changes and spot trend changes.

##### Declaration

```csharp
public Indicator TSI(int firstPeriod, int secondPeriod, IndicatorCalculationType calculationType = IndicatorCalculationType.AllAvailableData)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | firstPeriod | First MA period. |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | secondPeriod | Second MA period. |
| [IndicatorCalculationType](TradingPlatform.BusinessLayer.IndicatorCalculationType.html) | calculationType | Calculation type |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### Volume()

Returns an instance of the Volume indicator.

Volume allows to confirm the strength of a trend or to suggest about it's weakness.

##### Declaration

```csharp
public Indicator Volume()
```

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |

#### ZZ(double)

Returns an instance of the ZigZag indicator.

ZigZag is a trend following indicator that is used to predict when a given symbol's momentum is reversing.

##### Declaration

```csharp
public Indicator ZZ(double deviation)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | deviation | Percent Deviation |

##### Returns

| Type | Description |
| --- | --- |
| [Indicator](TradingPlatform.BusinessLayer.Indicator.html) |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentOutOfRangeException](https://learn.microsoft.com/dotnet/api/system.argumentoutofrangeexception) |  |