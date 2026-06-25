using System;
using System.Collections.Generic;

namespace MBO_Market_Data_Analytics.LegacyV1
{
    // ---------------------------------------------------------------------------------------------
    // MBO Paired Floating Quote Detector  (audit 4b — replaces the misnamed "grid maker" intent).
    //
    // Detects an INDIVIDUAL same-size bid order + ask order that rest in the book and FLOAT through
    // the DOM, repricing to follow the BBO — a single coordinated-looking matched quote pair. This is
    // a different object from the aggregate symmetric-lattice metric (which stays as a separate
    // structural score): here we track per-order identity, exact-equal size, touch offsets, and
    // temporal co-movement, and only call a pair "floating-confirmed" after it follows the market.
    //
    // It does NOT claim to identify a market maker: without participant/firm IDs the strongest
    // defensible statement is "the book contains a coordinated-looking matched pair".
    //
    // Pure module: driven by ticks + integer sizes only (no platform types), so it is unit-testable
    // in isolation. The calculator feeds it per-order MBO events and samples it on the throttled
    // cadence. In MBP mode (no genuine per-order IDs) it reports Unavailable, never a misleading zero.
    //
    // Scope note (v1): pairs are same-ID only — cancel/replace stitching across new order IDs is not
    // yet implemented (audit recommendation #8) and is left for a follow-up.
    // ---------------------------------------------------------------------------------------------

    public enum PqDataStatus : byte { Unavailable = 0, Warming = 1, MboReady = 2, BookInvalid = 3 }
    public enum PairTier : byte { None = 0, Large = 1, VeryLarge = 2 }
    public enum PairState : byte { None = 0, Candidate = 1, Persistent = 2, FloatingConfirmed = 3 }

    public sealed class PairedQuoteConfig
    {
        public long MinSize = 20;                 // eligible "large" order threshold (contracts)
        public long VeryLargeSize = 200;          // "very large" tier threshold
        public long MaxDistanceTicks = 100;       // ignore orders farther than this from the touch
        public long MaxOffsetMismatchTicks = 3;   // |bidOffset - askOffset| to qualify as a candidate
        public double MinPairAgeSeconds = 1.0;    // coexistence before a pair becomes Persistent
        public int MinSyncMoves = 2;              // synchronized market-follows for FloatingConfirmed
        public double MinFollowRatio = 0.80;      // followed / eligible market moves for confirmation
        public long MaxTrackingErrorTicks = 1;    // per-leg drift from reference offset allowed on a follow
        public int MaxPairsTracked = 16;
    }

    public readonly struct PairedQuoteSnapshot
    {
        public readonly PqDataStatus Status;
        public readonly int EligibleLargeBids, EligibleLargeAsks;
        public readonly int EligibleVeryLargeBids, EligibleVeryLargeAsks;
        public readonly int ActivePairs;     // Candidate or better
        public readonly int ConfirmedPairs;  // FloatingConfirmed
        public readonly long TopPairSize;
        public readonly PairTier TopPairTier;
        public readonly long TopPairBidOffset, TopPairAskOffset;
        public readonly double TopPairAgeSeconds;
        public readonly int TopPairSyncMoves;
        public readonly double TopPairFollowRatio;
        public readonly PairState TopPairState;
        public readonly double Confidence;   // 0..1 summary for the strongest pair

        public PairedQuoteSnapshot(PqDataStatus status, int elB, int elA, int evB, int evA,
            int active, int confirmed, long topSize, PairTier topTier, long topBidOff, long topAskOff,
            double topAgeSec, int topSync, double topFollow, PairState topState, double confidence)
        {
            Status = status;
            EligibleLargeBids = elB; EligibleLargeAsks = elA;
            EligibleVeryLargeBids = evB; EligibleVeryLargeAsks = evA;
            ActivePairs = active; ConfirmedPairs = confirmed;
            TopPairSize = topSize; TopPairTier = topTier;
            TopPairBidOffset = topBidOff; TopPairAskOffset = topAskOff;
            TopPairAgeSeconds = topAgeSec; TopPairSyncMoves = topSync;
            TopPairFollowRatio = topFollow; TopPairState = topState; Confidence = confidence;
        }

        public static readonly PairedQuoteSnapshot Empty =
            new PairedQuoteSnapshot(PqDataStatus.Unavailable, 0, 0, 0, 0, 0, 0, 0,
                PairTier.None, 0, 0, 0, 0, 0, PairState.None, 0);
    }

    public sealed class PairedQuoteDetector
    {
        private sealed class Ord
        {
            public string Id = "";
            public bool IsBid;
            public long Ticks;
            public long Size;
            public long FirstSeenTicks;
            public long LastSeenTicks;
        }

        private sealed class Pair
        {
            public string BidId = "", AskId = "";
            public long Size;
            public long FirstSeenTicks;
            public long RefBidOffset, RefAskOffset; // touch offsets at formation — the band the pair holds
            public long LastBidOffset, LastAskOffset;
            public int EligibleMoves, FollowedMoves;
            public PairState State = PairState.Candidate;
        }

        private readonly PairedQuoteConfig cfg;
        private readonly Dictionary<string, Ord> orders = new Dictionary<string, Ord>();
        private readonly List<Pair> pairs = new List<Pair>();

        private bool hasMarketCenter;
        private long lastMarketCenter2;
        private long bookEpoch;
        private PairedQuoteSnapshot snapshot = PairedQuoteSnapshot.Empty;

        public PairedQuoteDetector(PairedQuoteConfig? config = null)
        {
            cfg = config ?? new PairedQuoteConfig();
        }

        public PairedQuoteSnapshot Snapshot => snapshot;
        public long BookEpoch => bookEpoch;

        public void Reset(long epoch)
        {
            orders.Clear();
            pairs.Clear();
            hasMarketCenter = false;
            lastMarketCenter2 = 0;
            bookEpoch = epoch;
            snapshot = PairedQuoteSnapshot.Empty;
        }

        // Add or modify a single order (price/side/size). Covers MBO Add, Update, and reprice.
        public void OnOrderUpsert(string id, bool isBid, long ticks, long size, long timeTicks)
        {
            if (string.IsNullOrEmpty(id) || size <= 0) { OnOrderRemove(id, timeTicks); return; }
            if (orders.TryGetValue(id, out var o))
            {
                o.IsBid = isBid;
                o.Ticks = ticks;
                o.Size = size;
                o.LastSeenTicks = timeTicks;
            }
            else
            {
                orders[id] = new Ord
                {
                    Id = id, IsBid = isBid, Ticks = ticks, Size = size,
                    FirstSeenTicks = timeTicks, LastSeenTicks = timeTicks
                };
            }
        }

        public void OnOrderRemove(string id, long timeTicks)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (orders.Remove(id))
                pairs.RemoveAll(p => p.BidId == id || p.AskId == id);
        }

        // Samples the book on the throttled cadence: refresh eligible counts, update tracked pairs
        // (offsets, market-follow), form new exact-size candidates, classify, and build the snapshot.
        public void AdvanceTime(long timeTicks, bool hasBest, long bestBidTicks, long bestAskTicks, PqDataStatus status)
        {
            if (status != PqDataStatus.MboReady || !hasBest)
            {
                // Out of MBO mode / no two-sided book: report status, do not invent pairs.
                snapshot = new PairedQuoteSnapshot(status, 0, 0, 0, 0, 0, 0, 0,
                    PairTier.None, 0, 0, 0, 0, 0, PairState.None, 0);
                // Keep order map; just suspend center tracking so we don't false-fire a move on resume.
                hasMarketCenter = false;
                return;
            }

            long marketCenter2 = bestBidTicks + bestAskTicks;
            bool marketMoved = hasMarketCenter && marketCenter2 != lastMarketCenter2;

            // --- eligible-order census + index by exact size for pairing ---
            int elB = 0, elA = 0, evB = 0, evA = 0;
            var eligBidsBySize = new Dictionary<long, List<Ord>>();
            var eligAsksBySize = new Dictionary<long, List<Ord>>();
            foreach (var o in orders.Values)
            {
                if (o.Size < cfg.MinSize) continue;
                long off = o.IsBid ? bestBidTicks - o.Ticks : o.Ticks - bestAskTicks;
                if (off < 0 || off > cfg.MaxDistanceTicks) continue;

                if (o.IsBid) { elB++; if (o.Size >= cfg.VeryLargeSize) evB++; }
                else { elA++; if (o.Size >= cfg.VeryLargeSize) evA++; }

                var idx = o.IsBid ? eligBidsBySize : eligAsksBySize;
                if (!idx.TryGetValue(o.Size, out var lst)) idx[o.Size] = lst = new List<Ord>();
                lst.Add(o);
            }

            // --- update existing pairs; drop broken ones ---
            var paired = new HashSet<string>();
            for (int i = pairs.Count - 1; i >= 0; i--)
            {
                var p = pairs[i];
                if (!orders.TryGetValue(p.BidId, out var b) || !orders.TryGetValue(p.AskId, out var a)
                    || !b.IsBid || a.IsBid || b.Size != p.Size || a.Size != p.Size)
                {
                    pairs.RemoveAt(i);
                    continue;
                }
                long bidOff = bestBidTicks - b.Ticks;
                long askOff = a.Ticks - bestAskTicks;
                if (bidOff < 0 || bidOff > cfg.MaxDistanceTicks || askOff < 0 || askOff > cfg.MaxDistanceTicks)
                {
                    pairs.RemoveAt(i);
                    continue;
                }

                if (marketMoved)
                {
                    // A genuine floating pair holds its offsets from the touch as the BBO moves. A leg
                    // that fails to reprice drifts cumulatively from its reference offset; once either
                    // leg drifts past tolerance the move was not followed (audit: touch-offset stability).
                    p.EligibleMoves++;
                    bool bidTracks = Math.Abs(bidOff - p.RefBidOffset) <= cfg.MaxTrackingErrorTicks;
                    bool askTracks = Math.Abs(askOff - p.RefAskOffset) <= cfg.MaxTrackingErrorTicks;
                    if (bidTracks && askTracks) p.FollowedMoves++;
                }
                p.LastBidOffset = bidOff;
                p.LastAskOffset = askOff;

                double ageSec = (timeTicks - p.FirstSeenTicks) / (double)TimeSpan.TicksPerSecond;
                double followRatio = p.EligibleMoves > 0 ? p.FollowedMoves / (double)p.EligibleMoves : 0.0;
                if (ageSec >= cfg.MinPairAgeSeconds && p.FollowedMoves >= cfg.MinSyncMoves && followRatio >= cfg.MinFollowRatio)
                    p.State = PairState.FloatingConfirmed;
                else if (ageSec >= cfg.MinPairAgeSeconds)
                    p.State = PairState.Persistent;
                else
                    p.State = PairState.Candidate;

                paired.Add(p.BidId);
                paired.Add(p.AskId);
            }

            // --- form new exact-size candidate pairs from unpaired eligible orders ---
            foreach (var kv in eligBidsBySize)
            {
                if (pairs.Count >= cfg.MaxPairsTracked) break;
                if (!eligAsksBySize.TryGetValue(kv.Key, out var askList)) continue;

                var bidList = kv.Value;
                foreach (var b in bidList)
                {
                    if (pairs.Count >= cfg.MaxPairsTracked) break;
                    if (paired.Contains(b.Id)) continue;
                    long bidOff = bestBidTicks - b.Ticks;

                    // greedy nearest-offset match among unpaired asks of the same exact size
                    Ord? best = null; long bestMis = long.MaxValue;
                    foreach (var a in askList)
                    {
                        if (paired.Contains(a.Id)) continue;
                        long askOff = a.Ticks - bestAskTicks;
                        long mis = Math.Abs(bidOff - askOff);
                        if (mis <= cfg.MaxOffsetMismatchTicks && mis < bestMis) { bestMis = mis; best = a; }
                    }
                    if (best == null) continue;

                    long askOff2 = best.Ticks - bestAskTicks;
                    pairs.Add(new Pair
                    {
                        BidId = b.Id, AskId = best.Id, Size = kv.Key,
                        FirstSeenTicks = Math.Max(b.FirstSeenTicks, best.FirstSeenTicks),
                        RefBidOffset = bidOff, RefAskOffset = askOff2,
                        LastBidOffset = bidOff, LastAskOffset = askOff2,
                        State = PairState.Candidate
                    });
                    paired.Add(b.Id);
                    paired.Add(best.Id);
                }
            }

            // --- summarize ---
            int active = 0, confirmed = 0;
            Pair? top = null;
            foreach (var p in pairs)
            {
                active++;
                if (p.State == PairState.FloatingConfirmed) confirmed++;
                if (top == null || ComparePairStrength(p, top) > 0) top = p;
            }

            long topSize = 0, topBidOff = 0, topAskOff = 0;
            int topSync = 0;
            double topAge = 0, topFollow = 0, confidence = 0;
            PairTier topTier = PairTier.None;
            PairState topState = PairState.None;
            if (top != null)
            {
                topSize = top.Size;
                topTier = top.Size >= cfg.VeryLargeSize ? PairTier.VeryLarge : PairTier.Large;
                topBidOff = top.LastBidOffset;
                topAskOff = top.LastAskOffset;
                topAge = (timeTicks - top.FirstSeenTicks) / (double)TimeSpan.TicksPerSecond;
                topSync = top.FollowedMoves;
                topFollow = top.EligibleMoves > 0 ? top.FollowedMoves / (double)top.EligibleMoves : 0.0;
                topState = top.State;
                confidence = PairConfidence(top, topAge);
            }

            snapshot = new PairedQuoteSnapshot(PqDataStatus.MboReady, elB, elA, evB, evA,
                active, confirmed, topSize, topTier, topBidOff, topAskOff, topAge, topSync, topFollow, topState, confidence);

            lastMarketCenter2 = marketCenter2;
            hasMarketCenter = true;
        }

        // Strength ordering for picking the displayed "top" pair: state, then size, then follow ratio.
        private static int ComparePairStrength(Pair a, Pair b)
        {
            int s = a.State.CompareTo(b.State);
            if (s != 0) return s;
            s = a.Size.CompareTo(b.Size);
            if (s != 0) return s;
            double fa = a.EligibleMoves > 0 ? a.FollowedMoves / (double)a.EligibleMoves : 0.0;
            double fb = b.EligibleMoves > 0 ? b.FollowedMoves / (double)b.EligibleMoves : 0.0;
            return fa.CompareTo(fb);
        }

        private double PairConfidence(Pair p, double ageSec)
        {
            // Blend presence (state), market-following quality, and offset symmetry into a 0..1 score.
            double stateW = p.State == PairState.FloatingConfirmed ? 1.0
                          : p.State == PairState.Persistent ? 0.5
                          : 0.2;
            double follow = p.EligibleMoves > 0 ? p.FollowedMoves / (double)p.EligibleMoves : 0.0;
            long mis = Math.Abs(p.LastBidOffset - p.LastAskOffset);
            double sym = cfg.MaxOffsetMismatchTicks > 0
                ? Math.Max(0.0, 1.0 - mis / (double)cfg.MaxOffsetMismatchTicks) : 1.0;
            return stateW * (0.5 + 0.5 * follow) * (0.5 + 0.5 * sym);
        }
    }
}
