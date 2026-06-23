# Class Core
> Namespace: `TradingPlatform.BusinessLayer`
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Core.html

The main entry point of the Quantower API. Access via:
```csharp
Core core = Core.Instance;
```

---

## Properties

| Property | Type | Description |
|---|---|---|
| `Instance` | `Core` (static) | Singleton entry point |
| `Accounts` | `Account[]` | All accounts from open connections |
| `Symbols` | `Symbol[]` | All symbols from open connections |
| `Orders` | `Order[]` | All orders from open connections |
| `Positions` | `Position[]` | All positions from open connections |
| `ClosedPositions` | `ClosedPosition[]` | All closed positions |
| `OrderTypes` | `OrderType[]` | All order types from open connections |
| `SymbolTypes` | `SymbolType[]` | All symbol types from open connections |
| `Exchanges` | `Exchange[]` | All exchanges from open connections |
| `Assets` | `Asset[]` | All assets from open connections |
| `CorporateActions` | `CorporateAction[]` | All corporate actions |
| `TradingSignals` | `TradingSignal[]` | All trading signals |
| `ReportTypes` | `ReportType[]` | All report types |
| `DeliveredAssets` | `DeliveredAsset[]` | All delivered assets |
| `AccountOperations` | `AccountOperation[]` | Account operations |
| `Connections` | `ConnectionsManager` | Manages connections |
| `Strategies` | `StrategyManager` | Manages trading strategies |
| `Indicators` | `IndicatorManager` | Manages indicators |
| `Loggers` | `LoggerManager` | System logging |
| `MailUtils` | `MailUtils` | SMTP mail service |
| `Messengers` | `MessengersManager` | Messaging manager |
| `VolumeAnalysis` | `VolumeAnalysisManager` | Volume analysis calculations |
| `LocalOrders` | `LocalOrdersManager` | Local order management |
| `OrderPlacingStrategies` | `OrderPlacingStrategiesManager` | Order placing strategies |
| `TimeUtils` | `TimeUtils` | Time conversion/synchronization |
| `TradingProtection` | `TradingProtector` | Trading protection |
| `TradingStatus` | `TradingStatus` | Current trading status (get/set) |
| `CustomSessions` | `CustomSessionsManager` | Custom session management |
| `SymbolsMapping` | `SymbolsMappingManager` | Symbol mapping |
| `CustomAccountPropertiesProvider` | `CustomAccountPropertiesProvider` | Custom account properties |
| `AdvancedTradingOperations` | `AdvancedTradingOperations` | Advanced trading ops |
| `BrandingInformation` | `IBrandingInformation` | Branding info |
| `CurrentVersion` | `Version` | Current platform version |

---

## Methods

### Symbol Methods

```csharp
// Get symbol by business object info
Symbol GetSymbol(BusinessObjectInfo symbolInfo)

// Get symbol by request parameters
Symbol GetSymbol(GetSymbolRequestParameters requestParameters,
                 string connectionId = null,
                 NonFixedListDownload downloadSymbol = NonFixedListDownload.Download)
```

### Order Methods

```csharp
// Place order
TradingOperationResult PlaceOrder(PlaceOrderRequestParameters request)

// Modify order
TradingOperationResult ModifyOrder(ModifyOrderRequestParameters request)

// Cancel order (3 overloads)
TradingOperationResult CancelOrder(CancelOrderRequestParameters request)
TradingOperationResult CancelOrder(IOrder order, string sendingSource = null)
TradingOperationResult CancelOrder(Order order, string sendingSource = null)

// Get order by ID
Order GetOrderById(string orderId, string connectionId = null)

// Get order type by ID
OrderType GetOrderType(string orderTypeId, string connectionId = null)

// Get orders history
IList<OrderHistory> GetOrdersHistory(OrdersHistoryRequestParameters parameters,
                                      string connectionId = null)
```

### Position Methods

```csharp
// Close position (2 overloads)
TradingOperationResult ClosePosition(ClosePositionRequestParameters request)
TradingOperationResult ClosePosition(Position position, double closeQuantity = -1)

// Get position by ID
Position GetPositionById(string positionId, string connectionId = null)
```

### Account Methods

```csharp
Account GetAccount(BusinessObjectInfo accountInfo)
PnL CalculatePnL(PnLRequestParameters parameters)
Report GetReport(ReportRequestParameters requestParameters)
```

### Alert Methods

```csharp
// 3 overloads
void Alert(string text, string symbolName = "", string connectionName = "",
           Action onConfirm = null)
void Alert(string text, string symbolName, string connectionName,
           Action onConfirm, string alertName)
void Alert(Alert alert)
```

### Misc

```csharp
void ForceTimeSync()
```

---

## Events (via Core.Instance)

Subscribe through specific managers or business objects.
Key events are on `Symbol`, `Connection`, and `StrategyManager`.

---

## Common Patterns for DOM Strategy

```csharp
// Subscribe to DOM (Level2) updates
var symbol = Core.Instance.GetSymbol(new BusinessObjectInfo("NQ", "CME", ...));
symbol.NewLevel2 += OnNewLevel2;

// Place limit order
var result = Core.Instance.PlaceOrder(new PlaceOrderRequestParameters
{
    Symbol       = symbol,
    Account      = Core.Instance.Accounts[0],
    Side         = Side.Buy,
    OrderTypeId  = OrderType.Limit,
    Price        = 21000.25,
    Quantity     = 1
});

if (result.Status == TradingOperationResultStatus.Success)
    Core.Instance.Loggers.Log($"Order placed: {result.OrderId}");
```
