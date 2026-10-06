---
title: Schaff Trend Cycle
description: Created by Doug Schaff, the Schaff Trend Cycle is a stochastic oscillator view of two converging/diverging exponential moving averages.  In other words, it's a Stochastic Oscillator of Moving Average Convergence / Divergence (MACD).
---

# Schaff Trend Cycle

Created by Doug Schaff, the [Schaff Trend Cycle](https://www.investopedia.com/articles/forex/10/schaff-trend-cycle-indicator.asp) is a stochastic oscillator view of two converging/diverging exponential moving averages.  In other words, it's a Stochastic Oscillator of Moving Average Convergence / Divergence (MACD).
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/570 "Community discussion about this indicator")

<StockIndicatorChart indicator="Stc" withOverlay />

```csharp
// C# usage syntax
IReadOnlyList<StcResult> results =
  bars.ToStc(cyclePeriods, fastPeriods, slowPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `cyclePeriods` | _`int`_ | Number of periods (`C`) for the Trend Cycle.  Must be greater than or equal to 0.  Default is 10. |
| `fastPeriods` | _`int`_ | Number of periods (`F`) for the faster moving average.  Must be greater than 0.  Default is 23. |
| `slowPeriods` | _`int`_ | Number of periods (`S`) for the slower moving average.  Must be greater than `fastPeriods`.  Default is 50. |

### Historical price bars requirements

You must have at least `2×(S+C)` or `S+C+100` worth of `bars`, whichever is more, to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `S+C+250` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<StcResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `S+C` slow periods will have `null` values since there's not enough data to calculate.

::: warning 🚩 ⚞ Convergence warning
The first `S+C+250` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `StcResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Stc` | _`double`_ | Schaff Trend Cycle |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.HL2)
    .ToStc(..);
```

Results can be further processed on `Stc` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToStc(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
StcList stcList = new(cyclePeriods, fastPeriods, slowPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => stcList.Add(bar);

// based on `ICollection<StcResult>`; fills as the handler runs
IReadOnlyList<StcResult> results = stcList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
StcHub observer = barHub.ToStcHub(cyclePeriods, fastPeriods, slowPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<StcResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
