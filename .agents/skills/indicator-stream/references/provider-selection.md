# Provider selection

Use this when choosing a hub's base class, or when the hub does not emit exactly one final result per input item.

## Base classes

| Base class | Type constraint | Downstream use | Examples |
| ---------- | --------------- | -------------- | -------- |
| `ChainHub<IReusable, TResult>` | `TResult : IReusable` | Chainable | `EmaHub`, `SmaHub`, `RsiHub`, `MacdHub` |
| `ChainHub<IBar, TResult>` | `TResult : IReusable` | Chainable | `AdxHub`, `AtrHub`, `CciHub`, `ObvHub` |
| `StreamHub<IBar, TResult>` | `TResult : ISeries` | Terminal | `DonchianHub`, `KeltnerHub`, `VortexHub`, `IchimokuHub` |
| `StreamHub<IReusable, TResult>` | `TResult : ISeries` | Terminal | `AlligatorHub`, `MaEnvelopesHub` |
| `BarProvider<IBar, TResult>` | `TResult : IBar` | Bar source for other hubs | `HeikinAshiHub`, `RenkoHub` |

- Choose `ChainHub` when the result implements `IReusable` (it has one meaningful `Value`), so other hubs can chain from it.
- Choose `StreamHub` directly only when the result is a plain `ISeries` with several bands or lines and no single `Value`.
- Choose `BarProvider` when the result is itself a bar (its result record derives from `Bar`), so bar-input hubs can subscribe to it.
- A hub whose input is another hub's typed result (`StcHub : ChainHub<MacdResult, StcResult>`, `GatorHub : StreamHub<AlligatorResult, GatorResult>`) is a compound hub; follow [compound hubs](compound-hubs.md).

## Test interfaces

| Hub input | Observer interface | Add `ITestChainProvider` when |
| --------- | ------------------ | ----------------------------- |
| `IReusable` (or another hub's chainable result) | `ITestChainObserver` | base is `ChainHub` |
| `IBar` | `ITestBarObserver` | base is `ChainHub` or `BarProvider` |

The testing-standards skill owns the methods each interface requires.

## Hubs that are not one-result-per-input

The default `OnAdd` calls `ToIndicator` once and appends one result. Override `OnAdd` (taking `CacheLock` yourself, as the base does) when that does not hold:

- **Lookahead results** (`FractalHub`, `DpoHub`, `PivotsHub`): a result becomes calculable only after later items arrive. Call `base.OnAdd`, then recompute the earlier position and notify with `NotifyObserversOnRebuild`, or call `Rebuild` from that timestamp.
- **Zero or many outputs per input** (`RenkoHub`): emit through `AppendCache` from `OnAdd`, override `Properties` with bit 1 set (`new(0b00000010)`) so same-timestamp results append instead of triggering a rebuild, and override `ShouldPruneOnProviderPrune => false` because result timestamps do not align with provider pruning. Make `ToIndicator` throw; it is unused.

A hub that keeps position-based state (absolute indexes, item counters) overrides `PruneState(DateTime)` to adjust it when the cache front is pruned; `EpmaHub` is the reference. A hub that opts out of pruning overrides `OnProviderPrune(DateTime)` instead.

## Aggregator hubs

`BarAggregatorHub` (`src/Common/Bars/`) and `TradeTickAggregatorHub` (`src/Common/TradeTicks/`) bucket bars or ticks into larger periods and derive from `BarProvider<TIn, IBar>`. Extend this pattern for a new quantizer rather than writing bespoke bucketing:

- Offer a `BarInterval` constructor and a `TimeSpan` constructor. The `BarInterval` constructor throws for `BarInterval.Month`, which has no fixed `TimeSpan`.
- Take an optional `fillGaps` flag, default `false` (empty buckets are omitted). When `true`, synthesize zero-volume bars whose open, high, low, and close all carry the prior bar's close.
- In `OnAdd`, round the input timestamp down to its bucket, then update the forming bar in place (replace `Cache[^1]` and notify with `NotifyObserversOnRebuild`) or append a new bucket. Call `Rebuild` for input that lands in an earlier bucket.
- Override `Rebuild(DateTime)` to round the timestamp down to the bucket boundary before calling `base.Rebuild`. Without it, a mid-bucket rebuild keeps the partial bar and the replay appends a duplicate.
- In `RollbackState`, clear the forming bar. When `restoreIndex < 0`, clear every duplicate-detection tracker entry; otherwise remove entries later than `ProviderCache[restoreIndex].Timestamp`.

## Self-rooted source hubs

`BarHub` and `TradeTickHub` originate a stream and pass an inert `BaseProvider<T>` to the base constructor. Indicator hubs never use this; do not add new `BaseProvider<T>` derivations without asking.
