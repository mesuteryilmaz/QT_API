# MBO Market Data Analytics — Project Overview

This document describes the project's philosophy, the full signal chain from raw
feed to execution decision, the three-phase adaptive stack, and the key design
decisions behind the architecture. For build/install steps see
[README.md](README.md); for the full metric specification and configuration
reference see [DataAnalytics.md](DataAnalytics.md).

---

## Philosophy

Three convictions drive this project:

**1. The edge lives in the order book, not the tape.**
Price and volume alone describe what *happened*; the order book reveals *intent*.
Who is adding large passive size? Who cancels before trades arrive? Which resting
orders are being hit vs ignored? These questions separate informed from uninformed
flow, and they require per-order (MBO) resolution to answer precisely. Price-level
MBP aggregates this information away; MBO preserves it.

**2. Signals must earn live capital.**
The shadow → promote → demote execution envelope enforces a simple discipline: the
strategy must demonstrate a statistically credible edge on the live tape — with
real bid/ask spreads, real slippage, real commissions — before broker orders are
placed. There is no backtested equity curve to flatter; the gate is forward-only.
If the live record later degrades the strategy demotes back to paper and
re-validates on fresh data. On-the-job training, not historical fitting.

**3. The regime, not the operator, should set parameters.**
Fixed buy/sell thresholds and fixed TP/SL sizes are fragile — they encode one
market condition. The adaptive stack lets the strategy learn the right thresholds
from the current distribution of flow, size brackets from current volatility,
trading direction from current autocorrelation, and when to stand aside entirely
from detected regime conditions. The operator sets the tuning knobs; the market
sets the values.

---

## Signal chain

The full data path from the CME feed to an execution decision:

```
CME Level 2 Feed  (dxFeed / Interactive Brokers)
  │
  │  Level2Quote: Id, Price, Size, Priority, NumberOrders, Closed
  │  ┌─ per-order MBO  (Id populated, one event per resting order)
  │  └─ price-level MBP  (Id null/empty; feed determines which)
  │
  ▼
AnalyticsEngineHost
  │  Market-data callbacks only enqueue to a bounded
  │  BlockingCollection (10 000 events). No computation here.
  │
  │  Single background worker drains the queue:
  │
  ├── MboOrderBook.Apply()                          [ MBO path ]
  │     Raw Level2Quote → MboEvent
  │     {Add / Update / Remove / Snapshot / Trade}
  │     Maintains per-order state for accurate classification
  │
  └── DataAnalyticsCalculator
        │
        ├── Order book  (SortedDictionary<tick, size>)
        │     bidBook  — reverse comparer → best bid first
        │     askBook  — ascending comparer → best ask first
        │     O(log n + k) top-N traversal; best quote tracked
        │     incrementally (UpdateBest / RescanBest)
        │
        ├── bidCountBook / askCountBook             [ MBO only ]
        │     per-price order-count dictionaries
        │     → bestBidOrderCount / bestAskOrderCount (Exact quality)
        │     → best-level avg order size
        │
        ├── TradeWindowAggregator
        │     sliding windows: 1s / 5s / 30s / 60s / 5m / 15m
        │     buy & sell qty, count, VWAP, delta, absorption
        │
        ├── L2WindowAggregator
        │     sliding windows: 60s / 900s
        │     add/cancel counts & quantities per side
        │
        ├── OFI accumulator  (Order Flow Imbalance)
        │
        └── Microstructure detectors
              Spoof:          MBO → exact order-ID match (Derived)
                              MBP → price+time heuristic (Heuristic)
              Iceberg / Replenishment: O(1) running sums
  │
  │  Published every 250 ms (configurable):
  ▼
AnalyticsSnapshot  (immutable record, volatile reference)
  │  Zero-copy read from UI thread — double buffering
  │
  ├── DataAnalyticsIndicator ──► GDI+ HUD (chart overlay)
  │
  └── DataAnalyticsStrategy.EvaluateTradingSignal()
        │
        ├── AdaptiveParameterController  (Phase 1–3, see below)
        │     produces: thresholds, brackets, Polarity, Regime
        │
        ├── Regime gate ──► StandAside?  ──► skip entry, return
        │
        ├── Polarity-aware signal matching
        │     Momentum:      buy ratio ≥ buyTh  /  sell ratio ≤ sellTh
        │     MeanReversion: buy ratio ≤ sellTh /  sell ratio ≥ buyTh
        │
        └── ExecutionDecision
              Shadow  ──► ShadowSimulator  ──► paper fill + perf tracker
              Live    ──► IB broker  (promoted from shadow via LCB gate)
              Demote  ──► back to Shadow if live record degrades
```

---

## Adaptive stack — three phases

The controller (`AdaptiveParameters.cs`) adds regime-awareness layer by layer.
Each phase answers one question; each builds on the answer of the one above it.

```
┌─────────────────────────────────────────────────────────────────┐
│  Phase 1 — WHAT SIZE?                                           │
│                                                                 │
│  Rolling percentile thresholds  (recalibrate every 2 s)        │
│    entry upper  = Adaptive Entry Percentile  (default 85th)    │
│    entry lower  = 1 − upper                  (default 15th)    │
│    re-arm level = running median             (50th)            │
│                                                                 │
│  Dual-ATR brackets                                             │
│    baseline ATR  14-min Wilder  →  multi-hour vol reference   │
│    fast ATR       5-min Wilder  →  current vol (diverges fast) │
│    TP = round(ATR_ticks × TpMultiplier)  clamped [2, 200]     │
│    SL = round(ATR_ticks × SlMultiplier)  clamped [2, 200]     │
│                                                                 │
│  Warmup gate: stand aside until ATR seeded + ~480 samples      │
└────────────────────────┬────────────────────────────────────────┘
                         │ thresholds & brackets
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  Phase 2 — WHICH DIRECTION?                                     │
│                                                                 │
│  Lag-1 Pearson ACF of the last 60 ratio samples (~15 s)        │
│                                                                 │
│    ACF > +0.10  →  Momentum                                    │
│                    buy when buyers dominate                     │
│                    sell when sellers dominate                   │
│                                                                 │
│    ACF < −0.10  →  MeanReversion                               │
│                    buy when sellers dominate  (expect bounce)  │
│                    sell when buyers dominate  (expect fade)    │
│                                                                 │
│    ACF ∈ [−0.10, +0.10]  →  hold current polarity             │
│                               (hysteresis — no rapid flip)     │
│                                                                 │
│  On transition: log event, reset both signal flags             │
└────────────────────────┬────────────────────────────────────────┘
                         │ polarity
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│  Phase 3 — SHOULD WE TRADE AT ALL?  (regime classifier)        │
│                                                                 │
│  Gate A  Volatility spike                                       │
│          fastATR / baselineATR  >  VolatilityGateRatio (1.5)   │
│          current vol 50 %+ above recent baseline → stand aside │
│                                                                 │
│  Gate B  Extreme ratio                                          │
│          last ratio  >  97th percentile  (flash buy event)     │
│          last ratio  <   3rd percentile  (flash sell event)    │
│          pathological tail — fills/spreads unreliable          │
│                                                                 │
│  Gate C  Thin queue  (MBO only, per-tick)                      │
│          bestBidOrders + bestAskOrders  ≤  threshold (2)       │
│          book too thin for reliable passive fill               │
│          auto-disabled in MBP mode (quality = Unavailable)     │
│                                                                 │
│  Any gate firing  →  RegimeState.StandAside                    │
│    no new entries; TP/SL exits still process normally          │
│    transition logged with reason and volatility ratio          │
└─────────────────────────────────────────────────────────────────┘
```

---

## Shadow → live execution envelope

```
Strategy starts
      │
      ▼
  ┌─────────┐
  │ Shadow  │◄──────────────────────────────────────┐
  │ (paper) │  paper trades accumulate               │
  └────┬────┘                                        │
       │                                             │
       │  trades ≥ MinTrades                         │
       │  mean net PnL ≥ MinExpectancy               │ demotion:
       │  LCB = mean − Z·σ/√n > 0                   │  live record
       │                                             │  LCB turns
       ▼                                             │  negative
  ┌─────────┐                                        │
  │  Live   │────────────────────────────────────────┘
  │  (IB)   │
  └─────────┘

LCB (lower confidence bound) gate properties:
  • Demands more trades when per-trade variance is high
  • Fresh record on each transition — no reuse of prior data
  • Luck-resistant: a lucky streak with high variance doesn't promote
```

The paper fill model uses the same entry price as live orders:
- Entry: fill at near-touch (buy at bid, sell at ask) + slippage — same price as the live passive limit
- Stop exits: fill at stop price + slippage
- Cost model: commission both sides
- Known limitation: fill probability is not modelled. Shadow always fills immediately; live passive
  limits may not fill if the market moves away. The thin-queue stand-aside gate (Phase 3, Gate C)
  prevents shadow from entering when the queue is too thin to expect a fill.

---

## Key design decisions

### Single worker thread
All calculator state is owned by one background thread. Market-data callbacks
only enqueue; they never compute. This eliminates the need for locks inside the
calculator (which would serialise throughput), ensures deterministic event-time
ordering, and makes live and replay modes **identical code paths**.

### Event-time windows (replay-safe)
Rolling windows are driven by event timestamps, not wall-clock time:
- Quiet periods don't artificially age windows between ticks
- Historical replay produces the same metric values as live
- The replay backtester reuses the exact same `DataAnalyticsCalculator` and
  `AdaptiveParameterController` — train and serve use the same code

### MBO-first with graceful MBP fallback
Per-order depth is attempted unconditionally. If the feed returns null for the
MBO snapshot, the engine downgrades to price-level MBP transparently. The same
30+ metrics run in both modes; quality tags reflect which data was available:

| Quality | Meaning | MBP → MBO change |
|---------|---------|-----------------|
| Exact | Direct 1:1 measurement | Order counts, volumes |
| Derived | Computed from Exact inputs | Cancel volumes, microstructure scores |
| Heuristic | Estimated with assumptions | Unchanged in MBP |
| Unavailable | Requires data not present | Order counts in MBP |

### Metric quality tags
Every `MetricValue` carries an `Exact / Derived / Heuristic / Unavailable` label,
colour-coded on the HUD. This prevents the strategy from acting on metrics whose
quality is too low for the current decision — `Unavailable` and not-warm values
are explicitly skipped in signal evaluation.

### No vendor lock-in at the engine level
The calculator knows nothing about Quantower's API. It receives normalized
`MarketEvent` structs (price, size, side, id, closed, priority) from the host.
Swapping the host for a different platform or a replay harness requires no changes
to the analytics or strategy logic.

---

## Current state

| Component | Status |
|-----------|--------|
| MBO ingestion + MboOrderBook reconstruction | ✅ |
| Per-order spoof detection (exact order-ID match) | ✅ |
| Best-level order counts + avg order size (Exact in MBO) | ✅ |
| Queue-aware fill model (thin queue → mid-price) | ✅ |
| Phase 1: percentile thresholds + dual-ATR brackets | ✅ |
| Phase 2: ACF polarity selection (Momentum / MeanReversion) | ✅ |
| Phase 3: regime classifier (3 stand-aside gates) | ✅ |
| Shadow → live envelope with LCB promotion gate | ✅ |
| Replay backtester (trade-tick history) | ✅ |
| MBO recorder (daily CSV, offline corpus) | ✅ |
| Feature store export (wide-format CSV per sample) | ✅ |

## Potential future work

| Area | Notes |
|------|-------|
| Per-regime parameter sets | Separate calibrated thresholds for Momentum vs MeanReversion, validated independently |
| Historical MBO replay | Feed the recorder CSV through the full MBO ingestion path (not just trade-tick replay) |
| Classifier feature validation | With a live MBO corpus, measure which of the 30+ metrics are predictive per regime before committing to them in the signal |
| L2-aware backtester | Incorporate depth snapshots into the replay path for DOM/cancel signal validation |
| Binary MBO recording | More compact than CSV at sustained very high MBO event rates |
