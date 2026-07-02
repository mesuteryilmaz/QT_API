# Order-Flow Directional Signal — Validation Study

Date: 2026-07-02
Instrument: MNQU6 (CME Micro E-mini Nasdaq-100), Interactive Brokers feed via Quantower
Signal under test: `OrderFlowMonitor` (`src/QT.Features/OrderFlow/`), commit `4d32b53`
Capture tool: `SignalValidationProbe` ("MBO Signal Probe (log-only)"), commit `cbe8790` — analytics-only, no orders
Verdict: **NO ACTIONABLE EDGE — the signal is significantly ANTI-predictive at the 10s horizon. Do not use Signal mode for live entries.**

This confirms and extends the earlier strategy-signal edge study (2026-06-24), which also found no
actionable directional edge.

## 1. What was tested

The order-flow signal combines three transparent components into a directional lean
(`DirectionalBias` Up/Down/Neutral plus a 0–1 confidence):

- **Trade imbalance** — aggressor buy volume / total volume (5s window)
- **CVD slope** — signed volume delta per second
- **Book pressure** — resting bid-vs-ask depth imbalance (top 5 levels)

Hypothesis: a confident Up/Down lean predicts the direction of the mid-price move over the next
10 seconds (momentum continuation).

## 2. Method

The probe sampled the signal every ~250 ms together with the current mid, then recorded the
realized mid move 10 s later. Per row: bias, confidence, lean score, all three components,
`fwd_ticks` (signed forward move) and `signed_ticks` (forward move × signal direction — positive
means the signal was right).

**De-overlapping.** Rows sampled 250 ms apart share almost the same 10 s forward window and are
heavily autocorrelated; raw row counts overstate the sample by ~40×. The analysis greedily keeps
rows spaced ≥ 10 s apart, yielding independent forward windows. All headline statistics below use
independent windows only.

Cost benchmark: MNQ round-trip commission ≈ **2.48 ticks** (tick value $0.50, RT $1.24).

## 3. Data

| | |
|---|---|
| Capture window | 2026-07-02 01:33 → 09:49 UTC (~8 hours, 3 main sessions) |
| Raw rows | 106,115 |
| Independent 10s windows | 2,621 |
| Bias mix (raw) | Neutral 67,917 / Up 23,297 / Down 14,901 |
| Forward-move noise | σ(fwd_ticks) ≈ 14.2t per 10s; drift ≈ 0 |

Files: `C:\Quantower\Settings\Scripts\ScriptsData\QT_API_V2\SignalProbe\signal_probe_MNQU6_20260702_*.csv`

## 4. Results

### 4.1 Directional accuracy (independent windows)

`mean` = average forward move in the signal's direction, in ticks. Edge requires mean > 0,
|t| > ~2, and mean > RT cost.

| Filter | n | mean (t) | SE | t-stat | hit % | net of cost (t) |
|---|---|---|---|---|---|---|
| conf ≥ 0.0 (all directional) | 903 | **−1.12** | 0.48 | −2.32 | 45.6 | −3.60 |
| conf ≥ 0.5 | 617 | **−1.96** | 0.58 | **−3.38** | 42.9 | −4.44 |
| conf ≥ 0.7 | 250 | **−3.33** | 0.93 | **−3.56** | 39.6 | −5.81 |
| conf ≥ 0.5, Up only | 420 | −1.67 | 0.73 | −2.30 | 44.5 | −4.15 |
| conf ≥ 0.5, Down only | 197 | −2.58 | 0.95 | −2.72 | 39.6 | −5.06 |

Key observations:

1. **The mean is negative and statistically significant** — when the signal calls a direction,
   price tends to move the *other* way over the next 10 s.
2. **Higher confidence is monotonically worse** (−1.1t → −2.0t → −3.3t). This is structure, not
   noise.
3. The effect is symmetric: both Up and Down calls are anti-predictive.

### 4.2 Per-session consistency (conf ≥ 0.5, independent windows)

| Session (UTC start) | n | mean (t) | hit % |
|---|---|---|---|
| 02:08 | 183 | −1.55 | 42.1 |
| 04:14 | 186 | −2.90 | 41.9 |
| 07:23 | 215 | −1.88 | 43.7 |

All three substantive sessions are negative. (Four early mini-files with n = 4–13 are noise and
excluded from conclusions.)

### 4.3 Continuous predictive power (information coefficients)

Correlation of each raw component with the signed forward move (independent windows):

| Component | IC |
|---|---|
| lean_score | −0.046 |
| trade imbalance (−0.5) | −0.043 |
| CVD slope | −0.046 |
| book pressure | −0.007 |

All flat-to-negative. No component carries positive continuation information at this horizon.

## 5. Interpretation

The pattern is the classic **short-horizon flow mean-reversion**: a burst of aggressive one-sided
flow (which is exactly what spikes the signal) tends to mark a local extreme that reverts within
seconds — liquidity refills and the aggressors are done. Momentum-continuation at 10s on MNQ is
the wrong model for this feature set.

**Could it be faded (contrarian use)?** On this data: at conf ≥ 0.5 fading nets ≈ −0.5t after
commission (still a loser); at conf ≥ 0.7 fading is ≈ +0.85t net of commission but before spread
and slippage on an aggressive entry, which would consume it. With σ ≈ 14t noise per window, one
instrument, ~8 hours, in-sample, and a single 10s horizon, this is **not tradeable evidence** —
only a hypothesis that would need a dedicated multi-day, multi-horizon backtest with realistic
fills.

## 6. Decisions

1. **Signal mode stays OFF.** `PyramidMomentumStrategy` input "Entry direction source" must remain
   **Manual bias**. The order-flow signal must not originate live entries.
2. **The analytics remain in service as the risk governor** — regime veto on entries/adds,
   flow-opposition gate on pyramid adds, risk-off flatten, persistence-filtered soft exit. None of
   these require the signal to predict direction; several exploit exactly the reversion behavior
   documented here (e.g. treating a flow flip against the position as exit confirmation).
3. Any future directional work should target the **mean-reversion hypothesis** (fade confident
   flow bursts) and must be validated offline across multiple days, horizons, and regimes before
   touching an order path — same bar as this study.

## 7. Reproduction

1. Run "MBO Signal Probe (log-only)" on the target symbol (defaults: 250 ms sample, 10 s horizon).
   It writes per-sample CSVs and places no orders.
2. Analyze with de-overlapping (keep samples ≥ horizon apart), compute mean/SE/t of
   `signed_ticks` by confidence bucket, per-session splits, and component ICs against
   `fwd_ticks`. Compare the mean against the instrument's round-trip cost in ticks
   (MNQ ≈ 2.48t, NQ ≈ 0.90t).
