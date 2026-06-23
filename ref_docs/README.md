# QT API Ref Docs — Setup & Usage
> For: QT_API (MBO Market Data Analytics — NQ/MNQ)

---

## What's In This Folder

| File | Description |
|---|---|
| `00_INDEX.md` | Navigation index |
| `01_Core.md` | `Core` class — full API entry point reference |
| `02_Connection.md` | `Connection` class — state, events, trading data |
| `03_HistoricalData.md` | `HistoricalData` class — bars, indicators, events |
| `04_BusinessLayer_Overview.md` | Full namespace map, key classes, patterns |
| `crawler/fetch_all_docs.py` | Script to download ALL pages from api.quantower.com |

---

## Step 1 — Copy This Folder

Place the entire `ref_docs` folder at:
```
C:\Users\mesuteryilmaz\Desktop\QT_API\ref_docs\
```

---

## Step 2 — Run the Crawler (one-time, ~10-15 min)

The crawler will download every single class page from `api.quantower.com`
and save them as clean Markdown files.

```powershell
# Install dependencies (once)
pip install requests beautifulsoup4 markdownify

# Run crawler
cd C:\Users\mesuteryilmaz\Desktop\QT_API\ref_docs
python crawler\fetch_all_docs.py
```

Output goes to:
```
ref_docs\api_quantower\
```

---

## Step 3 — Open in VS Code

```powershell
code C:\Users\mesuteryilmaz\Desktop\QT_API\ref_docs
```

Install the **Markdown All in One** extension for navigation, preview, and search.

**Search across all docs:**
```
Ctrl+Shift+F → type class name (e.g., "DepthOfMarket", "Level2Quote")
```

---

## Key Classes for DOM Strategy

| What you need | Class |
|---|---|
| Entry point | `Core.Instance` |
| Get NQ/MNQ symbol | `Core.Instance.GetSymbol(...)` |
| Subscribe to DOM | `symbol.NewLevel2 += handler` |
| Read DOM levels | `symbol.DepthOfMarket.GetBids() / GetAsks()` |
| Place limit order | `Core.Instance.PlaceOrder(PlaceOrderRequestParameters)` |
| Cancel order | `Core.Instance.CancelOrder(order)` |
| Close position | `Core.Instance.ClosePosition(position)` |
| Get historical bars | `symbol.GetHistory(HistoryRequestParameters)` |
| Log messages | `Core.Instance.Loggers.Log("msg")` |
| Session time check | `Core.Instance.TimeUtils.DateTimeUtcNow` |

---

## Notes

- Crawler respects the site with 0.3s delay between requests
- Safe to re-run — skips already-downloaded files
- All pages saved as UTF-8 Markdown
