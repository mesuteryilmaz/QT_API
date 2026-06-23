# Class Connection
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Connection.html
> Fetched: 2026-04-10

---

# Class Connection

Represents information about connection and provides an access to the current trading information(Symbols, Orders, Position, Accounts etc.).

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public sealed class Connection
```

### Properties

#### BusinessObjects

Provides access to all business objects which are belong to this connection

##### Declaration

```csharp
public IBusinessObjectsProvider BusinessObjects { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IBusinessObjectsProvider](TradingPlatform.BusinessLayer.IBusinessObjectsProvider.html) |  |

#### ConnectingProgress

Represents information about connection and provides an access to the current trading information(Symbols, Orders, Position, Accounts etc.).

##### Declaration

```csharp
public string ConnectingProgress { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### DateTimeUtcNow

Represents information about connection and provides an access to the current trading information(Symbols, Orders, Position, Accounts etc.).

##### Declaration

```csharp
public DateTime DateTimeUtcNow { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

#### HistoryMetaData

Gets a matched available metadata info with the vendor's side

##### Declaration

```csharp
public HistoryMetadata HistoryMetaData { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [HistoryMetadata](TradingPlatform.BusinessLayer.Integration.HistoryMetadata.html) |  |

#### Id

Gets connection Id

##### Declaration

```csharp
public string Id { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Info

Represents information about connection and provides an access to the current trading information(Symbols, Orders, Position, Accounts etc.).

##### Declaration

```csharp
public ConnectionInfo Info { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [ConnectionInfo](TradingPlatform.BusinessLayer.ConnectionInfo.html) |  |

#### LastConnectionResult

Represents information about connection and provides an access to the current trading information(Symbols, Orders, Position, Accounts etc.).

##### Declaration

```csharp
public ConnectionResult LastConnectionResult { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [ConnectionResult](TradingPlatform.BusinessLayer.Integration.ConnectionResult.html) |  |

#### MessagesQueueDepth

Messages count that one is waited to process

##### Declaration

```csharp
public int MessagesQueueDepth { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### Name

Gets connection Name

##### Declaration

```csharp
public string Name { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### NewsFeedSettings

Represents information about connection and provides an access to the current trading information(Symbols, Orders, Position, Accounts etc.).

##### Declaration

```csharp
public IEnumerable<SettingItem> NewsFeedSettings { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)<[SettingItem](TradingPlatform.BusinessLayer.SettingItem.html)> |  |

#### PingTime

Represents connection ping time

##### Declaration

```csharp
public TimeSpan? PingTime { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)? |  |

#### RoundTripTime

Represents information about connection and provides an access to the current trading information(Symbols, Orders, Position, Accounts etc.).

##### Declaration

```csharp
public TimeSpan? RoundTripTime { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)? |  |

#### ServerTime

Represents information about connection and provides an access to the current trading information(Symbols, Orders, Position, Accounts etc.).

##### Declaration

```csharp
public DateTime ServerTime { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

#### Settings

Contains list of connection settings. Will be reused on each population time.

##### Declaration

```csharp
public IList<SettingItem> Settings { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)<[SettingItem](TradingPlatform.BusinessLayer.SettingItem.html)> |  |

#### State

Gets connection's state (Connected/Connecting/Fail etc.)

##### Declaration

```csharp
public ConnectionState State { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [ConnectionState](TradingPlatform.BusinessLayer.ConnectionState.html) |  |

#### TotalSubscriptionsCount

Represents information about connection and provides an access to the current trading information(Symbols, Orders, Position, Accounts etc.).

##### Declaration

```csharp
public int TotalSubscriptionsCount { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### TradesHistoryMetadata

Represents information about connection and provides an access to the current trading information(Symbols, Orders, Position, Accounts etc.).

##### Declaration

```csharp
public TradesHistoryMetadata TradesHistoryMetadata { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [TradesHistoryMetadata](TradingPlatform.BusinessLayer.Integration.TradesHistoryMetadata.html) |  |

#### Type

Defines connection type

##### Declaration

```csharp
public ConnectionType Type { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [ConnectionType](TradingPlatform.BusinessLayer.ConnectionType.html) |  |

#### Uptime

Represents information about connection and provides an access to the current trading information(Symbols, Orders, Position, Accounts etc.).

##### Declaration

```csharp
public TimeSpan Uptime { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan) |  |

#### VendorName

Gets connection's vendor name

##### Declaration

```csharp
public string VendorName { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### VolumeAnalysisMetadata

Represents information about connection and provides an access to the current trading information(Symbols, Orders, Position, Accounts etc.).

##### Declaration

```csharp
public VolumeAnalysisMetadata VolumeAnalysisMetadata { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [VolumeAnalysisMetadata](TradingPlatform.BusinessLayer.Integration.VolumeAnalysisMetadata.html) |  |

### Methods

#### CompareTo(object)

Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.

##### Declaration

```csharp
public int CompareTo(object obj)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [object](https://learn.microsoft.com/dotnet/api/system.object) | obj | An object to compare with this instance. |

##### Returns

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | A value that indicates the relative order of the objects being compared. The return value has these meanings:   | Value | Meaning | | --- | --- | | Less than zero | This instance precedes `obj` in the sort order. | | Zero | This instance occurs in the same position in the sort order as `obj`. | | Greater than zero | This instance follows `obj` in the sort order. | |

##### Exceptions

| Type | Condition |
| --- | --- |
| [ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception) | `obj` is not the same type as this instance. |

#### Connect()

Establishes a connection to a specified vendor

##### Declaration

```csharp
public ConnectionResult Connect()
```

##### Returns

| Type | Description |
| --- | --- |
| [ConnectionResult](TradingPlatform.BusinessLayer.Integration.ConnectionResult.html) |  |

#### Disconnect()

Closes a connection.

##### Declaration

```csharp
public void Disconnect()
```

#### GetNews(GetNewsRequestParameters)

Represents information about connection and provides an access to the current trading information(Symbols, Orders, Position, Accounts etc.).

##### Declaration

```csharp
public IEnumerable<NewsArticle> GetNews(GetNewsRequestParameters requestParameters)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [GetNewsRequestParameters](TradingPlatform.BusinessLayer.GetNewsRequestParameters.html) | requestParameters |  |

##### Returns

| Type | Description |
| --- | --- |
| [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)<[NewsArticle](TradingPlatform.BusinessLayer.NewsArticle.html)> |  |

#### GetNewsArticleContent(GetNewsArticleContentRequestParameters)

Represents information about connection and provides an access to the current trading information(Symbols, Orders, Position, Accounts etc.).

##### Declaration

```csharp
public string GetNewsArticleContent(GetNewsArticleContentRequestParameters requestParameters)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [GetNewsArticleContentRequestParameters](TradingPlatform.BusinessLayer.GetNewsArticleContentRequestParameters.html) | requestParameters |  |

##### Returns

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### GetOrdersHistory(OrdersHistoryRequestParameters)

Represents information about connection and provides an access to the current trading information(Symbols, Orders, Position, Accounts etc.).

##### Declaration

```csharp
public IList<OrderHistory> GetOrdersHistory(OrdersHistoryRequestParameters parameters)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [OrdersHistoryRequestParameters](TradingPlatform.BusinessLayer.OrdersHistoryRequestParameters.html) | parameters |  |

##### Returns

| Type | Description |
| --- | --- |
| [IList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)<[OrderHistory](TradingPlatform.BusinessLayer.OrderHistory.html)> |  |

#### GetTrades(TradesHistoryRequestParameters)

Represents information about connection and provides an access to the current trading information(Symbols, Orders, Position, Accounts etc.).

##### Declaration

```csharp
public IList<Trade> GetTrades(TradesHistoryRequestParameters parameters)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [TradesHistoryRequestParameters](TradingPlatform.BusinessLayer.TradesHistoryRequestParameters.html) | parameters |  |

##### Returns

| Type | Description |
| --- | --- |
| [IList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)<[Trade](TradingPlatform.BusinessLayer.Trade.html)> |  |

#### SendCustomRequest(RequestParameters)

Represents information about connection and provides an access to the current trading information(Symbols, Orders, Position, Accounts etc.).

##### Declaration

```csharp
public void SendCustomRequest(RequestParameters parameters)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [RequestParameters](TradingPlatform.BusinessLayer.RequestParameters.html) | parameters |  |

#### SubscribeNewsUpdates(SubscribeNewsRequestParameters, Action<NewsArticle>)

Represents information about connection and provides an access to the current trading information(Symbols, Orders, Position, Accounts etc.).

##### Declaration

```csharp
public void SubscribeNewsUpdates(SubscribeNewsRequestParameters subscribeNewsRequestParameters, Action<NewsArticle> updateAction)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [SubscribeNewsRequestParameters](TradingPlatform.BusinessLayer.SubscribeNewsRequestParameters.html) | subscribeNewsRequestParameters |  |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[NewsArticle](TradingPlatform.BusinessLayer.NewsArticle.html)> | updateAction |  |

#### ToString()

Returns a string that represents the current object.

##### Declaration

```csharp
public override string ToString()
```

##### Returns

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | A string that represents the current object. |

##### Overrides

[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring)

#### UnsubscribeNewsUpdates(SubscribeNewsRequestParameters, Action<NewsArticle>)

Represents information about connection and provides an access to the current trading information(Symbols, Orders, Position, Accounts etc.).

##### Declaration

```csharp
public void UnsubscribeNewsUpdates(SubscribeNewsRequestParameters subscribeNewsRequestParameters, Action<NewsArticle> updateAction)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [SubscribeNewsRequestParameters](TradingPlatform.BusinessLayer.SubscribeNewsRequestParameters.html) | subscribeNewsRequestParameters |  |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[NewsArticle](TradingPlatform.BusinessLayer.NewsArticle.html)> | updateAction |  |

### Events

#### ConnectingProgressChanged

Will be triggered when [ConnectingProgress](TradingPlatform.BusinessLayer.Connection.html#TradingPlatform_BusinessLayer_Connection_ConnectingProgress) changed.

##### Declaration

```csharp
public event EventHandler<ConnectionConnectingProgressChangedEventArgs> ConnectingProgressChanged
```

##### Event Type

| Type | Description |
| --- | --- |
| [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler-1)<[ConnectionConnectingProgressChangedEventArgs](TradingPlatform.BusinessLayer.ConnectionConnectingProgressChangedEventArgs.html)> |  |

#### NewPerformedRequest

Represents information about connection and provides an access to the current trading information(Symbols, Orders, Position, Accounts etc.).

##### Declaration

```csharp
public event EventHandler<PerformedRequestEventArgs> NewPerformedRequest
```

##### Event Type

| Type | Description |
| --- | --- |
| [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler-1)<[PerformedRequestEventArgs](TradingPlatform.BusinessLayer.Utils.PerformedRequestEventArgs.html)> |  |

#### NewRequest

Represents information about connection and provides an access to the current trading information(Symbols, Orders, Position, Accounts etc.).

##### Declaration

```csharp
public event EventHandler<RequestEventArgs> NewRequest
```

##### Event Type

| Type | Description |
| --- | --- |
| [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler-1)<[RequestEventArgs](TradingPlatform.BusinessLayer.Utils.RequestEventArgs.html)> |  |

#### StateChanged

Will be triggered when [State](TradingPlatform.BusinessLayer.Connection.html#TradingPlatform_BusinessLayer_Connection_State) changed.

##### Declaration

```csharp
public event EventHandler<ConnectionStateChangedEventArgs> StateChanged
```

##### Event Type

| Type | Description |
| --- | --- |
| [EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler-1)<[ConnectionStateChangedEventArgs](TradingPlatform.BusinessLayer.ConnectionStateChangedEventArgs.html)> |  |