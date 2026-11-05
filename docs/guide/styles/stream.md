---
title: Stream hub style
description: Learn how to use Stream hub style for real-time indicator processing with observable patterns and coordinated updates
---

# Hub style indicators for advanced streaming

Stream hubs calculate indicators in real time. You add bars to one `BarHub`, and every indicator hub subscribed to it updates automatically, including when a bar arrives late or is revised.

## When to use stream hubs

Use stream hubs when any of these apply:

- Several indicators share one live feed
- An indicator is calculated from another indicator, such as RSI of EMA
- Bars can arrive out of order, be revised, or be removed
- The current bar is still forming and changes on every tick

For one indicator over bars that always arrive in order, a [Buffer list](/guide/styles/buffer) is simpler. For a one-time calculation over history, use [Batch style](/guide/styles/batch). To see how per-bar cost compares with re-running a Series method, see [Bars that arrive one at a time](/guide/styles/#bars-that-arrive-one-at-a-time).

## Basic usage

Create a bar hub, subscribe indicator hubs to it, and add bars as they arrive:

```csharp
using FacioQuo.Stock.Indicators;

// the data source
BarHub barHub = new();

// indicator hubs subscribe to the bar hub
SmaHub smaHub = barHub.ToSmaHub(20);
RsiHub rsiHub = barHub.ToRsiHub(14);

// call from your feed's message handler as each bar arrives
void OnBarReceived(Bar bar)
{
    barHub.Add(bar);

    RsiResult rsi = rsiHub.Results[^1];

    if (rsi.Rsi > 70)
    {
        Console.WriteLine($"{bar.Timestamp:d}: Overbought at {bar.Close:C2}");
    }
}
```

When `Add` returns, every subscribed hub has already calculated its result for the new bar, so you can read `Results` right away. Each hub holds one result per bar. Values are `null` until the indicator has enough bars to warm up; each indicator's page lists its warmup period.

## Start from history

Indicators need prior bars before they produce values. Load your history before going live, so results are ready from the first live bar:

```csharp
BarHub barHub = new();

// add historical bars in one call
barHub.Add(historicalBars);

// a hub created after the bars are loaded calculates over them at once
RsiHub rsiHub = barHub.ToRsiHub(14);

// then add live bars as they arrive
barHub.Add(liveBar);
```

You can subscribe hubs before or after loading history; either way, they end up with the same results.

## Update the current bar

Add a bar with the same `Timestamp` as one the hub already holds to replace it. Use this to update the bar that is still forming as each tick arrives:

```csharp
// each tick revises the one-minute bar that is still forming
barHub.Add(new Bar(minuteStart, open, high, low, lastPrice, volume));
```

Every subscribed hub recalculates from that bar forward. A bar that arrives late, with a timestamp earlier than the newest bar, is inserted in time order and recalculated the same way. To remove bars, or to handle bars the hub refuses, see [Corrections and memory](/guide/styles/stream/corrections).

## Chaining indicators

Subscribe one indicator hub to another to calculate an indicator from another indicator's results. For the broader concept, see [Chaining indicators](/guide/chaining).

```csharp
BarHub barHub = new();

// RSI of EMA
EmaHub emaHub = barHub.ToEmaHub(20);
RsiHub rsiOfEma = emaHub.ToRsiHub(14);

// or in one expression
RsiHub rsiHub = barHub
    .ToEmaHub(20)
    .ToRsiHub(14);

// adding a bar updates the EMA, then the RSI
barHub.Add(newBar);
```

## Two-series indicators

A two-series hub observes two providers, such as two indicators of one `BarHub` or two `BarHub` feeds:

```csharp
BarHub barHub = new();

// spread between a fast and a slow moving average
PairMathHub spread = barHub
    .ToEmaHub(12)
    .ToMinusHub(barHub.ToEmaHub(26));
```

A result is added once both providers hold the same timestamp. A timestamp that only one provider holds produces no result, where the Series method throws. Late, revised, or removed values on either side are recalculated like any other correction.

Two-series stream hubs:

- [IndyMath](/indicators/indy-math#streaming) two-series methods, such as `.ToMinusHub(other)`
- [Beta coefficient (β)](/indicators/beta#streaming)
- [Correlation coefficient](/indicators/correlation#streaming)
- [Price Relative Strength (PRS)](/indicators/prs#streaming)

## Live feeds and threads

A hub accepts changes from one thread at a time. If your client delivers messages one after another, as a receive loop does, call `Add` directly from the loop. If its callbacks can run at the same time on different threads, write bars to a feed instead, which applies them to the hub one at a time in arrival order:

```csharp
BarHub barHub = new();
RsiHub rsiHub = barHub.ToRsiHub(14);

await using StreamFeed<IBar> feed = barHub.CreateFeed();

// safe to call from any thread
async Task OnBarReceived(Bar bar, CancellationToken cancellationToken)
    => await feed.WriteAsync(bar, cancellationToken);
```

To read results on a different thread from the one adding bars, call `Snapshot()` for a consistent copy, such as `rsiHub.Snapshot()`. For feed options, error handling, and locking, see [Threading and live feeds](/guide/styles/stream/threading).

If your feed delivers trade ticks, or smaller bars than you want to analyze, combine them into larger bars with a [streaming aggregator hub](/utilities/bars#streaming-aggregator-hubs) and subscribe your indicator hubs to it.

## Cache size

Each hub keeps up to 100,000 results by default and drops the oldest when it exceeds that. To use less memory, set a smaller `maxCacheSize` on the bar hub; every hub subscribed to it uses the same size.

```csharp
BarHub barHub = new(maxCacheSize: 1_000);
RsiHub rsiHub = barHub.ToRsiHub(14);
```

A hub throws `ArgumentOutOfRangeException` when it is created if the size is too small for its warmup. After you create your hubs, `barHub.MinCacheSize` reports the smallest size the whole chain needs. Keep the cache larger than the history you read or might correct; see [Cache size and pruning](/guide/styles/stream/corrections#cache-size-and-pruning).

## Stop streaming

```csharp
// stop one indicator; the others keep updating
smaHub.Unsubscribe();

// end the stream: every hub in the chain stops updating
barHub.EndTransmission();
```

A stopped hub keeps its `Results`, which you can still read.

## Example: moving average crossover

```csharp
BarHub barHub = new();
barHub.Add(historicalBars);

EmaHub fastEma = barHub.ToEmaHub(50);
EmaHub slowEma = barHub.ToEmaHub(200);

void OnBarReceived(Bar bar)
{
    barHub.Add(bar);

    // a crossover needs two results
    if (fastEma.Results.Count < 2)
    {
        return;
    }

    EmaResult fastPrior = fastEma.Results[^2];
    EmaResult slowPrior = slowEma.Results[^2];
    EmaResult fastNow = fastEma.Results[^1];
    EmaResult slowNow = slowEma.Results[^1];

    // comparisons with null (during warmup) are false
    if (fastPrior.Ema < slowPrior.Ema && fastNow.Ema > slowNow.Ema)
    {
        Console.WriteLine($"{bar.Timestamp:u}: fast EMA crossed above slow EMA");
    }
}
```

## See also

- [Threading and live feeds](/guide/styles/stream/threading) for multi-threaded feeds, feed options, and reading results across threads
- [Corrections and memory](/guide/styles/stream/corrections) for removing bars, refused bars, cache sizing, and fault recovery
- [Custom observers](/guide/custom-observers) for pushing results into a UI, database, or alerting pipeline
- [Streaming aggregator hubs](/utilities/bars#streaming-aggregator-hubs) for combining ticks or bars into larger bars in real time
- [Buffer lists](/guide/styles/buffer) for simple incremental processing
- [Indicators](/indicators) for available stream hub indicators
