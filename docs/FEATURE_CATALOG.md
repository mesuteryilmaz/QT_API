# Feature Catalog

All features are emitted in `FeatureSnapshot.Values` with `FeatureValue` metadata: key, display name, numeric value, quality, event time, unit, and reason.

## Book Health

| Key | Formula | Unit | Source | Quality |
|---|---|---:|---|---|
| `book.epoch` | Current book epoch | count | lifecycle | Exact |
| `book.mode` | `BookMode` enum ordinal | enum | lifecycle | Exact |
| `book.lifecycle` | `BookLifecycleState` enum ordinal | enum | lifecycle | Exact |
| `book.spread_ticks` | `BestAskTicks - BestBidTicks` | ticks | two-sided book | Derived/Invalid |
| `book.mbo_identity` | real-ID events / total MBO events | ratio | MBO only | Derived |

## Market State

| Key | Formula | Unit | Window |
|---|---|---:|---|
| `market.regime` | `MarketRegime` enum ordinal | enum | current |
| `market.risk` | `RiskEnvironment` enum ordinal | enum | current |
| `market.activity_score` | normalized book-event rate versus EWMA baseline | score | 30s |
| `market.volatility_score` | normalized midpoint realized volatility plus jump pressure | score | 30s |
| `market.liquidity_stress` | spread, depth withdrawal, top-5 withdrawal, cancel pressure blend | score | 30s/60s |
| `market.confidence` | warm-up and regime stability confidence | ratio | current |

## Spread & Liquidity

| Key | Formula | Unit | Window |
|---|---|---:|---|
| `spread.mean_30s` | time-weighted mean spread | ticks | 30s |
| `spread.p90_60s` | 90th percentile spread | ticks | 60s |
| `spread.time_at_one_tick` | fraction of samples with spread <= configured one-tick spread | ratio | 60s |

Additional panel fields come from `MarketStateSnapshot`: max spread, widening/narrowing rates, spread episode age/peak/recovery, touch-depth ratio, top-5 depth ratio, cancellation pressure, BBO changes/sec, book events/sec, trades/sec, volume/sec, bursts, RV 5s/30s, ranges, and multi-tick jump rate.

## Floating Pairs

| Key | Formula | Unit | Source |
|---|---|---:|---|
| `fp.status` | detector status enum ordinal | enum | MBO |
| `fp.eligible_large_bids` | bid orders with size >= large threshold and valid offset | count | MBO |
| `fp.eligible_large_asks` | ask orders with size >= large threshold and valid offset | count | MBO |
| `fp.candidates` | exact-size one-to-one bid/ask candidates | count | MBO |
| `fp.persistent` | pairs aged beyond persistence time | count | MBO |
| `fp.confirmed` | pairs with required coordinated follows and follow ratio | count | MBO |
| `fp.top_size` | top active pair size | contracts | MBO |
| `fp.top_follow_ratio` | synchronized follows / opportunities | ratio | MBO |

When MBO identity is unavailable, pair features serialize as unavailable with empty numeric fields.

## Aggregate Lattice

| Key | Formula | Unit | Source |
|---|---|---:|---|
| `lattice.score` | aggregate repeated ladder score from rung count, regularity, and bid/ask symmetry | score | MBP/MBO levels |

This module is experimental and disabled by default. It detects aggregate multi-level ladders, not individual floating pairs.
