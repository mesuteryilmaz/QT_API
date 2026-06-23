# NeoNix-Lab — Code Analysis Notes
> Fetched: 2026-04-10
> Repos: Quantower-Orders-Manager | NeoQuantowerTools

---

## Repo 1: Quantower-Orders-Manager

### Architecture

```
IConditionable (interface)
    └─ ConditionableBase<R> (abstract)
            ├─ Trade(side, price)         → TpSlManager<R>.PlaceOrder()
            ├─ Close()                    → TpSlManager<R>.Stop()
            ├─ GetSlTpItemById(guid)      → looks up live SlTpItems
            ├─ abstract GeTp(R, guid)     → YOU implement
            ├─ abstract GetSl(R, guid)    → YOU implement
            └─ abstract SetCondictionHolder()

SlTpCondictionHolder<T> (struct)
    ├─ SlDelegateObj[] + SlDelegate[]    → SL indicator + function pairs
    ├─ TpDelegateObj[] + TpDelegate[]    → TP indicator + function pairs
    └─ Computator: TpSlComputator<T>     → places/modifies bracket orders

TpSlComputator<T>
    ├─ PlaceOrder(entryOrder, SlTpItems) → fires SL (Stop) + TP (Limit) orders
    └─ UpdateOrder(List<SlTpItems>)      → modifies existing SL/TP prices

TpSlManager<T> (static singleton)
    ├─ init()         → subscribes Core events: OrderAdded, TradeAdded, OrdersHistoryAdded
    ├─ PlaceOrder()   → GUID tagging, exposure limits, POST_ONLY flag
    ├─ Stop()         → unsubscribes all Core events
    └─ SlTpItems[]    → live list of all bracket sets
```

### Key Patterns to Borrow

**1. GUID-based order correlation**
```csharp
// TpSlManager assigns a Guid as order Comment before placing:
Guid guid = Guid.NewGuid();
requestParameters.Comment = guid.ToString();
UnfilledIds.Add(guid.ToString());

// When OrderAdded fires, match by Comment:
if (UnfilledIds.Contains(obj.Comment))
    SlTpItems.Add(new SlTpItems(obj, obj.Comment));
    ListOfDelegates.Computator.PlaceOrder(obj, ...);
```
This is the correct pattern for DOM scalping where you fire many bracket orders
quickly and need to track which SL/TP belongs to which entry.

**2. Exposure limiting**
```csharp
if (MaxShortExo <= ShortExpo & reqParameters.Side == Side.Sell) return;
if (MaxLongExo <= LongExpo  & reqParameters.Side == Side.Buy)  return;
```
Simple max-exposure gate before any order reaches the exchange.

**3. Delegate-injected SL/TP (RaphaelStrategy.cs — real example)**
```csharp
// TP: 1% above entry for Buy, 1% below for Sell
public override double GeTp(Indicator indicator, string guidOrdersReference)
{
    SlTpItems item = base.GetSlTpItemById(guidOrdersReference);
    return item.Side == Side.Buy
        ? item.EntryPrice * 1.01
        : item.EntryPrice * 0.99;
}
```
Replace `1.01` with your tick-based distance from DOM analysis.

**4. Volume profile init pattern (QuantStrategy.cs)**
```csharp
// Deferred HD init on first Last/Quote — correct pattern for live strategies
private void _Symbol_NewLast(Symbol symbol, Last last)
{
    if (this.hd == null)
    {
        this.hd = this._Symbol.GetHistory(new HistoryRequestParameters { ... });
        var progress = this.hd.CalculateVolumeProfile(new VolumeAnalysisCalculationParameters
        {
            CalculatePriceLevels = false,
            DeltaCalculationType = _Symbol.DeltaCalculationType,
        });
        progress.ProgressChanged += VolumeAnalysisCalculationProgress_ProgressChanged;
        this.hd.NewHistoryItem += Hd_NewHistoryItem;
        this._Symbol.NewLast -= this._Symbol_NewLast; // unsubscribe self
    }
}
```

### Known Issues / Code Quality

| Issue | Location | Impact |
|---|---|---|
| `catch (Exception)` with no logging in `ClosedAll()` | `SlTpItems.cs` | Silent failures — against your coding standards |
| `TpItems.Count != this.tp_items` check uses wrong list (`sl_items`) | `TpSlComputator.cs:UpdateOrder` | Bug: will log "Unmatching Orders" incorrectly |
| `REDUCE_ONLY` flag on SL/TP orders | `TpSlComputator.cs` | Only valid for crypto — **remove for IB/CME futures** |
| `POST_ONLY` flag on entry orders | `TpSlManager.cs:PlaceOrder` | IB rejects this for many order types — **remove for CME** |
| `UnaddedSl/Tp` lists are never cleaned if order fails | `SlTpItems.cs` | Memory leak on repeated failed orders |
| `MaxShortExo = maxLongExpo` (copy-paste bug) | `ConditionableBase.cs:ctor` | MaxShortExpo is set twice, MaxLongExpo never set |

---

## Repo 2: NeoQuantowerTools

### Architecture

```
Neo.Quantower.Abstractions (net472 + net8 NuGet)
    ├─ AsyncTaskQueue          — prioritized async task queue
    ├─ RingBuffer<T>           — fixed-size circular buffer
    ├─ Subscription            — IDisposable unsubscribe token
    ├─ PipeFactory             — lazy singleton loader via reflection
    └─ Interfaces: IAsyncTaskQueueFactory, ICustomLogger<T>, IPipeDispatcher, IRingBuffer<T>

Neo.Quantower.Toolkit (net472 + net8 NuGet)
    └─ PipeDispatcher          — Named Pipe pub/sub message bus
            ├─ PipeServer      — NamedPipeServerStream
            ├─ PipeClient      — NamedPipeClientStream
            ├─ DispatcherRegistry — ConcurrentDictionary<typeName, ImmutableHashSet<handlers>>
            └─ MessageEnvelope — {TypeName, JsonPayload}
```

### AsyncTaskQueue — Directly Useful for DOM Strategy

```csharp
// In your DOM scalping strategy:
private AsyncTaskQueue _orderQueue = new AsyncTaskQueue
{
    MaxRetryAttempts = 2,
    TaskTimeout = TimeSpan.FromSeconds(5)
};

// In Level2 handler (fires on every DOM tick — never block here):
private void OnLevel2Update(Symbol sym, Level2Quote level2, DOMQuote dom)
{
    if (ShouldPlaceOrder(dom))
    {
        // Offload to queue — non-blocking
        _orderQueue.Enqueue(async ct =>
        {
            var result = Core.Instance.PlaceOrder(new PlaceOrderRequestParameters { ... });
            if (result.Status != TradingOperationResultStatus.Success)
                Log($"Order failed: {result.Message}", StrategyLoggingLevel.Error);
        }, TaskPriority.High);
    }
}

// Cleanup
protected override void OnStop()
{
    _orderQueue.Dispose();
}
```

Priority levels: `High`, `Normal`, `Low`
Soft backpressure: tasks delayed (not dropped) when queue > `MaxQueueLength` (default 100)

### RingBuffer<T> — Useful for DOM Snapshot History

```csharp
// Keep rolling window of last N DOM snapshots
var domHistory = new RingBuffer<DomSnapshot>(capacity: 50, fromBeginning: false);
// fromBeginning=false → index[0] = most recent (same convention as HistoricalData)

domHistory.Add(new DomSnapshot { ... });
var latest = domHistory[0];   // most recent
var prev   = domHistory[1];   // one tick ago

// Range query
foreach (var snap in domHistory.GetRange(0, 10))
    ProcessSnapshot(snap);
```

### PipeDispatcher — Optional, for Multi-Strategy Coordination

Publish/subscribe over Windows Named Pipes. Useful if you ever run multiple
Quantower strategy instances that need to coordinate (e.g. a signal generator
strategy + an execution strategy in separate runners).

```csharp
// Strategy A (signal generator):
await PipeDispatcher.Instance.Initialize("QT_DOM_Bus");
await PipeDispatcher.Instance.PublishAsync(new DomSignal { Side = Side.Buy, Price = 21000.25 });

// Strategy B (executor):
await PipeDispatcher.Instance.Initialize("QT_DOM_Bus"); // auto-falls back to client
var sub = PipeDispatcher.Instance.Subscribe<DomSignal>(async signal =>
{
    Core.Instance.PlaceOrder(new PlaceOrderRequestParameters { ... });
}, tag: Guid.NewGuid());
```

### Code Quality Assessment

| Component | Quality | Notes |
|---|---|---|
| `AsyncTaskQueue` | ✅ Good | Thread-safe, clean dispose, proper CancellationToken usage |
| `RingBuffer<T>` | ✅ Good | `ArrayPool<T>` usage, both directions, range queries |
| `PipeDispatcher` | 🟡 Medium | No reconnect logic yet; `ReadLoopAsync` buffer fixed at 8192 bytes (fine for small messages) |
| `PipeFactory` | 🟡 Medium | Reflection-based loading is fragile if assembly name changes |
| Tests | 🟡 Partial | `AsyncTaskQueueTests.cs` + `RingBufferTests.cs` exist — good sign |
