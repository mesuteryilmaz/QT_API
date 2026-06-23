# Class OrdersHistoryRequestParameters
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.OrdersHistoryRequestParameters.html
> Fetched: 2026-04-10

---

# Class OrdersHistoryRequestParameters

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class OrdersHistoryRequestParameters : ProgressRequestParameters<float>
```

### Constructors

#### OrdersHistoryRequestParameters()

##### Declaration

```csharp
public OrdersHistoryRequestParameters()
```

#### OrdersHistoryRequestParameters(OrdersHistoryRequestParameters)

##### Declaration

```csharp
public OrdersHistoryRequestParameters(OrdersHistoryRequestParameters original)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [OrdersHistoryRequestParameters](TradingPlatform.BusinessLayer.OrdersHistoryRequestParameters.html) | original |  |

### Properties

#### From

##### Declaration

```csharp
public DateTime From { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

#### SymbolIds

##### Declaration

```csharp
public string[] SymbolIds { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string)[] |  |

#### To

##### Declaration

```csharp
public DateTime To { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

#### Type

##### Declaration

```csharp
public override RequestType Type { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [RequestType](TradingPlatform.BusinessLayer.RequestType.html) |  |

##### Overrides

[RequestParameters.Type](TradingPlatform.BusinessLayer.RequestParameters.html#TradingPlatform_BusinessLayer_RequestParameters_Type)

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