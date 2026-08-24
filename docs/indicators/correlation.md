---
title: Correlation coefficient
description: Created by Karl Pearson, the correlation coefficient depicts the linear statistical correlation between two price bar histories; includes R-squared (R²) / coefficient of determination, variance, and covariance.
---

# Correlation coefficient

Created by Karl Pearson, the [Correlation coefficient](https://en.wikipedia.org/wiki/Correlation_coefficient) depicts the linear statistical correlation between two price bar histories; includes R-squared (R²) / coefficient of determination, variance, and covariance. [[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/259 "Community discussion about this indicator")

```csharp
// C# usage syntax
IReadOnlyList<CorrResult> results =
  barsA.ToCorrelation(barsB, lookbackPeriods);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `barsA` | _`IReadOnlyList<TBar>`_ | [Historical price bars](/guide/getting-started#historical-bars) (A) must have at least the same matching date elements of `barsB`. |
| `barsB` | _`IReadOnlyList<TBar>`_ | [Historical price bars](/guide/getting-started#historical-bars) (B) must have at least the same matching date elements of `barsA`. |
| `lookbackPeriods` | _`int`_ | Number of periods (`N`) in the lookback period.  Must be greater than 0 to calculate; however we suggest a larger period for statistically appropriate sample size. |

### Historical price bars requirements

You must have at least `N` periods for both versions of `bars` to cover the warmup periods. More than the minimum is typically specified.

`barsA` and `barsB` must have consistent frequency (day, hour, minute, etc).  Mismatch histories will throw `InvalidBarsException`. See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<CorrResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The first `N-1` periods will have `null` values since there's not enough data to calculate.

### `CorrResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `VarianceA` | _`double`_ | Variance of A |
| `VarianceB` | _`double`_ | Variance of B |
| `Covariance` | _`double`_ | Covariance of A+B |
| `Correlation` | _`double`_ | Correlation `R` |
| `RSquared` | _`double`_ | R-squared (`R²`), aka Coefficient of determination |

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
    .ToCorrelation(barsMarket.Use(CandlePart.HL2),20);
```

::: warning 🚩
Both `barsA` and `barsB` arguments must contain the same number of elements and be the results of a chainable indicator or `.Use()` method.
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

Streaming is not supported for this indicator. This indicator requires a second synchronized bar series, which cannot be expressed in the single-series streaming model. Use the Series (batch) implementation with periodic recalculation instead.
