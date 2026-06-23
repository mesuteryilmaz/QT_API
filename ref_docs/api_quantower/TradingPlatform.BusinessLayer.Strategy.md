# Class Strategy
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Strategy.html
> Fetched: 2026-04-10

---

# Class Strategy

The base class for strategies

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public abstract class Strategy : ExecutionEntity
```

### Constructors

#### Strategy()

The base class for strategies

##### Declaration

```csharp
protected Strategy()
```

### Properties

#### Id

Unique ID of the strategy

##### Declaration

```csharp
public string Id { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### InstanceName

The base class for strategies

##### Declaration

```csharp
public string InstanceName { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### MonitoringConnectionsIds

The base class for strategies

##### Declaration

```csharp
public virtual string[] MonitoringConnectionsIds { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string)[] |  |

#### NewVersionAvailable

The base class for strategies

##### Declaration

```csharp
public bool NewVersionAvailable { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### Settings

The base class for strategies

##### Declaration

```csharp
public override IList<SettingItem> Settings { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [IList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ilist-1)<[SettingItem](TradingPlatform.BusinessLayer.SettingItem.html)> |  |

##### Overrides

[ExecutionEntity.Settings](TradingPlatform.BusinessLayer.ExecutionEntity.html#TradingPlatform_BusinessLayer_ExecutionEntity_Settings)

#### State

The current state of the strategy

##### Declaration

```csharp
public StrategyState State { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [StrategyState](TradingPlatform.BusinessLayer.StrategyState.html) |  |

### Methods

#### GetConnectionStateDependency()

The base class for strategies

##### Declaration

```csharp
public ConnectionDependency GetConnectionStateDependency()
```

##### Returns

| Type | Description |
| --- | --- |
| [ConnectionDependency](TradingPlatform.BusinessLayer.ConnectionDependency.html) |  |

#### GetLogs(DateTime, DateTime)

Get logs from the strategy for specified date range

##### Declaration

```csharp
public LoggerEvent[] GetLogs(DateTime from, DateTime to)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) | from |  |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) | to |  |

##### Returns

| Type | Description |
| --- | --- |
| [LoggerEvent](TradingPlatform.BusinessLayer.LoggerEvent.html)[] |  |

#### GetMetrics()

Get current metrics from the strategy

##### Declaration

```csharp
public List<StrategyMetric> GetMetrics()
```

##### Returns

| Type | Description |
| --- | --- |
| [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)<[StrategyMetric](TradingPlatform.BusinessLayer.StrategyMetric.html)> |  |

#### Log(string, StrategyLoggingLevel)

Write log message

##### Declaration

```csharp
protected void Log(string message, StrategyLoggingLevel level = StrategyLoggingLevel.Info)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | message |  |
| [StrategyLoggingLevel](TradingPlatform.BusinessLayer.StrategyLoggingLevel.html) | level |  |

#### OnCreated()

The base class for strategies

##### Declaration

```csharp
protected virtual void OnCreated()
```

#### OnGetMetrics()

The base class for strategies

##### Declaration

```csharp
protected virtual List<StrategyMetric> OnGetMetrics()
```

##### Returns

| Type | Description |
| --- | --- |
| [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)<[StrategyMetric](TradingPlatform.BusinessLayer.StrategyMetric.html)> |  |

#### OnInitializeMetrics(Meter)

The base class for strategies

##### Declaration

```csharp
protected virtual void OnInitializeMetrics(Meter meter)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Meter](https://learn.microsoft.com/dotnet/api/system.diagnostics.metrics.meter) | meter |  |

#### OnRemove()

The base class for strategies

##### Declaration

```csharp
protected virtual void OnRemove()
```

#### OnRun()

The base class for strategies

##### Declaration

```csharp
protected virtual void OnRun()
```

#### OnStop()

The base class for strategies

##### Declaration

```csharp
protected virtual void OnStop()
```

#### Remove()

Remove the strategy

##### Declaration

```csharp
public void Remove()
```

#### Run()

Run strategy

##### Declaration

```csharp
public void Run()
```

#### Stop()

Stop strategy

##### Declaration

```csharp
public void Stop()
```

### Events

#### NewLog

Event occured when strategy write a new log

##### Declaration

```csharp
public event StrategyEventHandler NewLog
```

##### Event Type

| Type | Description |
| --- | --- |
| [StrategyEventHandler](TradingPlatform.BusinessLayer.StrategyEventHandler.html) |  |

#### SettingsChanged

Event occured if any of strategy settings was changed

##### Declaration

```csharp
public event Action<Strategy> SettingsChanged
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[Strategy](TradingPlatform.BusinessLayer.Strategy.html)> |  |