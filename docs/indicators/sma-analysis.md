---
title: SMA, MAD, MAPE, and MSE analysis
description: Mean absolute deviation (MAD), mean square error (MSE), and mean absolute percentage error (MAPE).
---

# SMA with extended analysis

[Simple Moving Average](https://en.wikipedia.org/wiki/Moving_average#Simple_moving_average) with extended statistical analysis including mean absolute deviation (MAD), mean square error (MSE), and mean absolute percentage error (MAPE).  See also [Simple Moving Average](/indicators/sma).
[[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/240 "Community discussion about this indicator")

Each error metric charts on its own scale — MAD is in price units, MSE in price units squared, and MAPE is a fraction — so they are depicted separately.

<ClientOnly>
  <StockIndicatorChart indicator="SmaMad" withOverlay />
  <StockIndicatorChart indicator="SmaMape" />
  <StockIndicatorChart indicator="SmaMse" />
</ClientOnly>

```csharp
// C# usage syntax
IReadOnlyList<SmaAnalysisResult> results =
  bars.ToSmaAnalysis(lookbackPeriods);
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
IReadOnlyList<SmaAnalysisResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.

### `SmaAnalysisResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Sma` | _`double`_ | Simple moving average |
| `Mad` | _`double`_ | Mean absolute deviation |
| `Mse` | _`double`_ | Mean square error |
| `Mape` | _`double`_ | Mean absolute percentage error |

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
    .ToSmaAnalysis(..);
```

Results can be further processed on `Sma` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToSmaAnalysis(..)
    .ToRsi(..);
```

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub:

```csharp
SmaAnalysisList smaAnalysisList = new(lookbackPeriods);

foreach (IReusable value in bars)  // simulating stream
{
  smaAnalysisList.Add(value);
}

// based on `ICollection<SmaAnalysisResult>`
IReadOnlyList<SmaAnalysisResult> results = smaAnalysisList;
```

Subscribe to a chain-enabled hub for advanced streaming scenarios:

```csharp
BarHub barHub = new();
SmaAnalysisHub observer = barHub.ToSmaAnalysisHub(lookbackPeriods);

foreach (IBar bar in bars)  // simulating stream
{
  barHub.Add(bar);
}

IReadOnlyList<SmaAnalysisResult> results = observer.Results;
```

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
