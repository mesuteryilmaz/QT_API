# Interface IChartDrawingsCollection
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Chart.IChartDrawingsCollection.html
> Fetched: 2026-04-10

---

# Interface IChartDrawingsCollection

Access to the chart drawingsCollection collection

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html).[Chart](TradingPlatform.BusinessLayer.Chart.html)

##### Syntax

```csharp
public interface IChartDrawingsCollection
```

### Methods

#### Add(IDrawing)

Add chart drawing to the collection

##### Declaration

```csharp
void Add(IDrawing drawing)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [IDrawing](TradingPlatform.BusinessLayer.Chart.IDrawing.html) | drawing |  |

#### FindById(string)

Get chart drawing by ID

##### Declaration

```csharp
IDrawing FindById(string drawingId)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) | drawingId |  |

##### Returns

| Type | Description |
| --- | --- |
| [IDrawing](TradingPlatform.BusinessLayer.Chart.IDrawing.html) |  |

#### GetAll(Symbol)

Get all chart drawingsCollection assigned to specified symbol

##### Declaration

```csharp
List<IDrawing> GetAll(Symbol symbol = null)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [Symbol](TradingPlatform.BusinessLayer.Symbol.html) | symbol |  |

##### Returns

| Type | Description |
| --- | --- |
| [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)<[IDrawing](TradingPlatform.BusinessLayer.Chart.IDrawing.html)> |  |

#### Remove(IDrawing)

Remove specified chart drawing from collection

##### Declaration

```csharp
void Remove(IDrawing drawing)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [IDrawing](TradingPlatform.BusinessLayer.Chart.IDrawing.html) | drawing |  |

### Events

#### Added

The Added events occured, when new chart drawing was added to collection

##### Declaration

```csharp
event Action<DrawingEventArgs> Added
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[DrawingEventArgs](TradingPlatform.BusinessLayer.Chart.DrawingEventArgs.html)> |  |

#### Moved

The Moved events occured, when chart drawing was moved

##### Declaration

```csharp
event Action<DrawingEventArgs> Moved
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[DrawingEventArgs](TradingPlatform.BusinessLayer.Chart.DrawingEventArgs.html)> |  |

#### Removed

The Removed events occured, when chart drawing was removed from the collection

##### Declaration

```csharp
event Action<DrawingEventArgs> Removed
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[DrawingEventArgs](TradingPlatform.BusinessLayer.Chart.DrawingEventArgs.html)> |  |

#### SelectionChanged

The SelectionChanged events occured, when selected chart drawing was changed

##### Declaration

```csharp
event Action<DrawingSelectionEventArgs> SelectionChanged
```

##### Event Type

| Type | Description |
| --- | --- |
| [Action](https://learn.microsoft.com/dotnet/api/system.action-1)<[DrawingSelectionEventArgs](TradingPlatform.BusinessLayer.Chart.DrawingSelectionEventArgs.html)> |  |