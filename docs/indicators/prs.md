---
title: Price Relative Strength (PRS)
description: Price Relative Strength, also called Comparative Relative Strength, shows the ratio of two bar histories, based on price.  It is often used to compare against a market index or sector ETF.  When using the optional lookback window, this also returns relative percent change over the specified periods.  This is not the same as the more prevalent Relative Strength Index (RSI).
---

# Price Relative Strength (PRS)

[Price Relative Strength (PRS)](https://en.wikipedia.org/wiki/Relative_strength), also called Comparative Relative Strength, shows the ratio of two bar histories, based on price.  It is often used to compare against a market index or sector ETF.  When using the optional `lookbackPeriods`, this also returns relative percent change over the specified periods.  This is not the same as the more prevalent [Relative Strength Index (RSI)](/indicators/rsi). [[Discuss] &#128172;](https://github.com/facioquo/stock-indicators-dotnet/discussions/243 "Community discussion about this indicator")

```csharp
// C# usage syntax
IReadOnlyList<PrsResult> results =
  barsEval.ToPrs(barsBase);
```

## Parameters

| param | type | description |
| ----- | ---- | ----------- |
| `barsEval` | _`IReadOnlyList<TBar>`_ | [Historical price bars](/guide/getting-started#historical-bars) used as the evaluation subject.  You must have the same number of periods as `barsBase`. |
| `barsBase` | _`IReadOnlyList<TBar>`_ | [Historical price bars](/guide/getting-started#historical-bars) used as the benchmark basis for comparison.  This is usually market index data.  You must have the same number of periods as `barsEval`. |
| `lookbackPeriods` | _`int`_ | Optional.  Number of periods (`N`) to lookback to compute % ROC difference.  Must be greater than 0 if specified or `null`. |

### Historical price bars requirements

You must have at least `N+1` periods of price bars to calculate `PrsPercent` if `lookbackPeriods` is specified; otherwise, you must specify at least `2` periods. More than the minimum is typically specified.

`barsEval` and `barsBase` must have consistent frequency (day, hour, minute, etc).  Mismatch histories will throw `InvalidBarsException`. See [the Guide](/guide/getting-started#historical-bars) for more information.

## Response

```csharp
IReadOnlyList<PrsResult>
```

- This method returns a time series of all available indicator values for the `bars` provided.
- It always returns the same number of elements as there are in the historical price bars.
- It does not return a single incremental indicator value.
- The `N` periods will have `null` values for `PrsPercent` since there's not enough data to calculate.

### `PrsResult`

| property | type | description |
| -------- | ---- | ----------- |
| `Timestamp` | _`DateTime`_ | Date from evaluated `TBar` |
| `Prs` | _`double`_ | Price Relative Strength is the ratio of `Eval` / `Base` price values |
| `PrsPercent` | _`double`_ | Percent change difference between `Eval` and `Base` over `N` periods |

::: info Example
At period `i`, `Prs` is `Eval[i] / Base[i]` and `PrsPercent` is the difference in `Eval` percent change and `Base` percent change over the prior `N` periods. Example: if `Eval` gains 10% and `Base` gains 3%, `PrsPercent` depicts +7% relative outperformance.
:::

### Utilities

- [.Condense()](/utilities/results#condense)
- [.Find(lookupDate)](/utilities/results#find-by-date)
- [.RemoveWarmupPeriods(removePeriods)](/utilities/results#remove-warmup-periods)

See [Utilities and helpers](/utilities/) for more information.

## Chaining

This indicator may be generated from any chain-enabled indicator or method.

```csharp
// example
var results = barsEval
    .Use(CandlePart.HL2)
    .ToPrs(barsBase, ..);
```

::: warning
Both `barsEval` and `barsBase` arguments must contain the same number of elements and be the results of a chainable indicator or `.Use()` method.
:::

Results can be further processed on `Beta` with additional chain-enabled indicators.

```csharp
// example
var results = barsEval
    .ToPrs(barsBase, ..)
    .ToSlope(..);
```

See [Chaining indicators](/guide/chaining) for more.

## Streaming

Streaming is not supported for this indicator. This indicator requires a second synchronized bar series, which cannot be expressed in the single-series streaming model. Use the Series (batch) implementation with periodic recalculation instead.
