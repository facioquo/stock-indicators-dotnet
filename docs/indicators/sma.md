---
title: Simple Moving Average (SMA)
description: Simple moving average.  Extended to include mean absolute deviation, mean square error, and mean absolute percentage error
---

# Simple Moving Average (SMA)

[Simple Moving Average](https://en.wikipedia.org/wiki/Moving_average#Simple_moving_average) is the average price over a lookback window.  An [extended SMA analysis](/indicators/sma-analysis) option includes mean absolute deviation (MAD), mean square error (MSE), and mean absolute percentage error (MAPE).
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/240 "Community discussion about this indicator")

<ClientOnly>
  <StockIndicatorChart indicator="Sma" />
</ClientOnly>

```csharp
// C# usage syntax (with Close price)
IReadOnlyList<SmaResult> results =
  bars.ToSma(lookbackPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) in the lookback window. Must be greater than 0. |

### Historical price bars requirements

You must have at least `N` periods of `bars` to cover the warmup periods.

`bars` is a collection of generic `TBar` historical price bars.  It should have a consistent frequency (day, hour, minute, etc).  See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<SmaResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.

### `SmaResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Sma` | _`double`_ | Simple moving average |

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods()](/utilities/results#remove-warmup-periods)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Analysis

This indicator has an extended version with more analysis.  See [SMA with extended analysis](/indicators/sma-analysis) for the full documentation including streaming support.

```csharp
// C# usage syntax
IReadOnlyList<SmaAnalysisResult> analysis =
  bars.ToSmaAnalysis(lookbackPeriods);
```

### `SmaAnalysisResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Sma` | _`double`_ | Simple moving average |
| `Mad` | _`double`_ | Mean absolute deviation |
| `Mse` | _`double`_ | Mean square error |
| `Mape` | _`double`_ | Mean absolute percentage error |

## Chaining

This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = bars
    .Use(CandlePart.Volume)
    .ToSma(..);
```

Results can be further processed on `Sma` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToSma(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
SmaList smaList = new(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => smaList.Add(bar);

// based on `ICollection<SmaResult>`; fills as the handler runs
IReadOnlyList<SmaResult> results = smaList;
```

Subscribe to a `BarHub` for advanced streaming scenarios:

```csharp
BarHub barHub = new();
SmaHub observer = barHub.ToSmaHub(lookbackPeriods);

// call from your WebSocket or SSE message handler
void OnBarReceived(IBar bar) => barHub.Add(bar);

// results fill as the handler runs
IReadOnlyList<SmaResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
