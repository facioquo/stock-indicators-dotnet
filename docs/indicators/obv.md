---
title: On-Balance Volume (OBV)
description: Popularized by Joseph Granville, On-balance Volume is a rolling accumulation of volume based on Close price direction.
---

# On-Balance Volume (OBV)

Popularized by Joseph Granville, [On-balance Volume](https://en.wikipedia.org/wiki/On-balance_volume) is a rolling accumulation of volume based on Close price direction.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/246 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Obv" withOverlay />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<ObvResult> results =
  bars.ToObv();
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
ObvList obvList = new();

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => obvList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<ObvResult> results = obvList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
ObvHub observer = barHub.ToObvHub();

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<ObvResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Historical price bars requirements

You must have at least two historical price bars to cover the warmup periods; however, since this is a trendline, more is recommended.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<ObvResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first period OBV will have a `0` value since there's not enough data to calculate.
:::

::: buffer

```csharp
ObvList : IReadOnlyList<ObvResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first period OBV will have a `0` value since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<ObvResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first period OBV will have a `0` value since there's not enough data to calculate.
:::

### `ObvResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Obv`<br>_`double`_ | On-balance Volume |

::: warning 🚩 Relative values only
Absolute values in OBV are somewhat meaningless. Use with caution.
:::

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

::: series
Results can be further processed on `Obv` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToObv(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
Add each new result to another chain-enabled buffer list to process `Obv` further.

```csharp
// example
ObvList obvList = new();
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    obvList.Add(bar);
    rsiList.Add(obvList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
Subscribe other chain-enabled hubs to it to process `Obv` further.

```csharp
// example
RsiHub observer = barHub
    .ToObvHub()
    .ToRsiHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
