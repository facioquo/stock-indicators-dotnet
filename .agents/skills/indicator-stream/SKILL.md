---
name: indicator-stream
description: Implement StreamHub real-time indicators with O(1) performance. Use for ChainHub or BarProvider implementations. Covers provider selection, RollbackState patterns, performance anti-patterns, and comprehensive testing with StreamHubTestBase.
---

# StreamHub indicator development

## Provider selection

| Provider Base | Input | Output | Use Case |
| ------------- | ----- | ------ | -------- |
| `ChainHub<IReusable, TResult>` | Single value | IReusable | Chainable indicators |
| `ChainHub<IBar, TResult>` | OHLCV | IReusable | Bar-driven, chainable output |
| `BarProvider<IBar, TResult>` | OHLCV | IBar | Bar-to-bar transformation |
| `BarProvider<TIn, TOut>` (self-rooted) | None | TOut | Source hubs with no upstream — bootstrap with an inert sentinel provider |
| `StreamHub<TProviderResult, TResult>` | Any hub result | Any result | Compound hubs (internal hub dependency) |

Self-rooted source hubs (those that originate a stream rather than transform another hub's output) take an inert sentinel provider so the base-class constructor has something to subscribe to; the sentinel rejects subscriptions and carries no cache.

## Aggregator / quantizer hubs

Hubs that bucket small bars (or raw ticks) into larger time periods derive from `BarProvider<TIn, IBar>`. Conventions:

- Constructors accept a `BarInterval` enum **and** a custom `TimeSpan` overload; the enum overload throws for month-or-longer periods (use `TimeSpan` instead) since calendar arithmetic is not a fixed `TimeSpan`.
- Take an optional `fillGaps` flag. Default `false` (silent buckets are simply omitted from the output stream); `true` synthesizes zero-volume bars whose `Open`/`High`/`Low`/`Close` all carry forward the prior bar's close through the silent period.
- Round the input timestamp down to the current bucket on every `OnAdd`, then either update the current bar in place or emit a new bucket.
- Override `Rebuild(DateTime)` to align the requested rebuild timestamp to the bucket boundary before delegating to base — an upstream rebuild whose timestamp is mid-bucket must clear the in-cache partial bar, not duplicate it.
- Implement `RollbackState(int)` to reset the in-flight bar state and prune any per-input tracker (e.g. duplicate-detection map) past the rollback point.

Aggregator hubs ship full StreamHub semantics: late-arriving inputs whose timestamp lands in an already-emitted bucket trigger a `Rebuild` of that bucket; downstream observers see the corrected sequence.

## Performance targets

Use the project's performance-analysis document as the source of truth for measured overhead bands; the categorical targets below are guidance, not contracts.

| Band | StreamHub overhead | Status |
| ---- | ------------------ | ------ |
| Target | ≤ 1.5x | ✅ meets target |
| Acceptable | 1.5x – 3x | ✅ acceptable |
| Review | 3x – framework floor | ⚠️ investigate |
| Critical | indicator-specific algorithmic issue (e.g. O(n²)) | 🔴 fix |

The "framework floor" is the per-tick overhead inherent to the observer pattern, cache management, and read-only collection wrappers. Simple stateless indicators routinely measure 6–11x against Series while still achieving tens of thousands of bars per second; this is acceptable. Optimization effort should target indicator-specific algorithmic issues, not the framework floor.

Forbid O(n²) recalculation — rebuild entire history on each tick:

```csharp
// WRONG
for (int k = 0; k <= i; k++) { subset.Add(cache[k]); }
var result = subset.ToIndicator();
```

O(1) incremental update:

```csharp
// CORRECT
_avgGain = ((_avgGain * (period - 1)) + gain) / period;
```

Use `RollingWindowMax/Min` utilities instead of O(n) linear scans.

## Thread safety contract

StreamHub mutating operations (`Add`, `Rebuild`, `RemoveRange`, `RemoveAt`) hold a private monitor for the duration of cache mutation, and observer notification happens **inside** the lock so subscribers cannot desynchronize. Subclasses must not release the lock before notifying observers.

The base class also carries a rebuilding flag that suppresses self-recursive `Rebuild` while replaying provider items through `OnAdd`. Observer cascading is still allowed and desired. Subclass code must not bypass this flag.

The public `Results` surface is a **live read-only view** over the cache, not an immutable snapshot — and `.ToList()`/`.ToArray()` on it enumerate that live view *without* the lock, so they can still throw or tear under a concurrent writer. A consumer on a different thread must call `Snapshot()` (an atomic, immutable copy taken under the hub's `CacheLock`) instead.

## RollbackState pattern

Override when maintaining stateful fields.
The base class computes `restoreIndex` via `IndexBefore` before calling this method.
`restoreIndex` is the last `ProviderCache` index to preserve, or `-1` to reset everything.

```csharp
protected override void RollbackState(int restoreIndex)
{
    _window.Clear();
    if (restoreIndex < 0) return;
    int startIdx = Math.Max(0, restoreIndex + 1 - LookbackPeriods);
    for (int p = startIdx; p <= restoreIndex; p++)
        _window.Add(ProviderCache[p].Value);
}
```

Replay up to `restoreIndex` (inclusive). The item at the rollback timestamp is recalculated via normal processing.

## Testing requirements

- Inherit `StreamHubTestBase`
- Abstract method (compile error if missing): `ToStringOverride_ReturnsExpectedName()`
- Implement ONE observer interface:
  - `ITestChainObserver` (most indicators — chain input): inherits `ITestBarObserver`, adds `ChainObserver_ChainedProvider_MatchesSeriesExactly()`
  - `ITestBarObserver` (direct bar input only): `BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()`, `WithCachePruning_MatchesSeriesExactly()`
- If hub acts as chain provider, also implement `ITestChainProvider`: `ChainProvider_MatchesSeriesExactly()`

## Required implementation

- [ ] Source code: `src/**/{IndicatorName}Hub.cs` file exists
  - [ ] Uses appropriate provider base (ChainHub or BarProvider)
  - [ ] Validates parameters in constructor; calls Reinitialize() as needed
  - [ ] Implements O(1) state updates; avoids O(n²) recalculation
  - [ ] Overrides RollbackState() when maintaining stateful fields
  - [ ] Overrides ToString() with concise hub name
- [ ] Unit testing: `tests/Indicators/Indicators/**/{IndicatorName}HubTests.cs` exists
  - [ ] Inherits StreamHubTestBase with correct test interfaces
  - [ ] Comprehensive rollback validation present
  - [ ] Verifies Series parity
- [ ] **Catalog registration**: Registered in `Catalog.Listings.cs`
- [ ] **Performance benchmark**: Add to `tools/performance/Perf.Stream.cs`
- [ ] **Public documentation**: Update `docs/indicators/{IndicatorName}.md`
- [ ] **Regression tests**: Add to `tests/Indicators/Indicators/**/{IndicatorName}RegressionTests.cs`
- [ ] **Migration guide**: Update `docs/migration/v3.md` for notable and breaking changes from v2

## References

- [Provider selection](references/provider-selection.md)
- [Rollback patterns](references/rollback-patterns.md)
- [Performance patterns](references/performance-patterns.md)
- [Compound hubs](references/compound-hubs.md)

## Constraints

- O(n²) recalculation is forbidden; all updates must be O(1)
- `RollbackState(int restoreIndex)` receives the last index to preserve (`-1` = reset all); replay is inclusive of `restoreIndex`, exclusive of the rollback timestamp
- Series parity required: results must be numerically identical to StaticSeries
