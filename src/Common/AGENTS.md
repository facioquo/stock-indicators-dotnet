# Common framework code

## Purpose of this file

This file (AGENTS.md) carries the **operational guidance for AI agents and contributors** editing files in `src/_common/`: framework-level invariants, repo-specific file locations, registration conventions, and boundaries. Its companion [README.md](README.md) carries the **public directory inventory and per-file descriptions** for anyone browsing the source on GitHub. The two files are deliberately non-overlapping — keep behavioral rules here and descriptive inventory there.

This folder holds the streaming framework (`StreamHub/`, `BufferLists/`), the catalog system (`Catalog/`), core types (`Bars/`, `TradeTicks/`, `Reusable/`, `BarPart/`), and shared utilities.

Stateful changes here (cache, rollback, pruning, notification) must preserve the framework invariants documented below. Load the relevant skill first for the portable patterns.

## Skills to load

| Working on | Load |
| ---------- | ---- |
| `StreamHub/`, `Bars/Bar.StreamHub.cs`, `Bars/*.AggregatorHub.cs`, any `**/*.StreamHub.cs` | `#skill:indicator-stream` |
| `BufferLists/`, any `**/*.BufferList.cs` | `#skill:indicator-buffer` |
| `Catalog/`, any `**/*.Catalog.cs` | `#skill:indicator-catalog` |
| `Bars/`, new aggregator hubs | `#skill:indicator-stream` + sections below |

Skills carry the portable patterns. The sections below carry the repository-specific specifics the skills intentionally do not duplicate.

## Streaming framework specifics

### Self-rooted source hubs

Hubs that originate a stream (no upstream provider) bootstrap their base class with an inert `BaseProvider<T>` sentinel. The sentinel:

- Lives at `src/_common/StreamHub/Providers/BaseProvider.cs`
- Carries no cache (`Results` is `Array.Empty<T>().AsReadOnly()`)
- Throws on `Subscribe(...)` and is a no-op on `Unsubscribe`/`EndTransmission`
- Masks `Properties` bit 0 (`0b11111110`) so downstream hubs become proper observers even though the sentinel itself is not

Canonical examples:

- `src/_common/Bars/Bar.StreamHub.cs` — `BarHub` (default IBar source)
- `src/_common/TradeTicks/TradeTick.StreamHub.cs` — `TradeTickHub` (default ITradeTick source)

`BaseProvider<T>` is acknowledged in its source comments as a workaround pending a cleaner `StreamSource<T>` root class. Do not extend `BaseProvider<T>` beyond the existing self-rooted sources.

### Aggregator hubs

`Bar.AggregatorHub.cs` and `TradeTick.AggregatorHub.cs` quantize incoming bars/ticks into larger time periods. They:

- Derive from `BarProvider<TIn, IBar>`
- Accept a `BarInterval` (or raw `TimeSpan`) plus an optional `fillGaps` flag
- Emit closed bars at period boundaries; reject `BarInterval.Month` (use `TimeSpan` overload for custom periods)
- Inherit the standard `RollbackState(int)` semantics so out-of-order ticks reconstruct correctly

When implementing a new quantizer, prefer extending the aggregator pattern over writing bespoke bucketing.

### Thread safety contract

`StreamHub<TIn, TOut>` (`src/_common/StreamHub/StreamHub.cs`) is thread-safe by holding `CacheLock` (a private `object` monitor) for the duration of every cache-mutating operation. Two invariants matter when subclassing:

1. **Observer notification happens inside `CacheLock`.** `Rebuild` and `RemoveAt` call `NotifyObserversOnRebuild` / `NotifyObserversOnPrune` inside the lock specifically to prevent new items from being added between cache mutation and downstream notification. Subclasses must not release the lock before notifying.
2. **The `_isRebuilding` flag suppresses self-rebuild during `Rebuild`.** While `Rebuild` is replaying provider items through `OnAdd`/`AppendCache`, the flag forces `Act.Add` instead of recursing into another `Rebuild`. Observer cascading is still allowed and desired. Do not bypass this flag from subclass code.

`Results` returns `Cache.AsReadOnly()` — a **live read-only view**, not an immutable snapshot. The view forbids mutation (closing #1585's deviant-mutation hole) but enumeration during a concurrent `Add` will throw `InvalidOperationException`. Consumers iterating while upstream may emit should call `Snapshot()` first. Subclass code accessing `Cache[i-1]` directly is safe because it executes inside `ToIndicator`, which holds the lock transitively via `OnAdd`.

`Snapshot()` (on `StreamHub<TIn, TOut>`, surfaced via `IStreamObservable<T>.Snapshot()`) returns an immutable copy taken under `CacheLock`.

### `RollbackState(int restoreIndex)` index contract

Implemented by the stateful hubs. The base contract is:

- The base class computes `restoreIndex` via `IndexBefore` **before** calling `RollbackState`
- `restoreIndex` is the last `ProviderCache` index to **preserve**, or `-1` to reset all state
- Existing cache entries at `[restoreIndex + 1, Count)` have already been removed before this method is invoked
- The item at the rollback timestamp will be recalculated via normal `ToIndicator` processing — do not re-emit it from `RollbackState`

When adding new hubs, follow the canonical `RollbackState` pattern in `src/_common/StreamHub/StreamHub.cs` and the examples in `references/rollback-patterns.md` of the indicator-stream skill.

## BufferList framework specifics

`BufferList<TResult>` (`src/_common/BufferLists/BufferList.cs`) is a standalone `IReadOnlyList` for synchronous incremental compute. `MaxListSize` enables pruning when long-running. Two interfaces drive incremental adds:

- `IIncrementFromChain` — `Add(DateTime, double)`, `Add(IReusable)`, `Add(IReadOnlyList<IReusable>)` — for chainable single-value indicators
- `IIncrementFromBar` — `Add(IBar)`, `Add(IReadOnlyList<IBar>)` — for indicators requiring full OHLCV

The implementation uses the C# `field` keyword in `BufferList.cs`, which is currently why `<EnablePreviewFeatures>true</EnablePreviewFeatures>` remains in `src/Indicators.csproj`.

## Catalog framework specifics

`PopulateCatalog()` in `src/_common/Catalog/Catalog.Listings.cs` registers all indicator listings. Convention enforced by the existing file:

- Indicators grouped alphabetically by full name
- Each indicator block has a comment header `// {ABBR} ({Full Name})`
- Within each block: **Buffer → Series → Stream** registration order
- Blank line between indicator blocks

Backing field in this repository is `_listings` (private static `List<IndicatorListing>`). The catalog test `tests/indicators/_common/Catalog/Catalog.Metrics.Tests.cs` asserts the exact per-style listing counts — update it when adding or removing a listing.

## NaN handling policy

See the parent [src/AGENTS.md](../AGENTS.md#nan-handling-policy) for the canonical policy. In this folder specifically: rolling-window utilities (`CircularDoubleBuffer`, `RollingWindowMax/Min`) accept NaN values and return NaN for Min/Max when NaN is present in the window.

## Boundaries

✅ Always preserve the `CacheLock` / `_isRebuilding` invariants when subclassing `StreamHub`

✅ Always provide a `RollbackState(int)` override when adding stateful fields to a hub

✅ Always register new indicators in `Catalog.Listings.cs` in Buffer → Series → Stream order

⚠️ Ask before adding new derivations of `BaseProvider<T>` — the class is a documented workaround; current usage is limited to `BarHub` and `TradeTickHub`

⚠️ Ask before adding `#pragma warning disable` directives in this folder — existing suppressions are deliberate and tightly scoped (the load-bearing one is `CA1031, RCS1075` around the observer-isolation notification region in `StreamHub/StreamHub.Observable.cs`, where catching the general `Exception` and the deliberate empty catch are required for the observer-isolation boundary). New pragmas require explicit justification; prefer fixing the underlying issue, and keep `BufferLists/` pragma-free

🚫 Never expose `Cache` mutation from a subclass — go through `AppendCache`, `RemoveRange`, or `RemoveAt`

🚫 Never release `CacheLock` before notifying observers in `Rebuild` or `RemoveAt`
