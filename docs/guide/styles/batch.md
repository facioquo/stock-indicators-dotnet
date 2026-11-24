---
title: Batch (Series) style
description: Learn how to use Series batch style indicators for one-time bulk conversions of historical bar data
---

# Series style indicators for batch processing

Series style calculates an indicator over a full collection of bars in one call. It is the simplest and fastest style, and the one to start with.

## When to use batch style

Use Series style when you have the bars you need up front, as in:

- Backtesting and historical research
- Screening many symbols
- Charts and reports

When bars keep arriving, don't re-run a Series method for every new bar; its cost grows with the history it recalculates. Use [Buffer lists](/guide/styles/buffer) or [Stream hubs](/guide/styles/stream) instead, which add each bar at a flat cost. See [Bars that arrive one at a time](/guide/styles/#bars-that-arrive-one-at-a-time) for a comparison.

## Basic usage

Call an indicator method on your bars to get a result for every bar:

```csharp
using FacioQuo.Stock.Indicators;

// your own method that gets historical bars from your data source
IReadOnlyList<Bar> bars = GetBarsFromFeed("MSFT");

// calculate a 20-period SMA
IReadOnlyList<SmaResult> results = bars.ToSma(20);

foreach (SmaResult r in results)
{
    Console.WriteLine($"SMA on {r.Timestamp:d} was {r.Sma:N4}");
}
```

The results hold one result per bar, in the same order as the bars. Values are `null` until the indicator has enough bars to warm up; each indicator's page lists its warmup period. For how much history to provide, see [How much historical bar data do I need?](/guide/getting-started#how-much-historical-bar-data-do-i-need)

## Bar order

Series methods expect bars in time order, oldest first, with no duplicate timestamps. They don't sort or check your bars.

::: warning 🚩 Silent errors from unsorted bars
Bars that are out of order, or that repeat a timestamp, produce incorrect results without any error. If your data source doesn't guarantee the order, sort the bars first with [`ToSortedList()`](/utilities/bars#sort-bars). To check for duplicates and order problems, use [`Validate()`](/utilities/bars#validate-bar-history).
:::

## Chaining indicators

Call one indicator method on another's results to calculate an indicator from that indicator. For the broader concept, see [Chaining indicators](/guide/chaining).

```csharp
// RSI of On-Balance Volume
IReadOnlyList<RsiResult> rsiOfObv = bars
    .ToObv()
    .ToRsi(14);

// EMA of the (High + Low) / 2 price instead of Close
IReadOnlyList<EmaResult> emaOfHl2 = bars
    .Use(CandlePart.HL2)
    .ToEma(20);
```

## Working with results

[Result utilities](/utilities/results) help with common follow-up steps:

```csharp
IReadOnlyList<AdxResult> results = bars.ToAdx(14);

// find the result for a timestamp
AdxResult? result = results.Find(new DateTime(2024, 1, 15));

// remove the warmup period's results
IReadOnlyList<AdxResult> warmedUp = results.RemoveWarmupPeriods();

// keep only the bars where a candlestick pattern matched
IReadOnlyList<CandleResult> signals = bars
    .ToMarubozu()
    .Condense();
```

Results are lists of immutable records, so you can use LINQ on them and share them across threads. A Series method doesn't change the bars you pass, so you can also calculate several indicators from the same bars on different threads at once.

## See also

- [Getting started](/guide/getting-started) for installation and supplying bar data
- [Result utilities](/utilities/results) for finding, trimming, and condensing results
- [Buffer lists](/guide/styles/buffer) and [Stream hubs](/guide/styles/stream) for bars that arrive one at a time
- [Indicators](/indicators) for available indicators
