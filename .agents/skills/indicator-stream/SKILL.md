---
name: indicator-stream
description: Implement StreamHub real-time indicators — base-class choice (ChainHub, BarProvider, StreamHub), constructor and ToIndicator conventions, RollbackState overrides, per-tick performance rules, compound hubs, and the stream-specific test interfaces. Use when creating or editing a src/Indicators/**/{Name}Hub.cs file or its {Name}HubTests.cs, when a hub test fails Series parity after late arrival, removal, or pruning, or when a Stream benchmark shows a hub is slow.
---

# StreamHub indicator development

A hub computes one result per provider item, incrementally, and must match the Series result exactly — including after late arrivals, removals, and pruning. The indicator-series skill owns the full per-indicator file set and completion checklist; this skill covers only the hub.

Load the reference that matches the moment:

- Choosing a base class, or the hub is not one-result-per-input (aggregator, Renko-style) → [provider selection](references/provider-selection.md)
- Adding any stateful field, or a hub test fails after late arrival or removal → [rollback patterns](references/rollback-patterns.md)
- Writing `ToIndicator`, or a Stream benchmark is slow → [performance patterns](references/performance-patterns.md)
- The hub needs another hub's output as input (StochRSI on RSI, Gator on Alligator) → [compound hubs](references/compound-hubs.md)

## Choose a base class

| Input | Output | Base class | Example |
| ----- | ------ | ---------- | ------- |
| Single value | Chainable (`IReusable`) | `ChainHub<IReusable, TResult>` | `EmaHub`, `RsiHub` |
| OHLCV bar | Chainable (`IReusable`) | `ChainHub<IBar, TResult>` | `AdxHub`, `AtrHub` |
| OHLCV bar | Multi-value, not chainable | `StreamHub<IBar, TResult>` | `DonchianHub`, `KeltnerHub` |
| Single value | Multi-value, not chainable | `StreamHub<IReusable, TResult>` | `AlligatorHub` |
| OHLCV bar | Bar (`IBar`) | `BarProvider<IBar, TResult>` | `HeikinAshiHub`, `RenkoHub` |

`ChainHub` requires `TResult : IReusable`; `BarProvider` requires `TResult : IBar`. Only those two can feed downstream hubs.

## Hub shape

Follow `EmaHub` (`src/Indicators/e-j/Ema/EmaHub.cs`):

```csharp
public class EmaHub
    : ChainHub<IReusable, EmaResult>, IEma
{
    internal EmaHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods) : base(provider)
    {
        Ema.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        K = 2d / (lookbackPeriods + 1);
        Name = $"EMA({lookbackPeriods})";

        ValidateCacheSize(lookbackPeriods, Name);
        Reinitialize();
    }

    protected override (EmaResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);
        // compute from ProviderCache[i], Cache[i - 1], and incremental state
        // ...
        return (result, i);
    }
}
```

- The constructor is `internal`; the public entry point is a `To{Name}Hub(this IChainProvider<IReusable> …)` or `(this IBarProvider<IBar> …)` extension in the indicator's static partial class.
- Initialize every field `ToIndicator` reads before calling `Reinitialize()`, and call it last: it replays the provider's existing history through `ToIndicator`.
- Set `Name` in the constructor; the base `ToString()` returns it and the hub test asserts it.
- Call `ValidateCacheSize(warmupPeriods, Name)` with the indicator's warmup length so a too-small `MaxCacheSize` fails at construction.

## Per-tick cost

`ToIndicator` runs once per provider item. Its cost must not grow with history length: read `Cache[i - 1]` or keep incremental state, never re-run a Series method or loop back to index 0. A scan bounded by the lookback window is acceptable. The performance-testing skill owns the overhead targets and benchmark authoring.

## Rollback state

Any field that carries state between `ToIndicator` calls (buffers, running sums, smoothed averages, previous values) needs a `RollbackState(int restoreIndex)` override. `restoreIndex` is the last `ProviderCache` index to keep, or `-1` to reset everything; rebuild state as if items `[0..restoreIndex]` had just been processed. The base class owns the call order and locking (documented in `src/Common/AGENTS.md`).

```csharp
protected override void RollbackState(int restoreIndex)
{
    _window.Clear();
    if (restoreIndex < 0) { return; }

    int startIdx = Math.Max(0, restoreIndex + 1 - LookbackPeriods);
    for (int p = startIdx; p <= restoreIndex; p++)
    {
        _window.Add(ProviderCache[p].Value);
    }
}
```

A hub whose `ToIndicator` reads only `ProviderCache` and `Cache` (like `EmaHub`) needs no override.

## Tests

`{Name}HubTests` inherits `StreamHubTestBase`. Pick the observer interface by input type — `ITestChainObserver` for `IReusable` input, `ITestBarObserver` for `IBar` input — and add `ITestChainProvider` when the hub derives from `ChainHub` or `BarProvider`. The testing-standards skill owns the required methods and assertion patterns.

## Hub-specific completion items

- `src/Indicators/{range}/{Name}/{Name}Hub.cs` holds the hub class and its `To{Name}Hub` extension.
- Every stateful field is restored by `RollbackState` and, if it tracks absolute positions, adjusted by `PruneState`.
- `tests/Library/Indicators/{range}/{Name}/{Name}HubTests.cs` passes, including late-arrival, removal, and pruning parity.
- `tools/performance/Perf.Stream.cs` has a benchmark for the hub.

## Do not do these

- Do not recompute history inside `ToIndicator` (calling `.To{Name}()` on a subset, or looping from index 0). It turns streaming into O(n²).
- Do not detect rebuilds inside `ToIndicator` (for example by comparing `i` to a last-processed index). The base class calls `RollbackState` for every rollback.
- Do not re-emit results or touch `Cache` from `RollbackState`. The base class removes and replays results after it returns.
- Do not create a hub inside `ToIndicator`. Construct any internal hub once, in the constructor.
- Do not store a `CircularDoubleBuffer` in a `readonly` field. It is a mutable struct, so `Add` on a readonly field mutates a copy and the window never fills.
- Do not call `Add`, `RemoveAt`, `RemoveRange`, or a second `Reinitialize` on a subscribed hub. They throw on non-root hubs; drive changes through the root `BarHub` or `TradeTickHub`.
