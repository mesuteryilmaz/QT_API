using QT.Market.Events;

namespace QT.Runtime.AnalyticsRuntime;

public interface IAnalyticsRuntime
{
    void Start(DateTime nowUtc);
    void Stop();
    void BeginSnapshot(DateTime eventTimeUtc);
    void BeginSnapshot(DateTime eventTimeUtc, QT.Core.Primitives.BookMode mode);
    void ApplySnapshotLevel(DateTime eventTimeUtc, QT.Core.Primitives.BookSide side, long priceTicks, long quantity, string? orderId = null, long priority = 0, int numberOrders = 0);
    AnalyticsRuntimeSnapshot EndSnapshot(DateTime eventTimeUtc, DateTime receiveTimeUtc);
    AnalyticsRuntimeSnapshot OnMarketEvent(in NormalizedMarketEvent marketEvent);
    AnalyticsRuntimeSnapshot AdvanceTime(DateTime eventTimeUtc);
    AnalyticsRuntimeSnapshot Current { get; }
}
