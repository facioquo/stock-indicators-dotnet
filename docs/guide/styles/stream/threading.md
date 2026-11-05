---
title: Stream hub threading
description: Feed stream hubs from multi-threaded sources and read their results safely across threads
---

# Threading and live feeds

A stream hub accepts changes from one thread at a time. This page covers how to meet that rule when your data source calls back on several threads, and how to read results from other threads. For the basics, start with [Stream hubs](/guide/styles/stream).

::: warning 🚩 Data corruption risk
A hub is not safe for concurrent use. If two threads change the same hub at the same time, its results can be corrupted without any error. Every call that changes a hub, not only `Add`, must come from one thread at a time, and reads from other threads need a [snapshot](#reading-results).
:::

## The single-writer rule

These calls change a hub, and must come from one thread at a time:

- `Add`
- `Remove`, `RemoveAt`, and `RemoveRange`
- `Rebuild` and `Reinitialize`

Make them on the root hub, the `BarHub` (or `TradeTickHub`) you add bars to. It passes every change on to the hubs subscribed to it. Calling `Add`, `Remove`, `RemoveAt`, `RemoveRange`, or `Reinitialize` on a subscribed hub, such as an `SmaHub`, throws `InvalidOperationException`.

How you meet the rule depends on how bars reach your code:

| Bars arrive | Approach |
| ----------- | -------- |
| One at a time, such as from a receive loop | Call `Add` directly; nothing more is needed |
| From several threads at once | Put a [lock](#locking) around `Add` |
| From several threads, and writers must not wait while indicators calculate | Write to a [feed](#feeds) |

A two-series hub follows the rule across both of its providers: change both from the same single thread or feed. Providers that descend from one `BarHub` already do.

## Reading results

`Results` is a live view of the hub's cache. While another thread is adding bars, reading it can return a result set that is partway through an update. To read on a different thread from the writer, call `Snapshot()`, which returns a consistent copy:

```csharp
IReadOnlyList<RsiResult> results = rsiHub.Snapshot();
RsiResult? latest = results.Count > 0 ? results[^1] : null;
```

`Snapshot()` copies the whole cache, so with a large cache, call it when you need the results rather than on every bar. Reading `Results` on the same thread that adds bars needs no snapshot.

## Locking

A lock is the simplest way to accept bars from several threads. Each writer waits while the hub and its indicators update, and a reader can take the same lock to read `Results` consistently:

```csharp
BarHub barHub = new();
SmaHub smaHub = barHub.ToSmaHub(20);

object hubLock = new();

// message callbacks, which can run on several threads
void OnBarReceived(Bar bar)
{
    lock (hubLock)
    {
        barHub.Add(bar);
    }
}

// a reader on another thread
SmaResult? ReadLatest()
{
    lock (hubLock)
    {
        return smaHub.Results.Count > 0 ? smaHub.Results[^1] : null;
    }
}
```

A lock prevents corruption but does not restore arrival order when two callbacks race. A late bar is still placed by its timestamp, so the results stay correct.

## Feeds

A feed is an optional alternative to a lock. `CreateFeed()` on a root hub returns a `StreamFeed` that queues bars written from any thread and adds them to the hub one at a time, in the order they were queued. A write returns as soon as the bar is queued, so writers don't wait while indicators calculate, and the queue absorbs bursts:

```csharp
BarHub barHub = new();
RsiHub rsiHub = barHub.ToRsiHub(14);

await using StreamFeed<IBar> feed = barHub.CreateFeed(
    capacity: 1024,
    fullMode: StreamFeedFullMode.Wait,
    endTransmissionOnDispose: true);

// from any thread
await feed.WriteAsync(bar, cancellationToken);
```

| Parameter | Default | Description |
| --------- | ------- | ----------- |
| `capacity` | 1024 | Maximum items waiting in the queue. |
| `fullMode` | `Wait` | When the queue is full, `Wait` makes a write wait for space; `DropOldest` discards the oldest queued item, which loses data under load. |
| `endTransmissionOnDispose` | `false` | When `true`, disposing the feed calls `EndTransmission()` after the queue empties. |

How a feed behaves:

- `WriteAsync` completes when the item is queued, not when the hub has applied it. Its cancellation token cancels the wait for queue space.
- Results update on the feed's own task, not on the thread that wrote the item. Read them from other threads with `Snapshot()`.
- Only one feed can be open per hub. Dispose it before creating another, for example when a connection resets.
- While a feed is open, send every bar through it. Call `Remove`, `RemoveAt`, `RemoveRange`, `Rebuild`, and `Reinitialize` only after the feed is disposed, since they cannot wait in its queue.
- Disposing the feed stops new writes and applies the items already queued; that wait cannot be canceled. Don't dispose a feed from a [custom observer](/guide/custom-observers) callback, which runs on the feed's task and would wait on itself.

### Feed errors

If the hub throws while applying an item, the feed stops:

- Awaiting `Completion`, every later `WriteAsync`, and every `DisposeAsync` call throw the hub's exception.
- `DisposeAsync` does not call `EndTransmission()` after a failure.
- Inside an `await using` block, the exception from `DisposeAsync` replaces any exception already being thrown. Catch and record the original inside the block if you need both.

A bar the hub refuses with a [`BarRejected` event](/guide/styles/stream/corrections#refused-bars) is not an error and does not stop the feed.

## Callbacks

[Custom observer](/guide/custom-observers) callbacks and `BarRejected` handlers run while the hub is mid-update, on the thread that changed it. Keep them fast, don't throw from them, and don't call back into the hub from them.

## See also

- [Stream hubs](/guide/styles/stream) for the basics
- [Corrections and memory](/guide/styles/stream/corrections) for removing bars, refused bars, cache sizing, and fault recovery
- [Custom observers](/guide/custom-observers) for reacting to hub updates
