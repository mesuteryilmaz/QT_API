# Market State Regimes

## Inputs

The monitor samples immutable `BookSnapshot` objects and normalized trade events.

Primary inputs:

- Current spread.
- Time-weighted spread mean over 30 seconds.
- Spread distribution over 60 seconds.
- Book events/sec and burst ratio.
- BBO price changes/sec.
- Trade and volume rates.
- Midpoint realized volatility over 5s and 30s.
- Midpoint range over 5s and 30s.
- Multi-tick jump rate.
- Touch and top-5 depth versus EWMA baselines.
- Cancellation pressure.

## Scores

Activity score normalizes 30s book-event rate versus its EWMA baseline.

Volatility score normalizes 30s midpoint realized volatility versus its EWMA baseline and adds jump pressure.

Liquidity stress blends:

- spread widening above one tick,
- touch-depth withdrawal,
- top-5-depth withdrawal,
- cancellation pressure.

Scores are clamped to `[0, 1]`.

## Regimes

- `Unknown`: invalid, stale, or not initialized.
- `QuietTight`: one-tick spread, low activity, low volatility, low stress.
- `ActiveLiquid`: activity present, stress controlled.
- `FastOrderly`: high activity and volatility with acceptable liquidity stress.
- `ThinFragile`: liquidity stress elevated.
- `VolatileDislocated`: emergency/wide spread or high stress with volatility.
- `Recovering`: stress falling after a fragile/dislocated episode.

## Transition Rules

The monitor uses candidate persistence and minimum dwell before normal transitions. Emergency `VolatileDislocated` transitions occur immediately when spread or liquidity stress breaches emergency thresholds.

Stale feed is never classified as `QuietTight`; invalid/crossed books classify as `Unknown`.
