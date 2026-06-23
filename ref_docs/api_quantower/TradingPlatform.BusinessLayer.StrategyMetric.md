# Class StrategyMetric
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.StrategyMetric.html
> Fetched: 2026-04-10

---

# Class StrategyMetric

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class StrategyMetric
```

### Properties

#### FormattedValue

##### Declaration

```csharp
public string FormattedValue { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

#### Name

##### Declaration

```csharp
public string Name { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

### Methods

#### FromXElement(XElement, DeserializationInfo)

##### Declaration

```csharp
public void FromXElement(XElement element, DeserializationInfo deserializationInfo)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [XElement](https://learn.microsoft.com/dotnet/api/system.xml.linq.xelement) | element |  |
| [DeserializationInfo](TradingPlatform.BusinessLayer.Serialization.DeserializationInfo.html) | deserializationInfo |  |

#### ToXElement()

##### Declaration

```csharp
public XElement ToXElement()
```

##### Returns

| Type | Description |
| --- | --- |
| [XElement](https://learn.microsoft.com/dotnet/api/system.xml.linq.xelement) |  |