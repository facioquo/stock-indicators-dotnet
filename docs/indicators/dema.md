---
title: Double Exponential Moving Average (DEMA)
description: Created by Patrick G. Mulloy, the Double Exponential Moving Average is a faster smoothed EMA of financial market prices.
---

# Double Exponential Moving Average (DEMA)

Created by Patrick G. Mulloy, the [Double exponential moving average](https://en.wikipedia.org/wiki/Double_exponential_moving_average) is a faster smoothed EMA of the price over a lookback window.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/807 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Dema" />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<DemaResult> results =
  bars.ToDema(lookbackPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) in the moving average.  Must be greater than 0. |

### Historical price bars requirements

You must have at least `3×N` or `2×N+100` periods of `bars`, whichever is more, to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  Since this uses a smoothing technique, we recommend you use at least `2×N+250` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<DemaResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.

::: warning 🚩 ⚞ Convergence warning
The first `2×N+100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `DemaResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Dema` | _`double`_ | Double exponential moving average |

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

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
DemaList demaList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => demaList.Add(bar);

// based on `ICollection<DemaResult>`
IReadOnlyList<DemaResult> results = demaList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
DemaHub observer = barHub.ToDemaHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

IReadOnlyList<DemaResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
