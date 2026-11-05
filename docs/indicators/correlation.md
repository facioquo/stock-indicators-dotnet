---
title: Correlation coefficient
description: Created by Karl Pearson, the correlation coefficient depicts the linear statistical correlation between two price bar histories; includes R-squared (R²) / coefficient of determination, variance, and covariance.
---

# Correlation coefficient

Created by Karl Pearson, the [Correlation coefficient](https://en.wikipedia.org/wiki/Correlation_coefficient) depicts the linear statistical correlation between two price bar histories; includes R-squared (R²) / coefficient of determination, variance, and covariance. [[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/259 "Community discussion about this indicator")

```csharp
// C# usage syntax
IReadOnlyList<CorrResult> results =
  sourceA.ToCorrelation(sourceB, lookbackPeriods);
```

## Parameters

| param | description |
| ----- | ----------- |
| `sourceA`<br>_`IReadOnlyList<IReusable>`_ | First source series (A), such as an indicator's results or `.Use()` output.  It must have at least the same matching date elements of `sourceB`. |
| `sourceB`<br>_`IReadOnlyList<IReusable>`_ | Second source series (B), such as an indicator's results or `.Use()` output.  It must have at least the same matching date elements of `sourceA`. |
| `lookbackPeriods`<br>_`int`_ | Number of periods (`N`) in the lookback period.  Must be greater than 0 to calculate; however we suggest a larger period for statistically appropriate sample size. |

### Historical price bars requirements

You must have at least `N` periods for both `sourceA` and `sourceB` to cover the warmup periods. More than the minimum is typically specified.

`sourceA` and `sourceB` must have consistent frequency (day, hour, minute, etc).  Mismatch histories will throw `InvalidBarsException`. See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<CorrResult>
```

- This method returns a time series of all available indicator values for the `sourceA` and `sourceB` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.

### `CorrResult`

| property | description |
| ----- | ----------- |
| `Timestamp`<br>_`DateTime`_ | Date from evaluated `TBar` |
| `VarianceA`<br>_`double?`_ | Variance of A |
| `VarianceB`<br>_`double?`_ | Variance of B |
| `Covariance`<br>_`double?`_ | Covariance of A+B |
| `Correlation`<br>_`double?`_ | Correlation `R` |
| `RSquared`<br>_`double?`_ | R-squared (`R²`), aka Coefficient of determination |

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
    .ToCorrelation(barsMarket.Use(CandlePart.HL2), 20);
```

::: warning 🚩 Matching inputs required
Both `sourceA` and `sourceB` arguments must contain the same number of elements and be the results of a chainable indicator or `.Use()` method.
:::

Results can be further processed on `Correlation` with additional chain-enabled indicators.

```csharp
// example
var results = bars
    .ToCorrelation(..)
    .ToSlope(..);
```

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Use the buffer-style `List<T>` when you need incremental calculations without a hub.  Add each series separately as its values arrive; either one may run ahead, and a result is added once both hold the same timestamp:

```csharp
CorrelationList correlationList = new(lookbackPeriods);

// call from each series' message handler
void OnAReceived(IReusable value) => correlationList.AddSource(value);
void OnBReceived(IReusable value) => correlationList.AddOther(value);

// based on `IReadOnlyList<CorrResult>`; fills as the handlers run
IReadOnlyList<CorrResult> results = correlationList;
```

A timestamp that only one series holds produces no result, where the Series method throws.  Each series must arrive in strictly increasing order: an older or equal timestamp on a side throws an `InvalidOperationException`.  Each side stages at most `MaxListSize` values, so a side that falls further behind than that loses the pairs the leading side has dropped.

Subscribe to a `BarHub` for each series for advanced streaming scenarios:

```csharp
BarHub hubA = new();
BarHub hubB = new();
CorrelationHub observer = hubA.ToCorrelationHub(hubB, lookbackPeriods);

// call from each series' message handler
void OnAReceived(IBar bar) => hubA.Add(bar);
void OnBReceived(IBar bar) => hubB.Add(bar);

// results fill as the handlers run
IReadOnlyList<CorrResult> results = observer.Results;
```

A result is added once both providers hold the same timestamp.  A value that arrives late, or is revised or removed, on either side is handled by the usual rebuild.  Timestamps match exactly, and the lookback window spans `N` matched timestamps.  Drive both providers from one thread at a time.

See [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for full usage guides.
