using QT.Core.Primitives;

namespace QT.Market.Events;

public readonly record struct NormalizedMarketEvent(
    long LocalSequence,
    DateTime EventTimeUtc,
    DateTime ReceiveTimeUtc,
    string Symbol,
    long BookEpoch,
    MarketEventKind Kind,
    MarketDataQuality Quality,
    NormalizedBookAction BookAction,
    BookSide Side,
    long PriceTicks,
    double Price,
    long Quantity,
    string? OrderId,
    bool Closed,
    TradeAggressor Aggressor,
    long Priority,
    int NumberOrders,
    bool IsSnapshotSeed)
{
    public static NormalizedMarketEvent Trade(
        long sequence,
        DateTime eventTimeUtc,
        DateTime receiveTimeUtc,
        string symbol,
        double price,
        long quantity,
        TradeAggressor aggressor,
        long bookEpoch = 0)
        => new(sequence, eventTimeUtc, receiveTimeUtc, symbol, bookEpoch, MarketEventKind.Trade,
            MarketDataQuality.Raw, NormalizedBookAction.Trade, BookSide.Unknown, 0, price, quantity,
            null, false, aggressor, 0, 0, false);

    public static NormalizedMarketEvent BookLevel(
        long sequence,
        DateTime eventTimeUtc,
        DateTime receiveTimeUtc,
        string symbol,
        BookSide side,
        long priceTicks,
        double price,
        long quantity,
        string? orderId,
        bool closed,
        long priority = 0,
        int numberOrders = 0,
        long bookEpoch = 0,
        bool isSnapshotSeed = false)
        => new(sequence, eventTimeUtc, receiveTimeUtc, symbol, bookEpoch,
            isSnapshotSeed ? MarketEventKind.BookSnapshotLevel : MarketEventKind.BookLevel,
            MarketDataQuality.Raw,
            isSnapshotSeed ? NormalizedBookAction.Snapshot : NormalizedBookAction.None,
            side, priceTicks, price, quantity, orderId, closed, TradeAggressor.Unknown,
            priority, numberOrders, isSnapshotSeed);

    public static NormalizedMarketEvent Flush(
        long sequence,
        DateTime eventTimeUtc,
        DateTime receiveTimeUtc,
        string symbol,
        string? reason = null,
        long bookEpoch = 0)
        => new(sequence, eventTimeUtc, receiveTimeUtc, symbol, bookEpoch, MarketEventKind.BookFlush,
            MarketDataQuality.Raw, NormalizedBookAction.Flush, BookSide.Unknown, 0, 0, 0,
            reason ?? "FLUSH", true, TradeAggressor.Unknown, 0, 0, false);
}
