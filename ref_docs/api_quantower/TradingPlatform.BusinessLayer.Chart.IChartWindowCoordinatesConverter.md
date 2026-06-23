# Interface IChartWindowCoordinatesConverter
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.Chart.IChartWindowCoordinatesConverter.html
> Fetched: 2026-04-10

---

# Interface IChartWindowCoordinatesConverter

Converter between x/y and Time/Price scales

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html).[Chart](TradingPlatform.BusinessLayer.Chart.html)

##### Syntax

```csharp
public interface IChartWindowCoordinatesConverter
```

### Methods

#### GetBarIndex(DateTime)

Get the bar index that is corresponding to specified DateTime value

##### Declaration

```csharp
double GetBarIndex(DateTime dt)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) | dt |  |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### GetChartX(DateTime)

Get the X coordinate that is corresponding to specified DateTime value

##### Declaration

```csharp
double GetChartX(DateTime dt)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) | dt |  |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### GetChartY(double)

Get the Y coordinate that is corresponding to specified price value

##### Declaration

```csharp
double GetChartY(double price)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | price |  |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### GetPrice(double)

Get the Price value that is corresponding to specified y coordinate

##### Declaration

```csharp
double GetPrice(double y)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | y |  |

##### Returns

| Type | Description |
| --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) |  |

#### GetTime(double)

Get the DateTime value that is corresponding to specified x coordinate

##### Declaration

```csharp
DateTime GetTime(double x)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [double](https://learn.microsoft.com/dotnet/api/system.double) | x |  |

##### Returns

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |