# Class Core
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Core.html
> Fetched: 2026-04-10

---

# Class Core

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class Core
```

### Properties

#### AccountOperations

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public AccountOperation[] AccountOperations { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [AccountOperation](TradingPlatform.BusinessLayer.AccountOperation.html)[] |  |

#### Accounts

Gets all available [Account](TradingPlatform.BusinessLayer.Account.html)s from open connections

##### Declaration

```csharp
public Account[] Accounts { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Account](TradingPlatform.BusinessLayer.Account.html)[] |  |

#### AdvancedTradingOperations

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public AdvancedTradingOperations AdvancedTradingOperations { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [AdvancedTradingOperations](TradingPlatform.BusinessLayer.AdvancedTradingOperations.html) |  |

#### Assets

Gets all available [Asset](TradingPlatform.BusinessLayer.Asset.html)s from open connections

##### Declaration

```csharp
public Asset[] Assets { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Asset](TradingPlatform.BusinessLayer.Asset.html)[] |  |

#### BrandingInformation

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public IBrandingInformation BrandingInformation { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IBrandingInformation](TradingPlatform.BusinessLayer.IBrandingInformation.html) |  |

#### ClosedPositions

Gets all available [ClosedPosition](TradingPlatform.BusinessLayer.ClosedPosition.html)s from open connections

##### Declaration

```csharp
public ClosedPosition[] ClosedPositions { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [ClosedPosition](TradingPlatform.BusinessLayer.ClosedPosition.html)[] |  |

#### Connections

Gets an access to all created connections and manages them

##### Declaration

```csharp
public ConnectionsManager Connections { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [ConnectionsManager](TradingPlatform.BusinessLayer.ConnectionsManager.html) |  |

#### CorporateActions

Gets all available [CorporateAction](TradingPlatform.BusinessLayer.CorporateAction.html)s from open connections

##### Declaration

```csharp
public CorporateAction[] CorporateActions { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [CorporateAction](TradingPlatform.BusinessLayer.CorporateAction.html)[] |  |

#### CurrentVersion

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public Version CurrentVersion { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Version](https://learn.microsoft.com/dotnet/api/system.version) |  |

#### CustomAccountPropertiesProvider

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public CustomAccountPropertiesProvider CustomAccountPropertiesProvider { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [CustomAccountPropertiesProvider](TradingPlatform.BusinessLayer.CustomAccountPropertiesProvider.html) |  |

#### CustomSessions

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public CustomSessionsManager CustomSessions { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [CustomSessionsManager](TradingPlatform.BusinessLayer.CustomSessionsManager.html) |  |

#### DeliveredAssets

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public DeliveredAsset[] DeliveredAssets { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DeliveredAsset](TradingPlatform.BusinessLayer.DeliveredAsset.html)[] |  |

#### Exchanges

Gets all available [Exchange](TradingPlatform.BusinessLayer.Exchange.html)s from open connections

##### Declaration

```csharp
public Exchange[] Exchanges { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Exchange](TradingPlatform.BusinessLayer.Exchange.html)[] |  |

#### Indicators

Gets an access to the all available indicators and creates them

##### Declaration

```csharp
public IndicatorManager Indicators { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IndicatorManager](TradingPlatform.BusinessLayer.IndicatorManager.html) |  |

#### Instance

Gets a singleton instance of [Core](TradingPlatform.BusinessLayer.Core.html). API entry point

##### Declaration

```csharp
public static Core Instance { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Core](TradingPlatform.BusinessLayer.Core.html) |  |

#### LocalOrders

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public LocalOrdersManager LocalOrders { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [LocalOrdersManager](TradingPlatform.BusinessLayer.LocalOrders.LocalOrdersManager.html) |  |

#### Loggers

Gets an access to the system logging mechanism

##### Declaration

```csharp
public LoggerManager Loggers { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [LoggerManager](TradingPlatform.BusinessLayer.LoggerManager.html) |  |

#### MailUtils

Gets SMTP mail service for sending emails

##### Declaration

```csharp
public MailUtils MailUtils { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [MailUtils](TradingPlatform.BusinessLayer.MailUtils.html) |  |

#### Messengers

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public MessengersManager Messengers { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [MessengersManager](TradingPlatform.BusinessLayer.Media.Messengers.MessengersManager.html) |  |

#### OrderPlacingStrategies

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public OrderPlacingStrategiesManager OrderPlacingStrategies { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [OrderPlacingStrategiesManager](TradingPlatform.BusinessLayer.OrderPlacingStrategiesManager.html) |  |

#### OrderTypes

Gets all available [OrderType](TradingPlatform.BusinessLayer.OrderType.html)s from open connections

##### Declaration

```csharp
public OrderType[] OrderTypes { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [OrderType](TradingPlatform.BusinessLayer.OrderType.html)[] |  |

#### Orders

Gets all available [Order](TradingPlatform.BusinessLayer.Order.html)s from open connections

##### Declaration

```csharp
public Order[] Orders { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Order](TradingPlatform.BusinessLayer.Order.html)[] |  |

#### Positions

Gets all available [Position](TradingPlatform.BusinessLayer.Position.html)s from open connections

##### Declaration

```csharp
public Position[] Positions { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Position](TradingPlatform.BusinessLayer.Position.html)[] |  |

#### ReportTypes

Gets all available [ReportType](TradingPlatform.BusinessLayer.ReportType.html)s from open connections. Otherwise returns empty list

##### Declaration

```csharp
public ReportType[] ReportTypes { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [ReportType](TradingPlatform.BusinessLayer.ReportType.html)[] |  |

#### Strategies

Gets an access to the all available trading strategies and manages them

##### Declaration

```csharp
public StrategyManager Strategies { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [StrategyManager](TradingPlatform.BusinessLayer.StrategyManager.html) |  |

#### SymbolTypes

Gets all available [SymbolType](TradingPlatform.BusinessLayer.SymbolType.html)s from open connections

##### Declaration

```csharp
public SymbolType[] SymbolTypes { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [SymbolType](TradingPlatform.BusinessLayer.SymbolType.html)[] |  |

#### Symbols

Gets all available [Symbol](TradingPlatform.BusinessLayer.Symbol.html)s from open connections

##### Declaration

```csharp
public Symbol[] Symbols { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Symbol](TradingPlatform.BusinessLayer.Symbol.html)[] |  |

#### SymbolsMapping

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public SymbolsMappingManager SymbolsMapping { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [SymbolsMappingManager](TradingPlatform.BusinessLayer.SymbolsMappingManager.html) |  |

#### TimeUtils

Gets a time based conversion and synchronization mechanism

##### Declaration

```csharp
public TimeUtils TimeUtils { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [TimeUtils](TradingPlatform.BusinessLayer.TimeUtils.html) |  |

#### TradingProtection

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public TradingProtector TradingProtection { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [TradingProtector](TradingPlatform.BusinessLayer.Utils.TradingProtection.TradingProtector.html) |  |

#### TradingSignals

Gets all available [TradingSignal](TradingPlatform.BusinessLayer.TradingSignal.html)s from open connections. Otherwise returns empty list

##### Declaration

```csharp
public TradingSignal[] TradingSignals { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [TradingSignal](TradingPlatform.BusinessLayer.TradingSignal.html)[] |  |

#### TradingStatus

Represents current trading status

##### Declaration

```csharp
public TradingStatus TradingStatus { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [TradingStatus](TradingPlatform.BusinessLayer.TradingStatus.html) |  |

#### VolumeAnalysis

Access to Volume Analysis calculations

##### Declaration

```csharp
public VolumeAnalysisManager VolumeAnalysis { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [VolumeAnalysisManager](TradingPlatform.BusinessLayer.VolumeAnalysisManager.html) |  |

### Methods

#### Alert(string, string, string, Action)

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public void Alert(string text, string symbolName = "", string connectionName = "", Action onConfirm = null)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | text |  |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | symbolName |  |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | connectionName |  |
| [Action](https://learn.microsoft.com/dotnet/api/system.action) | onConfirm |  |

#### Alert(string, string, string, Action, string)

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public void Alert(string text, string symbolName, string connectionName, Action onConfirm, string alertName)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | text |  |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | symbolName |  |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | connectionName |  |
| [Action](https://learn.microsoft.com/dotnet/api/system.action) | onConfirm |  |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | alertName |  |

#### Alert(Alert)

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public void Alert(Alert alert)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Alert](TradingPlatform.BusinessLayer.Utils.Alert.html) | alert |  |

#### CalculatePnL(PnLRequestParameters)

Gets Profit'n'Loss [PnL](TradingPlatform.BusinessLayer.PnL.html) with given request parameters from open connection. Otherwise returns null

##### Declaration

```csharp
public PnL CalculatePnL(PnLRequestParameters parameters)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [PnLRequestParameters](TradingPlatform.BusinessLayer.PnLRequestParameters.html) | parameters |  |

##### Returns

| Type | Description |
| --- | --- |
| [PnL](TradingPlatform.BusinessLayer.PnL.html) |  |

#### CancelOrder(CancelOrderRequestParameters)

Cancels [Order](TradingPlatform.BusinessLayer.Order.html) with given request parameters

##### Declaration

```csharp
public TradingOperationResult CancelOrder(CancelOrderRequestParameters request)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [CancelOrderRequestParameters](TradingPlatform.BusinessLayer.CancelOrderRequestParameters.html) | request |  |

##### Returns

| Type | Description |
| --- | --- |
| [TradingOperationResult](TradingPlatform.BusinessLayer.TradingOperationResult.html) |  |

#### CancelOrder(IOrder, string)

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public TradingOperationResult CancelOrder(IOrder order, string sendingSource = null)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [IOrder](TradingPlatform.BusinessLayer.IOrder.html) | order |  |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | sendingSource |  |

##### Returns

| Type | Description |
| --- | --- |
| [TradingOperationResult](TradingPlatform.BusinessLayer.TradingOperationResult.html) |  |

#### CancelOrder(Order, string)

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public TradingOperationResult CancelOrder(Order order, string sendingSource = null)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Order](TradingPlatform.BusinessLayer.Order.html) | order |  |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | sendingSource |  |

##### Returns

| Type | Description |
| --- | --- |
| [TradingOperationResult](TradingPlatform.BusinessLayer.TradingOperationResult.html) |  |

#### ClosePosition(ClosePositionRequestParameters)

Closes [Position](TradingPlatform.BusinessLayer.Position.html) with given request parameters

##### Declaration

```csharp
public TradingOperationResult ClosePosition(ClosePositionRequestParameters request)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [ClosePositionRequestParameters](TradingPlatform.BusinessLayer.ClosePositionRequestParameters.html) | request |  |

##### Returns

| Type | Description |
| --- | --- |
| [TradingOperationResult](TradingPlatform.BusinessLayer.TradingOperationResult.html) |  |

#### ClosePosition(Position, double)

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public TradingOperationResult ClosePosition(Position position, double closeQuantity = -1)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Position](TradingPlatform.BusinessLayer.Position.html) | position |  |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | closeQuantity |  |

##### Returns

| Type | Description |
| --- | --- |
| [TradingOperationResult](TradingPlatform.BusinessLayer.TradingOperationResult.html) |  |

#### ForceTimeSync()

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public void ForceTimeSync()
```

#### GetAccount(BusinessObjectInfo)

Gets an instance of exist Account or creates a new one with given info parameter

##### Declaration

```csharp
public Account GetAccount(BusinessObjectInfo accountInfo)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [BusinessObjectInfo](TradingPlatform.BusinessLayer.BusinessObjectInfo.html) | accountInfo |  |

##### Returns

| Type | Description |
| --- | --- |
| [Account](TradingPlatform.BusinessLayer.Account.html) |  |

#### GetOrderById(string, string)

Gets [Order](TradingPlatform.BusinessLayer.Order.html) instance by given Id string. Otherwise returns null

##### Declaration

```csharp
public Order GetOrderById(string orderId, string connectionId = null)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | orderId |  |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | connectionId | Must be specified if open connections total is more than one |

##### Returns

| Type | Description |
| --- | --- |
| [Order](TradingPlatform.BusinessLayer.Order.html) |  |

#### GetOrderType(string, string)

Gets [OrderType](TradingPlatform.BusinessLayer.OrderType.html) instance by given Id string. Otherwise returns null

##### Declaration

```csharp
public OrderType GetOrderType(string orderTypeId, string connectionId = null)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | orderTypeId |  |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | connectionId | Must be specified if open connections total is more than one |

##### Returns

| Type | Description |
| --- | --- |
| [OrderType](TradingPlatform.BusinessLayer.OrderType.html) |  |

#### GetOrdersHistory(OrdersHistoryRequestParameters, string)

Gets collection of [OrderHistory](TradingPlatform.BusinessLayer.OrderHistory.html) by given parameters

##### Declaration

```csharp
public IList<OrderHistory> GetOrdersHistory(OrdersHistoryRequestParameters parameters, string connectionId = null)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [OrdersHistoryRequestParameters](TradingPlatform.BusinessLayer.OrdersHistoryRequestParameters.html) | parameters |  |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | connectionId |  |

##### Returns

| Type | Description |
| --- | --- |
| [IList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)<[OrderHistory](TradingPlatform.BusinessLayer.OrderHistory.html)> |  |

#### GetPositionById(string, string)

Gets [Position](TradingPlatform.BusinessLayer.Position.html) instance by given Id string. Otherwise returns null

##### Declaration

```csharp
public Position GetPositionById(string positionId, string connectionId = null)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | positionId |  |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | connectionId | Must be specified if open connections total is more than one |

##### Returns

| Type | Description |
| --- | --- |
| [Position](TradingPlatform.BusinessLayer.Position.html) |  |

#### GetReport(ReportRequestParameters)

Returns [Report](TradingPlatform.BusinessLayer.Report.html) with given request parameters from open connection

##### Declaration

```csharp
public Report GetReport(ReportRequestParameters requestParameters)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [ReportRequestParameters](TradingPlatform.BusinessLayer.ReportRequestParameters.html) | requestParameters |  |

##### Returns

| Type | Description |
| --- | --- |
| [Report](TradingPlatform.BusinessLayer.Report.html) |  |

#### GetSymbol(BusinessObjectInfo)

Gets an instance of exist symbol or creates a new one with given info parameter

##### Declaration

```csharp
public Symbol GetSymbol(BusinessObjectInfo symbolInfo)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [BusinessObjectInfo](TradingPlatform.BusinessLayer.BusinessObjectInfo.html) | symbolInfo |  |

##### Returns

| Type | Description |
| --- | --- |
| [Symbol](TradingPlatform.BusinessLayer.Symbol.html) |  |

#### GetSymbol(GetSymbolRequestParameters, string, NonFixedListDownload)

Retrieves any [Symbol](TradingPlatform.BusinessLayer.Symbol.html) by given request parameters. Otherwise returns null

##### Declaration

```csharp
public Symbol GetSymbol(GetSymbolRequestParameters requestParameters, string connectionId = null, NonFixedListDownload downloadSymbol = NonFixedListDownload.Download)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [GetSymbolRequestParameters](TradingPlatform.BusinessLayer.GetSymbolRequestParameters.html) | requestParameters |  |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | connectionId | Must be specified if open connections total is more than one. Will search only in Synthetic symbols list if id is equal to [SYNTHETIC\_CONNECTION\_ID](TradingPlatform.BusinessLayer.Synthetic.html#TradingPlatform_BusinessLayer_Synthetic_SYNTHETIC_CONNECTION_ID) |
| [NonFixedListDownload](TradingPlatform.BusinessLayer.NonFixedListDownload.html) | downloadSymbol |  |

##### Returns

| Type | Description |
| --- | --- |
| [Symbol](TradingPlatform.BusinessLayer.Symbol.html) |  |

#### GetTrades(TradesHistoryRequestParameters, string)

Gets collection of [Trade](TradingPlatform.BusinessLayer.Trade.html) by given parameters

##### Declaration

```csharp
public IList<Trade> GetTrades(TradesHistoryRequestParameters parameters, string connectionId = null)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [TradesHistoryRequestParameters](TradingPlatform.BusinessLayer.TradesHistoryRequestParameters.html) | parameters |  |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | connectionId |  |

##### Returns

| Type | Description |
| --- | --- |
| [IList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)<[Trade](TradingPlatform.BusinessLayer.Trade.html)> |  |

#### GetTrades(TradesHistoryRequestParameters, AccountTradesLoadingCallback, string)

Gets collection of [Trade](TradingPlatform.BusinessLayer.Trade.html) by given parameters and callback

##### Declaration

```csharp
public void GetTrades(TradesHistoryRequestParameters parameters, AccountTradesLoadingCallback callback, string connectionId = null)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [TradesHistoryRequestParameters](TradingPlatform.BusinessLayer.TradesHistoryRequestParameters.html) | parameters |  |
| [AccountTradesLoadingCallback](TradingPlatform.BusinessLayer.AccountTradesLoadingCallback.html) | callback |  |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | connectionId |  |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception) |  |

#### InitializeBrandingInformation()

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public void InitializeBrandingInformation()
```

#### LockAccount(Account)

Lock trading for the specified account

##### Declaration

```csharp
public void LockAccount(Account account)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Account](TradingPlatform.BusinessLayer.Account.html) | account |  |

#### ModifyOrder(ModifyOrderRequestParameters)

Modifies [Order](TradingPlatform.BusinessLayer.Order.html) by given request parameters

##### Declaration

```csharp
public TradingOperationResult ModifyOrder(ModifyOrderRequestParameters request)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [ModifyOrderRequestParameters](TradingPlatform.BusinessLayer.ModifyOrderRequestParameters.html) | request |  |

##### Returns

| Type | Description |
| --- | --- |
| [TradingOperationResult](TradingPlatform.BusinessLayer.TradingOperationResult.html) |  |

#### ModifyOrder(Order, TimeInForce, double, double, double, double)

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public TradingOperationResult ModifyOrder(Order order, TimeInForce timeInForce = TimeInForce.Default, double quantity = 1, double price = -1, double triggerPrice = -1, double trailOffset = -1)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Order](TradingPlatform.BusinessLayer.Order.html) | order |  |
| [TimeInForce](TradingPlatform.BusinessLayer.TimeInForce.html) | timeInForce |  |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | quantity |  |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | price |  |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | triggerPrice |  |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | trailOffset |  |

##### Returns

| Type | Description |
| --- | --- |
| [TradingOperationResult](TradingPlatform.BusinessLayer.TradingOperationResult.html) |  |

#### PlaceOrder(PlaceOrderRequestParameters)

Places [Order](TradingPlatform.BusinessLayer.Order.html) with given request parameters

##### Declaration

```csharp
public TradingOperationResult PlaceOrder(PlaceOrderRequestParameters request)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [PlaceOrderRequestParameters](TradingPlatform.BusinessLayer.PlaceOrderRequestParameters.html) | request |  |

##### Returns

| Type | Description |
| --- | --- |
| [TradingOperationResult](TradingPlatform.BusinessLayer.TradingOperationResult.html) |  |

#### PlaceOrder(Symbol, Account, Side, TimeInForce, double, double, double, double)

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public TradingOperationResult PlaceOrder(Symbol symbol, Account account, Side side, TimeInForce timeInForce = TimeInForce.Day, double quantity = 1, double price = -1, double triggerPrice = -1, double trailOffset = -1)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Symbol](TradingPlatform.BusinessLayer.Symbol.html) | symbol |  |
| [Account](TradingPlatform.BusinessLayer.Account.html) | account |  |
| [Side](TradingPlatform.BusinessLayer.Side.html) | side |  |
| [TimeInForce](TradingPlatform.BusinessLayer.TimeInForce.html) | timeInForce |  |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | quantity |  |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | price |  |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | triggerPrice |  |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | trailOffset |  |

##### Returns

| Type | Description |
| --- | --- |
| [TradingOperationResult](TradingPlatform.BusinessLayer.TradingOperationResult.html) |  |

#### PlaceOrders(ICollection<PlaceOrderRequestParameters>, GroupOrderType)

Places multiple [Order](TradingPlatform.BusinessLayer.Order.html)s with given request parameters

##### Declaration

```csharp
public void PlaceOrders(ICollection<PlaceOrderRequestParameters> requests, GroupOrderType groupOrderType = GroupOrderType.None)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [ICollection](https://learn.microsoft.com/dotnet/api/system.collections.generic.icollection-1)<[PlaceOrderRequestParameters](TradingPlatform.BusinessLayer.PlaceOrderRequestParameters.html)> | requests |  |
| [GroupOrderType](TradingPlatform.BusinessLayer.GroupOrderType.html) | groupOrderType |  |

#### RequestOTP(OTPHolder, string, string)

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public void RequestOTP(OTPHolder otpHolder, string title, string text)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [OTPHolder](TradingPlatform.BusinessLayer.Settings.OTP.OTPHolder.html) | otpHolder |  |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | title |  |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | text |  |

#### SendCustomRequest(string, RequestParameters)

Sends custom request if connection with given Id is open

##### Declaration

```csharp
public void SendCustomRequest(string connectionId, RequestParameters parameters)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | connectionId |  |
| [RequestParameters](TradingPlatform.BusinessLayer.RequestParameters.html) | parameters |  |

#### SubscribeToCustomMessages(Action<CustomMessage>, params int[])

Subscribe on custom messages

##### Declaration

```csharp
public void SubscribeToCustomMessages(Action<CustomMessage> handler, params int[] messagesTypes)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[CustomMessage](TradingPlatform.BusinessLayer.Integration.CustomMessage.html)> | handler | custom message handler |
| [int](https://learn.microsoft.com/dotnet/api/system.int32)[] | messagesTypes | custom messages Id |

#### UnLockAccount(Account)

Unlock trading for the specified account

##### Declaration

```csharp
public void UnLockAccount(Account account)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Account](TradingPlatform.BusinessLayer.Account.html) | account |  |

#### UnsubscribeFromCustomMessages(Action<CustomMessage>, params int[])

Unsubscribe from custom messages

##### Declaration

```csharp
public void UnsubscribeFromCustomMessages(Action<CustomMessage> handler, params int[] messagesTypes)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[CustomMessage](TradingPlatform.BusinessLayer.Integration.CustomMessage.html)> | handler | custom message handler |
| [int](https://learn.microsoft.com/dotnet/api/system.int32)[] | messagesTypes | custom messages Id |

### Events

#### AccountAdded

Will be triggered when new [Account](TradingPlatform.BusinessLayer.Account.html) added to the core

##### Declaration

```csharp
public event Action<Account> AccountAdded
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[Account](TradingPlatform.BusinessLayer.Account.html)> |  |

#### ClosedPositionAdded

Will be triggered when new [ClosedPosition](TradingPlatform.BusinessLayer.ClosedPosition.html) added

##### Declaration

```csharp
public event Action<ClosedPosition> ClosedPositionAdded
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[ClosedPosition](TradingPlatform.BusinessLayer.ClosedPosition.html)> |  |

#### ClosedPositionRemoved

Will be triggered when [ClosedPosition](TradingPlatform.BusinessLayer.ClosedPosition.html) removed

##### Declaration

```csharp
public event Action<ClosedPosition> ClosedPositionRemoved
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[ClosedPosition](TradingPlatform.BusinessLayer.ClosedPosition.html)> |  |

#### CorporateActionAdded

Will be triggered when new [CorporateAction](TradingPlatform.BusinessLayer.CorporateAction.html) occured

##### Declaration

```csharp
public event Action<CorporateAction> CorporateActionAdded
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[CorporateAction](TradingPlatform.BusinessLayer.CorporateAction.html)> |  |

#### DealTicketReceived

Will be triggered when new [DealTicket](TradingPlatform.BusinessLayer.DealTicket.html) received

##### Declaration

```csharp
public event Action<DealTicket> DealTicketReceived
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[DealTicket](TradingPlatform.BusinessLayer.DealTicket.html)> |  |

#### DeliveredAssetAdded

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public event Action<DeliveredAsset> DeliveredAssetAdded
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[DeliveredAsset](TradingPlatform.BusinessLayer.DeliveredAsset.html)> |  |

#### DeliveredAssetRemoved

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public event Action<DeliveredAsset> DeliveredAssetRemoved
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[DeliveredAsset](TradingPlatform.BusinessLayer.DeliveredAsset.html)> |  |

#### NewPerformedRequest

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public event EventHandler<PerformedRequestEventArgs> NewPerformedRequest
```

##### Event Type

| Type | Description |
| --- | --- |
| [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler-1)<[PerformedRequestEventArgs](TradingPlatform.BusinessLayer.Utils.PerformedRequestEventArgs.html)> |  |

#### NewRequest

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public event EventHandler<RequestEventArgs> NewRequest
```

##### Event Type

| Type | Description |
| --- | --- |
| [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler-1)<[RequestEventArgs](TradingPlatform.BusinessLayer.Utils.RequestEventArgs.html)> |  |

#### OnAlert

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public event Action<Alert> OnAlert
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[Alert](TradingPlatform.BusinessLayer.Utils.Alert.html)> |  |

#### OnAskUserConfirmationForTradingWithRunningEmulator

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public event Func<string, string, bool> OnAskUserConfirmationForTradingWithRunningEmulator
```

##### Event Type

| Type | Description |
| --- | --- |
| [Func](https://learn.microsoft.com/dotnet/api/system.func-3)<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string), [bool](https://learn.microsoft.com/dotnet/api/system.boolean)> |  |

#### OnRequestOTP

The main entry point in the API. Core keeps access to all business logic entities and their properties:
connections, accounts, symbols, positions, orders, etc. Some of them can be reached through using managers or directly via specified collections.
You can always access the Core object via static Core.Instance property.

##### Declaration

```csharp
public event Action<OTPHolder, string, string> OnRequestOTP
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-3)<[OTPHolder](TradingPlatform.BusinessLayer.Settings.OTP.OTPHolder.html), [string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)> |  |

#### OnTradingStatusChanged

Will be triggered when [TradingStatus](TradingPlatform.BusinessLayer.Core.html#TradingPlatform_BusinessLayer_Core_TradingStatus) changed

##### Declaration

```csharp
public event Action<TradingStatus> OnTradingStatusChanged
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[TradingStatus](TradingPlatform.BusinessLayer.TradingStatus.html)> |  |

#### OrderAdded

Will be triggered when new [Order](TradingPlatform.BusinessLayer.Order.html) placed

##### Declaration

```csharp
public event Action<Order> OrderAdded
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[Order](TradingPlatform.BusinessLayer.Order.html)> |  |

#### OrderRemoved

Will be triggered when [Order](TradingPlatform.BusinessLayer.Order.html) canceled

##### Declaration

```csharp
public event Action<Order> OrderRemoved
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[Order](TradingPlatform.BusinessLayer.Order.html)> |  |

#### OrdersHistoryAdded

Will be triggered when new [OrderHistory](TradingPlatform.BusinessLayer.OrderHistory.html) added

##### Declaration

```csharp
public event Action<OrderHistory> OrdersHistoryAdded
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[OrderHistory](TradingPlatform.BusinessLayer.OrderHistory.html)> |  |

#### PositionAdded

Will be triggered when new [Position](TradingPlatform.BusinessLayer.Position.html) opened

##### Declaration

```csharp
public event Action<Position> PositionAdded
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[Position](TradingPlatform.BusinessLayer.Position.html)> |  |

#### PositionRemoved

Will be triggered when [Position](TradingPlatform.BusinessLayer.Position.html) closed

##### Declaration

```csharp
public event Action<Position> PositionRemoved
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[Position](TradingPlatform.BusinessLayer.Position.html)> |  |

#### SymbolAdded

Will be triggered when new [Symbol](TradingPlatform.BusinessLayer.Symbol.html) added to the core

##### Declaration

```csharp
public event Action<Symbol> SymbolAdded
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[Symbol](TradingPlatform.BusinessLayer.Symbol.html)> |  |

#### TradeAdded

Will be triggered when new [Trade](TradingPlatform.BusinessLayer.Trade.html) occured

##### Declaration

```csharp
public event Action<Trade> TradeAdded
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[Trade](TradingPlatform.BusinessLayer.Trade.html)> |  |

#### TradingSignalUpdate

Will be triggered when [TradingSignal](TradingPlatform.BusinessLayer.TradingSignal.html) created/chenged/removed

##### Declaration

```csharp
public event EventHandler<TradingSignalEventArgs> TradingSignalUpdate
```

##### Event Type

| Type | Description |
| --- | --- |
| [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler-1)<[TradingSignalEventArgs](TradingPlatform.BusinessLayer.TradingSignalEventArgs.html)> |  |