using System.Linq;
using TradingPlatform.BusinessLayer;

namespace MBO_Market_Data_Analytics
{
    /// <summary>
    /// Result of an outbound broker operation, in platform-neutral form so the execution logic and
    /// its tests do not depend on Quantower's TradingOperationResult.
    /// </summary>
    public readonly struct OrderOpResult
    {
        public readonly bool Success;
        public readonly string OrderId;
        public readonly string Message;

        public OrderOpResult(bool success, string orderId, string message)
        {
            Success = success;
            OrderId = orderId ?? "";
            Message = message ?? "";
        }

        public static OrderOpResult Ok(string orderId) => new OrderOpResult(true, orderId, "");
        public static OrderOpResult Fail(string message) => new OrderOpResult(false, "", message);
    }

    /// <summary>
    /// Seam over the broker. All OUTBOUND order operations the execution logic performs go through
    /// this interface so the logic can be driven by a deterministic fake broker in tests, instead of
    /// calling the unmockable Core.Instance singleton directly. Inbound events (order updates, fills,
    /// position changes) are delivered separately by the host adapter.
    /// </summary>
    public interface IBroker
    {
        /// <summary>True when the symbol exposes a Stop order type (for protective stops).</summary>
        bool HasStopOrderType { get; }

        /// <summary>Entry limit order (Order usage). Returns success + the assigned order id.</summary>
        OrderOpResult PlaceEntryLimit(Side side, double price, double qty, string comment);

        /// <summary>Protective stop (CloseOrder usage), triggered at <paramref name="triggerPrice"/>.</summary>
        OrderOpResult PlaceProtectiveStop(Side side, double triggerPrice, double qty, string comment);

        /// <summary>Protective take-profit limit (CloseOrder usage).</summary>
        OrderOpResult PlaceProtectiveLimit(Side side, double price, double qty, string comment);

        /// <summary>Market order to close/flatten (Order usage).</summary>
        OrderOpResult PlaceMarketClose(Side side, double qty, string comment);

        /// <summary>Modify a working order's TOTAL quantity.</summary>
        OrderOpResult ModifyQuantity(string orderId, double newTotalQty);

        /// <summary>Cancel a working order by id (no-op if it is already gone).</summary>
        void Cancel(string orderId);

        /// <summary>Signed net broker position for the strategy's symbol+account (long &gt; 0).</summary>
        double GetNetPositionSize();
    }

    /// <summary>
    /// Live <see cref="IBroker"/> implementation over Quantower's Core.Instance. Resolves order types
    /// from the symbol and routes orders to the connected account. Order operations are addressed by
    /// id, so callers no longer need to hold the platform Order instance to cancel/modify.
    /// </summary>
    internal sealed class QuantowerBroker : IBroker
    {
        private readonly Symbol symbol;
        private readonly Account account;

        public QuantowerBroker(Symbol symbol, Account account)
        {
            this.symbol = symbol;
            this.account = account;
        }

        private string? ResolveTypeId(OrderTypeUsage usage, OrderTypeBehavior behavior)
        {
            var ot = symbol.GetAlowedOrderTypes(usage).FirstOrDefault(o => o.Behavior == behavior)
                  ?? symbol.GetAlowedOrderTypes(OrderTypeUsage.All).FirstOrDefault(o => o.Behavior == behavior);
            return ot?.Id;
        }

        public bool HasStopOrderType => ResolveTypeId(OrderTypeUsage.CloseOrder, OrderTypeBehavior.Stop) != null;

        public OrderOpResult PlaceEntryLimit(Side side, double price, double qty, string comment)
        {
            string typeId = ResolveTypeId(OrderTypeUsage.Order, OrderTypeBehavior.Limit) ?? OrderType.Limit;
            return Wrap(Core.Instance.PlaceOrder(new PlaceOrderRequestParameters
            {
                Symbol = symbol, Account = account, Side = side, OrderTypeId = typeId,
                Price = price, Quantity = qty, Comment = comment
            }));
        }

        public OrderOpResult PlaceProtectiveStop(Side side, double triggerPrice, double qty, string comment)
        {
            string? typeId = ResolveTypeId(OrderTypeUsage.CloseOrder, OrderTypeBehavior.Stop);
            if (typeId == null) return OrderOpResult.Fail("no stop order type available");
            return Wrap(Core.Instance.PlaceOrder(new PlaceOrderRequestParameters
            {
                Symbol = symbol, Account = account, Side = side, OrderTypeId = typeId,
                TriggerPrice = triggerPrice, Quantity = qty, Comment = comment
            }));
        }

        public OrderOpResult PlaceProtectiveLimit(Side side, double price, double qty, string comment)
        {
            string typeId = ResolveTypeId(OrderTypeUsage.CloseOrder, OrderTypeBehavior.Limit) ?? OrderType.Limit;
            return Wrap(Core.Instance.PlaceOrder(new PlaceOrderRequestParameters
            {
                Symbol = symbol, Account = account, Side = side, OrderTypeId = typeId,
                Price = price, Quantity = qty, Comment = comment
            }));
        }

        public OrderOpResult PlaceMarketClose(Side side, double qty, string comment)
        {
            string typeId = ResolveTypeId(OrderTypeUsage.Order, OrderTypeBehavior.Market) ?? OrderType.Market;
            var p = new PlaceOrderRequestParameters
            {
                Symbol = symbol, Account = account, Side = side, OrderTypeId = typeId, Quantity = qty
            };
            if (!string.IsNullOrEmpty(comment)) p.Comment = comment;
            return Wrap(Core.Instance.PlaceOrder(p));
        }

        public OrderOpResult ModifyQuantity(string orderId, double newTotalQty)
        {
            var order = Core.Instance.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null) return OrderOpResult.Fail($"order {orderId} not found");
            var result = Core.Instance.ModifyOrder(new ModifyOrderRequestParameters(order) { Quantity = newTotalQty });
            return Wrap(result, orderId);
        }

        public void Cancel(string orderId)
        {
            var order = Core.Instance.Orders.FirstOrDefault(o => o.Id == orderId);
            order?.Cancel();
        }

        public double GetNetPositionSize()
        {
            var pos = Core.Instance.Positions.FirstOrDefault(p => p.Symbol?.Id == symbol.Id && p.Account?.Id == account.Id);
            return pos != null ? (pos.Side == Side.Buy ? pos.Quantity : -pos.Quantity) : 0.0;
        }

        private static OrderOpResult Wrap(TradingOperationResult result, string? knownId = null)
        {
            bool ok = result.Status == TradingOperationResultStatus.Success;
            string id = knownId ?? result.OrderId ?? "";
            return new OrderOpResult(ok, id, result.Message ?? "");
        }
    }
}
