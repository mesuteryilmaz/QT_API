# Namespace TradingPlatform.BusinessLayer
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.html
> Fetched: 2026-04-10

---

# Namespace TradingPlatform.BusinessLayer

### Classes

#### [Account](TradingPlatform.BusinessLayer.Account.html)

Contains all user's account information

#### [Asset](TradingPlatform.BusinessLayer.Asset.html)

Defines asset entity

#### [BuiltInIndicators](TradingPlatform.BusinessLayer.BuiltInIndicators.html)

#### [CancelOrderRequestParameters](TradingPlatform.BusinessLayer.CancelOrderRequestParameters.html)

#### [ClosePositionRequestParameters](TradingPlatform.BusinessLayer.ClosePositionRequestParameters.html)

#### [Connection](TradingPlatform.BusinessLayer.Connection.html)

Represents information about connection and provides an access to the current trading information(Symbols, Orders, Position, Accounts etc.).

#### [ConnectionInfo](TradingPlatform.BusinessLayer.ConnectionInfo.html)

Represents all needed parameters for the connection constructing process.

#### [Core](TradingPlatform.BusinessLayer.Core.html)

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

#### [CorporateAction](TradingPlatform.BusinessLayer.CorporateAction.html)

Represents information about corporate action.

#### [CryptoAccount](TradingPlatform.BusinessLayer.CryptoAccount.html)

#### [CryptoAssetBalances](TradingPlatform.BusinessLayer.CryptoAssetBalances.html)

#### [DOMQuote](TradingPlatform.BusinessLayer.DOMQuote.html)

Represent access to DOM2 quote, which contains Bids and Asks.

#### [DayBar](TradingPlatform.BusinessLayer.DayBar.html)

Represent access to DayBar quote, which contains summary information about instrument prices.

#### [DepthOfMarket](TradingPlatform.BusinessLayer.DepthOfMarket.html)

Represent access to level2 data.

#### [DepthOfMarketAggregatedCollections](TradingPlatform.BusinessLayer.DepthOfMarketAggregatedCollections.html)

Leve2 data. Contains Bids and Ask collections

#### [Exchange](TradingPlatform.BusinessLayer.Exchange.html)

Contains all information which belong to the given exchange

#### [GetDepthOfMarketParameters](TradingPlatform.BusinessLayer.GetDepthOfMarketParameters.html)

Represent parameters of DepthOfMarket

#### [GetLevel2ItemsParameters](TradingPlatform.BusinessLayer.GetLevel2ItemsParameters.html)

Represent parameters of request for Leve2Item collection

#### [GetSymbolRequestParameters](TradingPlatform.BusinessLayer.GetSymbolRequestParameters.html)

#### [HistoricalData](TradingPlatform.BusinessLayer.HistoricalData.html)

Represent access to historical data information and indicators control.

#### [HistoryAggregation](TradingPlatform.BusinessLayer.HistoryAggregation.html)

#### [HistoryAggregationHeikenAshi](TradingPlatform.BusinessLayer.HistoryAggregationHeikenAshi.html)

#### [HistoryAggregationKagi](TradingPlatform.BusinessLayer.HistoryAggregationKagi.html)

#### [HistoryAggregationLineBreak](TradingPlatform.BusinessLayer.HistoryAggregationLineBreak.html)

#### [HistoryAggregationPointsAndFigures](TradingPlatform.BusinessLayer.HistoryAggregationPointsAndFigures.html)

#### [HistoryAggregationRangeBars](TradingPlatform.BusinessLayer.HistoryAggregationRangeBars.html)

#### [HistoryAggregationRenko](TradingPlatform.BusinessLayer.HistoryAggregationRenko.html)

#### [HistoryAggregationTick](TradingPlatform.BusinessLayer.HistoryAggregationTick.html)

#### [HistoryAggregationTime](TradingPlatform.BusinessLayer.HistoryAggregationTime.html)

#### [HistoryItemBar](TradingPlatform.BusinessLayer.HistoryItemBar.html)

Represents historical data bar item

#### [HistoryItemLast](TradingPlatform.BusinessLayer.HistoryItemLast.html)

Represents historical data trade item

#### [HistoryItemTick](TradingPlatform.BusinessLayer.HistoryItemTick.html)

Represents historical data tick item

#### [HistoryRequestParameters](TradingPlatform.BusinessLayer.HistoryRequestParameters.html)

Resolves a history request parameters per symbol

#### [Indicator](TradingPlatform.BusinessLayer.Indicator.html)

Base class for all indicators.

#### [InputParameterAttribute](TradingPlatform.BusinessLayer.InputParameterAttribute.html)

Use this attribute to mark input parameters of your script. You will see them in the settings screen on adding

#### [Last](TradingPlatform.BusinessLayer.Last.html)

Represent access to trade information.

#### [Level2Item](TradingPlatform.BusinessLayer.Level2Item.html)

Represent access to level2 item.

#### [Level2Quote](TradingPlatform.BusinessLayer.Level2Quote.html)

Represent access to Level2 quote.

#### [Mark](TradingPlatform.BusinessLayer.Mark.html)

#### [ModifyOrderRequestParameters](TradingPlatform.BusinessLayer.ModifyOrderRequestParameters.html)

#### [Order](TradingPlatform.BusinessLayer.Order.html)

Represents trading information about pending order

#### [OrderRequestParameters](TradingPlatform.BusinessLayer.OrderRequestParameters.html)

#### [OrdersHistoryRequestParameters](TradingPlatform.BusinessLayer.OrdersHistoryRequestParameters.html)

#### [PaintChartEventArgs](TradingPlatform.BusinessLayer.PaintChartEventArgs.html)

#### [PlaceOrderRequestParameters](TradingPlatform.BusinessLayer.PlaceOrderRequestParameters.html)

#### [PnLRequestParameters](TradingPlatform.BusinessLayer.PnLRequestParameters.html)

#### [Position](TradingPlatform.BusinessLayer.Position.html)

Represents trading information about related position

#### [Quote](TradingPlatform.BusinessLayer.Quote.html)

Represent access to quote information.

#### [ReportRequestParameters](TradingPlatform.BusinessLayer.ReportRequestParameters.html)

#### [RequestParameters](TradingPlatform.BusinessLayer.RequestParameters.html)

#### [SettingItemAction](TradingPlatform.BusinessLayer.SettingItemAction.html)

Typecasts setting as Button item

#### [SettingItemBoolean](TradingPlatform.BusinessLayer.SettingItemBoolean.html)

Typecasts setting as CheckBox item

#### [SettingItemColor](TradingPlatform.BusinessLayer.SettingItemColor.html)

Typecasts setting as Color item

#### [SettingItemDouble](TradingPlatform.BusinessLayer.SettingItemDouble.html)

Typecasts setting as NumericUpDown item

#### [SettingItemDoubleRange](TradingPlatform.BusinessLayer.SettingItemDoubleRange.html)

#### [SettingItemGroup](TradingPlatform.BusinessLayer.SettingItemGroup.html)

Typecasts setting as TabControl item

#### [SettingItemInteger](TradingPlatform.BusinessLayer.SettingItemInteger.html)

Typecasts setting as NumericUpDown item

#### [SettingItemIntegerRange](TradingPlatform.BusinessLayer.SettingItemIntegerRange.html)

#### [SettingItemLong](TradingPlatform.BusinessLayer.SettingItemLong.html)

#### [SettingItemPeriod](TradingPlatform.BusinessLayer.SettingItemPeriod.html)

Typecasts setting as Period item

#### [SettingItemSelector](TradingPlatform.BusinessLayer.SettingItemSelector.html)

Typecasts setting as ComboBox item

#### [SettingItemSeparatorGroup](TradingPlatform.BusinessLayer.SettingItemSeparatorGroup.html)

Typecasts setting as GroupBox item

#### [SettingItemString](TradingPlatform.BusinessLayer.SettingItemString.html)

Typecasts setting as TextBox item

#### [Strategy](TradingPlatform.BusinessLayer.Strategy.html)

The base class for strategies

#### [StrategyMetric](TradingPlatform.BusinessLayer.StrategyMetric.html)

#### [Symbol](TradingPlatform.BusinessLayer.Symbol.html)

Represent access to symbol information and properties.

#### [SymbolGroup](TradingPlatform.BusinessLayer.SymbolGroup.html)

Provides possibility to group and sort symbols for each connection

#### [Trade](TradingPlatform.BusinessLayer.Trade.html)

Represents information about trade.

#### [TradesHistoryRequestParameters](TradingPlatform.BusinessLayer.TradesHistoryRequestParameters.html)

#### [TradingRequestParameters](TradingPlatform.BusinessLayer.TradingRequestParameters.html)

#### [VolumeAnalysisCalculationParameters](TradingPlatform.BusinessLayer.VolumeAnalysisCalculationParameters.html)

Provides VA calculation parameters

#### [VolumeAnalysisCalculationRequest](TradingPlatform.BusinessLayer.VolumeAnalysisCalculationRequest.html)

Provides VA calculation request per [Symbol](TradingPlatform.BusinessLayer.Symbol.html)

#### [VolumeAnalysisData](TradingPlatform.BusinessLayer.VolumeAnalysisData.html)

#### [VolumeAnalysisDataEventArgs](TradingPlatform.BusinessLayer.VolumeAnalysisDataEventArgs.html)

#### [VolumeAnalysisItem](TradingPlatform.BusinessLayer.VolumeAnalysisItem.html)

Represent item with Volume Analysis calculation results

#### [VolumeAnalysisManager](TradingPlatform.BusinessLayer.VolumeAnalysisManager.html)

Volume Analysis calculations

### Structs

#### [Period](TradingPlatform.BusinessLayer.Period.html)

Represents mechanism for supporting predefined and custom periods

### Interfaces

#### [IHistoryAggregationHistoryTypeSupport](TradingPlatform.BusinessLayer.IHistoryAggregationHistoryTypeSupport.html)

#### [IVolumeAnalysisCalculationProgress](TradingPlatform.BusinessLayer.IVolumeAnalysisCalculationProgress.html)

#### [IVolumeAnalysisCalculationTask](TradingPlatform.BusinessLayer.IVolumeAnalysisCalculationTask.html)

### Enums

#### [AggregateMethod](TradingPlatform.BusinessLayer.AggregateMethod.html)

Aggregation method

#### [BasePeriod](TradingPlatform.BusinessLayer.BasePeriod.html)

Period that can be used as a basis for history aggregations

#### [LineStyle](TradingPlatform.BusinessLayer.LineStyle.html)

Specifies the style of indicator line.

#### [MaMode](TradingPlatform.BusinessLayer.MaMode.html)

Moving average mode

#### [PointsAndFiguresStyle](TradingPlatform.BusinessLayer.PointsAndFiguresStyle.html)

#### [PriceType](TradingPlatform.BusinessLayer.PriceType.html)

#### [RenkoStyle](TradingPlatform.BusinessLayer.RenkoStyle.html)

#### [StrategyLoggingLevel](TradingPlatform.BusinessLayer.StrategyLoggingLevel.html)

#### [StrategyState](TradingPlatform.BusinessLayer.StrategyState.html)

#### [VolumeAnalysisCalculationState](TradingPlatform.BusinessLayer.VolumeAnalysisCalculationState.html)

#### [VolumeAnalysisField](TradingPlatform.BusinessLayer.VolumeAnalysisField.html)