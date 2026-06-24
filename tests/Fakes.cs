using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MBO_Market_Data_Analytics;
using TradingPlatform.BusinessLayer;

namespace MBO_Market_Data_Analytics.Tests
{
    /// <summary>
    /// Deterministic in-memory <see cref="IBroker"/> for execution tests. Records every order
    /// operation for assertions, hands out stable ids, lets tests script success/failure per order
    /// kind, and exposes a settable net position. No threading, no platform calls.
    /// </summary>
    public sealed class FakeBroker : IBroker
    {
        public bool StopOrderTypeAvailable = true;
        public bool EntrySucceeds = true;
        public bool StopSucceeds = true;
        public bool LimitSucceeds = true;
        public bool MarketCloseSucceeds = true;
        public string FailMessage = "rejected by fake broker";

        /// <summary>Signed net position returned by <see cref="GetNetPositionSize"/> (long &gt; 0).</summary>
        public double NetPosition = 0.0;

        private int nextId = 1;

        public sealed record PlacedOrder(string Id, string Kind, Side Side, double Price, double Qty, string Comment);

        public readonly List<PlacedOrder> Placed = new();
        public readonly List<string> Cancelled = new();
        public readonly List<(string Id, double Qty)> Modified = new();
        public int MarketCloseCount { get; private set; }

        public bool HasStopOrderType => StopOrderTypeAvailable;

        private OrderOpResult Place(bool ok, string kind, Side side, double price, double qty, string comment)
        {
            if (!ok) return OrderOpResult.Fail(FailMessage);
            string id = $"FAKE-{nextId++}";
            Placed.Add(new PlacedOrder(id, kind, side, price, qty, comment ?? ""));
            return OrderOpResult.Ok(id);
        }

        public OrderOpResult PlaceEntryLimit(Side side, double price, double qty, string comment)
            => Place(EntrySucceeds, "EntryLimit", side, price, qty, comment);

        public OrderOpResult PlaceProtectiveStop(Side side, double triggerPrice, double qty, string comment)
            => Place(StopSucceeds && StopOrderTypeAvailable, "Stop", side, triggerPrice, qty, comment);

        public OrderOpResult PlaceProtectiveLimit(Side side, double price, double qty, string comment)
            => Place(LimitSucceeds, "Limit", side, price, qty, comment);

        public OrderOpResult PlaceMarketClose(Side side, double qty, string comment)
        {
            var r = Place(MarketCloseSucceeds, "MarketClose", side, 0.0, qty, comment);
            if (r.Success) MarketCloseCount++;
            return r;
        }

        public OrderOpResult ModifyQuantity(string orderId, double newTotalQty)
        {
            Modified.Add((orderId, newTotalQty));
            return OrderOpResult.Ok(orderId);
        }

        public void Cancel(string orderId) => Cancelled.Add(orderId);

        public double GetNetPositionSize() => NetPosition;
    }

    /// <summary>
    /// Deterministic <see cref="IExecScheduler"/>. In auto-run mode (default) posted work runs
    /// immediately and synchronously; in manual mode work is queued so a test can control ordering
    /// (via <see cref="Drain"/>, priority-ordered) and exercise <see cref="CancelPendingEntries"/>.
    /// </summary>
    public sealed class FakeScheduler : IExecScheduler
    {
        public bool AutoRun = true;
        public bool Accepting = true;
        public bool ShutdownCalled = false;
        public int StopAcceptingCalls = 0;
        public int PostedCount { get; private set; }

        private readonly List<(int priority, TaskCategory category, System.Func<Task> work)> pending = new();
        public IReadOnlyList<(int priority, TaskCategory category, System.Func<Task> work)> Pending => pending;

        public void Post(System.Func<Task> work, int priority, TaskCategory category)
        {
            if (!Accepting) return;
            PostedCount++;
            if (AutoRun) work(); // bodies complete synchronously and return Task.CompletedTask
            else pending.Add((priority, category, work));
        }

        public void StopAccepting()
        {
            Accepting = false;
            StopAcceptingCalls++;
        }

        public int CancelPendingEntries()
            => pending.RemoveAll(p => p.category == TaskCategory.EntryPlacement);

        public void Shutdown()
        {
            ShutdownCalled = true;
            Accepting = false;
        }

        /// <summary>Run all queued work in priority order (lower priority value first).</summary>
        public void Drain()
        {
            var ordered = pending.OrderBy(p => p.priority).ToList();
            pending.Clear();
            foreach (var p in ordered) p.work();
        }
    }
}
