namespace QT.Core.Primitives;

[Flags]
public enum FeedCapabilities
{
    None = 0,
    Trades = 1 << 0,
    TradeAggressor = 1 << 1,
    TradeId = 1 << 2,
    TopOfBook = 1 << 3,
    MarketByPrice = 1 << 4,
    MarketByOrder = 1 << 5,
    NonAggregatedDepth = 1 << 6,
    OrderId = 1 << 7,
    OrderAction = 1 << 8,
    ExchangeTimestamp = 1 << 9,
    SequenceNumber = 1 << 10,
    HistoricalTrades = 1 << 11,
    HistoricalBars = 1 << 12
}

public enum MarketEventKind : byte
{
    Unknown = 0,
    Trade = 1,
    BookLevel = 2,
    BookSnapshotBegin = 3,
    BookSnapshotLevel = 4,
    BookSnapshotEnd = 5,
    BookFlush = 6,
    SessionReset = 7,
    ConnectionState = 8,
    Timer = 9
}

public enum MarketDataQuality : byte
{
    Unknown = 0,
    Raw = 1,
    Derived = 2,
    Replayed = 3,
    Degraded = 4,
    Stale = 5,
    Invalid = 6
}

public enum BookSide : byte
{
    Unknown = 0,
    Bid = 1,
    Ask = 2
}

public enum TradeAggressor : byte
{
    Unknown = 0,
    Buy = 1,
    Sell = 2
}

public enum BookMode : byte
{
    Unknown = 0,
    Mbp = 1,
    Mbo = 2
}

public enum BookLifecycleState : byte
{
    Disconnected = 0,
    AwaitingSnapshot = 1,
    ApplyingSnapshot = 2,
    ReplayingBufferedEvents = 3,
    WarmingUp = 4,
    Valid = 5,
    Invalid = 6,
    Recovering = 7,
    DowngradedToMbp = 8
}

public enum NormalizedBookAction : byte
{
    None = 0,
    Add = 1,
    Update = 2,
    Remove = 3,
    Snapshot = 4,
    Flush = 5,
    Trade = 6
}
