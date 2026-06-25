using QT.Core.Primitives;
using QT.Core.Quality;
using QT.Market.Events;
using QT.Market.Snapshots;

namespace QT.Features.FloatingPairs;

public sealed class MboFloatingPairDetector
{
    private sealed class TrackedOrder
    {
        public required string Id;
        public required BookSide Side;
        public required long PriceTicks;
        public required long Size;
        public required DateTime FirstSeenUtc;
        public required DateTime LastSeenUtc;
        public required DateTime LastPriceChangeUtc;
        public long PreviousPriceTicks;
        public bool HeuristicallyStitched;
    }

    private sealed class Pair
    {
        public required string PairId;
        public required string BidId;
        public required string AskId;
        public required long Size;
        public required DateTime FirstSeenUtc;
        public required long RefBidOffset;
        public required long RefAskOffset;
        public required long InitialWidthTicks;
        public FloatingPairState State = FloatingPairState.Candidate;
        public long LastBidOffset;
        public long LastAskOffset;
        public long LastBidTicks;
        public long LastAskTicks;
        public DateTime LastActivityUtc;
        public int Opportunities;
        public int SynchronizedMoves;
        public int OneLegOnlyMoves;
        public int LateCounterpartMoves;
        public int MissedFollows;
        public double SyncDelaySumMs;
        public double MaxSyncDelayMs;
        public bool UsesHeuristicStitching;
        public BookSide MissingSide = BookSide.Unknown;
        public DateTime MissingSinceUtc;
        public string MissingOrderId = "";
        public long MissingExpectedPriceTicks;
    }

    private sealed record RemovedLeg(string OrderId, BookSide Side, long Size, long LastPriceTicks, DateTime RemovedUtc, long BookEpoch);

    private readonly FloatingPairConfig cfg;
    private readonly Dictionary<string, TrackedOrder> orders = new(StringComparer.Ordinal);
    private readonly List<Pair> pairs = new();
    private readonly List<RemovedLeg> removedLegs = new();
    private readonly List<FloatingPairBreakEvent> breaks = new();

    private long epoch = -1;
    private long lastBestBid;
    private long lastBestAsk;
    private bool hasBbo;
    private DateTime lastSnapshotUtc;
    private int pairSequence;

    public MboFloatingPairDetector(FloatingPairConfig? config = null)
    {
        cfg = config ?? new FloatingPairConfig();
        cfg.Validate();
    }

    public FloatingPairSnapshot Current { get; private set; } =
        FloatingPairSnapshot.Unavailable(0, FloatingPairDetectorStatus.Unavailable, MetricQuality.Unavailable, "not initialized");

    public IReadOnlyList<FloatingPairBreakEvent> DrainBreakEvents()
    {
        if (breaks.Count == 0) return Array.Empty<FloatingPairBreakEvent>();
        var copy = breaks.ToArray();
        breaks.Clear();
        return copy;
    }

    public void Reset(long newEpoch, DateTime eventTimeUtc)
    {
        epoch = newEpoch;
        orders.Clear();
        pairs.Clear();
        removedLegs.Clear();
        hasBbo = false;
        lastBestBid = 0;
        lastBestAsk = 0;
        lastSnapshotUtc = eventTimeUtc;
        pairSequence = 0;
        Current = FloatingPairSnapshot.Unavailable(newEpoch, FloatingPairDetectorStatus.WarmingUp, MetricQuality.WarmingUp, "epoch reset");
    }

    public void OnMarketEvent(in NormalizedMarketEvent evt)
    {
        if (evt.Kind is not (MarketEventKind.BookLevel or MarketEventKind.BookSnapshotLevel))
            return;
        if (evt.BookEpoch != 0 && evt.BookEpoch != epoch)
            Reset(evt.BookEpoch, evt.EventTimeUtc);
        if (evt.Side is not (BookSide.Bid or BookSide.Ask))
            return;

        if (evt.Closed || evt.Quantity <= 0 || evt.BookAction == NormalizedBookAction.Remove)
            OnOrderRemove(evt.OrderId ?? "", evt.EventTimeUtc);
        else
            OnOrderUpsert(evt.OrderId ?? "", evt.Side, evt.PriceTicks, evt.Quantity, evt.EventTimeUtc);
    }

    public void OnOrderUpsert(string orderId, BookSide side, long priceTicks, long size, DateTime eventTimeUtc)
    {
        if (string.IsNullOrWhiteSpace(orderId) || side is not (BookSide.Bid or BookSide.Ask) || priceTicks <= 0 || size <= 0)
            return;

        if (orders.TryGetValue(orderId, out var existing))
        {
            existing.PreviousPriceTicks = existing.PriceTicks;
            if (existing.PriceTicks != priceTicks)
                existing.LastPriceChangeUtc = eventTimeUtc;
            existing.Side = side;
            existing.PriceTicks = priceTicks;
            existing.Size = size;
            existing.LastSeenUtc = eventTimeUtc;
        }
        else
        {
            var stitched = TryStitchReplacement(orderId, side, priceTicks, size, eventTimeUtc);
            orders[orderId] = new TrackedOrder
            {
                Id = orderId,
                Side = side,
                PriceTicks = priceTicks,
                Size = size,
                FirstSeenUtc = stitched ? eventTimeUtc - TimeSpan.FromTicks(1) : eventTimeUtc,
                LastSeenUtc = eventTimeUtc,
                LastPriceChangeUtc = eventTimeUtc,
                PreviousPriceTicks = priceTicks,
                HeuristicallyStitched = stitched
            };
        }
    }

    public void OnOrderRemove(string orderId, DateTime eventTimeUtc)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            return;
        if (!orders.TryGetValue(orderId, out var order))
            return;

        removedLegs.Add(new RemovedLeg(orderId, order.Side, order.Size, order.PriceTicks, eventTimeUtc, epoch));
        orders.Remove(orderId);

        foreach (var pair in pairs)
        {
            if (pair.State == FloatingPairState.Broken)
                continue;
            if (pair.BidId == orderId)
                MarkMissing(pair, BookSide.Bid, orderId, order.PriceTicks, eventTimeUtc);
            else if (pair.AskId == orderId)
                MarkMissing(pair, BookSide.Ask, orderId, order.PriceTicks, eventTimeUtc);
        }
    }

    public FloatingPairSnapshot OnBookSnapshot(BookSnapshot book, DateTime nowUtc)
    {
        if (book.BookEpoch != epoch)
            Reset(book.BookEpoch, book.EventTimeUtc);

        TrimRemoved(book.EventTimeUtc);

        if (book.Mode != BookMode.Mbo)
        {
            Current = FloatingPairSnapshot.Unavailable(book.BookEpoch, FloatingPairDetectorStatus.Unavailable,
                MetricQuality.Unavailable, "MBO identity unavailable");
            return Current;
        }

        if (!book.HasTwoSidedBook || book.DataQuality == MetricQuality.Invalid || book.IsCrossed)
        {
            BreakAll(book.EventTimeUtc, book.BookEpoch, "book invalid");
            Current = FloatingPairSnapshot.Unavailable(book.BookEpoch, FloatingPairDetectorStatus.BookInvalid,
                MetricQuality.Invalid, book.InvalidReason ?? "book invalid");
            return Current;
        }

        ReconcileSnapshotOrders(book);

        long bestBid = book.BestBidTicks!.Value;
        long bestAsk = book.BestAskTicks!.Value;
        bool marketMoved = hasBbo && (bestBid != lastBestBid || bestAsk != lastBestAsk);
        long marketDelta2 = hasBbo ? (bestBid + bestAsk) - (lastBestBid + lastBestAsk) : 0;

        var eligible = BuildEligible(bestBid, bestAsk);
        UpdatePairs(book.EventTimeUtc, bestBid, bestAsk, marketMoved, marketDelta2);
        FormNewPairs(book.EventTimeUtc, bestBid, bestAsk, eligible);
        BreakInactive(book.EventTimeUtc, book.BookEpoch);

        Current = BuildSnapshot(book, eligible);

        lastBestBid = bestBid;
        lastBestAsk = bestAsk;
        hasBbo = true;
        lastSnapshotUtc = book.EventTimeUtc;
        return Current;
    }

    private void ReconcileSnapshotOrders(BookSnapshot book)
    {
        var present = new HashSet<string>(StringComparer.Ordinal);
        foreach (var order in book.MboOrders)
        {
            present.Add(order.OrderId);
            OnOrderUpsert(order.OrderId, order.Side, order.PriceTicks, order.Quantity, order.LastUpdateUtc);
        }

        var missing = orders.Keys.Where(k => !present.Contains(k)).ToArray();
        foreach (var id in missing)
            OnOrderRemove(id, book.EventTimeUtc);
    }

    private EligibleOrders BuildEligible(long bestBid, long bestAsk)
    {
        var result = new EligibleOrders();
        foreach (var order in orders.Values)
        {
            if (order.Size < cfg.LargeThreshold)
                continue;
            long offset = order.Side == BookSide.Bid ? bestBid - order.PriceTicks : order.PriceTicks - bestAsk;
            if (offset < 0 || offset > cfg.MaximumOffsetTicks)
                continue;

            if (order.Side == BookSide.Bid)
            {
                result.LargeBids++;
                if (order.Size >= cfg.VeryLargeThreshold) result.VeryLargeBids++;
                AddBySize(result.BidsBySize, order);
            }
            else
            {
                result.LargeAsks++;
                if (order.Size >= cfg.VeryLargeThreshold) result.VeryLargeAsks++;
                AddBySize(result.AsksBySize, order);
            }
        }
        return result;
    }

    private void UpdatePairs(DateTime t, long bestBid, long bestAsk, bool marketMoved, long marketDelta2)
    {
        foreach (var pair in pairs.ToArray())
        {
            if (pair.State == FloatingPairState.Broken)
                continue;

            if (pair.MissingSide != BookSide.Unknown)
            {
                if (t - pair.MissingSinceUtc > cfg.ReplacementStitchingWindow)
                    BreakPair(pair, t, "missing leg beyond replacement window");
                continue;
            }

            if (!orders.TryGetValue(pair.BidId, out var bid) || !orders.TryGetValue(pair.AskId, out var ask))
            {
                BreakPair(pair, t, "leg disappeared");
                continue;
            }

            if (bid.Side != BookSide.Bid || ask.Side != BookSide.Ask || bid.Size != pair.Size || ask.Size != pair.Size)
            {
                BreakPair(pair, t, "size mismatch or side change");
                continue;
            }

            long bidOffset = bestBid - bid.PriceTicks;
            long askOffset = ask.PriceTicks - bestAsk;
            if (bidOffset < 0 || askOffset < 0 ||
                bidOffset > cfg.MaximumOffsetTicks || askOffset > cfg.MaximumOffsetTicks ||
                Math.Abs(bidOffset - askOffset) > cfg.OffsetToleranceTicks)
            {
                BreakPair(pair, t, "offset divergence");
                continue;
            }

            long width = ask.PriceTicks - bid.PriceTicks;
            if (Math.Abs(width - pair.InitialWidthTicks) > cfg.OffsetToleranceTicks * 2 + 1)
            {
                BreakPair(pair, t, "pair width changed");
                continue;
            }

            if (marketMoved)
                TrackMarketFollow(pair, bid, ask, t, marketDelta2);

            pair.LastBidOffset = bidOffset;
            pair.LastAskOffset = askOffset;
            pair.LastBidTicks = bid.PriceTicks;
            pair.LastAskTicks = ask.PriceTicks;
            pair.LastActivityUtc = t;

            double age = (t - pair.FirstSeenUtc).TotalSeconds;
            double followRatio = pair.Opportunities > 0 ? pair.SynchronizedMoves / (double)pair.Opportunities : 0;
            if (age >= cfg.PersistenceTime.TotalSeconds &&
                pair.SynchronizedMoves >= cfg.RequiredCoordinatedMoves &&
                followRatio >= cfg.MinimumFollowRatio)
                pair.State = FloatingPairState.FloatingConfirmed;
            else if (age >= cfg.PersistenceTime.TotalSeconds)
                pair.State = FloatingPairState.Persistent;
            else
                pair.State = FloatingPairState.Candidate;
        }

        pairs.RemoveAll(p => p.State == FloatingPairState.Broken);
    }

    private void TrackMarketFollow(Pair pair, TrackedOrder bid, TrackedOrder ask, DateTime t, long marketDelta2)
    {
        pair.Opportunities++;
        long previousPairCenter2 = pair.LastBidTicks + pair.LastAskTicks;
        long currentPairCenter2 = bid.PriceTicks + ask.PriceTicks;
        long pairDelta2 = currentPairCenter2 - previousPairCenter2;

        bool centerTracks = Math.Abs(pairDelta2 - marketDelta2) <= cfg.OffsetToleranceTicks * 2;
        bool bidUpdated = bid.LastPriceChangeUtc >= lastSnapshotUtc && t - bid.LastPriceChangeUtc <= cfg.SynchronizationWindow;
        bool askUpdated = ask.LastPriceChangeUtc >= lastSnapshotUtc && t - ask.LastPriceChangeUtc <= cfg.SynchronizationWindow;
        bool bothUpdated = bidUpdated && askUpdated;
        bool oneUpdated = bidUpdated ^ askUpdated;
        double syncDelay = Math.Abs((bid.LastPriceChangeUtc - ask.LastPriceChangeUtc).TotalMilliseconds);
        bool delayOk = syncDelay <= cfg.SynchronizationWindow.TotalMilliseconds;

        long bidOffset = lastBestBid + (marketDelta2 / 2) - bid.PriceTicks;
        long askOffset = ask.PriceTicks - (lastBestAsk + (marketDelta2 / 2));
        bool offsetsStable = Math.Abs(bidOffset - pair.RefBidOffset) <= cfg.OffsetToleranceTicks + 1 &&
                             Math.Abs(askOffset - pair.RefAskOffset) <= cfg.OffsetToleranceTicks + 1;

        if (centerTracks && offsetsStable && bothUpdated && delayOk)
        {
            pair.SynchronizedMoves++;
            pair.SyncDelaySumMs += syncDelay;
            pair.MaxSyncDelayMs = Math.Max(pair.MaxSyncDelayMs, syncDelay);
        }
        else if (oneUpdated)
        {
            pair.OneLegOnlyMoves++;
            pair.MissedFollows++;
        }
        else if (!delayOk)
        {
            pair.LateCounterpartMoves++;
            pair.MissedFollows++;
        }
        else
        {
            pair.MissedFollows++;
        }
    }

    private void FormNewPairs(DateTime t, long bestBid, long bestAsk, EligibleOrders eligible)
    {
        var assigned = new HashSet<string>(pairs.SelectMany(p => new[] { p.BidId, p.AskId }), StringComparer.Ordinal);
        foreach (var size in eligible.BidsBySize.Keys.OrderByDescending(x => x))
        {
            if (pairs.Count >= cfg.MaxPairsTracked) break;
            if (!eligible.AsksBySize.TryGetValue(size, out var asks)) continue;

            var bids = eligible.BidsBySize[size]
                .OrderBy(o => bestBid - o.PriceTicks)
                .ThenBy(o => o.FirstSeenUtc)
                .ThenBy(o => o.Id, StringComparer.Ordinal)
                .ToArray();

            var askList = asks
                .OrderBy(o => o.PriceTicks - bestAsk)
                .ThenBy(o => o.FirstSeenUtc)
                .ThenBy(o => o.Id, StringComparer.Ordinal)
                .ToList();

            foreach (var bid in bids)
            {
                if (pairs.Count >= cfg.MaxPairsTracked) break;
                if (assigned.Contains(bid.Id)) continue;
                long bidOffset = bestBid - bid.PriceTicks;

                TrackedOrder? chosen = null;
                long bestMismatch = long.MaxValue;
                TimeSpan bestAgeMismatch = TimeSpan.MaxValue;
                foreach (var ask in askList)
                {
                    if (assigned.Contains(ask.Id)) continue;
                    long askOffset = ask.PriceTicks - bestAsk;
                    long mismatch = Math.Abs(bidOffset - askOffset);
                    if (mismatch > cfg.OffsetToleranceTicks) continue;
                    var ageMismatch = (bid.FirstSeenUtc - ask.FirstSeenUtc).Duration();
                    if (mismatch < bestMismatch || (mismatch == bestMismatch && ageMismatch < bestAgeMismatch))
                    {
                        chosen = ask;
                        bestMismatch = mismatch;
                        bestAgeMismatch = ageMismatch;
                    }
                }

                if (chosen == null)
                    continue;

                long askOffset2 = chosen.PriceTicks - bestAsk;
                var pair = new Pair
                {
                    PairId = $"FP-{epoch}-{++pairSequence}",
                    BidId = bid.Id,
                    AskId = chosen.Id,
                    Size = size,
                    FirstSeenUtc = Max(bid.FirstSeenUtc, chosen.FirstSeenUtc),
                    RefBidOffset = bidOffset,
                    RefAskOffset = askOffset2,
                    InitialWidthTicks = chosen.PriceTicks - bid.PriceTicks,
                    LastBidOffset = bidOffset,
                    LastAskOffset = askOffset2,
                    LastBidTicks = bid.PriceTicks,
                    LastAskTicks = chosen.PriceTicks,
                    LastActivityUtc = t,
                    UsesHeuristicStitching = bid.HeuristicallyStitched || chosen.HeuristicallyStitched
                };
                pairs.Add(pair);
                assigned.Add(bid.Id);
                assigned.Add(chosen.Id);
            }
        }
    }

    private FloatingPairSnapshot BuildSnapshot(BookSnapshot book, EligibleOrders eligible)
    {
        int candidateCount = 0;
        foreach (var size in eligible.BidsBySize.Keys)
            if (eligible.AsksBySize.TryGetValue(size, out var asks))
                candidateCount += Math.Min(eligible.BidsBySize[size].Count, asks.Count);

        var summaries = pairs
            .Where(p => p.State != FloatingPairState.Broken && p.MissingSide == BookSide.Unknown)
            .Select(p => ToSummary(p, book.EventTimeUtc))
            .OrderByDescending(p => p.State)
            .ThenByDescending(p => p.Size)
            .ThenByDescending(p => p.FollowRatio)
            .ThenBy(p => p.PairId, StringComparer.Ordinal)
            .ToArray();

        int persistent = summaries.Count(s => s.State is FloatingPairState.Persistent or FloatingPairState.FloatingConfirmed);
        int confirmed = summaries.Count(s => s.State == FloatingPairState.FloatingConfirmed);
        int large = summaries.Count(s => s.Tier == FloatingPairTier.Large);
        int veryLarge = summaries.Count(s => s.Tier == FloatingPairTier.VeryLarge);
        var top = summaries.FirstOrDefault();
        var topThree = summaries.Take(3).ToArray();
        var recentBreaks = breaks.TakeLast(10).ToArray();
        var quality = book.DataQuality == MetricQuality.Exact ? MetricQuality.Exact : MetricQuality.Derived;

        return new FloatingPairSnapshot(
            FloatingPairDetectorStatus.MboReady,
            quality,
            quality == MetricQuality.Exact ? "MBO order identity confirmed" : "MBO-derived with incomplete identity coverage",
            book.BookEpoch,
            eligible.LargeBids,
            eligible.LargeAsks,
            eligible.VeryLargeBids,
            eligible.VeryLargeAsks,
            candidateCount,
            persistent,
            confirmed,
            large,
            veryLarge,
            top,
            topThree,
            recentBreaks.LastOrDefault(),
            recentBreaks);
    }

    private FloatingPairSummary ToSummary(Pair pair, DateTime t)
    {
        orders.TryGetValue(pair.BidId, out var bid);
        orders.TryGetValue(pair.AskId, out var ask);
        double followRatio = pair.Opportunities > 0 ? pair.SynchronizedMoves / (double)pair.Opportunities : 0;
        double meanDelay = pair.SynchronizedMoves > 0 ? pair.SyncDelaySumMs / pair.SynchronizedMoves : 0;
        double confidence = Confidence(pair, followRatio);
        return new FloatingPairSummary(
            pair.PairId,
            pair.State,
            pair.Size >= cfg.VeryLargeThreshold ? FloatingPairTier.VeryLarge : FloatingPairTier.Large,
            pair.Size,
            pair.BidId,
            pair.AskId,
            bid?.PriceTicks ?? pair.LastBidTicks,
            ask?.PriceTicks ?? pair.LastAskTicks,
            pair.LastBidOffset,
            pair.LastAskOffset,
            t - pair.FirstSeenUtc,
            pair.SynchronizedMoves,
            pair.Opportunities,
            followRatio,
            meanDelay,
            pair.MaxSyncDelayMs,
            confidence,
            pair.UsesHeuristicStitching);
    }

    private double Confidence(Pair pair, double followRatio)
    {
        double state = pair.State == FloatingPairState.FloatingConfirmed ? 1.0
            : pair.State == FloatingPairState.Persistent ? 0.60
            : 0.30;
        double symmetry = 1.0 - Math.Min(1.0, Math.Abs(pair.LastBidOffset - pair.LastAskOffset) / (double)Math.Max(1, cfg.OffsetToleranceTicks + 1));
        double stitchPenalty = pair.UsesHeuristicStitching ? 0.85 : 1.0;
        return Math.Max(0, Math.Min(1, state * (0.55 + 0.45 * followRatio) * (0.5 + 0.5 * symmetry) * stitchPenalty));
    }

    private bool TryStitchReplacement(string newId, BookSide side, long priceTicks, long size, DateTime t)
    {
        foreach (var pair in pairs)
        {
            if (pair.MissingSide != side)
                continue;
            if (t - pair.MissingSinceUtc > cfg.ReplacementStitchingWindow)
                continue;

            bool sizeOk = pair.Size == size;
            bool priceOk = Math.Abs(priceTicks - pair.MissingExpectedPriceTicks) <= cfg.OffsetToleranceTicks + 1;
            bool epochOk = epoch >= 0;
            if (!sizeOk || !priceOk || !epochOk)
                continue;

            if (side == BookSide.Bid) pair.BidId = newId;
            else pair.AskId = newId;
            pair.MissingSide = BookSide.Unknown;
            pair.MissingOrderId = "";
            pair.UsesHeuristicStitching = true;
            pair.LastActivityUtc = t;
            return true;
        }
        return false;
    }

    private void MarkMissing(Pair pair, BookSide side, string orderId, long priceTicks, DateTime t)
    {
        pair.MissingSide = side;
        pair.MissingOrderId = orderId;
        pair.MissingSinceUtc = t;
        pair.MissingExpectedPriceTicks = priceTicks;
    }

    private void BreakInactive(DateTime t, long bookEpoch)
    {
        foreach (var pair in pairs.ToArray())
        {
            if (pair.State != FloatingPairState.Broken && t - pair.LastActivityUtc > cfg.MaximumInactivity)
                BreakPair(pair, t, "maximum inactivity");
        }
        pairs.RemoveAll(p => p.State == FloatingPairState.Broken);
    }

    private void BreakAll(DateTime t, long bookEpoch, string reason)
    {
        foreach (var pair in pairs.ToArray())
            BreakPair(pair, t, reason);
        pairs.RemoveAll(p => p.State == FloatingPairState.Broken);
    }

    private void BreakPair(Pair pair, DateTime t, string reason)
    {
        if (pair.State == FloatingPairState.Broken)
            return;
        var previous = pair.State;
        pair.State = FloatingPairState.Broken;
        var ev = new FloatingPairBreakEvent(t, epoch, pair.PairId, reason, previous);
        breaks.Add(ev);
        if (breaks.Count > 100)
            breaks.RemoveAt(0);
    }

    private void TrimRemoved(DateTime t)
    {
        removedLegs.RemoveAll(x => x.BookEpoch != epoch || t - x.RemovedUtc > cfg.ReplacementStitchingWindow);
    }

    private static void AddBySize(Dictionary<long, List<TrackedOrder>> map, TrackedOrder order)
    {
        if (!map.TryGetValue(order.Size, out var list))
            map[order.Size] = list = new List<TrackedOrder>();
        list.Add(order);
    }

    private static DateTime Max(DateTime a, DateTime b) => a >= b ? a : b;

    private sealed class EligibleOrders
    {
        public int LargeBids;
        public int LargeAsks;
        public int VeryLargeBids;
        public int VeryLargeAsks;
        public readonly Dictionary<long, List<TrackedOrder>> BidsBySize = new();
        public readonly Dictionary<long, List<TrackedOrder>> AsksBySize = new();
    }
}
