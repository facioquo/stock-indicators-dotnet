---
title: Volume Weighted Moving Average (VWMA)
description: Volume Weighted Moving Average is the volume adjusted average price over a lookback window.
---

# Volume Weighted Moving Average (VWMA)

Volume Weighted Moving Average is the volume adjusted average price over a lookback window. [[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/657 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Vwma" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<VwmaResult> results =
  bars.ToVwma(lookbackPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
VwmaList vwmaList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => vwmaList.Add(bar);

// or add a price and volume without a bar
vwmaList.Add(DateTime.Now, price: 100.50, volume: 1000);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<VwmaResult> results = vwmaList;
```

**Note:** VWMA requires both price and volume data, so it only supports methods that accept `IBar` or direct price/volume parameters.

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
VwmaHub observer = barHub.ToVwmaHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<VwmaResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) in the moving average.  Must be greater than 0. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<VwmaResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values for `Vwma` since there's not enough data to calculate.
:::

::: buffer

```csharp
VwmaList : IReadOnlyList<VwmaResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N-1` periods will have `null` values for `Vwma` since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<VwmaResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N-1` periods will have `null` values for `Vwma` since there's not enough data to calculate.
:::

### `VwmaResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Vwma`<br>_`double?`_ | Volume Weighted Moving Average |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

::: series
Results can be further processed on `Vwma` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToVwma(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
Add each new result to another chain-enabled buffer list to process `Vwma` further.

```csharp
// example
VwmaList vwmaList = new(..);
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    vwmaList.Add(bar);
    rsiList.Add(vwmaList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
Subscribe other chain-enabled hubs to it to process `Vwma` further.

```csharp
// example
RsiHub observer = barHub
    .ToVwmaHub(..)
    .ToRsiHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
