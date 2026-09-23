---
title: Stream hub style
description: Learn how to use Stream hub style for real-time indicator processing with observable patterns and coordinated updates
---

# Hub style indicators for advanced streaming

Stream hub style provides real-time processing with observable patterns and state management. Multiple indicators can subscribe to a central `BarHub` for coordinated updates, making it ideal for live data feeds and complex event-driven architectures.

## When to use stream hubs

**Ideal for:**

- Live data feeds and WebSocket integration
- Multiple indicators requiring synchronized updates
- Trading applications with low-latency requirements
- Real-time dashboards and monitoring
- Complex event-driven architectures

**Not ideal for:**

- One-time historical analysis (use [Batch style](/guide/styles/batch))
- Simple incremental processing (use [Buffer lists](/guide/styles/buffer))
- Scenarios without real-time requirements

## Basic usage

Create a bar hub and subscribe indicators as observers:

```csharp
using FacioQuo.Stock.Indicators;

// create bar hub (the data source)
BarHub barHub = new();

// subscribe indicators to the hub
SmaHub smaHub = barHub.ToSmaHub(20);
RsiHub rsiHub = barHub.ToRsiHub(14);

// stream bars as they arrive
foreach (Bar bar in liveBars)
{
    // adding to barHub automatically updates all subscribers
    barHub.Add(bar);

    // safely get latest results
    if (smaHub.Results.Count > 0)
    {
        SmaResult sma = smaHub.Results[^1];
        RsiResult rsi = rsiHub.Results[^1];

        // use results for trading logic, alerts, etc.
        if (sma.Sma is not null && rsi.Rsi > 70)
        {
            Console.WriteLine($"{bar.Timestamp:d}: Overbought at {bar.Close:C2}");
        }
    }
}
```

## Key features

### Observable pattern

The hub-observer architecture ensures coordinated updates:

- Single bar update propagates to all subscribed indicators
- Automatic cascade execution in correct sequence
- Reduced coordination complexity
- Minimal latency overhead

### Chaining indicators

Create sophisticated derived indicators. For the broader concept, see [Chaining indicators](/guide/chaining).

```csharp
BarHub barHub = new();

// chain RSI from EMA
EmaHub emaHub = barHub.ToEmaHub(20);
RsiHub rsiHub = emaHub.ToRsiHub(14);  // RSI of EMA

// or chain directly
RsiHub rsiOfEma = barHub
    .ToEmaHub(20)
    .ToRsiHub(14);

// publish bar - both EMA and RSI update automatically
barHub.Add(newBar);
```

### State management and rollback

Handle late-arriving data and corrections:

```csharp
BarHub barHub = new();
SmaHub smaHub = barHub.ToSmaHub(20);

// add bars normally
barHub.Add(bar1);
barHub.Add(bar2);

// late-arriving data with earlier timestamp
barHub.Add(lateBar);  // triggers recalculation in dependent hubs

// correct a bad value: re-add a bar with the same timestamp
barHub.Add(correctedBar);  // same timestamp, revised values → rebuild

// or remove a bad bar (and everything at/after its timestamp)
barHub.RemoveRange(badBar.Timestamp, notify: true);  // triggers recalculation

// or remove a single bar by its timestamp
barHub.Remove(badBar);  // finds the entry by timestamp, then rebuilds
```

The hub automatically handles state rollback and recalculation when data arrives out of order or needs correction. To revise a single value in place, re-`Add` a bar with the same `Timestamp`; to drop data, remove it by timestamp with `RemoveRange(fromTimestamp, notify)`, by position with `RemoveAt(cacheIndex)`, or by bar with `Remove(bar)` (which locates the entry by timestamp). These mutations are **enforced to run on the root hub only** — the `BarHub` (or `TradeTickHub`) you add bars to. Calling `Add`, `RemoveAt`, `RemoveRange`, `Remove`, or `Reinitialize` on a subscribed/chained hub throws `InvalidOperationException`; mutate the root hub, which cascades every change to the dependent hubs (see [Thread safety](#thread-safety)).

A bar older than everything the hub still holds is refused when it falls inside history the hub already pruned, or when the cache is full. `BarHub` raises `BarRejected` for each refusal, with the bar and a `BarRejectionReason`, and leaves its cache unchanged:

```csharp
barHub.BarRejected += (_, e) =>
    logger.LogWarning("Refused {Timestamp}: {Reason}", e.Bar.Timestamp, e.Reason);
```

## Performance characteristics

- **Overhead:** ~20-30% slower than batch style for equivalent datasets
- **Memory:** Maintains cache and state for all subscribed indicators
- **Latency:** Optimized for real-time updates, typically <1ms per bar
- **Scalability:** Supports multiple concurrent observers with single propagation

## Thread safety

Stream hubs follow a **single-writer** model. Internal locking protects cache *integrity* during the rebuild and rollback that out-of-order data triggers — but it does **not** make concurrent mutation safe. Two threads calling mutating methods at the same time can still interleave incorrectly.

- **Internal locking** guards the cache so a rebuild never exposes a half-updated state to readers.
- **Serialize all mutating calls** — `Add`, `RemoveAt`, `RemoveRange`, `Reinitialize`, and `Rebuild` must come from a single writer (one thread, or funneled through a lock / `Channel<Bar>`).
- **Single-threaded usage** requires no additional synchronization.
- **Multi-threaded usage** must serialize every external call that mutates the hub; reads of `Results` should also be coordinated with the writer (see the example below).

### What's safe, and what to avoid

The library deliberately keeps the mutation surface small: `Results` is a read-only view, so you can't reach the underlying cache with `List` / `ICollection` methods. Stay within that surface:

- ✅ **Do** feed and correct data through the **root** hub — the `BarHub` (or `TradeTickHub`) you created and add bars to. It cascades every change to the dependent hubs automatically.
- ✅ **Do** read results through `Results`; if you hand them to another thread, call `Snapshot()` (an atomic, immutable copy taken under the hub's lock) rather than enumerating the live `Results` view while the writer mutates it.
- ❌ **Don't** call `Add` / `RemoveAt` / `RemoveRange` / `Remove` / `Reinitialize` on a *subscribed* (chained) hub such as a `SmaHub` — these throw `InvalidOperationException`. Those hubs are driven by their provider; mutating one directly would desynchronize it from that provider, and a later rebuild could produce wrong results the hub can't heal from. Feed and correct through the root hub instead.
- ❌ **Don't** mutate from more than one thread at a time (see the example below).

This single-writer expectation isn't unique to this library: most built-in .NET collections (`List<T>`, `Dictionary<TKey,TValue>`, `Queue<T>`) are likewise unsafe for concurrent writers. The hub adds internal locking to keep its *own* cache consistent during a rebuild, but coordinating *your* calls is still your responsibility — exactly as it would be for any ordinary collection.

### Thread-safe external access example

```csharp
BarHub barHub = new();
SmaHub smaHub = barHub.ToSmaHub(20);

// Use lock for coordinated multi-threaded access
object hubLock = new();

// Thread 1: Adding bars
Task producer = Task.Run(() =>
{
    foreach (Bar bar in liveBars)
    {
        lock (hubLock)
        {
            barHub.Add(bar);
        }
    }
});

// Thread 2: Reading results
Task consumer = Task.Run(() =>
{
    while (running)
    {
        SmaResult? latest;
        lock (hubLock)
        {
            latest = smaHub.Results.Count > 0
              ? smaHub.Results[^1]
              : null;
        }
        ProcessResult(latest);
    }
});
```

::: warning 🚩 Data corruption risk
Internal thread-safety protects cache integrity during rebuilds (e.g., out-of-order data reconciliation). However, a hub instance is not safe for concurrent external use from multiple threads. Callers must ensure only one thread accesses a given instance at a time.
:::

## Advanced patterns

### Reactive strategies

```csharp
BarHub barHub = new();

EmaHub emaFast = barHub.ToEmaHub(50);
EmaHub emaSlow = barHub.ToEmaHub(200);

// add bars to barHub (from stream)
barHub.Add(newBar);
// and the 2 EmaHub will be in sync

if(emaFast.Results[^2].Ema < emaSlow.Results[^2].Ema
&& emaFast.Results[^1].Ema > emaSlow.Results[^1].Ema)
{
    // cross over occurred
}
```

### Event-driven alerts

```csharp
BarHub barHub = new();
RsiHub rsiHub = barHub.ToRsiHub(14);

void ProcessLiveData(Bar bar)
{
    barHub.Add(bar);
    
    RsiResult latest = rsiHub.Results[^1];
    
    if (latest?.Rsi > 70)
    {
        TriggerAlert("Overbought", bar.Close, latest.Rsi);
    }
    else if (latest?.Rsi < 30)
    {
        TriggerAlert("Oversold", bar.Close, latest.Rsi);
    }
}
```

## WebSocket/SSE integration

This example demonstrates how to connect stream hubs to a live WebSocket feed. The pattern applies to any real-time data source (WebSocket, SSE, message queue, etc.) where bars arrive asynchronously. The hub's `Add` method integrates each incoming bar, automatically propagating updates to all subscribed indicators.

If your feed delivers raw trade ticks or finer-grained bars than you want to analyze, quantize them first with a [streaming aggregator hub](/utilities/bars#streaming-aggregator-hubs) — `TradeTickHub` → `TradeTickAggregatorHub` for ticks, or `BarAggregatorHub` for bar-to-bar resampling — then chain your indicator hubs off the aggregated output.

Because socket callbacks can fire concurrently, every `Add` must go through a single writer (see [Thread safety](#thread-safety) above). The handler below serializes with a lock; for higher throughput, post incoming bars onto a single-consumer `Channel<Bar>` and call `Add` from one drain loop instead.

```csharp
// setup hubs
BarHub barHub = new();
SmaHub smaHub = barHub.ToSmaHub(20);

// single-writer gate: serialize every Add to the hub
object hubLock = new();

// WebSocket message handler (may be invoked concurrently)
async Task OnBarReceived(WebSocketBar wsBar)
{
    // convert WebSocket bar to library Bar
    Bar bar = new()
    {
        Timestamp = wsBar.Timestamp,
        Open = wsBar.Open,
        High = wsBar.High,
        Low = wsBar.Low,
        Close = wsBar.Close,
        Volume = wsBar.Volume
    };

    // update hub through the single-writer gate -
    // all observers cascade automatically
    lock (hubLock)
    {
        barHub.Add(bar);
    }

    // in this example, the subscribing SmaHub will
    // auto-generate the next corresponding SMA value
}
```

## Memory management

Stream hubs automatically prune old results when the cache exceeds the configured maximum size:

```csharp
// default max cache size (100,000 items)
BarHub barHub = new();

// or configure custom max cache size
BarHub limitedHub = new(maxCacheSize: 500);

// automatic FIFO pruning when limit reached
SmaHub smaHub = limitedHub.ToSmaHub(20);

// as new bars arrive, oldest results are removed automatically
foreach (Bar bar in liveBars)
{
    limitedHub.Add(bar);  // oldest pruned if over limit
}
```

The default cache size is 100,000 items. For applications with different requirements, specify a custom `maxCacheSize` when creating the BarHub.

## Fault handling and recovery

A hub guards against runaway feedback (for example, an accidental circular chain, or a provider stuck re-sending the same tick). If the **same timestamp with identical values** is re-sent more than 100 times in a row, the hub faults:

- `IsFaulted` flips to `true`,
- subscribed observers receive `OnError(OverflowException)`, and
- the offending `Add` throws `OverflowException` ("A repeated stream update exceeded the 100 attempt threshold…").

```csharp
BarHub barHub = new();
SmaHub smaHub = barHub.ToSmaHub(20);

try
{
    barHub.Add(bar);
}
catch (OverflowException)
{
    // a circular chain or a stuck provider
    // re-sent an identical tick 100+ times
}

if (barHub.IsFaulted)
{
    // clear the fault and resume streaming
    barHub.ResetFault();
}
```

`ResetFault()` clears the faulted state so the hub can keep processing. The threshold only trips on *byte-identical* repeats — a normal correction (same timestamp, **different** values) is handled as a rollback, not a fault. If your feed legitimately re-sends identical trade prints, dedupe upstream or vary a field before calling `Add` so a real burst of identical ticks doesn't trip the guard.

::: warning 🚩 Reinitialize is a single-writer operation
`Reinitialize()` unsubscribes, rebuilds the cache from the provider, then re-subscribes. Bars that arrive in the brief window between the rebuild and the re-subscribe can be missed. Call `Reinitialize()` only when no concurrent `Add` is in flight — i.e. from the same single writer that feeds the hub.
:::

::: info Cache size inheritance
Hubs will inherit the `maxCacheSize` of its provider.  For example, if you set a size of 1,000 for your `BarHub`, then a chained `SmaHub` will also have a maximum cache size of 1,000.
:::

::: tip ✨ Optimize cache size for your use case

Set your `maxCacheSize` according to how you use the data produced in the hub cache. If you only need the latest indicator values and don't read history, a smaller cache saves memory.

```csharp
// configure a modest cache that still clears every warmup floor
BarHub limitedHub = new(maxCacheSize: 500);

// automatic FIFO pruning when limit reached
SmaHub smaHub = limitedHub.ToSmaHub(20);
```

Don't size the cache down to the indicator's lookback, though — two things bite:

- **Inheritance.** The `BarHub`'s `maxCacheSize` flows to every chained hub (above), so the cache must satisfy the *largest* warmup in the whole chain, not just one indicator.
- **Warmup floor.** Each hub validates `maxCacheSize` against its own warmup requirement at construction and throws `ArgumentOutOfRangeException` if it's too small. That requirement is often a multiple of the lookback: RSI needs ~2× its period, TEMA and TRIX ~3×, and the Hilbert-transform trendline needs a fixed 63 periods regardless of lookback.

Pick a floor comfortably above the deepest warmup in the chain (with headroom for late-arrival rollbacks), rather than the bare indicator minimum.

:::

::: warning 🚩 Corrections after pruning are approximate for stateful indicators
Once the cache has pruned old bars, a correction or late arrival that triggers a rebuild can no longer see the pruned history, so the rebuilt results don't match a hub that received the same data in order:

- **Indicators with a warmup/lookback** (window indicators like SMA, and recursive smoothers like EMA and the Wilder family) lose their leading results to `null` — the earliest retained bars no longer have enough history to recompute their warmup. For a pure window indicator like SMA the *remaining* values stay exact; the leading ones just disappear.
- **Recursive smoothers** (EMA and everything built on it; the Wilder family — RSI, ATR, ADX, SMMA) additionally re-seed from the truncated history and drift, re-converging as the recursion re-stabilizes. The Wilder hubs that re-derive their seed on *every* rollback (RSI, ADX, SMMA) drift on any correction once the cache has pruned, not only deep ones.
- **Cumulative indicators** (OBV, ADL) have no warmup to lose, so no leading `null`s — instead they restart their running total from the truncated history and carry a *permanent* offset across every result.

This only happens when `maxCacheSize` is **smaller than the history you revise**. Size `maxCacheSize` to retain the full history you may need to correct — with an adequate cache, a rebuild reproduces an in-order hub exactly.
:::

## See also

- [Batch style](/guide/styles/batch) for one-time calculations
- [Buffer lists](/guide/styles/buffer) for simple incremental processing
- [Custom observers](/guide/custom-observers) for subscribing an observer to a hub to push results into a UI, persistence, or alerting pipeline
- [Streaming aggregator hubs](/utilities/bars#streaming-aggregator-hubs) for quantizing live ticks or bars into larger period bars in real time
- [Indicators](/indicators) for available stream hub indicators
