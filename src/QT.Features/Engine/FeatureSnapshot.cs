using QT.Core.Quality;
using QT.Features.AggregateLattice;
using QT.Features.FloatingPairs;
using QT.Features.MarketState;
using QT.Market.Snapshots;

namespace QT.Features.Engine;

public sealed record FeatureSnapshot(
    long Sequence,
    DateTime EventTimeUtc,
    DateTime ReceiveTimeUtc,
    string Symbol,
    string RuntimeSessionId,
    string ConfigurationHash,
    BookSnapshot Book,
    MarketStateSnapshot MarketState,
    FloatingPairSnapshot FloatingPairs,
    SymmetricAggregateLatticeSnapshot AggregateLattice,
    IReadOnlyDictionary<string, FeatureValue> Values)
{
    public static FeatureSnapshot Empty(string sessionId, string configurationHash, BookSnapshot book)
        => new(0, book.EventTimeUtc, book.ReceiveTimeUtc, book.Symbol, sessionId, configurationHash, book,
            MarketStateSnapshot.Empty(book.EventTimeUtc, book.BookEpoch, MetricQuality.Unavailable, "not initialized"),
            FloatingPairSnapshot.Unavailable(book.BookEpoch, FloatingPairDetectorStatus.Unavailable, MetricQuality.Unavailable, "not initialized"),
            SymmetricAggregateLatticeSnapshot.Disabled,
            new Dictionary<string, FeatureValue>());
}
