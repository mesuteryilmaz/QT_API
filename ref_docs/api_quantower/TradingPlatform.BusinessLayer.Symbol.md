# Class Symbol
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Symbol.html
> Fetched: 2026-04-10

---

# Class Symbol

Represent access to symbol information and properties.

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class Symbol : BusinessObject
```

### Constructors

#### Symbol()

Represent access to symbol information and properties.

##### Declaration

```csharp
protected Symbol()
```

### Fields

#### SPOT\_SYMBOL\_ID

Represent access to symbol information and properties.

##### Declaration

```csharp
public const string SPOT_SYMBOL_ID = "spotSymbolId"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### TRADING\_SYMBOL\_ID

Represent access to symbol information and properties.

##### Declaration

```csharp
public const string TRADING_SYMBOL_ID = "TradingSymbol"
```

##### Field Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### historyMetadata

Represent access to symbol information and properties.

##### Declaration

```csharp
protected HistoryMetadata historyMetadata
```

##### Field Value

| Type | Description |
| --- | --- |
| [HistoryMetadata](TradingPlatform.BusinessLayer.Integration.HistoryMetadata.html) |  |

#### volumeAnalysisMetadata

Represent access to symbol information and properties.

##### Declaration

```csharp
protected VolumeAnalysisMetadata volumeAnalysisMetadata
```

##### Field Value

| Type | Description |
| --- | --- |
| [VolumeAnalysisMetadata](TradingPlatform.BusinessLayer.Integration.VolumeAnalysisMetadata.html) |  |

### Properties

#### Ask

Gets Ask price

##### Declaration

```csharp
public double Ask { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### AskSize

Gets Ask size

##### Declaration

```csharp
public double AskSize { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### AvailableFutures

Represent access to symbol information and properties.

##### Declaration

```csharp
public AvailableDerivatives AvailableFutures { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [AvailableDerivatives](TradingPlatform.BusinessLayer.AvailableDerivatives.html) |  |

#### AvailableOptions

Represent access to symbol information and properties.

##### Declaration

```csharp
public AvailableDerivatives AvailableOptions { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [AvailableDerivatives](TradingPlatform.BusinessLayer.AvailableDerivatives.html) |  |

#### AvailableOptionsExchanges

Represent access to symbol information and properties.

##### Declaration

```csharp
public Exchange[] AvailableOptionsExchanges { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Exchange](TradingPlatform.BusinessLayer.Exchange.html)[] |  |

#### AverageTradedPrice

Represent access to symbol information and properties.

##### Declaration

```csharp
public double AverageTradedPrice { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Bid

Gets Bid price

##### Declaration

```csharp
public double Bid { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### BidSize

Gets Bid size

##### Declaration

```csharp
public double BidSize { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### BottomPriceLimit

Represent access to symbol information and properties.

##### Declaration

```csharp
public double BottomPriceLimit { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Change

Gets change value between Bid/Last and Close price

##### Declaration

```csharp
public double Change { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### ChangePercentage

Gets [Change](TradingPlatform.BusinessLayer.Symbol.html#TradingPlatform_BusinessLayer_Symbol_Change) percentage value

##### Declaration

```csharp
public double ChangePercentage { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### ComplexId

Represent access to symbol information and properties.

##### Declaration

```csharp
public SymbolComplexIdentifier ComplexId { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [SymbolComplexIdentifier](TradingPlatform.BusinessLayer.SymbolComplexIdentifier.html) |  |

#### CurrentSessionsInfo

Represent access to symbol information and properties.

##### Declaration

```csharp
public SessionsContainer CurrentSessionsInfo { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [SessionsContainer](TradingPlatform.BusinessLayer.SessionsContainer.html) |  |

#### Delta

Represent access to symbol information and properties.

##### Declaration

```csharp
public double Delta { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### DeltaCalculationType

Represent access to symbol information and properties.

##### Declaration

```csharp
public DeltaCalculationType DeltaCalculationType { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DeltaCalculationType](TradingPlatform.BusinessLayer.DeltaCalculationType.html) |  |

#### DepthOfMarket

Gets Level2 data

##### Declaration

```csharp
public DepthOfMarket DepthOfMarket { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DepthOfMarket](TradingPlatform.BusinessLayer.DepthOfMarket.html) |  |

#### Description

Gets symbol description

##### Declaration

```csharp
public string Description { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Exchange

Gets Exchange of current symbol

##### Declaration

```csharp
public Exchange Exchange { get; protected set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Exchange](TradingPlatform.BusinessLayer.Exchange.html) |  |

#### ExchangeId

Gets Exchange id of current symbol

##### Declaration

```csharp
public string ExchangeId { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### ExpirationDate

Gets derivative expiration date

##### Declaration

```csharp
public DateTime ExpirationDate { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

#### FundingRate

Represent access to symbol information and properties.

##### Declaration

```csharp
public double FundingRate { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### FundingTime

Represent access to symbol information and properties.

##### Declaration

```csharp
public DateTime? FundingTime { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)? |  |

#### FutureContractType

Represent access to symbol information and properties.

##### Declaration

```csharp
public FutureContractType? FutureContractType { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [FutureContractType](TradingPlatform.BusinessLayer.FutureContractType.html)? |  |

#### Gamma

Represent access to symbol information and properties.

##### Declaration

```csharp
public double Gamma { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Group

Gets SymbolGroup

##### Declaration

```csharp
public SymbolGroup Group { get; protected set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [SymbolGroup](TradingPlatform.BusinessLayer.SymbolGroup.html) |  |

#### High

Gets high price

##### Declaration

```csharp
public double High { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### HistoryType

Default history type

##### Declaration

```csharp
public HistoryType HistoryType { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [HistoryType](TradingPlatform.BusinessLayer.HistoryType.html) |  |

#### IV

Represent access to symbol information and properties.

##### Declaration

```csharp
public double IV { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Id

Gets symbol Id

##### Declaration

```csharp
public string Id { get; protected set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Last

Gets last price

##### Declaration

```csharp
public double Last { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### LastDateTime

Gets last time

##### Declaration

```csharp
public DateTime LastDateTime { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

#### LastSize

Gets last size

##### Declaration

```csharp
public double LastSize { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### LastTradingDate

Gets derivative last trading date

##### Declaration

```csharp
public DateTime LastTradingDate { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

#### LotSize

Amount of base asset [Product](TradingPlatform.BusinessLayer.Symbol.html#TradingPlatform_BusinessLayer_Symbol_Product) for one lot.

##### Declaration

```csharp
public double LotSize { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### LotStep

Step of the lot changes

##### Declaration

```csharp
public double LotStep { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Low

Gets low price

##### Declaration

```csharp
public double Low { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Mark

Gets mark price

##### Declaration

```csharp
public double Mark { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### MarkSize

Gets mark size

##### Declaration

```csharp
public double MarkSize { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### MaturityDate

Gets derivative maturity date

##### Declaration

```csharp
public DateTime MaturityDate { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

#### MaxLot

The highest trade allowed

##### Declaration

```csharp
public double MaxLot { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### MinLot

The lowest trade allowed

##### Declaration

```csharp
public double MinLot { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### MinVolumeAnalysisTickSize

Represent access to symbol information and properties.

##### Declaration

```csharp
public double MinVolumeAnalysisTickSize { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Name

Gets symbol name

##### Declaration

```csharp
public string Name { get; protected set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### NettingType

Gets symbol NettingType

##### Declaration

```csharp
public NettingType NettingType { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [NettingType](TradingPlatform.BusinessLayer.NettingType.html) |  |

#### NotionalValueStep

Step of the notional value changes

##### Declaration

```csharp
public double NotionalValueStep { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Open

Gets open price

##### Declaration

```csharp
public double Open { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### OpenInterest

Represent access to symbol information and properties.

##### Declaration

```csharp
public double OpenInterest { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### PrevClose

Gets previous close price

##### Declaration

```csharp
public double PrevClose { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Product

Gets symbol base Asset

##### Declaration

```csharp
public Asset Product { get; protected set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Asset](TradingPlatform.BusinessLayer.Asset.html) |  |

#### QuoteAssetVolume

Gets quote asset volume value

##### Declaration

```csharp
public double QuoteAssetVolume { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### QuoteDateTime

Gets quote time

##### Declaration

```csharp
public DateTime QuoteDateTime { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

#### QuoteDelay

Returns delay with which quote come in platform.

##### Declaration

```csharp
public TimeSpan QuoteDelay { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan) |  |

#### QuotingCurrency

Gets symbol counter Asset

##### Declaration

```csharp
public Asset QuotingCurrency { get; protected set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Asset](TradingPlatform.BusinessLayer.Asset.html) |  |

#### QuotingType

Gets current SymbolQuotingType

##### Declaration

```csharp
public SymbolQuotingType QuotingType { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [SymbolQuotingType](TradingPlatform.BusinessLayer.SymbolQuotingType.html) |  |

#### Rho

Represent access to symbol information and properties.

##### Declaration

```csharp
public double Rho { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Root

Gets derivative underlier name

##### Declaration

```csharp
public string Root { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Spread

Gets spread value between Bid and Ask

##### Declaration

```csharp
public double Spread { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### SpreadPercentage

Gets [Spread](TradingPlatform.BusinessLayer.Symbol.html#TradingPlatform_BusinessLayer_Symbol_Spread) percentage value

##### Declaration

```csharp
public double SpreadPercentage { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### SymbolType

Gets symbol type

##### Declaration

```csharp
public SymbolType SymbolType { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [SymbolType](TradingPlatform.BusinessLayer.SymbolType.html) |  |

#### Theta

Represent access to symbol information and properties.

##### Declaration

```csharp
public double Theta { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### TickSize

Gets cached tick size if it available, else tries to obtain [GetTickSize(double)](TradingPlatform.BusinessLayer.Symbol.html#TradingPlatform_BusinessLayer_Symbol_GetTickSize_System_Double_) with Last, Bid, Ask, first element of [VariableTick](TradingPlatform.BusinessLayer.VariableTick.html) list otherwise - [DOUBLE\_UNDEFINED](TradingPlatform.BusinessLayer.Utils.Const.html#TradingPlatform_BusinessLayer_Utils_Const_DOUBLE_UNDEFINED)

##### Declaration

```csharp
public double TickSize { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Ticks

Gets ticks amount

##### Declaration

```csharp
public long Ticks { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [long](https://learn.microsoft.com/dotnet/api/system.int64) |  |

#### TopPriceLimit

Represent access to symbol information and properties.

##### Declaration

```csharp
public double TopPriceLimit { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### TotalBuyQuantity

Represent access to symbol information and properties.

##### Declaration

```csharp
public double TotalBuyQuantity { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### TotalSellQuantity

Represent access to symbol information and properties.

##### Declaration

```csharp
public double TotalSellQuantity { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Trades

Gets trades amount

##### Declaration

```csharp
public long Trades { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [long](https://learn.microsoft.com/dotnet/api/system.int64) |  |

#### Underlier

Gets derivative underlier symbol

##### Declaration

```csharp
public Symbol Underlier { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Symbol](TradingPlatform.BusinessLayer.Symbol.html) |  |

#### UnderlierId

Gets derivative underlier symbol id

##### Declaration

```csharp
public string UnderlierId { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### VariableTickList

Stores list of symbol ticksizes

##### Declaration

```csharp
public List<VariableTick> VariableTickList { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)<[VariableTick](TradingPlatform.BusinessLayer.VariableTick.html)> |  |

#### Vega

Represent access to symbol information and properties.

##### Declaration

```csharp
public double Vega { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### Volume

Gets volume value

##### Declaration

```csharp
public double Volume { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### VolumeType

Gets SymbolVolumeType

##### Declaration

```csharp
public SymbolVolumeType VolumeType { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [SymbolVolumeType](TradingPlatform.BusinessLayer.SymbolVolumeType.html) |  |

### Methods

#### CalculatePrice(double, double)

Calculates new price which equal to given price shifted by a number of given ticks

##### Declaration

```csharp
public double CalculatePrice(double price, double ticks)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | price |  |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | ticks |  |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### CalculateTicks(double, double)

Calculates ticks between two prices

##### Declaration

```csharp
public double CalculateTicks(double price1, double price2)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | price1 |  |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | price2 |  |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### DetermineTickDirection(double, double, TickDirection)

Represent access to symbol information and properties.

##### Declaration

```csharp
public static TickDirection DetermineTickDirection(double previousPrice, double currentPrice, TickDirection prevItemTickDirection)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | previousPrice |  |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | currentPrice |  |
| [TickDirection](TradingPlatform.BusinessLayer.TickDirection.html) | prevItemTickDirection |  |

##### Returns

| Type | Description |
| --- | --- |
| [TickDirection](TradingPlatform.BusinessLayer.TickDirection.html) |  |

#### Equals(object)

Determines whether the specified object is equal to the current object.

##### Declaration

```csharp
public override bool Equals(object obj)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [object](https://learn.microsoft.com/dotnet/api/system.object) | obj | The object to compare with the current object. |

##### Returns

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | [true](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool) if the specified object is equal to the current object; otherwise, [false](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool). |

##### Overrides

[object.Equals(object)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals(system-object))

#### Equals(Symbol)

Indicates whether the current object is equal to another object of the same type.

##### Declaration

```csharp
public bool Equals(Symbol other)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Symbol](TradingPlatform.BusinessLayer.Symbol.html) | other | An object to compare with this object. |

##### Returns

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | [true](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool) if the current object is equal to the `other` parameter; otherwise, [false](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool). |

#### FindVariableTick(double)

Returns VariableTick if it can be retrived from [VariableTick](TradingPlatform.BusinessLayer.VariableTick.html) list by price or null

##### Declaration

```csharp
public VariableTick FindVariableTick(double price)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | price |  |

##### Returns

| Type | Description |
| --- | --- |
| [VariableTick](TradingPlatform.BusinessLayer.VariableTick.html) |  |

#### FormatOffset(double, string)

Returns string with formatted ticks value

##### Declaration

```csharp
public string FormatOffset(double offset, string dimension = "ticks")
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | offset |  |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | dimension |  |

##### Returns

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### FormatPrice(double)

Formats price value to the appropriative string with a counting on tick precision.

##### Declaration

```csharp
public string FormatPrice(double price)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | price |  |

##### Returns

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### FormatPrice(double, VariableTick)

Formats price value to the appropriative string with a counting on tick precision.

##### Declaration

```csharp
public string FormatPrice(double price, VariableTick variableTick)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | price |  |
| [VariableTick](TradingPlatform.BusinessLayer.VariableTick.html) | variableTick |  |

##### Returns

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### FormatPriceWithMaxPrecision(double)

Formats price value to the appropriative string with a counting on max tick precision.

##### Declaration

```csharp
public string FormatPriceWithMaxPrecision(double price)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | price |  |

##### Returns

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### FormatQuantity(double, bool, bool)

Represent access to symbol information and properties.

##### Declaration

```csharp
public virtual string FormatQuantity(double quantity, bool inLots = true, bool abbreviate = false)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | quantity |  |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | inLots |  |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | abbreviate |  |

##### Returns

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### GetConnectionStateDependency()

Represent access to symbol information and properties.

##### Declaration

```csharp
public virtual ConnectionDependency GetConnectionStateDependency()
```

##### Returns

| Type | Description |
| --- | --- |
| [ConnectionDependency](TradingPlatform.BusinessLayer.ConnectionDependency.html) |  |

#### GetHashCode()

Serves as the default hash function.

##### Declaration

```csharp
public override int GetHashCode()
```

##### Returns

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | A hash code for the current object. |

##### Overrides

[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

#### GetHistory(HistoryAggregation, DateTime, DateTime)

Gets historical data according to aggregation and other parameters

##### Declaration

```csharp
public HistoricalData GetHistory(HistoryAggregation aggregation, DateTime fromTime, DateTime toTime = default)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [HistoryAggregation](TradingPlatform.BusinessLayer.HistoryAggregation.html) | aggregation |  |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) | fromTime |  |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) | toTime |  |

##### Returns

| Type | Description |
| --- | --- |
| [HistoricalData](TradingPlatform.BusinessLayer.HistoricalData.html) |  |

#### GetHistory(HistoryRequestParameters)

Gets historical data according to given history request

##### Declaration

```csharp
public HistoricalData GetHistory(HistoryRequestParameters historyRequestParameters)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [HistoryRequestParameters](TradingPlatform.BusinessLayer.HistoryRequestParameters.html) | historyRequestParameters |  |

##### Returns

| Type | Description |
| --- | --- |
| [HistoricalData](TradingPlatform.BusinessLayer.HistoricalData.html) |  |

#### GetHistory(Period, DateTime, DateTime)

Gets historical data according to period and other parameters

##### Declaration

```csharp
public HistoricalData GetHistory(Period period, DateTime fromTime, DateTime toTime = default)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) | period |  |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) | fromTime |  |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) | toTime |  |

##### Returns

| Type | Description |
| --- | --- |
| [HistoricalData](TradingPlatform.BusinessLayer.HistoricalData.html) |  |

#### GetHistory(Period, HistoryType, DateTime, DateTime)

Gets historical data according to period and other parameters

##### Declaration

```csharp
public HistoricalData GetHistory(Period period, HistoryType historyType, DateTime fromTime, DateTime toTime = default)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Period](TradingPlatform.BusinessLayer.Period.html) | period |  |
| [HistoryType](TradingPlatform.BusinessLayer.HistoryType.html) | historyType |  |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) | fromTime |  |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) | toTime |  |

##### Returns

| Type | Description |
| --- | --- |
| [HistoricalData](TradingPlatform.BusinessLayer.HistoricalData.html) |  |

#### GetMarginInfo(OrderRequestParameters)

Represent access to symbol information and properties.

##### Declaration

```csharp
public MarginInfo GetMarginInfo(OrderRequestParameters orderRequestParameters)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [OrderRequestParameters](TradingPlatform.BusinessLayer.OrderRequestParameters.html) | orderRequestParameters |  |

##### Returns

| Type | Description |
| --- | --- |
| [MarginInfo](TradingPlatform.BusinessLayer.Integration.MarginInfo.html) |  |

#### GetTickCost(double)

Gets symbol tick cost retrived from the [VariableTick](TradingPlatform.BusinessLayer.VariableTick.html) list by price

##### Declaration

```csharp
public double GetTickCost(double price)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | price |  |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### GetTickHistory(HistoryType, DateTime, DateTime)

Gets historical ticks data according to given parameters

##### Declaration

```csharp
public HistoricalData GetTickHistory(HistoryType historyType, DateTime fromTime, DateTime toTime = default)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [HistoryType](TradingPlatform.BusinessLayer.HistoryType.html) | historyType |  |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) | fromTime |  |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) | toTime |  |

##### Returns

| Type | Description |
| --- | --- |
| [HistoricalData](TradingPlatform.BusinessLayer.HistoricalData.html) |  |

#### GetTickSize(double)

Gets cached symbol tick size or retrives it from the [VariableTick](TradingPlatform.BusinessLayer.VariableTick.html) list

##### Declaration

```csharp
public double GetTickSize(double price)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | price |  |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### IsTradingAllowed(Account)

Represent access to symbol information and properties.

##### Declaration

```csharp
public virtual bool IsTradingAllowed(Account account)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Account](TradingPlatform.BusinessLayer.Account.html) | account |  |

##### Returns

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### OnConnectionStateChanged(Connection, ConnectionStateChangedEventArgs)

Represent access to symbol information and properties.

##### Declaration

```csharp
public virtual void OnConnectionStateChanged(Connection connection, ConnectionStateChangedEventArgs e)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Connection](TradingPlatform.BusinessLayer.Connection.html) | connection |  |
| [ConnectionStateChangedEventArgs](TradingPlatform.BusinessLayer.ConnectionStateChangedEventArgs.html) | e |  |

#### RoundPriceToTickSize(double, double)

Returns rounded to [TickSize](TradingPlatform.BusinessLayer.Symbol.html#TradingPlatform_BusinessLayer_Symbol_TickSize) price

##### Declaration

```csharp
public double RoundPriceToTickSize(double price, double tickSize = NaN)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | price |  |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | tickSize |  |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### SubscribeAction(SubscribeQuoteType)

Represent access to symbol information and properties.

##### Declaration

```csharp
protected virtual void SubscribeAction(SubscribeQuoteType type)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [SubscribeQuoteType](TradingPlatform.BusinessLayer.SubscribeQuoteType.html) | type |  |

#### UnSubscribeAction(SubscribeQuoteType)

Represent access to symbol information and properties.

##### Declaration

```csharp
protected virtual void UnSubscribeAction(SubscribeQuoteType type)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [SubscribeQuoteType](TradingPlatform.BusinessLayer.SubscribeQuoteType.html) | type |  |

### Events

#### NewDayBar

Will be triggered when new correctional quote is comming from the vendor.

##### Declaration

```csharp
public event DayBarHandler NewDayBar
```

##### Event Type

| Type | Description |
| --- | --- |
| [DayBarHandler](TradingPlatform.BusinessLayer.DayBarHandler.html) |  |

#### NewLast

Will be triggered when new trade quote is comming

##### Declaration

```csharp
public event LastHandler NewLast
```

##### Event Type

| Type | Description |
| --- | --- |
| [LastHandler](TradingPlatform.BusinessLayer.LastHandler.html) |  |

#### NewLevel2

Will be triggered when new Level2 quote is comming

##### Declaration

```csharp
public event Level2Handler NewLevel2
```

##### Event Type

| Type | Description |
| --- | --- |
| [Level2Handler](TradingPlatform.BusinessLayer.Level2Handler.html) |  |

#### NewMark

Represent access to symbol information and properties.

##### Declaration

```csharp
public event MarkHandler NewMark
```

##### Event Type

| Type | Description |
| --- | --- |
| [MarkHandler](TradingPlatform.BusinessLayer.MarkHandler.html) |  |

#### NewQuote

Will be triggered when new Level1 quote is comming

##### Declaration

```csharp
public event QuoteHandler NewQuote
```

##### Event Type

| Type | Description |
| --- | --- |
| [QuoteHandler](TradingPlatform.BusinessLayer.QuoteHandler.html) |  |

#### Updated

Will be triggered when symbol updated.

##### Declaration

```csharp
public event SymbolUpdateHandler Updated
```

##### Event Type

| Type | Description |
| --- | --- |
| [SymbolUpdateHandler](TradingPlatform.BusinessLayer.SymbolUpdateHandler.html) |  |