---
title: Klinger Volume Oscillator
description: Created by Stephen Klinger, the Klinger Volume Oscillator depicts volume-based trend reversal and divergence between short and long-term money flow.
---

# Klinger Volume Oscillator

Created by Stephen Klinger, the [Klinger Volume Oscillator](https://www.investopedia.com/terms/k/klingeroscillator.asp) depicts volume-based trend reversal and divergence between short and long-term money flow.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/446 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Kvo" withOverlay />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<KvoResult> results =
  bars.ToKvo(fastPeriods, slowPeriods, signalPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `fastPeriods` | _`int`_ | Number of lookback periods (`F`) for the short-term EMA.  Must be greater than 2.  Default is 34. |
| `slowPeriods` | _`int`_ | Number of lookback periods (`L`) for the long-term EMA.  Must be greater than `F`.  Default is 55. |
| `signalPeriods` | _`int`_ | Number of lookback periods for the signal line.  Must be greater than 0.  Default is 13. |

### Historical price bars requirements

You must have at least `L+100` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `L+150` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<KvoResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `L+1` periods will have `null` values since there's not enough data to calculate.

::: warning 🚩 ⚞ Convergence warning
The first `L+150` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `KvoResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Oscillator` | _`double`_ | Klinger Oscillator |
| `Signal` | _`double`_ | EMA of Klinger Oscillator (signal line) |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results can be further processed on `Kvo` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToKvo(..)
    .ToSlope(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
KvoList kvoList = new(34, 55, 13);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => kvoList.Add(bar);

// based on `ICollection<KvoResult>`; fills as the handler runs
IReadOnlyList<KvoResult> results = kvoList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
KvoHub observer = barHub.ToKvoHub(34, 55, 13);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<KvoResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
