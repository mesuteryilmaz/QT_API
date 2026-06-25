using QT.Core.Primitives;
using QT.Market.Snapshots;

namespace QT.Market.OrderBook;

public sealed class MbpOrderBook
{
    private readonly double tickSize;
    private readonly SortedDictionary<long, long> bids = new(Comparer<long>.Create((a, b) => b.CompareTo(a)));
    private readonly SortedDictionary<long, long> asks = new();

    public MbpOrderBook(double tickSize)
    {
        this.tickSize = tickSize > 0 ? tickSize : 1.0;
    }

    public int BidLevels => bids.Count;
    public int AskLevels => asks.Count;

    public void Clear()
    {
        bids.Clear();
        asks.Clear();
    }

    public void Apply(BookSide side, long priceTicks, long quantity, bool closed)
    {
        if (side is not (BookSide.Bid or BookSide.Ask) || priceTicks <= 0)
            return;

        var book = side == BookSide.Bid ? bids : asks;
        if (closed || quantity <= 0)
            book.Remove(priceTicks);
        else
            book[priceTicks] = quantity;
    }

    public bool TryBestBid(out long ticks, out long quantity) => TryBest(bids, out ticks, out quantity);
    public bool TryBestAsk(out long ticks, out long quantity) => TryBest(asks, out ticks, out quantity);

    public IReadOnlyList<BookLevelSnapshot> Top(BookSide side, int depth)
    {
        var source = side == BookSide.Bid ? bids : asks;
        var rows = new List<BookLevelSnapshot>(Math.Min(depth, source.Count));
        foreach (var (ticks, qty) in source)
        {
            if (rows.Count >= depth) break;
            rows.Add(new BookLevelSnapshot(side, ticks, ticks * tickSize, qty, qty > 0 ? 1 : 0));
        }
        return rows;
    }

    private static bool TryBest(SortedDictionary<long, long> side, out long ticks, out long quantity)
    {
        if (side.Count == 0)
        {
            ticks = 0;
            quantity = 0;
            return false;
        }

        var first = side.First();
        ticks = first.Key;
        quantity = first.Value;
        return true;
    }
}
