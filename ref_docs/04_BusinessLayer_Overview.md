# BusinessLayer Namespace Overview
> Namespace: `TradingPlatform.BusinessLayer`
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.html

This is the primary namespace containing all trading business objects.

---

## Key Classes

### Market Data

| Class | Description |
|---|---|
| `Symbol` | Tradeable instrument — NQ, ES, AKBNK etc. |
| `Quote` | Bid/ask update |
| `Last` | Trade tick (price, size, time) |
| `Mark` | Mark price update |
| `MessageQuote` | Level 2 / DOM quote message |
| `HistoricalData` | Historical bars/ticks with indicators |
| `IHistoryItem` | Base interface for bar/tick items |
| `HistoryItemBar` | OHLCV bar |
| `HistoryItemLast` | Trade tick history item |
| `HistoryRequestParameters` | Parameters for requesting history |
| `HistoryAggregation` | Base aggregation type |
| `HistoryAggregationTime` | Time-based bars |
| `HistoryAggregationVolume` | Volume bars |
| `HistoryAggregationTick` | Tick bars |
| `HistoryAggregationPriceRange` | Range bars |

### Orders & Trading

| Class | Description |
|---|---|
| `Order` | Active order |
| `OrderHistory` | Historical order record |
| `OrderType` | Order type definition (Market, Limit, Stop, etc.) |
| `PlaceOrderRequestParameters` | Parameters to place an order |
| `ModifyOrderRequestParameters` | Parameters to modify an order |
| `CancelOrderRequestParameters` | Parameters to cancel an order |
| `TradingOperationResult` | Result of any trading operation |
| `TradingOperationResultStatus` | Success / Failure / Rejected |
| `Side` | Buy / Sell |
| `TimeInForce` | GTC / DAY / IOC / FOK / GTD |

### Positions & Accounts

| Class | Description |
|---|---|
| `Position` | Open position |
| `ClosedPosition` | Closed position record |
| `ClosePositionRequestParameters` | Parameters to close a position |
| `Account` | Trading account |
| `AccountOperation` | Account-level operation |
| `PnL` | Profit and Loss |
| `PnLRequestParameters` | PnL calculation parameters |

### Connections

| Class | Description |
|---|---|
| `Connection` | A vendor connection object |
| `ConnectionsManager` | Manages all connections |
| `ConnectionInfo` | Static info about a connection |
| `ConnectionState` | Enum: Connected/Connecting/Disconnected/Fail |
| `ConnectionType` | Type classification |
| `ConnectionResult` | Result of connect attempt |

### Indicators

| Class | Description |
|---|---|
| `Indicator` | Base class for all indicators |
| `IndicatorManager` | Creates and manages indicators |
| `IndicatorsCollection` | Collection of indicators on HistoricalData |
| `BuiltInIndicators` | Access to built-in indicators (SMA, EMA, etc.) |
| `IndicatorCalculationBehavior` | How calculations are triggered |

### Strategies

| Class | Description |
|---|---|
| `Strategy` | **Base class** — extend this for all trading strategies |
| `StrategyManager` | Loads, runs, stops strategies |
| `StrategyState` | Running / Stopped / Paused |

### Utilities

| Class | Description |
|---|---|
| `LoggerManager` | Logging (via `Core.Instance.Loggers.Log(...)`) |
| `TimeUtils` | Time conversion, sync, DateTimeUtcNow |
| `VolumeAnalysisManager` | Volume profile, footprint calculations |
| `MailUtils` | SMTP email sending |
| `Alert` | Alert object |
| `TradingStatus` | Platform-wide trading status |
| `TradingProtector` | Risk/protection guard |
| `BusinessObjectInfo` | Key to look up symbols/accounts |
| `SettingItem` | Generic setting item (used for indicators, connections) |

---

## Strategy Base Class Pattern

```csharp
using TradingPlatform.BusinessLayer;

public class MyDOMStrategy : Strategy
{
    // Settings exposed in UI
    [InputParameter("Quantity")]
    public int Qty = 1;

    private Symbol symbol;
    private Account account;

    public MyDOMStrategy() : base()
    {
        Name        = "DOM Scalper";
        Description = "NQ DOM scalping strategy";
    }

    protected override void OnRun()
    {
        symbol  = Core.Instance.GetSymbol(...);
        account = Core.Instance.Accounts.FirstOrDefault();

        symbol.NewLevel2 += OnLevel2Update;
    }

    protected override void OnStop()
    {
        symbol.NewLevel2 -= OnLevel2Update;
    }

    private void OnLevel2Update(Symbol sym, Level2UpdateEventArgs e)
    {
        // Access DOM state
        var bids = sym.DepthOfMarket.GetAsks();
        var asks = sym.DepthOfMarket.GetBids();

        // Evaluate and place orders
        if (ShouldBuy(bids, asks))
        {
            Core.Instance.PlaceOrder(new PlaceOrderRequestParameters
            {
                Symbol      = symbol,
                Account     = account,
                Side        = Side.Buy,
                OrderTypeId = OrderType.Limit,
                Price       = bids[0].Price,
                Quantity    = Qty
            });
        }
    }
}
```

---

## Enums Quick Reference

```csharp
// Order types (string IDs)
OrderType.Market  = "Market"
OrderType.Limit   = "Limit"
OrderType.Stop    = "Stop"
OrderType.StopLimit = "StopLimit"

// Side
Side.Buy
Side.Sell

// TimeInForce
TimeInForce.Day
TimeInForce.GoodTillCancel
TimeInForce.ImmediateOrCancel
TimeInForce.FillOrKill

// TradingOperationResultStatus
TradingOperationResultStatus.Success
TradingOperationResultStatus.Failure

// SeekOriginHistory
SeekOriginHistory.Begin  // oldest
SeekOriginHistory.End    // newest (default)
```

---

## Symbol — Key Members

```csharp
// Identity
symbol.Name           // "NQ"
symbol.Description    // "E-mini NASDAQ-100"
symbol.ConnectionId
symbol.SymbolType     // Futures, Equity, etc.
symbol.Exchange

// Pricing
symbol.Last            // Most recent Last
symbol.Bid / symbol.Ask
symbol.High / symbol.Low
symbol.Volume
symbol.TickSize
symbol.PointCost       // $ value per point

// History
HistoricalData hd = symbol.GetHistory(request);

// Subscriptions
symbol.NewLast   += handler;  // Trade ticks
symbol.NewQuote  += handler;  // Bid/ask
symbol.NewLevel2 += handler;  // DOM
symbol.NewMark   += handler;  // Mark price

// DOM
var dom = symbol.DepthOfMarket;
Level2Quote[] bids = dom.GetBids();
Level2Quote[] asks = dom.GetAsks();

// Level2Quote members
bid.Price
bid.Size     // Volume at that level
bid.Count    // Order count at that level
```
