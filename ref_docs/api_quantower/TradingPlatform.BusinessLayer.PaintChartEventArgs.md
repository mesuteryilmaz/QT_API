# Class PaintChartEventArgs
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.PaintChartEventArgs.html
> Fetched: 2026-04-10

---

# Class PaintChartEventArgs

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class PaintChartEventArgs
```

### Constructors

#### PaintChartEventArgs(Graphics, Rectangle, Point, int)

##### Declaration

```csharp
public PaintChartEventArgs(Graphics graphics, Rectangle rectangle, Point mousePosition, int windowIndex)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| Graphics | graphics |  |
| [Rectangle](https://learn.microsoft.com/dotnet/api/system.drawing.rectangle) | rectangle |  |
| [Point](https://learn.microsoft.com/dotnet/api/system.drawing.point) | mousePosition |  |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) | windowIndex |  |

### Properties

#### DrawBackground

##### Declaration

```csharp
public bool DrawBackground { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### Graphics

##### Declaration

```csharp
public Graphics Graphics { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| Graphics |  |

#### LeftVisibleBarIndex

##### Declaration

```csharp
public int LeftVisibleBarIndex { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### MousePosition

##### Declaration

```csharp
public Point MousePosition { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Point](https://learn.microsoft.com/dotnet/api/system.drawing.point) |  |

#### Rectangle

##### Declaration

```csharp
public Rectangle Rectangle { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Rectangle](https://learn.microsoft.com/dotnet/api/system.drawing.rectangle) |  |

#### RightVisibleBarIndex

##### Declaration

```csharp
public int RightVisibleBarIndex { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |

#### WindowIndex

##### Declaration

```csharp
public int WindowIndex { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [int](https://learn.microsoft.com/dotnet/api/system.int32) |  |