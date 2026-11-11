---
title: Buffer list style
description: Learn how to use Buffer list style indicators for efficient incremental bar-by-bar processing
---

# Buffer style indicators for incremental processing

Buffer lists calculate an indicator one bar at a time. Each buffer list is a read-only list of results that grows as you add bars, and keeps only the state the indicator needs.

## When to use buffer lists

Use a buffer list when:

- Bars arrive one at a time, in time order
- Each bar is complete when you add it
- You call each indicator yourself, rather than having one data source update several indicators

If bars can arrive late or out of order, if the current bar is still forming and changes on every tick, or if several indicators should update from one source automatically, use [Stream hubs](/guide/styles/stream). For a one-time calculation over history, use [Batch style](/guide/styles/batch). To see how per-bar cost compares with re-running a Series method, see [Bars that arrive one at a time](/guide/styles/#bars-that-arrive-one-at-a-time).

## Basic usage

Create a buffer list and add each bar as it completes:

```csharp
using FacioQuo.Stock.Indicators;

SmaList smaList = new(lookbackPeriods: 20);

// call from your feed's message handler for each completed bar
void OnBarReceived(Bar bar)
{
    smaList.Add(bar);

    SmaResult latest = smaList[^1];

    if (latest.Sma is not null)
    {
        Console.WriteLine($"{latest.Timestamp:d}: SMA = {latest.Sma:N2}");
    }
}
```

Each `Add` adds one result per bar. Values are `null` until the indicator has enough bars to warm up; each indicator's page lists its warmup period. `RenkoList` is the exception: it can add zero or several bricks per bar, so check `Count` before reading its latest result.

## Prior bars (optional)

A buffer list works from its first bar; its values are simply `null` until the warmup period has passed. If you already have earlier bars, add them first, so values are available sooner:

```csharp
// create the list from earlier bars
SmaList smaList = historicalBars.ToSmaList(20);

// or add them to an existing list
smaList.Add(historicalBars);
```

Adding a collection gives the same results as adding its bars one at a time.

## Add complete bars in order

A buffer list assumes each bar you add is newer than the last one and is final. It does not reorder bars, skip duplicates, or let you revise a bar after adding it.

::: warning 🚩 Silent errors from out-of-order bars
Adding a bar that is late, repeated, or a revision of one already added produces incorrect results without any error. Sort bars by timestamp before adding them. For a feed that delivers late or revised bars, or updates the current bar on every tick, use a [Stream hub](/guide/styles/stream), which handles all of these.
:::

To catch this mistake while developing or testing, set `ThrowOnOutOfOrder`:

```csharp
SmaList smaList = new(20) { ThrowOnOutOfOrder = true };
```

`Add` then throws `InvalidOperationException` when a result's timestamp is not after the last one. It is off by default. The list has already updated its state when it throws, so discard the list rather than continuing. `RenkoList` checks brick timestamps, which can repeat, so it misses an out-of-order bar that completes no brick.

## Reading results

A buffer list is an `IReadOnlyList<TResult>`, so you can use its indexer, `Count`, and LINQ, and enumerate it:

```csharp
int count = smaList.Count;
SmaResult latest = smaList[^1];
IEnumerable<SmaResult> withValues = smaList.Where(r => r.Sma is not null);
```

It is not a general-purpose collection: you can't remove or insert results, and `Add` takes input values and calculates a result rather than adding the item you pass. To start over, call `Clear()`, which also resets the indicator's state.

## Chaining indicators

To calculate an indicator from another indicator's results, add each new result from one buffer list to the next. Unlike Series and stream hub chaining, you pass the results along yourself. For the broader concept, see [Chaining indicators](/guide/chaining).

```csharp
ObvList obvList = new();
RsiList rsiList = new(14);

void OnBarReceived(Bar bar)
{
    // add the bar to OBV, then add the new OBV result to RSI
    obvList.Add(bar);
    rsiList.Add(obvList[^1]);

    RsiResult latest = rsiList[^1];  // RSI of OBV
}
```

## Two-series indicators

A two-series buffer list takes each series separately. Add one with `AddSource` and the other with `AddOther` as their values arrive; either one may run ahead, and a result is added once both hold the same timestamp.

```csharp
CorrelationList correlation = new(20);

// call from each series' message handler
void OnStockReceived(IReusable value) => correlation.AddSource(value);
void OnIndexReceived(IReusable value) => correlation.AddOther(value);
```

- A timestamp that only one series holds produces no result, where the Series method throws.
- Each series must arrive in strictly increasing order: an older or equal timestamp on either side throws `InvalidOperationException`.
- Each side holds at most `MaxListSize` values waiting for a match, so a side that falls further behind than that loses pairs.

Two-series buffer lists:

- [Beta coefficient (β)](/indicators/beta#streaming)
- [Correlation coefficient](/indicators/correlation#streaming)
- [Price Relative Strength (PRS)](/indicators/prs#streaming)

## Live feeds and threads

::: warning 🚩 Data corruption risk
A buffer list is not safe for concurrent use. If two threads add bars to the same list at the same time, its results can be corrupted without any error. Add bars and read results from one thread at a time.
:::

A client that you read in a loop receives one message at a time, so you can call `Add` directly from the loop. This example reads server-sent events with `SseParser` from `System.Net.ServerSentEvents`, which ships with .NET 10 and later:

```csharp
using System.Net.ServerSentEvents;

SmaList smaList = new(20);

using HttpClient http = new();
using Stream stream = await http.GetStreamAsync(feedUri, cancellationToken);

await foreach (SseItem<string> item in SseParser.Create(stream).EnumerateAsync(cancellationToken))
{
    Bar bar = ParseBar(item.Data);  // your payload mapping
    smaList.Add(bar);

    SmaResult latest = smaList[^1];
    Console.WriteLine($"{latest.Timestamp:d}: SMA = {latest.Sma:N2}");
}
```

A WebSocket client such as .NET's `ClientWebSocket`, read in a loop, works the same way.

If your client library raises message events on several threads instead, don't lock around `Add`. A lock prevents corruption, but two events that race can still add their bars out of order, which a buffer list miscalculates without any error. Instead, have the events write to a single-reader `Channel<Bar>`, and add bars and read results from the one loop that reads it. Or use a [Stream hub](/guide/styles/stream), which places each bar by its timestamp.

To combine trade ticks or small bars into larger bars as they arrive, use a [streaming aggregator hub](/utilities/bars#streaming-aggregator-hubs); there is no buffer list equivalent.

## List size

Each buffer list keeps up to 100,000 results by default and drops the oldest when it exceeds that. To use less memory, set `MaxListSize`:

```csharp
SmaList smaList = new(20)
{
    MaxListSize = 1_000  // keep only the latest 1,000 results
};
```

Dropping old results doesn't change new ones, since each list keeps the state its indicator needs separately.

## See also

- [Stream hubs](/guide/styles/stream) for late or revised bars, a forming bar, and indicators that update together
- [Batch style](/guide/styles/batch) for one-time calculations
- [Chaining indicators](/guide/chaining) for building indicators from other indicators
- [Indicators](/indicators) for available buffer list indicators
