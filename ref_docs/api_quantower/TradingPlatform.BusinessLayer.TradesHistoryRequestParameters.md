# Class TradesHistoryRequestParameters
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.TradesHistoryRequestParameters.html
> Fetched: 2026-04-10

---

# Class TradesHistoryRequestParameters

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class TradesHistoryRequestParameters : ProgressRequestParameters<float>
```

### Constructors

#### TradesHistoryRequestParameters()

##### Declaration

```csharp
public TradesHistoryRequestParameters()
```

#### TradesHistoryRequestParameters(TradesHistoryRequestParameters)

##### Declaration

```csharp
public TradesHistoryRequestParameters(TradesHistoryRequestParameters origin)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [TradesHistoryRequestParameters](TradingPlatform.BusinessLayer.TradesHistoryRequestParameters.html) | origin |  |

### Properties

#### ForceReload

##### Declaration

```csharp
public bool ForceReload { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [bool](https://learn.microsoft.com/dotnet/api/system.boolean) |  |

#### From

##### Declaration

```csharp
public DateTime From { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime) |  |

#### Interval

##### Declaration

```csharp
public Interval<DateTime> Interval { get; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [Interval](TradingPlatform.BusinessLayer.Utils.Interval-1.html)<[DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)> |  |

#### SymbolId

##### Declaration

```csharp
public string SymbolId { get; set; }
```

##### Property Value

| Type | Description |
| --- | --- |
| [string](https://learn.microsoft.com/dotnet/api/system.string) |  |

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