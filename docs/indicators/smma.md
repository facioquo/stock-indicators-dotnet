---
title: Smoothed Moving Average (SMMA)
description: Smoothed Moving Average (SMMA), Modified Moving Average (MMA), Running Moving Average (RMA) are all the same simple rolling moving average of financial market prices.  New values are calculated based on the last known value only, making a more efficient but less accurate method for computing an average.
---

# Smoothed Moving Average (SMMA)

[Smoothed Moving Average](https://en.wikipedia.org/wiki/Moving_average) is the average of price over a lookback window using a smoothing method.  SMMA is also known as modified moving average (MMA) and running moving average (RMA).
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/375 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Smma" />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<SmmaResult> results =
  bars.ToSmma(lookbackPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
SmmaList smmaList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => smmaList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<SmmaResult> results = smmaList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
SmmaHub observer = barHub.ToSmmaHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<SmmaResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) in the moving average.  Must be greater than 0. |

### Historical price bars requirements

You must have at least `2×N` or `N+100` periods of `bars`, whichever is more, to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `N+250` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<SmmaResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

::: buffer

```csharp
SmmaList : IReadOnlyList<SmmaResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<SmmaResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.
:::

::: warning 🚩 ⚞ Convergence warning
The first `N+100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `SmmaResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Smma`<br>_`double?`_ | Smoothed moving average |

## Chaining

::: series
This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HL2)
    .ToSmma(..);
```

Results can be further processed on `Smma` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToSmma(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
This indicator may be generated from results of another chain-enabled buffer list.  Add each new result to it yourself.

```csharp
// example
SmaList smaList = new(..);
SmmaList smmaList = new(..);

void OnBarReceived(IBar bar)
{
    smaList.Add(bar);
    smmaList.Add(smaList[^1]);
}
```

Add each new result to another chain-enabled buffer list to process `Smma` further.

```csharp
// example
SmmaList smmaList = new(..);
RsiList rsiList = new(..);

void OnBarReceived(IBar bar)
{
    smmaList.Add(bar);
    rsiList.Add(smmaList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
This indicator may be subscribed to any chain-enabled hub.

```csharp
// example
SmmaHub observer = barHub
    .ToBarPartHub(CandlePart.HL2)
    .ToSmmaHub(..);
```

Subscribe other chain-enabled hubs to it to process `Smma` further.

```csharp
// example
RsiHub observer = barHub
    .ToSmmaHub(..)
    .ToRsiHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::

## Utilities

::: series

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

:::

::: buffer

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on the list itself, as in `smmaList.Find(lookupDate)`. `.Condense()` returns a new list and leaves `smmaList` unchanged.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on `observer.Results`, as in `observer.Results.Find(lookupDate)`. `.Condense()` returns a new list and leaves the hub's results unchanged.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
