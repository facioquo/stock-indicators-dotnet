---
title: Double Exponential Moving Average (DEMA)
description: Created by Patrick G. Mulloy, the Double Exponential Moving Average is a faster smoothed EMA of financial market prices.
---

# Double Exponential Moving Average (DEMA)

Created by Patrick G. Mulloy, the [Double exponential moving average](https://en.wikipedia.org/wiki/Double_exponential_moving_average) is a faster smoothed EMA of the price over a lookback window.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/807 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Dema" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<DemaResult> results =
  bars.ToDema(lookbackPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
DemaList demaList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => demaList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<DemaResult> results = demaList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
DemaHub observer = barHub.ToDemaHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<DemaResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) in the moving average.  Must be greater than 0.  Default is 14. |

### Historical price bars requirements

You must have at least `3×N` or `2×N+100` periods of `bars`, whichever is more, to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `2×N+250` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<DemaResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

::: buffer

```csharp
DemaList : IReadOnlyList<DemaResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<DemaResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

::: warning 🚩 ⚞ Convergence warning
The first `2×N+100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `DemaResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Dema`<br>_`double?`_ | Double exponential moving average |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

::: series
This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HL2)
    .ToDema(..);
```

Results can be further processed on `Dema` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToDema(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
DemaList demaList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    demaList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `Dema` further.

```csharp
// example
DemaList demaList = new(..);
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    demaList.Add(bar);
    rsiList.Add(demaList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
DemaHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToDemaHub(..);
```

Subscribe other chain-enabled hubs to it to process `Dema` further.

```csharp
// example
RsiHub observer = barHub
    .ToDemaHub(..)
    .ToRsiHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
