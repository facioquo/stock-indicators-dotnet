---
title: Result utilities
description: Utilities for working with indicator results after calculation and analysis.
---

# Result utilities

Utilities for working with indicator results after calculation and analysis.

## Condense

`results.Condense()` removes non-essential results so only meaningful records remain. For example, on [candlestick patterns](/indicators/doji) it returns only the dates where a signal occurs. It is a lightweight filter — it does not recalculate the indicator.

```csharp
// only the dates with Marubozu signals
IReadOnlyList<CandleResult> results =
  bars.ToMarubozu().Condense();
```

`.Condense()` removes results where the value is `null` or `NaN`; for candlestick patterns it removes records with no match (`Match.None`). Behavior varies by indicator type:

| Indicator type | Condensed behavior |
| -------------- | ------------------ |
| Candlestick patterns | Returns only pattern matches |
| Signal-based indicators | Returns only signal points |
| Continuous indicators | Removes warmup-period nulls |

::: warning 🚩 Data reduction
Condensed results contain fewer records than the input and may have gaps in the timeline. This is intentional — use it when you only care about specific events or signals, not the continuous time series.
:::

## Find by date

`results.Find(lookupDate)` looks up a single indicator result by date, using a binary search over the time-sorted series. It returns the matching result, or the default value (`null` for reference types) when no result has that exact timestamp.

```csharp
IReadOnlyList<SmaResult> results = bars.ToSma(20);

SmaResult? result = results.Find(DateTime.Parse("2024-01-15"));

if (result is not null)
{
  Console.WriteLine($"SMA: {result.Sma}");
}
```

The comparison is exact, so a timestamp with a time component (e.g. `09:30:00`) will not match a date-only entry.

::: tip Date-only comparison
To match on date only (ignoring time), use LINQ instead:

```csharp
var target = DateTime.Parse("2024-01-15").Date;
var result = results.FirstOrDefault(r => r.Timestamp.Date == target);
```

:::

For range queries or filtering by value, use LINQ `.Where()` rather than repeated `.Find()` calls.

## Result math

Result math utilities combine or transform chainable indicator results — any series of `IReusable` values, including price bars — so simple custom indicators need no hand-written loop. Each returns one `TimeValue` per source result, which chains into any indicator that accepts reusable values.

```csharp
// spread between a fast and a slow moving average, then smoothed
IReadOnlyList<TimeValue> spread = bars.ToSma(10).Minus(bars.ToSma(20));
IReadOnlyList<SmaResult> smoothed = spread.ToSma(5);

// MACD less its own 9-period EMA, plus a 9-period SMA of price
IReadOnlyList<MacdResult> macd = bars.ToMacd(12, 26, 9);
IReadOnlyList<TimeValue> composite = macd.Minus(macd.ToEma(9)).Plus(bars.ToSma(9));

// upper envelope 2.5% above the EMA
IReadOnlyList<TimeValue> upper = bars.ToEma(20).PlusPercent(2.5);

// highest close over the last 20 periods
IReadOnlyList<TimeValue> highestClose = bars.RollingMax(20);

// highest high over the last 20 periods
IReadOnlyList<TimeValue> highestHigh = bars.ToBarPart(CandlePart.High).RollingMax(20);
```

| Method | Returns |
| ------ | ------- |
| `.Plus(other)` / `.Plus(amount)` | sum of two series, or the series plus a fixed amount |
| `.PlusPercent(percentOffset)` | each value offset by a percent of itself, where `-2.5` subtracts 2.5% |
| `.Minus(other)` / `.Minus(amount)` | difference of two series, or the series minus a fixed amount |
| `.Times(other)` / `.Times(factor)` | product of two series, or the series times a fixed factor |
| `.DividedBy(other)` / `.DividedBy(divisor)` | quotient of two series (`NaN` where the divisor is 0), or the series over a fixed, non-zero divisor |
| `.MaxWith(other)` / `.MinWith(other)` | larger or smaller of two series at each period |
| `.RollingMax(lookbackPeriods)` / `.RollingMin(lookbackPeriods)` | highest or lowest value over a rolling window |
| `.RollingMedian(lookbackPeriods)` | median over a rolling window, averaging the two middle values when the window is even; its cost grows with the window, O(n·k) |

A fixed number is always the right-hand operand. For a percent decrease, pass `PlusPercent` a negative offset. Price bars contribute their close; select another part with [`.ToBarPart()`](/indicators/bar-part) first.

Rolling windows include the current period and return `NaN` for the first `lookbackPeriods − 1` periods. `NaN` values, such as indicator warmup periods, propagate: a pairwise result is `NaN` where either input is, and a rolling result is `NaN` while its window holds one. A non-finite fixed amount, factor, or offset, a fixed divisor of 0, or a lookback below 1 throws an `ArgumentOutOfRangeException`; a 0 divisor inside a series yields `NaN` for that period.

::: warning 🚩 Timestamps must match
Methods that combine two series require both to hold the same number of results with identical timestamps at every position, which holds for any two indicators calculated from the same bars. A length or timestamp mismatch throws an `InvalidBarsException` rather than pairing values from different periods. Series with different date ranges are not aligned for you: trim both to the same range first, and remove warmup periods from neither or both.
:::

### Incremental buffer lists

The single-series methods also come as [BufferList](/guide/styles/buffer) types that compute each result as you add a value, and match the Series results exactly. Pairwise methods that combine two series are available only as Series.

| Series | BufferList |
| ------ | ---------- |
| `.Plus(amount)`, `.Minus(amount)`, `.Times(factor)`, `.DividedBy(divisor)`, `.PlusPercent(percentOffset)` | `ScalarMathList`, from `.ToPlusList(amount)`, `.ToMinusList(amount)`, `.ToTimesList(factor)`, `.ToDividedByList(divisor)`, or `.ToPlusPercentList(percentOffset)` |
| `.RollingMax(lookbackPeriods)` | `RollingMaxList`, from `.ToRollingMaxList(lookbackPeriods)` |
| `.RollingMin(lookbackPeriods)` | `RollingMinList`, from `.ToRollingMinList(lookbackPeriods)` |
| `.RollingMedian(lookbackPeriods)` | `RollingMedianList`, from `.ToRollingMedianList(lookbackPeriods)` |

```csharp
// highest close over the last 20 periods, updated per bar
RollingMaxList highest = new(lookbackPeriods: 20);

foreach (Bar bar in bars)
{
  highest.Add(bar);
  double latest = highest[^1].Value;
}

// or construct a scalar list directly
ScalarMathList upper = new(ScalarOperation.PlusPercent, 2.5, bars.ToEma(20));
```

## Remove warmup periods

`results.RemoveWarmupPeriods()` trims the initial warmup periods from indicator results. An overload `.RemoveWarmupPeriods(removePeriods)` lets you specify the exact amount.

```csharp
// automatic — uses the indicator's recommended amount
IReadOnlyList<AdxResult> auto =
  bars.ToAdx(14).RemoveWarmupPeriods();

// custom — remove a specific quantity
IReadOnlyList<AdxResult> custom =
  bars.ToAdx(14).RemoveWarmupPeriods(114);
```

For most indicators, the automatic amount is every leading result with no calculated value. Indicators whose values converge gradually, such as EMA and RSI, also remove the periods they need to converge. See [individual indicator pages](/indicators) for each indicator's recommended pruning amount. Common values:

| Indicator | Removed warmup |
| --------- | -------------- |
| SMA(n) | n − 1 periods |
| EMA(n) | n + 100 periods |
| RSI(n) | 10×n periods |
| ADX(n) | 2×n + 100 periods |
| MACD(fast, slow, signal) | slow + signal + 250 periods |

When every result is still warming up, the automatic form returns an empty list.

::: info Limited availability
The parameterless `.RemoveWarmupPeriods()` is available on the indicators whose pages list it. For the others, use the `.RemoveWarmupPeriods(removePeriods)` overload to prune a specific amount.
:::

::: warning 🚩 Auto-pruning is unstable on chained indicators
Without a `removePeriods` value, the utility derives the pruning amount from the first calculated value. With unusual results or chained indicators, this can over-prune. Specify an explicit amount when chaining.

```csharp
// AVOID: auto-pruning on chained indicators may remove too much
bars.ToEma(20).ToRsi(14).RemoveWarmupPeriods();

// BETTER: predictable, explicit amount
bars.ToEma(20).ToRsi(14).RemoveWarmupPeriods(300);
```

:::

## Sort results

`results.ToSortedList()` sorts any collection of indicator results and returns an `IReadOnlyList` ordered by ascending `Timestamp`. Results from the built-in library indicators are already sorted, so you only need this when building [custom indicators](/guide/customization) or after manually manipulating timestamps.

```csharp
// ensure chronological order before returning custom results
return customResults.ToSortedList();
```

::: info When to use: custom indicators only
Built-in indicators preserve the chronological order of the input bars, so their results are already sorted. Reach for `.ToSortedList()` only in custom implementations or after merging or re-timestamping results. For raw price data, see [Sort bars](/utilities/bars#sort-bars).
:::

## See also

- [Bar utilities](/utilities/bars) — prepare and transform price bars
- [Additional helper utilities](/utilities/helpers) — math and numerical methods for custom indicators
- [Indicator catalog](/utilities/catalog) — discover indicator metadata programmatically
