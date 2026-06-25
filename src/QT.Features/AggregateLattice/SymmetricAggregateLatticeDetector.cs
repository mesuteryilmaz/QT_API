using QT.Core.Primitives;
using QT.Market.Snapshots;

namespace QT.Features.AggregateLattice;

public sealed class SymmetricAggregateLatticeConfig
{
    public int MinimumRungs { get; init; } = 3;
    public long MaximumOffsetTicks { get; init; } = 50;
    public double SizeToleranceFraction { get; init; } = 0.25;
    public bool Enabled { get; init; } = false;
}

public sealed record SymmetricAggregateLatticeSnapshot(
    bool Enabled,
    double Score,
    int BidRungs,
    int AskRungs,
    int SpacingTicks,
    long RungSize,
    double Regularity,
    string Description)
{
    public static SymmetricAggregateLatticeSnapshot Disabled { get; } =
        new(false, 0, 0, 0, 0, 0, 0, "experimental aggregate lattice detector disabled");
}

public sealed class SymmetricAggregateLatticeDetector
{
    private readonly SymmetricAggregateLatticeConfig cfg;

    public SymmetricAggregateLatticeDetector(SymmetricAggregateLatticeConfig? config = null)
    {
        cfg = config ?? new SymmetricAggregateLatticeConfig();
    }

    public SymmetricAggregateLatticeSnapshot Scan(BookSnapshot book)
    {
        if (!cfg.Enabled)
            return SymmetricAggregateLatticeSnapshot.Disabled;
        if (!book.HasTwoSidedBook)
            return new SymmetricAggregateLatticeSnapshot(true, 0, 0, 0, 0, 0, 0, "book not two-sided");

        var bid = FindLadder(book.Bids, BookSide.Bid);
        var ask = FindLadder(book.Asks, BookSide.Ask);
        if (bid.Rungs < cfg.MinimumRungs || ask.Rungs < cfg.MinimumRungs)
            return new SymmetricAggregateLatticeSnapshot(true, 0, bid.Rungs, ask.Rungs, 0, 0, 0,
                "insufficient repeated aggregate rungs");

        int spacing = bid.SpacingTicks == ask.SpacingTicks ? bid.SpacingTicks : 0;
        double rungSym = 1.0 - Math.Min(1.0, Math.Abs(bid.RungSize - ask.RungSize) /
            (double)Math.Max(1, Math.Max(bid.RungSize, ask.RungSize)));
        double rungScore = Math.Min(bid.Rungs, ask.Rungs) / 8.0;
        double regularity = (bid.Regularity + ask.Regularity) / 2.0;
        double score = Math.Max(0, Math.Min(1, rungScore * regularity * rungSym));
        return new SymmetricAggregateLatticeSnapshot(true, score, bid.Rungs, ask.Rungs, spacing,
            Math.Max(bid.RungSize, ask.RungSize), regularity,
            "aggregate repeated multi-level ladder; not individual floating pairs");
    }

    private Ladder FindLadder(IReadOnlyList<BookLevelSnapshot> levels, BookSide side)
    {
        var eligible = levels
            .Where(x => x.Quantity > 0)
            .TakeWhile(x =>
            {
                long offset = side == BookSide.Bid
                    ? levels[0].PriceTicks - x.PriceTicks
                    : x.PriceTicks - levels[0].PriceTicks;
                return offset <= cfg.MaximumOffsetTicks;
            })
            .ToArray();

        if (eligible.Length < cfg.MinimumRungs)
            return default;

        Ladder best = default;
        for (int spacing = 1; spacing <= 16; spacing++)
        {
            int rungs = 1;
            long sizeSum = eligible[0].Quantity;
            long prev = eligible[0].PriceTicks;
            for (int i = 1; i < eligible.Length; i++)
            {
                long diff = Math.Abs(eligible[i].PriceTicks - prev);
                if (diff == spacing)
                {
                    rungs++;
                    sizeSum += eligible[i].Quantity;
                    prev = eligible[i].PriceTicks;
                }
            }

            if (rungs >= best.Rungs)
            {
                long avg = sizeSum / Math.Max(1, rungs);
                double regular = 0;
                if (avg > 0)
                {
                    int close = eligible.Take(rungs).Count(x => Math.Abs(x.Quantity - avg) <= avg * cfg.SizeToleranceFraction);
                    regular = close / (double)rungs;
                }
                best = new Ladder(rungs, spacing, avg, regular);
            }
        }

        return best;
    }

    private readonly record struct Ladder(int Rungs, int SpacingTicks, long RungSize, double Regularity);
}
