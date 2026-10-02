---
title: Stochastic Momentum Index (SMI)
description: Created by William Blau, the Stochastic Momentum Index (SMI) oscillator is a double-smoothed variant of the traditional Stochastic Oscillator, depicted on a scale from -100 to 100.
---

# Stochastic Momentum Index (SMI)

Created by William Blau, the Stochastic Momentum Index (SMI) oscillator is a double-smoothed variant of the [Stochastic Oscillator](/indicators/stoch), depicted on a scale from -100 to 100.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/625 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Smi" withOverlay />
</ClientOnly>

```csharp
// C# usage syntax (standard)
IReadOnlyList<SmiResult> results =
  bars.ToSmi(lookbackPeriods, firstSmoothPeriods,
                 secondSmoothPeriods, signalPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Lookback period (`N`) for the stochastic.  Must be greater than 0.  Default is 13. |
| `firstSmoothPeriods` | _`int`_ | First smoothing factor lookback.  Must be greater than 0.  Default is 25. |
| `secondSmoothPeriods` | _`int`_ | Second smoothing factor lookback.  Must be greater than 0.  Default is 2. |
| `signalPeriods` | _`int`_ | EMA of SMI lookback periods.  Must be greater than 0. Default is 3. |

### Historical price bars requirements

You must have at least `N+100` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<SmiResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` SMI values since there's not enough data to calculate.

::: warning 🚩 ⚞ Convergence warning
The first `N+100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `SmiResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Smi` | _`double`_ | Stochastic Momentum Index (SMI) |
| `Signal` | _`double`_ | Signal line: an Exponential Moving Average (EMA) of SMI |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results can be further processed on `Smi` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToSmi(..)
    .ToSlope(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
SmiList smiList = new(lookbackPeriods, firstSmoothPeriods,
                 secondSmoothPeriods, signalPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => smiList.Add(bar);

// based on `ICollection<SmiResult>`; fills as the handler runs
IReadOnlyList<SmiResult> results = smiList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
SmiHub observer = barHub.ToSmiHub(lookbackPeriods, firstSmoothPeriods,
                 secondSmoothPeriods, signalPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<SmiResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
