---
title: Buffer list style
description: Learn how to use Buffer list style indicators for efficient incremental bar-by-bar processing
---

# Buffer style indicators for incremental processing

Buffer list style provides efficient incremental processing for growing datasets. Use this when bars arrive sequentially and you need to add them one at a time without the overhead of a full hub infrastructure.

## When to use buffer lists

**Ideal for:**

- Building up historical data incrementally
- Processing data feeds where bars arrive sequentially
- Self-managed incremental calculations
- Scenarios without multi-indicator coordination needs
- Memory-efficient processing with auto-pruning

**Not ideal for:**

- Complete historical datasets (use [Batch style](/guide/styles/batch))
- Multiple indicators needing coordinated updates (use [Stream hubs](/guide/styles/stream))
- One-time calculations (use [Batch style](/guide/styles/batch))

## Basic usage

Buffer lists maintain incremental state as you add new bars:

```csharp
using FacioQuo.Stock.Indicators;

// create buffer list with lookback period
SmaList smaList = new(lookbackPeriods: 20);

// add bars incrementally (e.g., from a data feed)
foreach (Bar bar in bars)
{
    smaList.Add(bar);

    // safely get latest result
    if (smaList.Count > 0)
    {
        SmaResult r = smaList[^1];

        // use result (SMA is null during warmup period)
        if (r.Sma is not null)
        {
            Console.WriteLine($"{r.Timestamp:d}: SMA = {r.Sma:N2}");
        }
    }
}
```

::: warning 🚩 Add bars in chronological order
A buffer list is a single-pass accumulator: every `Add` assumes the new value is the newest one. It does **not** reorder input, detect duplicates, or correct revised values — feeding an out-of-order, repeated, or late-arriving bar produces silently incorrect results. If your data can arrive out of order (e.g. a raw WebSocket feed, or merging two sources), sort by timestamp before adding, or use a [Stream hub](/guide/styles/stream) instead — stream hubs are built for late arrivals, same-timestamp corrections, and rollback.
:::

## Key features

### List interface

Buffer lists are read-only result lists you append to. The base `BufferList<TResult>` implements `IReadOnlyList<TResult>` — so you get indexer access, `Count`, and enumeration — plus indicator-specific `Add` overloads for feeding new values and a small set of list helpers (`Clear`, `Contains`, `CopyTo`):

```csharp
SmaList smaList = new(20);

// add individual bars
smaList.Add(bar);

// or add batches
smaList.Add(barList);

// read-only list access
int count = smaList.Count;
bool isEmpty = smaList.Count == 0;
SmaResult latest = smaList[^1];
```

Note that a buffer list is not a general-purpose mutable collection: it does not implement `ICollection<TResult>`, so there is no `Remove`, no insert-at-index, and `Add` appends a computed result rather than an arbitrary item. Pruning of old results is automatic (see [Memory management](#memory-management)).

### Automatic buffer management

Buffer lists automatically manage internal buffers needed for calculations:

- Maintains lookback periods internally
- No manual state management required
- Efficient incremental updates (typically O(1) or O(log n))

### Memory management

Control memory usage with `MaxListSize`:

```csharp
SmaList smaList = new(20)
{
    MaxListSize = 1000  // keep only last 1000 results
};
```

When the list exceeds `MaxListSize`, older results are automatically pruned. Default is 100,000 elements.

## Chaining indicators

Chain buffer lists for derived indicators. For the broader concept, see [Chaining indicators](/guide/chaining).

::: info Operator-orchestrated
Unlike series or stream-hub chaining, this is orchestrated by you rather than the library: you manually take each result from one list and add it to the next. The library does not coordinate the cascade.
:::

```csharp
// create OBV buffer list
ObvList obvList = new();

// add bars to OBV
foreach (var bar in bars)
{
    obvList.Add(bar);
}

// chain RSI from OBV results
RsiList rsiList = new(14);
foreach (var obvResult in obvList)
{
    rsiList.Add(obvResult);
}

// get latest RSI of OBV
if (rsiList.Count > 0)
{
    RsiResult latest = rsiList[^1];
}
```

## Performance characteristics

- **Overhead:** ~10-20% slower than batch style for equivalent datasets
- **Memory:** Maintains internal buffers for lookback periods
- **Latency:** Optimized for per-bar updates, typically O(1) or O(log n)
- **Thread safety:** Not thread-safe; synchronize external access if needed

## Usage patterns

### Batch addition with incremental updates

```csharp
SmaList smaList = new(20);

// add initial batch
smaList.Add(historicalBars);

// then add new bars incrementally, e.g. from a polling loop
Bar? newBar = await GetNextBarAsync();
while (newBar is not null)
{
    smaList.Add(newBar);
    ProcessLatestResult(smaList[^1]);

    newBar = await GetNextBarAsync();
}
```

## WebSocket/SSE integration

A live WebSocket or SSE feed delivers bars one message at a time, not as a collection you can enumerate up front. Only add through a buffer list when the feed guarantees chronological order — see the [order warning](#basic-usage) above; an out-of-order feed needs a [Stream hub](/guide/styles/stream) instead.

A callback-based client can invoke its event handler concurrently, and a lock around `Add` only prevents corruption — it does not restore arrival order once two callbacks race for it, and a buffer list mis-computes silently on an out-of-order `Add`. Drive `Add` from a single sequential receive loop instead, so only one bar is ever in flight:

```csharp
SmaList smaList = new(20);
using ClientWebSocket socket = new();
await socket.ConnectAsync(feedUri, CancellationToken.None);

byte[] buffer = new byte[4096];

while (socket.State == WebSocketState.Open)
{
    WebSocketReceiveResult result = await socket.ReceiveAsync(buffer, CancellationToken.None);
    if (result.MessageType == WebSocketMessageType.Close)
    {
        break;
    }

    // assumes each bar message fits in one frame; accumulate until
    // result.EndOfMessage if your feed can fragment a single bar
    WebSocketBar wsBar = ParseBar(buffer.AsSpan(0, result.Count));

    Bar bar = new(wsBar.Timestamp, wsBar.Open, wsBar.High, wsBar.Low, wsBar.Close, wsBar.Volume);
    smaList.Add(bar);

    SmaResult latest = smaList[^1];
    Console.WriteLine($"{latest.Timestamp:d}: SMA = {latest.Sma:N2}");
}
```

`ReceiveAsync` returns one message at a time, so `Add` and every read of `smaList` happen on this one loop — no lock needed. If your client library only exposes a callback rather than a receive loop you control, have the callback enqueue onto a single-consumer `Channel<Bar>`, and do the `Add` and every read from the one loop that drains it, instead of locking around `Add` in the callback.

## See also

- [Batch style](/guide/styles/batch) for one-time calculations
- [Stream hubs](/guide/styles/stream) for coordinated real-time updates
- [Indicators](/indicators) for available buffer list indicators
