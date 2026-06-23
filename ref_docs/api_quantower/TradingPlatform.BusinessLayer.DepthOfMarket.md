# Class DepthOfMarket
 | Quantower API
> Source: https://api.quantower.com/docs/TradingPlatform.BusinessLayer.DepthOfMarket.html
> Fetched: 2026-04-10

---

# Class DepthOfMarket

Represent access to level2 data.

###### **Namespace**: [TradingPlatform](TradingPlatform.html).[BusinessLayer](TradingPlatform.BusinessLayer.html)

##### Syntax

```csharp
public class DepthOfMarket
```

### Methods

#### GetDepthOfMarketAggregatedCollections(GetDepthOfMarketParameters)

Gets current Level2 data

##### Declaration

```csharp
public DepthOfMarketAggregatedCollections GetDepthOfMarketAggregatedCollections(GetDepthOfMarketParameters parameters = null)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [GetDepthOfMarketParameters](TradingPlatform.BusinessLayer.GetDepthOfMarketParameters.html) | parameters | Parameters of DepthOfMarket |

##### Returns

| Type | Description |
| --- | --- |
| [DepthOfMarketAggregatedCollections](TradingPlatform.BusinessLayer.DepthOfMarketAggregatedCollections.html) |  |

#### GetDepthOfMarketAggregatedCollections(GetLevel2ItemsParameters)

Gets current Level2 data

##### Declaration

```csharp
public DepthOfMarketAggregatedCollections GetDepthOfMarketAggregatedCollections(GetLevel2ItemsParameters parameters)
```

##### Parameters

| Type | Name | Description |
| --- | --- | --- |
| [GetLevel2ItemsParameters](TradingPlatform.BusinessLayer.GetLevel2ItemsParameters.html) | parameters | Parameters of request for Leve2Item collection |

##### Returns

| Type | Description |
| --- | --- |
| [DepthOfMarketAggregatedCollections](TradingPlatform.BusinessLayer.DepthOfMarketAggregatedCollections.html) |  |