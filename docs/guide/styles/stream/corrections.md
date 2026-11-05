---
title: Stream hub corrections and memory
description: Correct, remove, and refuse bars in stream hubs, size the cache, and recover from faults
---

# Corrections and memory

Stream hubs recalculate automatically when bars arrive late, are revised, or are removed. This page covers those corrections in detail, along with cache sizing and fault recovery. For the basics, start with [Stream hubs](/guide/styles/stream).

## Correct bars

Make every correction on the root hub, the `BarHub` (or `TradeTickHub`) you add bars to. Every hub subscribed to it recalculates from the changed bar forward. Calling these methods on a subscribed hub, such as an `SmaHub`, throws `InvalidOperationException`.

```csharp
BarHub barHub = new();
SmaHub smaHub = barHub.ToSmaHub(20);

// revise a bar: add one with the same timestamp and new values
barHub.Add(correctedBar);

// insert a late bar: add it, and it is placed by its timestamp
barHub.Add(lateBar);

// remove the bar with this bar's timestamp
barHub.Remove(badBar);

// remove every bar at or after a timestamp
barHub.RemoveRange(badBar.Timestamp, notify: true);
```

- `Remove(bar)` matches by `Timestamp` only, so a bar carrying just the timestamp is enough. It throws `ArgumentException` when no bar has that timestamp.
- `RemoveRange(timestamp, notify)` removes the bar at that timestamp and every later one. Pass `notify: true` so the subscribed hubs remove their matching results.
- `RemoveAt(index)` removes the bar at a position in `Results`.

## Refused bars

The hub accepts a late bar only while it still holds the history around it. It refuses a bar older than every bar it holds when either of these is true:

- The bar falls in history the hub has already dropped to stay within its cache size (`PrunedHistory`).
- The cache is full (`CacheFull`).

When both are true, the reason is `PrunedHistory`. `BarHub` raises `BarRejected` for each refused bar and leaves its results unchanged:

```csharp
barHub.BarRejected += (_, e) =>
    Console.WriteLine($"Refused bar at {e.Bar.Timestamp:u}: {e.Reason}");
```

The handler runs while the hub is mid-update, so keep it fast. An exception from the handler is thrown from `Add`, and stops a batch `Add` at the refused bar.

## Cache size and pruning

Each hub keeps up to 100,000 results by default. When the cache is full, the oldest result is dropped for each new one. Set `maxCacheSize` on the bar hub; every hub subscribed to it, directly or through a chain, uses the same size:

```csharp
BarHub barHub = new(maxCacheSize: 1_000);
RsiHub rsiHub = barHub.ToRsiHub(14);
```

Choose a size that covers:

- **The deepest warmup in the chain.** A hub throws `ArgumentOutOfRangeException` when it is created if the cache is smaller than its warmup, and the message states the size it needs. Once your hubs exist, read `MinCacheSize` on the bar hub for the size the whole chain needs; for example, RSI(14) needs 28 and TEMA(20) needs 60.
- **The history you read.** If you only read the latest values, a smaller cache saves memory.
- **The history you might correct.** See the warning below.

::: warning 🚩 Corrections beyond the cache are approximate
When a correction or late bar reaches back to the oldest bars the hub still holds, the hub can't see the history it already dropped, so its recalculated results differ from a hub that received the same bars in order:

- **Lookback indicators**, such as SMA, lose their oldest results to `null` because those bars no longer have enough history. For SMA, the remaining values stay exact.
- **Smoothed indicators**, such as EMA, and Wilder's RSI, ATR, ADX, and SMMA, restart their smoothing from the bars that remain. Their values drift, then gradually converge back to the correct values. RSI and ADX restore their exact state for a correction to one of the newest 31 bars. Once the hub has dropped any bars, a correction older than that recalculates their starting point from the bars that remain and drifts, even when it is far from the oldest bar.
- **Cumulative indicators**, such as OBV and ADL, restart their running total. Every later result carries a permanent offset.

To avoid this, keep the cache larger than the history you may need to correct; for RSI and ADX corrections older than the newest 31 bars, that means a cache that never fills. With enough cache, a recalculation matches an in-order hub exactly.
:::

## Faults

A hub guards against runaway loops, such as a circular chain or a source stuck re-sending the same bar. If a bar with the **same timestamp and identical values** is added more than 100 times in a row, the hub faults:

- `IsFaulted` becomes `true`.
- Subscribed hubs and observers receive `OnError` with an `OverflowException`.
- The `Add` call that crossed the limit throws `OverflowException`.

```csharp
try
{
    barHub.Add(bar);
}
catch (OverflowException)
{
    // the same bar was re-sent more than 100 times in a row
}

if (barHub.IsFaulted)
{
    // clear the fault and resume streaming
    barHub.ResetFault();
}
```

Only identical repeats count. A correction, with the same timestamp and **different** values, is a normal revision. If your source legitimately repeats identical bars, remove the duplicates before calling `Add`.

## Reset a hub

`Reinitialize()` on the root hub clears its fault and recalculates every subscribed hub from the bars the root hub holds. Like `Add`, call it from the thread that adds bars, and not while a [feed](/guide/styles/stream/threading#feeds) is open.

## See also

- [Stream hubs](/guide/styles/stream) for the basics
- [Threading and live feeds](/guide/styles/stream/threading) for multi-threaded feeds and reading results across threads
- [Custom observers](/guide/custom-observers) for reacting to recalculations with `OnRebuild` and `OnPrune`
