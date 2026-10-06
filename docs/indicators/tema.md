---
title: Triple Exponential Moving Average (TEMA)
description: Created by Patrick G. Mulloy, the Triple Exponential Moving Average is a faster multi-smoothed moving average. TEMA is often confused with the alternative TRIX oscillator.
---

# Triple Exponential Moving Average (TEMA)

Created by Patrick G. Mulloy, the [Triple exponential moving average](https://en.wikipedia.org/wiki/Triple_exponential_moving_average) is a faster multi-smoothed EMA of the price over a lookback window.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/808 "Community discussion about this indicator")

<StockIndicatorChart indicator="Tema" />

```csharp
// C# usage syntax
IReadOnlyList<TemaResult> results =
  bars.ToTema(lookbackPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) in the moving average.  Must be greater than 0. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to produce any TEMA values.  However, due to the nature of the smoothing technique, we recommend you use at least `3×N+250` data points prior to the intended usage date for better precision.  See [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) guidance for more information.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<TemaResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.  Also note that we are using the proper [weighted variant](https://en.wikipedia.org/wiki/Triple_exponential_moving_average) for TEMA.  If you prefer the unweighted raw 3 EMAs value, please use the `Ema3` output from the [TRIX](/indicators/trix) oscillator instead.

**Example for TEMA(20)**:

```text
Period 1-19:  null values (incalculable)
Period 20:    first TEMA value (may have convergence issues)
Period 160+:  fully converged, reliable values
```

>&#9432; **Incalculable periods**: The first `N-1` periods will have `null` values since there's not enough data to calculate.
>
::: warning 🚩 ⚞ Convergence warning
The first `3×N+100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.  Use the `.RemoveWarmupPeriods()` method to remove these potentially unreliable values.
:::

### `TemaResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Tema` | _`double`_ | Triple exponential moving average |

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
    .ToTema(..);
```

Results can be further processed on `Tema` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToTema(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
TemaList temaList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => temaList.Add(bar);

// based on `ICollection<TemaResult>`; fills as the handler runs
IReadOnlyList<TemaResult> results = temaList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
TemaHub observer = barHub.ToTemaHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<TemaResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
