---
title: Ehlers Fisher Transform
description: Created by John Ehlers, the Fisher Transform converts financial market prices into a Gaussian normal distribution.
---

# Ehlers Fisher Transform

Created by John Ehlers, the [Fisher Transform](https://www.investopedia.com/terms/f/fisher-transform.asp) converts prices into a Gaussian normal distribution.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/409 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="FisherTransform" withOverlay />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<FisherTransformResult> results =
  bars.ToFisherTransform(lookbackPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
FisherTransformList fisherList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => fisherList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<FisherTransformResult> results = fisherList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
FisherTransformHub observer = barHub.ToFisherTransformHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<FisherTransformResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) in the lookback window.  Must be greater than 0.  Default is 10. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<FisherTransformResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
:::

::: buffer

```csharp
FisherTransformList : IReadOnlyList<FisherTransformResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
:::

::: stream

```csharp
IReadOnlyList<FisherTransformResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
:::

::: warning 🚩 ⚞ Convergence warning
The first `N+15` warmup periods will have unusable decreasing magnitude, convergence-related precision errors that can be as high as ~25% deviation in earlier indicator values.
:::

### `FisherTransformResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Fisher`<br>_`double`_ | Fisher Transform |
| `Trigger`<br>_`double?`_ | FT offset by one period |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

For pruning of warmup periods, we recommend using the following guidelines:

```csharp
bars.ToFisherTransform(lookbackPeriods)
  .RemoveWarmupPeriods(lookbackPeriods+15);
```

See [Utilities and helpers](/utilities/) for more information.

## Chaining

::: series
This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HL2)
    .ToFisherTransform(..);
```

Results can be further processed on `Fisher` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToFisherTransform(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
FisherTransformList fisherList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    fisherList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `Fisher` further.

```csharp
// example
FisherTransformList fisherList = new(..);
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    fisherList.Add(bar);
    rsiList.Add(fisherList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
FisherTransformHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToFisherTransformHub(..);
```

Subscribe other chain-enabled hubs to it to process `Fisher` further.

```csharp
// example
RsiHub observer = barHub
    .ToFisherTransformHub(..)
    .ToRsiHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
