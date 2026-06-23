# Quantower API Reference Docs
> Source: https://api.quantower.com/
> Fetched: 2026-04-10
> Project: QT_API — MBO Market Data Analytics (NQ/MNQ)

---

## API Structure

| Namespace | URL | Description |
|---|---|---|
| `TradingPlatform.BusinessLayer.Core` | `/docs/TradingPlatform.BusinessLayer.Core.html` | Main entry point — access to all business logic entities |
| `TradingPlatform.BusinessLayer.Connection` | `/docs/TradingPlatform.BusinessLayer.Connection.html` | Connection info and trading data access |
| `TradingPlatform.BusinessLayer.HistoricalData` | `/docs/TradingPlatform.BusinessLayer.HistoricalData.html` | Historical data + indicator control |
| `TradingPlatform.BusinessLayer` | `/docs/TradingPlatform.BusinessLayer.html` | All business objects (Symbol, Order, Position, etc.) |

---

## Quick Reference — Entry Points

```csharp
// Singleton access to everything
Core core = Core.Instance;

// Collections
Account[]     accounts   = core.Accounts;
Symbol[]      symbols    = core.Symbols;
Order[]        orders    = core.Orders;
Position[]    positions  = core.Positions;

// Managers
ConnectionsManager  conn     = core.Connections;
StrategyManager     strats   = core.Strategies;
IndicatorManager    inds     = core.Indicators;
LoggerManager       log      = core.Loggers;
VolumeAnalysisManager va     = core.VolumeAnalysis;
```

---

## Files In This Folder

| File | Content |
|---|---|
| `01_Core.md` | Class Core — all properties, methods, events |
| `02_Connection.md` | Class Connection — state, trading objects, news |
| `03_HistoricalData.md` | Class HistoricalData — bars, indicators, events |
| `04_BusinessLayer_Overview.md` | BusinessLayer namespace overview |
| `crawler/fetch_all_docs.py` | **Python script to download all API pages locally** |

---

## Getting Started Links

- [Introduction](https://help.quantower.com/quantower-algo/introduction)
- [Install for Visual Studio](https://help.quantower.com/quantower-algo/installing-visual-studio)
- [Create a simple indicator](https://help.quantower.com/quantower-algo/simple-indicator)
- [Create a simple strategy](https://help.quantower.com/quantower-algo/simple-strategy)
