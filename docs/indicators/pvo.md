---
title: Percentage Volume Oscillator (PVO)
description: The Percentage Volume Oscillator is a simple oscillator view of the rate of change between two converging / diverging exponential moving averages of Volume.  It is presented similarly to MACD.
---

# Percentage Volume Oscillator (PVO)

The [Percentage Volume Oscillator](https://www.google.com/search?q=Percentage+Volume+Oscillator+(PVO)) is a simple oscillator view of the rate of change between two converging / diverging exponential moving averages of Volume.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/305 "Community discussion about this indicator")

<StockIndicatorChart indicator="Pvo" withOverlay />

```csharp
// C# usage syntax
IReadOnlyList<PvoResult> results =
  bars.ToPvo(fastPeriods, slowPeriods, signalPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `fastPeriods` | _`int`_ | Number of periods (`F`) for the faster moving average.  Must be greater than 0.  Default is 12. |
| `slowPeriods` | _`int`_ | Number of periods (`S`) for the slower moving average.  Must be greater than `fastPeriods`.  Default is 26. |
| `signalPeriods` | _`int`_ | Number of periods (`P`) for the moving average of PVO.  Must be greater than or equal to 0.  Default is 9. |

### Historical price bars requirements

You must have at least `2×(S+P)` or `S+P+100` worth of `bars`, whichever is more, to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `S+P+250` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<PvoResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `S-1` slow periods will have `null` values since there's not enough data to calculate.

::: warning 🚩 ⚞ Convergence warning
The first `S+P+250` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `PvoResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Pvo` | _`double`_ | Normalized difference between two Volume moving averages |
| `Signal` | _`double`_ | Moving average of the `Pvo` line |
| `Histogram` | _`double`_ | Gap between the `Pvo` and `Signal` line |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results can be further processed on `Pvo` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToPvo(..)
    .ToSlope(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
PvoList pvoList = new(fastPeriods, slowPeriods, signalPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => pvoList.Add(bar);

// based on `ICollection<PvoResult>`; fills as the handler runs
IReadOnlyList<PvoResult> results = pvoList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
PvoHub observer = barHub.ToPvoHub(fastPeriods, slowPeriods, signalPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<PvoResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
