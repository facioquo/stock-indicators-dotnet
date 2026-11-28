---
title: Klinger Volume Oscillator
description: Created by Stephen Klinger, the Klinger Volume Oscillator depicts volume-based trend reversal and divergence between short and long-term money flow.
---

# Klinger Volume Oscillator

Created by Stephen Klinger, the [Klinger Volume Oscillator](https://www.investopedia.com/terms/k/klingeroscillator.asp) depicts volume-based trend reversal and divergence between short and long-term money flow.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/446 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Kvo" withOverlay />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax
IReadOnlyList<KvoResult> results =
  bars.ToKvo(fastPeriods, slowPeriods, signalPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
KvoList kvoList = new(34, 55, 13);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => kvoList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<KvoResult> results = kvoList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
KvoHub observer = barHub.ToKvoHub(34, 55, 13);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<KvoResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `fastPeriods`<br>_`int`_ | Number of lookback periods (`F`) for the short-term EMA.  Must be greater than 2.  Default is 34. |
| `slowPeriods`<br>_`int`_ | Number of lookback periods (`L`) for the long-term EMA.  Must be greater than `F`.  Default is 55. |
| `signalPeriods`<br>_`int`_ | Number of lookback periods for the signal line.  Must be greater than 0.  Default is 13. |

### Historical price bars requirements

You must have at least `L+100` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `L+150` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<KvoResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `L+1` periods will have `null` values since there's not enough data to calculate.
:::

::: buffer

```csharp
KvoList : IReadOnlyList<KvoResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `L+1` periods will have `null` values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<KvoResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `L+1` periods will have `null` values since there's not enough data to calculate.
:::

::: warning 🚩 ⚞ Convergence warning
The first `L+150` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `KvoResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Oscillator`<br>_`double?`_ | Klinger Oscillator |
| `Signal`<br>_`double?`_ | EMA of Klinger Oscillator (signal line) |

## Chaining

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

::: series
Results can be further processed on `Kvo` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToKvo(..)
    .ToSlope(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
Add each new result to another chain-enabled buffer list to process `Kvo` further.

```csharp
// example
KvoList kvoList = new(..);
SlopeList slopeList = new(..);

void OnBarReceived(IBar bar)
{
    kvoList.Add(bar);
    slopeList.Add(kvoList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
Subscribe other chain-enabled hubs to it to process `Kvo` further.

```csharp
// example
SlopeHub observer = barHub
    .ToKvoHub(..)
    .ToSlopeHub(..);
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

Call these on the list itself, as in `kvoList.Find(lookupDate)`. `.Condense()` returns a new list and leaves `kvoList` unchanged.

Warmup removal is left out. Once the list is full it drops its oldest results, so trimming by position would remove valid values.
:::

::: stream

- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.Condense()](/utilities/results#condense)

Call these on `observer.Results`, as in `observer.Results.Find(lookupDate)`. `.Condense()` returns a new list and leaves the hub's results unchanged.

Warmup removal is left out. Once the cache is full the hub drops its oldest results, so trimming by position would remove valid values.
:::

See [Utilities and helpers](/utilities/) for more information.
