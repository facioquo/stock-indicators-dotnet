---
title: Average Directional Index (ADX)
description: Created by J. Welles Wilder, the Average Directional Index (ADX) is part of the Directional Movement system (commonly referred to as DMI). This system includes the Positive and Negative Directional Indicators (+DI and −DI), the Directional Index (DX), and ADX, and is used to measure the strength of price trends.
---

# Average Directional Index (ADX)

Created by J. Welles Wilder, the [Average Directional Movement Index](https://en.wikipedia.org/wiki/Average_directional_movement_index) (ADX) is part of the Directional Movement system (commonly referred to as DMI). This system includes the Positive and Negative Directional Indicators (+DI and −DI), the Directional Index (DX), and ADX, and is used to measure the strength of price trends.
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/270 "Community discussion about this indicator")

<StockIndicatorChart indicator="Adx" withOverlay />

```csharp
// C# usage syntax
IReadOnlyList<AdxResult> results =
  bars.ToAdx(lookbackPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) to consider.  Must be greater than 1.  Default is 14. |

### Historical price bars requirements

You must have at least `2×N+100` periods of `bars` to cover the [warmup and convergence](https://github.com/facioquo/stock-indicators-dotnet/discussions/688) periods.  We generally recommend you use at least `2×N+250` data points prior to the intended usage date for better precision.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<AdxResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `2×N-1` periods will have `null` values for `Adx` since there's not enough data to calculate.

::: warning 🚩 ⚞ Convergence warning
The first `2×N+100` periods will have decreasing magnitude, convergence-related precision errors that can be as high as ~5% deviation in indicator values for earlier periods.
:::

### `AdxResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Pdi` | _`double`_ | Plus Directional Index (+DI) |
| `Mdi` | _`double`_ | Minus Directional Index (-DI) |
| `Dx` | _`double`_ | Directional Index (DX) |
| `Adx` | _`double`_ | Average Directional Index (ADX) |
| `Adxr` | _`double`_ | Average Directional Index Rating (ADXR) |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

Results can be further processed on `Adx` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToAdx(..)
    .ToRsi(..);
```

This indicator must be generated from `bars` and **cannot** be generated from results of another chain-enabled indicator or method.

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations:

```csharp
AdxList adxList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => adxList.Add(bar);

// based on `ICollection<AdxResult>`; fills as the handler runs
IReadOnlyList<AdxResult> results = adxList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
AdxHub observer = barHub.ToAdxHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<AdxResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
