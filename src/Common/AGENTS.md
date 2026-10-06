# Common framework

This folder holds the streaming framework (`StreamHub/`, `BufferLists/`), the catalog (`Catalog/`), core types (`Bars/`, `TradeTicks/`, `Reusable/`), and shared utilities. [README.md](README.md) is the folder overview; this file holds the framework invariants that a change here must preserve.

| Working on | Load |
| ---------- | ---- |
| `StreamHub/`, `Bars/`, `TradeTicks/`, any `{Name}Hub.cs` | indicator-stream skill |
| `BufferLists/`, any `{Name}List.cs` | indicator-buffer skill |
| `Catalog/`, any `{Name}.Catalog.cs` | indicator-catalog skill |

## Self-rooted source hubs

`BarHub` and `TradeTickHub` originate a stream, so they bootstrap their base class with the internal `BaseProvider<T>` sentinel (`StreamHub/Providers/BaseProvider.cs`).

- The sentinel has an empty `Results`, throws on `Subscribe` and `EndTransmission`, and no-ops on `Unsubscribe`.
- Its `Properties` set bit 0 (not an observer) with mask `0b11111110`, so hubs downstream of it still observe.
- It is a stopgap until a dedicated `StreamSource<T>` root exists; do not derive anything else from it.

## Aggregator hubs

`BarAggregatorHub` and `TradeTickAggregatorHub` quantize bars or ticks into larger periods.

- Both derive from `BarProvider<TIn, IBar>` and take a `BarInterval` or `TimeSpan` plus an optional `fillGaps` flag or a `GapFillMode` (`None`, `ForwardFill`, `Interpolate`).
- They reject `BarInterval.Month`; a custom period uses the `TimeSpan` overload.
- They override `Rebuild(DateTime)` and `RollbackState(int)`: a gap-fill rebuild seeds the replay from the last kept bar, so the replay fills the silent buckets between it and the first replayed input.
- Under a gap mode the hub tracks synthesized runs in a `PruningList`. A rebuild that removes the input a run ends on rewinds to the last real bar before the run, so no trailing gap bars remain. `Interpolate` synthesizes gap bars from the input after them, so a rebuild inside a gap, or one that changes the anchor the gap ends on, rewinds as well.

Extend this pattern for a new quantizer instead of writing bespoke bucketing.

## Thread safety

`StreamHub<TIn, TOut>` (`StreamHub/StreamHub.cs`) holds the private `CacheLock` monitor for every cache mutation.

- Rebuild, `RemoveAt`, and provider-prune handling notify observers inside `CacheLock`, so nothing is added between a cache change and its notification. Never release the lock before notifying.
- While `Rebuild` replays provider items, `_isRebuilding` forces `Act.Add` in `AppendCache` instead of a recursive rebuild; observer cascading still happens. Never bypass the flag.
- `Results` is a live read-only view of the cache, not a snapshot: enumerating it during a concurrent add throws `InvalidOperationException`. Consumers that iterate while upstream may emit call `Snapshot()`, which copies the cache under the lock.
- `ToIndicator` runs under the lock (via `OnAdd`), so reading `Cache[i - 1]` there is safe.

## `RollbackState(int restoreIndex)` contract

The base computes `restoreIndex` as the last `ProviderCache` index to keep (`IndexGte(timestamp) - 1`, or -1 to reset all state), then calls `RollbackState`.

- It runs **before** the base removes result-cache entries past `restoreIndex` and before the replay re-emits items from `restoreIndex + 1`.
- An override may read `ProviderCache[0..restoreIndex]` to rebuild state; it never re-emits or mutates the result cache.
- Every hub with state beyond the cache overrides it.

## BufferList

`BufferList<TResult>` (`BufferLists/BufferList.cs`) is a standalone `IReadOnlyList` for synchronous incremental compute; `MaxListSize` prunes it for long-running use. `IIncrementFromChain` adds `Add(DateTime, double)` and `Add(IReusable)` overloads for chainable single-value indicators; `IIncrementFromBar` adds `Add(IBar)` for indicators that need OHLCV.

## Catalog

`PopulateCatalog()` in `Catalog/Catalog.Listings.cs` registers every listing into the private `_listings` field. Two tests in `tests/Library/Common/Catalog/` pin the result:

- `Catalog.Metrics.Tests.cs` asserts the exact per-style listing counts; update it whenever a listing is added or removed.
- `Catalog.Shape.Tests.cs` compares the catalog against `tests/Library/TestData/catalog/shape.snapshot.txt`; regenerate it with `UPDATE_CATALOG_SHAPE=1` set when listings change deliberately.

## Boundaries

✅ Always override `RollbackState(int)` when a hub keeps state outside its cache

⚠️ Ask before adding a `#pragma warning disable` here. The only one in the streaming framework, `CA1031, RCS1075` around observer notification in `StreamHub/StreamHub.Observable.cs`, is the observer-isolation catch-all; keep `BufferLists/` pragma-free

🚫 Never mutate `Cache` from a subclass except through `AppendCache`, `RemoveRange`, or `RemoveAt` — and only a root hub accepts direct `Add`, `RemoveAt`, `RemoveRange`, or `Reinitialize`; subscribed hubs throw

🚫 Never derive from `BaseProvider<T>` beyond `BarHub` and `TradeTickHub`
