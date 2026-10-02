---
title: Triple EMA Oscillator (TRIX)
description: Created by Jack Hutson, TRIX is a rolling rate of change for a 3 EMA smoothing of the price over a lookback window.  TRIX is often confused with Triple EMA (TEMA).
---

# Triple EMA Oscillator (TRIX)

Created by Jack Hutson, [TRIX](https://en.wikipedia.org/wiki/Trix_(technical_analysis)) is the rate of change for a 3 EMA smoothing of the price over a lookback window.  TRIX is often confused with [TEMA](/indicators/tema).
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/234 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Trix" withOverlay />
</ClientOnly>

```csharp
// C# usage syntax for Trix
IReadOnlyList<TrixResult> results =
  bars.ToTrix(lookbackPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) in each of the exponential moving averages.  Must be greater than 0. |

### Historical price bars requirements

You must have at least `4×N` or `3×N+100` periods of `bars`, whichever is more, to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `3×N+250` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<TrixResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `3×N-3` periods will have `null` values since there's not enough data to calculate.

::: warning 🚩 ⚞ Convergence warning
The first `3×N+250` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `TrixResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Ema3` | _`double`_ | 3 EMAs of the price |
| `Trix` | _`double`_ | Rate of Change of 3 EMAs |

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
    .ToTrix(..);
```

Results can be further processed on `Trix` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToTrix(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
TrixList trixList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => trixList.Add(bar);

// based on `ICollection<TrixResult>`; fills as the handler runs
IReadOnlyList<TrixResult> results = trixList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
TrixHub observer = barHub.ToTrixHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<TrixResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
