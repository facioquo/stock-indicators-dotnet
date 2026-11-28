---
title: Stochastic Momentum Index (SMI)
description: Created by William Blau, the Stochastic Momentum Index (SMI) oscillator is a double-smoothed variant of the traditional Stochastic Oscillator, depicted on a scale from -100 to 100.
---

# Stochastic Momentum Index (SMI)

Created by William Blau, the Stochastic Momentum Index (SMI) oscillator is a double-smoothed variant of the [Stochastic Oscillator](/indicators/stoch), depicted on a scale from -100 to 100.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/625 "Community discussion about this indicator")

<StyleSwitch />

<StockIndicatorChart indicator="Smi" withOverlay />

<a id="streaming"></a>

::: series

```csharp
// C# usage syntax (standard)
IReadOnlyList<SmiResult> results =
  bars.ToSmi(lookbackPeriods, firstSmoothPeriods,
                 secondSmoothPeriods, signalPeriods);
```

:::

::: buffer
Create a buffer list and add each bar as it arrives:

```csharp
SmiList smiList = new(lookbackPeriods, firstSmoothPeriods,
                 secondSmoothPeriods, signalPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => smiList.Add(bar);

// the list itself holds the results; fills as the handler runs
IReadOnlyList<SmiResult> results = smiList;
```

See [Buffer lists](/guide/styles/buffer) for the full usage guide.
:::

::: stream
Subscribe to a `BarHub` and add each bar as it arrives:

```csharp
BarHub barHub = new();
SmiHub observer = barHub.ToSmiHub(lookbackPeriods, firstSmoothPeriods,
                 secondSmoothPeriods, signalPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<SmiResult> results = observer.Results;
```

See [Stream hubs](/guide/styles/stream) for the full usage guide.
:::

## Parameters

| param | description |
| ----- | ----------- |
| `lookbackPeriods`<br>_`int`_ | Lookback period (`N`) for the stochastic.  Must be greater than 0.  Default is 13. |
| `firstSmoothPeriods`<br>_`int`_ | First smoothing factor lookback.  Must be greater than 0.  Default is 25. |
| `secondSmoothPeriods`<br>_`int`_ | Second smoothing factor lookback.  Must be greater than 0.  Default is 2. |
| `signalPeriods`<br>_`int`_ | EMA of SMI lookback periods.  Must be greater than 0. Default is 9. |

### Historical price bars requirements

You must have at least `N+100` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

::: series

```csharp
IReadOnlyList<SmiResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` SMI values since there's not enough data to calculate.
:::

::: buffer

```csharp
SmiList : IReadOnlyList<SmiResult>
```

- The list itself holds the results, one for each bar or value added, in timestamp order.
- It keeps up to 100,000 results by default and drops the oldest beyond that; set `MaxListSize` to keep fewer.
- The first `N-1` periods will have `null` SMI values since there's not enough data to calculate.
:::

::: stream

```csharp
IReadOnlyList<SmiResult> results = observer.Results;
```

- `observer.Results` holds the results, one for each bar in the `BarHub` cache, in timestamp order.
- Results update as bars arrive, including bars that arrive late or are revised.
- The cache keeps up to 100,000 results by default; set `maxCacheSize` on the `BarHub` to keep fewer.
- The first `N-1` periods will have `null` SMI values since there's not enough data to calculate.
:::

::: warning 🚩 ⚞ Convergence warning
The first `N+100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `SmiResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `Smi`<br>_`double?`_ | Stochastic Momentum Index (SMI) |
| `Signal`<br>_`double?`_ | Signal line: an Exponential Moving Average (EMA) of SMI |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

::: series
Results can be further processed on `Smi` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToSmi(..)
    .ToSlope(..);
```

See [Chaining indicators](/guide/chaining) for more.
:::

::: buffer
Add each new result to another chain-enabled buffer list to process `Smi` further.

```csharp
// example
SmiList smiList = new(..);
SlopeList slopeList = new(..);

void OnBarReceived(IBar bar)
{
    smiList.Add(bar);
    slopeList.Add(smiList[^1]);
}
```

See [Chaining buffer lists](/guide/styles/buffer#chaining-indicators) for more.
:::

::: stream
Subscribe other chain-enabled hubs to it to process `Smi` further.

```csharp
// example
SlopeHub observer = barHub
    .ToSmiHub(..)
    .ToSlopeHub(..);
```

See [Chaining stream hubs](/guide/styles/stream#chaining-indicators) for more.
:::
