---
title: Custom observers (external integration)
description: Implement an observer of a streaming hub for UI dispatch, persistence, logging, or alerting without subclassing.
---

# Custom observers (external integration)

Subscribing a **custom observer** is the supported way to integrate with a hub's event stream. Use it to *react to a hub's output* — push updates into a UI, persist values to a database, ship them to a message bus, feed an alerting pipeline, or filter/transform results and re-emit them to downstream indicators.

## When to use an observer

| Goal | Pattern |
| ---- | ------- |
| Forward hub results to a UI thread | Subscribe a custom `IStreamObserver<T>` |
| Persist results to a database | Subscribe a custom `IStreamObserver<T>` |
| Log every value the hub emits | Subscribe a custom `IStreamObserver<T>` |
| Trigger external alerts on threshold crosses | Subscribe a custom `IStreamObserver<T>` |
| Filter/transform results and feed downstream chained indicators | Implement `IChainProvider<IReusable>` in addition to `IStreamObserver<T>` (see below) |

Subscribing does not modify the source hub. The hub keeps its cache, its rollback behavior, and its other subscribers. Your observer is a peer subscriber that receives the same notifications; if it throws, the hub isolates it so the other subscribers are unaffected (see [If an observer throws](#if-an-observer-throws)).

::: info Fully customizable stream hubs are not yet supported
Stream hub customization has not been implemented yet (see [#2097](https://github.com/facioquo/stock-indicators-dotnet/issues/2097)). Until then, this observer pattern is the supported, idiomatic way to thread custom processing into a streaming pipeline.
:::

## `IStreamObserver` interface

```csharp
public interface IStreamObserver<in T>
{
    bool IsSubscribed { get; }

    void Unsubscribe();
    void OnAdd(T item, bool notify, int? indexHint);
    void OnRebuild(DateTime fromTimestamp);
    void OnPrune(DateTime toTimestamp);
    void OnError(Exception exception);
    void OnCompleted();
}
```

The hub calls these methods on every subscribed observer in response to upstream activity:

- **`OnAdd(item, notify, indexHint)`** — A new result is being appended (or, for late-arriving data, an item is being inserted at `indexHint`). The `notify` flag indicates whether downstream cascading is desired; for most external integrations, ignore it and just process `item`.
- **`OnRebuild(fromTimestamp)`** — The hub is replaying its cache from `fromTimestamp` onward in response to a late arrival, a `Rebuild(...)` call, or a `RemoveAt(...)`. Observers that maintain derived state (e.g. a UI list view) typically clear their state from `fromTimestamp` and let subsequent `OnAdd` calls re-populate it.
- **`OnPrune(toTimestamp)`** — The hub's cache exceeded its `MaxCacheSize` and entries up to `toTimestamp` were dropped from the head. Observers persisting older results can use this signal to flush their own retention window.
- **`OnError(exception)`** — The hub entered a faulted state. Observers should surface this to the operator (log, alert, halt the pipeline) and decide whether to call `Unsubscribe()`.
- **`OnCompleted()`** — The provider declared it will send no more data. Finite streams only; live feeds typically never call this.

### If an observer throws

The hub **isolates** a faulting subscriber. If your callback throws from `OnAdd`, `OnRebuild`, or `OnPrune`, the hub catches the exception, routes it to *your* observer's `OnError`, and keeps notifying the other subscribers — one bad observer can neither starve its siblings nor surface as an exception out of the hub's `Add`. A throw from `OnError` itself is swallowed (it has nowhere left to go).

Even with that safety net, design your callbacks to be robust:

- Handle your own failures (I/O, parsing, downstream user callbacks) inside the method. Your `OnError` now fires for two reasons — an upstream provider fault (above) *or* one of your own callbacks throwing and being isolated — and both arrive as a plain `Exception`, so don't assume a single cause.
- Keep the hand-off fast and non-blocking — your callback runs inside the hub's notification path; do risky or slow work elsewhere (see [Thread-safety expectations](#thread-safety-expectations)).
- Surface failures through your own channel — log, alert, or a flag your writer checks.

## Minimal external observer

Subscribe an observer to any hub's `Results` provider via `Subscribe(observer)`:

```csharp
using FacioQuo.Stock.Indicators;

// observer that pushes every EMA value into a UI dispatcher
public sealed class EmaUiObserver : IStreamObserver<EmaResult>, IDisposable
{
    private readonly Action<EmaResult> _dispatch;
    private IDisposable? _subscription;

    public EmaUiObserver(IStreamObservable<EmaResult> source, Action<EmaResult> dispatch)
    {
        _dispatch = dispatch;
        _subscription = source.Subscribe(this);
    }

    public bool IsSubscribed => _subscription is not null;

    public void OnAdd(EmaResult item, bool notify, int? indexHint)
        => _dispatch(item);

    public void OnRebuild(DateTime fromTimestamp) { /* clear UI from fromTimestamp */ }
    public void OnPrune(DateTime toTimestamp)     { /* drop UI rows older than toTimestamp */ }
    public void OnError(Exception exception)      { /* surface to operator */ }
    public void OnCompleted()                     { /* finalize UI (no more data expected) */ }

    public void Unsubscribe()
    {
        _subscription?.Dispose();
        _subscription = null;
    }

    public void Dispose() => Unsubscribe();
}
```

Wire it up alongside the rest of the pipeline:

```csharp
BarHub barHub = new();
EmaHub emaHub = barHub.ToEmaHub(20);

using EmaUiObserver ui = new(emaHub, result => uiDispatcher.Post(result));

// call from your WebSocket or SSE message handler
void OnBarReceived(Bar bar) => barHub.Add(bar);
```

Every bar published to `barHub` cascades through `emaHub.OnAdd(...)`; the EMA result then notifies `ui.OnAdd(...)`, which posts to the UI thread. Disposing `ui` unsubscribes cleanly.

## Observer as a chain provider

If you want downstream indicators to chain off your observer's processed output (rather than the raw hub output), implement `IChainProvider<IReusable>` on the same type. The pattern is a thin box that re-emits each item through its own subscriber list:

```csharp
public sealed class FilteringChainProvider
    : IStreamObserver<EmaResult>, IChainProvider<IReusable>
{
    private readonly List<IStreamObserver<IReusable>> _subscribers = [];
    private readonly List<IReusable> _results = [];

    public BinarySettings Properties { get; } = new(0b00000000, 0b11111111);
    public IReadOnlyList<IReusable> Results => _results;
    public int MaxCacheSize => 100_000;
    public int ObserverCount => _subscribers.Count;
    public bool HasObservers => _subscribers.Count > 0;

    public bool HasSubscriber(IStreamObserver<IReusable> observer)
        => _subscribers.Contains(observer);

    public IDisposable Subscribe(IStreamObserver<IReusable> observer)
    {
        _subscribers.Add(observer);
        return new Unsubscriber(_subscribers, observer);
    }

    public bool Unsubscribe(IStreamObserver<IReusable> observer)
        => _subscribers.Remove(observer);

    public void EndTransmission()
    {
        foreach (var s in _subscribers) s.OnCompleted();
        _subscribers.Clear();
    }

    public bool IsSubscribed { get; private set; }
    public void Unsubscribe() => IsSubscribed = false;

    public void OnAdd(EmaResult item, bool notify, int? indexHint)
    {
        // filter, transform, decorate — then re-emit downstream
        if (item.Ema is null) return;

        var emitted = new TimeValue(item.Timestamp, item.Ema.Value);
        _results.Add(emitted);
        foreach (var s in _subscribers) s.OnAdd(emitted, notify, indexHint);
    }

    public void OnRebuild(DateTime fromTimestamp) { /* propagate */ }
    public void OnPrune(DateTime toTimestamp)     { /* propagate */ }
    public void OnError(Exception exception)      { /* propagate */ }
    public void OnCompleted()                     => EndTransmission();

    private sealed record Unsubscriber(List<IStreamObserver<IReusable>> List, IStreamObserver<IReusable> Observer)
        : IDisposable
    {
        public void Dispose() => List.Remove(Observer);
    }
}
```

This lets external code do:

```csharp
RsiHub rsiOfFiltered = filteringProvider.ToRsiHub(14);
```

— treating the custom observer as just another `IChainProvider<IReusable>` source. This is the pattern community contributors have used to thread custom processing into the standard chaining surface without subclassing a hub.

## Thread-safety expectations

Source hubs hold their internal cache monitor for the duration of `OnAdd` / `OnRebuild` / `OnPrune` callbacks. That means:

- **Your observer's callbacks run inside the source hub's lock.** Keep them fast and non-blocking. Posting to a UI dispatcher or enqueuing onto a background channel is fine; doing synchronous I/O (database writes, HTTP calls) is not.
- **Do not re-enter the source hub from a callback.** Calling `source.Add(...)` from inside `OnAdd` will deadlock.
- **Do not block the calling thread.** If you need to do heavy work, offload it to a `Task`, `Channel<T>`, or background worker and return immediately.

If you need to expose a `Results` collection from your observer (as in the `FilteringChainProvider` example above), apply your own synchronization or use a concurrent collection — the source hub does not lock *your* state.

## Lifecycle and resource cleanup

The `Subscribe(...)` method returns an `IDisposable`. Always hold the reference for the lifetime you want the subscription to last, and dispose it explicitly:

```csharp
IDisposable subscription = emaHub.Subscribe(myObserver);

// ... later ...
subscription.Dispose();      // or equivalently: emaHub.Unsubscribe(myObserver)
```

Failing to unsubscribe keeps your observer rooted from the source hub's subscriber list, preventing GC of both the observer and any state it holds.

## See also

- [Custom Series (batch) style indicators](/guide/customization) — when you want to invent your own indicators
- [Stream hubs](/guide/styles/stream) — the source-side streaming guide
- [Buffer lists](/guide/styles/buffer) — alternative when you don't need observable propagation
