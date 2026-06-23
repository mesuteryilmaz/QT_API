# Class Connection
> Namespace: `TradingPlatform.BusinessLayer`
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Connection.html

Represents a vendor connection. Provides access to all trading data (Symbols, Orders, Positions, Accounts) scoped to that connection.

```csharp
public sealed class Connection
```

Access via:
```csharp
var connections = Core.Instance.Connections;
Connection conn  = connections.Connected.FirstOrDefault();
```

---

## Properties

| Property | Type | Description |
|---|---|---|
| `Id` | `string` | Connection unique ID |
| `Name` | `string` | Connection name (get/set) |
| `VendorName` | `string` | Vendor name |
| `State` | `ConnectionState` | Connected / Connecting / Disconnected / Fail |
| `Type` | `ConnectionType` | Connection type (get/set) |
| `Info` | `ConnectionInfo` | Connection info object |
| `LastConnectionResult` | `ConnectionResult` | Result of last connect attempt |
| `ConnectingProgress` | `string` | Progress message during connecting |
| `PingTime` | `TimeSpan?` | Current ping time |
| `RoundTripTime` | `TimeSpan?` | Round-trip time |
| `ServerTime` | `DateTime` | Server-side time |
| `DateTimeUtcNow` | `DateTime` | Current UTC time from connection |
| `Uptime` | `TimeSpan` | How long connected |
| `Settings` | `IList<SettingItem>` | Connection settings (get/set) |
| `BusinessObjects` | `IBusinessObjectsProvider` | All business objects for this connection |
| `HistoryMetaData` | `HistoryMetadata` | Matched history metadata |
| `TradesHistoryMetadata` | `TradesHistoryMetadata` | Trade history metadata |
| `VolumeAnalysisMetadata` | `VolumeAnalysisMetadata` | Volume analysis metadata |
| `NewsFeedSettings` | `IEnumerable<SettingItem>` | News feed settings |
| `MessagesQueueDepth` | `int` | Messages waiting to be processed |
| `TotalSubscriptionsCount` | `int` | Total active subscriptions |

---

## Methods

```csharp
// Connect / disconnect
ConnectionResult Connect()
void Disconnect()

// Orders history
IList<OrderHistory> GetOrdersHistory(OrdersHistoryRequestParameters parameters)

// Trades history
IList<Trade> GetTrades(TradesHistoryRequestParameters parameters)

// News
IEnumerable<NewsArticle> GetNews(GetNewsRequestParameters requestParameters)
string GetNewsArticleContent(GetNewsArticleContentRequestParameters requestParameters)
void SubscribeNewsUpdates(SubscribeNewsRequestParameters req, Action<NewsArticle> action)
void UnsubscribeNewsUpdates(SubscribeNewsRequestParameters req, Action<NewsArticle> action)

// Custom requests
void SendCustomRequest(RequestParameters parameters)

// Comparison
int CompareTo(object obj)
string ToString()
```

---

## Events

| Event | Args | Description |
|---|---|---|
| `StateChanged` | `ConnectionStateChangedEventArgs` | Fires when state changes |
| `ConnectingProgressChanged` | `ConnectionConnectingProgressChangedEventArgs` | Fires during connect |
| `NewRequest` | `RequestEventArgs` | New outbound request |
| `NewPerformedRequest` | `PerformedRequestEventArgs` | Completed request |

---

## ConnectionState Enum

```csharp
public enum ConnectionState
{
    Connected,
    Connecting,
    Disconnected,
    Disconnecting,
    Reconnecting,
    Fail,
    // ...
}
```

---

## Pattern — IB/NQ Connection

```csharp
// Detect connection ready
var conn = Core.Instance.Connections.Connected
               .FirstOrDefault(c => c.VendorName.Contains("Interactive Brokers"));

conn.StateChanged += (s, e) =>
{
    if (e.NewState == ConnectionState.Connected)
        InitStrategy();
};
```
