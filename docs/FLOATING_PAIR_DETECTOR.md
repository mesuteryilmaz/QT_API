# MBO Floating Pair Detector

`MboFloatingPairDetector` detects individual same-size bid/ask orders that remain paired and follow BBO movement.

It does not identify a firm or market maker. It reports coordinated-looking book objects only.

## Eligibility

An order is eligible when:

- Mode is MBO.
- Order id is present.
- Side is bid or ask.
- Price and remaining quantity are positive.
- Remaining quantity >= large threshold.
- Offset from touch is between 0 and configured maximum offset.

Defaults:

- Large: `20`
- Very large: `200`
- Offset tolerance: `1` tick
- Maximum offset: `250` ticks
- Synchronization window: `300 ms`

## Pairing

Pairs are matched by exact integer remaining size.

Offsets:

```text
BidOffsetTicks = BestBidTicks - BidOrderPriceTicks
AskOffsetTicks = AskOrderPriceTicks - BestAskTicks
```

Candidate rule:

```text
BidSize == AskSize
abs(BidOffsetTicks - AskOffsetTicks) <= OffsetToleranceTicks
```

Matching is deterministic and one-to-one.

## Lifecycle

- `Candidate`: exact-size pair exists.
- `Persistent`: pair age exceeds persistence time but has not proven floating movement.
- `FloatingConfirmed`: pair has required coordinated follows and minimum follow ratio.
- `Broken`: pair invalidated and event emitted.

Static pairs can become persistent but not floating-confirmed.

## Movement Confirmation

On BBO movement, the detector compares market-center movement and pair-center movement:

```text
MarketCenter2 = BestBidTicks + BestAskTicks
PairCenter2 = BidOrderTicks + AskOrderTicks
```

A synchronized follow requires stable offsets, stable pair width, both leg updates inside the synchronization window, and pair-center movement matching market-center movement within tolerance.

## Stitching

Same-ID repricing is preferred.

Strict cancel/re-entry stitching is allowed only inside the replacement window, with same side, exact same size, close expected price, unchanged book epoch, and no better competing match. Stitched pairs carry lower confidence.

## Break Conditions

Pairs break on size mismatch, side change, missing leg beyond replacement window, offset divergence, width change, invalid book, epoch reset, MBO downgrade, inactivity, or ambiguous reassignment.

## Published Fields

The panel and recorder expose detector status, eligible large/very-large counts, exact-size candidates, persistent/confirmed counts, top pair size/tier, offsets, age, synchronized moves/opportunities, follow ratio, confidence, top summaries, and last break reason.
