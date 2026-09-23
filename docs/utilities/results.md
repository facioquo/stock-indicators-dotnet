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
