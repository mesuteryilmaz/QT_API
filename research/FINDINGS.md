# Signal edge study — findings

**Date:** 2026-06-24
**Question:** Does the strategy's signal (and any other captured metric) have exploitable
predictive edge, after drift and costs?
**Method:** Forward-return *event study* / information-coefficient scan on the captured
feature-store CSV — deliberately **not** a full strategy backtest (that conflates the signal
with the broken fill model and risk rules). We measure only whether the signal predicts the
next move.

## Data

- Source: `C:\Quantower\Settings\Scripts\ScriptsData\microstructure_features.csv`
  (analytics-engine feature export, sampled ~every 250 ms while running).
- ~14k usable samples, MNQ, **one mostly down-trending session** (plus older fragmented rows,
  segmented out by symbol + time gaps).
- Spot price reconstructed from the captured columns: `last = SessionVWAP + VWAPDistance·tick`
  (verified against `GetVwapDistance`, which is `(last − sessionVWAP)/tick`).

## Method notes

- **Detrended** forward returns: raw forward return minus the segment's mean forward return at
  that horizon, so a one-way market can't masquerade as signal.
- **Spearman (rank) IC** as the primary measure — robust to the fat tails of flow/returns.
- Signals are edge-triggered (threshold crossings) for the trigger study; the IC scan uses the
  continuous signal over all samples.
- Round-trip cost assumed ≈ 2 ticks (≈1 tick to cross the spread + ≈1 tick commission/slippage).
  MNQ: 1 tick = 0.25 pt = $0.50.

## Headline result — detrended rank-IC vs forward return

```
                    H=1s    H=5s    H=30s   H=60s     (signal window)
Delta60s          +0.005  +0.017  +0.102  +0.132     60s
BuySellRatio60    +0.008  +0.021  +0.093  +0.113     60s   <- the strategy's signal
Delta30s          -0.006  +0.013  +0.065  +0.109     30s
Delta5s           -0.010  -0.003  -0.001  +0.035     5s
Delta1s           +0.007  -0.007  +0.006  +0.018     1s
```
Everything else measured (DOM imbalance, book pressure, cancel ratios, absorption, queue
imbalance) sits at |IC| ≲ 0.02–0.08, all long-horizon-only.

## Interpretation

**The IC scales with the signal's window length, and short-window flow predicts nothing.**

- If order flow genuinely predicted price, the *freshest* flow (`Delta1s`/`Delta5s`) should
  predict at least as well as stale 60s-old flow. Instead short-window flow has **IC ≈ 0 at every
  horizon**; only the 60s-window signals show non-zero IC, and only at 30–60s horizons.
- A 60s-smoothed signal correlating with a 30–60s forward return measures **slow price/trend
  autocorrelation**, not flow→return predictivity. The tell: a real flow edge *decays* with
  horizon; here the IC *grows* with horizon and matches the signal's own window length.
- The non-zero long-horizon IC is also **regime-unstable** — it sign-flips between high-vol
  (−0.05) and low-vol (+0.09) and only appears in down-trends (see the regime split in
  `edge_study.py`).
- Trigger-edge study: the "momentum edge" is one-sided (sell-only) and **net-negative after
  ~2-tick cost at every clean horizon**; it's positive only at the artifact-prone 30–60s horizons
  and not statistically significant there.

## Verdict

**No actionable edge.** At the sub-10s horizons where a flow signal would actually be tradeable on
a retail stack, every captured signal is noise (IC ≈ ±0.00–0.02). The only non-trivial ICs are
long-window signals at long horizons, shown to be trend-autocorrelation rather than flow alpha,
and not robust across regimes. This is the expected outcome for generic order-flow-imbalance on a
retail IB/dxFeed stack.

## Caveats

- One (mostly down-trending) session. Multiple days would firm up the long-horizon/regime picture,
  but the robust part — *short-window flow predicts nothing* — is regime-independent (zero is zero).
- Price is a 250 ms-sampled reconstruction from session-VWAP + distance, adequate for 1–60s
  forward returns but not tick-exact.

## Reproduce

```
python research/signal_scan.py    # IC scan of all captured signals vs forward returns
python research/edge_study.py     # detrended IC by horizon + regime split + trigger-edge study
```
Both read the feature-store CSV path hard-coded at the top of each file.
