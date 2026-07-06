---
status: accepted
date: 2026-05-26
deciders: Dave Skender (maintainer), AI agent contributors
consulted: streaming-indicators swarm review (2026-05-24) — Architect, Inspector, Tester, Stercorator, Researcher, Designer, Skills-auditor personas
informed: library consumers (via `docs/migration/v3.md` and per-indicator doc pages)
---

# Use dual-track BufferList and StreamHub for incremental indicators

## Context and problem statement

v3 adds incremental computation alongside the v2 batch Series API. Two distinct in-process incremental computation models ship side-by-side:

- **BufferList** — synchronous, pull-based, owns its in-memory list, callable from a tight `foreach` loop. Each indicator exposes a `*List` factory and a typed `*List : List<TResult>` class that derives from `BufferList<TResult>`.
- **StreamHub** — asynchronous, push-based, observer-chain composable, rollback-aware, fault-tracking, thread-safe. Each indicator exposes a `*Hub` factory and a typed `*Hub : ChainProvider<TIn, TOut>` (or `BarProvider`) class that derives from `StreamHub<TIn, TOut>`.

Both styles must produce results bit-equal to the canonical Series implementation for the same inputs. Why ship both? The question recurred in PR #1014 review threads, Discussion #1018, and every onboarding swarm review since the streaming framework landed. This ADR captures the rationale so future contributors can reason about the trade-offs without re-litigating them.

## Decision drivers

- **Performance**: pull-based incremental compute is roughly 1.5–2× faster than push-based for the same indicator on the same input window (measured against `tools/performance/baselines/`). The push model carries observer-notification and rollback-tracking overhead per emit.
- **Compositional clarity**: live observer chains need explicit `OnAdd` / `OnRebuild` / `OnPrune` / `OnError` semantics that don't fit a pull API. Multi-output cascades (Macd → Signal → Histogram, StochRsi inner RsiHub → outer StochRsiHub) compose naturally as observer chains.
- **Industry alignment**: comparable libraries pick one or the other; offering both lets the library compete in both spaces without sacrificing ergonomics in either. See [More information](#more-information).
- **Test-contract feasibility**: Series-parity is achievable for both styles when the underlying math is shared, but the test discipline differs — BufferList ↔ Series uses `IsExactly` bit-equality; StreamHub ↔ Series uses `IsExactly` plus rollback-equivalence contract tests.
- **Discoverability**: consumers must be able to see both options on every indicator's documentation page and pick the right one for their workload without expert knowledge.
- **Maintenance tax**: duplicating the indicator's math across two surfaces is the obvious cost. Mitigation via shared `*.Increment` kernels is in progress (~21% adoption today; target ≥80% — see [More information](#more-information)).

## Considered options

1. **Single unified streaming primitive** — ship only `StreamHub`. Represent batch warmup as a degenerate "feed everything synchronously" path.
2. **Single unified pull primitive** — ship only `BufferList`. Wrap it as an external `IObservable<T>` (Rx) adapter for push consumers.
3. **Dual-track BufferList + StreamHub** (chosen) — both, with shared `*.Increment` kernels for the per-step math and per-indicator surfaces that delegate to those kernels.
4. **Defer streaming to v4** — keep BufferList in v3.0 and re-litigate StreamHub later.

## Decision outcome

Chosen option: **3 — Dual-track BufferList + StreamHub**, because it is the only option that satisfies all three of (a) batch-warmup performance, (b) live observer-chain ergonomics, and (c) Series-parity test contract, without forcing consumers to wrap or unwrap data through an alien API for their primary use case.

### Consequences

- **Good**: consumers pick the surface that matches their workload — backtest replays use BufferList for raw speed; live trading dashboards use StreamHub for observer-chained UI updates and fault recovery; one-shot batch analytics use Series for the most direct API.
- **Good**: the StreamHub layer can ship advanced features (rollback, late-arrival, cache pruning, fault tracking, multi-output cascades) without dragging that complexity into the batch-warmup path that callers don't need.
- **Good**: Series remains the canonical oracle; both incremental surfaces are validated against it. A bug surfaces in exactly one place even when it affects all three.
- **Bad**: every streamable indicator pays for two implementations. ~78 indicators × 2 implementation files = ~156 files of per-indicator code. This is the maintenance tax.
- **Bad**: the discoverability surface doubles. Every indicator's doc page carries a "BufferList" section and a "StreamHub" section; every test class has a `*BufferListTests.cs` and `*HubTests.cs` sibling.
- **Bad**: contributors must understand both surfaces' invariants. The streaming swarm review surfaced that documenting `RollbackState(int)` semantics and `Results` live-view semantics required explicit ADR-adjacent work (DOC-ARCH-2, DOC-ARCH-3 in the streaming plan) to keep contributors aligned.

### Confirmation

The decision is confirmed correct in v3.0 if:

- All 78 streamable indicators ship working BufferList and StreamHub variants with Series-parity tests green.
- The shared `*.Increment` kernel pattern is preserved and extended (ARCH-V31-7 tracks ≥80% adoption in v3.1).
- No external consumer escalates a "why is there both" complaint that the migration guide cannot answer.

It is **reopened for reconsideration** in v3.1+ if:

- The shared kernel pattern reaches ≥80% adoption *and* a successful in-place refactor proves that one surface can be expressed as a thin shim over the other without measurable performance regression.
- An external library (Rx.NET, `System.Threading.Channels`, `IAsyncEnumerable<T>`) becomes a strictly-better composition target for the StreamHub layer (tracked as ARCH-V31-5 and ARCH-V31-6).

## Pros and cons of the options

### Option 1 — Single unified streaming primitive (StreamHub only)

- **Pro**: one mental model, one implementation per indicator, one set of tests.
- **Pro**: live composition (`hub.Use(...).ToOtherHub(...)`) becomes the canonical surface.
- **Con**: ~1.5–2× slowdown for batch warmup workloads. Consumers feeding a backtest engine ~40,000 bars/sec would see consistent overhead they cannot opt out of.
- **Con**: forces every consumer through observer-chain ceremony even when the workload is "fill a `List<T>` and walk it once". `foreach (var q in bars) hub.Add(q); var results = hub.Results;` is strictly worse than `var list = new EmaList(20); foreach (var q in bars) list.Add(q);` for the common case.
- **Con**: rollback and fault-tracking machinery is always live — non-zero per-emit cost even when the workload has no late arrivals.

### Option 2 — Single unified pull primitive (BufferList only) with Rx adapter

- **Pro**: one in-house mental model, one implementation per indicator. Live composition lives entirely in user-land via `Rx.NET`.
- **Pro**: no rollback/fault-tracking implementation cost in the library — that becomes a downstream user concern.
- **Con**: live consumers must implement their own observer pattern, late-arrival rollback, and cache-pruning every time. The library's primary value-add for live trading consumers evaporates.
- **Con**: a `*List` instance cannot meaningfully chain into another `*List` instance (compound indicators like Macd's signal-line or StochRsi's outer RSI need explicit re-feeding). Compositional ergonomics regress sharply.
- **Con**: the Rx adapter doesn't compose correctly across late arrivals — Rx has no native `OnRebuild` semantic. Out-of-order data quietly produces wrong results.

### Option 3 — Dual-track BufferList + StreamHub (chosen)

- **Pro**: each surface is optimized for its workload; consumers pick what fits without paying the other's cost.
- **Pro**: shared `*.Increment` kernels (`Ema.Increment`, `Sma.Increment`, `Tr.Increment`, `Atr.Increment`) absorb the math so the per-surface code is structural ceremony, not duplicated arithmetic. The pattern works; standardization is in progress.
- **Pro**: Series remains the single canonical oracle; both incremental surfaces validate against it with the same bit-equality test discipline.
- **Con**: 2× per-indicator file count; 2× per-indicator doc-page section count; 2× per-indicator test-class count.
- **Con**: contributors must learn both surfaces' invariants (currently captured in `.agents/skills/indicator-buffer/SKILL.md` and `.agents/skills/indicator-stream/SKILL.md`).

### Option 4 — Defer streaming to v4

- **Pro**: v3.0 ships faster with only the BufferList addition.
- **Con**: the streaming-API users on Discussion #1018 (the largest community feedback thread) have been waiting since 2021. Deferring to v4 would mean another 2+ years on top of v3's current timeline.
- **Con**: BufferList alone does not satisfy the live-feed workloads StreamHub targets. Shipping only half the incremental story would force live consumers to stay on v2 or implement their own observer layer.

## More information

### Prior art and industry comparison

Comparable in-process technical-analysis libraries pick one model or the other:

- **[ta4j](https://github.com/ta4j/ta4j) (Java)** — pull-only. Indicators are lazily-evaluated `Indicator<Num>` instances over a `BarSeries`. There is no native push/observer model. Live feeds are bolted on at the application layer.
- **[TA-Lib](https://ta-lib.org/) (C)** — batch-only. Indicators are pure functions over arrays; no incremental update API. Live feeds must recompute the full array per tick or maintain their own incremental state.
- **[Tulip Indicators](https://tulipindicators.org/) (C)** — batch-only. Same shape as TA-Lib; pure-function array transforms with no incremental surface.
- **[Pine Script v5](https://www.tradingview.com/pine-script-docs/v5/) (TradingView)** — bar-by-bar push. The Pine engine drives indicators with one bar at a time, exposing `barstate.isconfirmed` / `barstate.islast` for in-progress bar handling. Closest conceptual match to StreamHub's `OnAdd` semantics, but no equivalent of `OnRebuild` for late arrivals.
- **[QuantConnect / Lean](https://www.quantconnect.com/) (C#)** — push-style observer pattern (`IndicatorBase<T>.Update(...)`) without native rollback. Live composition is supported; late-arrival rollback is a user-implemented concern.

Stock Indicators for .NET v3 is the first widely-used library in this space to ship **both** an explicit pull surface (BufferList) and an explicit push surface (StreamHub with first-class late-arrival rollback). The dual-track model is the source of the library's competitive position in the live-feed space.

### Sources

- Streaming swarm review (2026-05-24) — Architect F5, Inspector F1, Researcher F3, Researcher F4 personas, captured in `docs/plans/streaming-indicators.plan.md`.
- Discussion #1018 — community feedback on state rollback (2021–2026).
- Performance baselines — `tools/performance/baselines/*.json` (dated 2026-02-28; refresh tracked as RG001 in the streaming plan).
- Increment-kernel adoption metric — ~34 of 160 streaming files (~21%) as of 2026-05-24; target ≥80% (ARCH-V31-7).

### Related decisions in the streaming plan

The following items in `docs/plans/streaming-indicators.plan.md` clarify or extend this decision without superseding it:

- DOC-ARCH-2 — `RollbackState(int)` index contract (clarified; v3.0).
- DOC-ARCH-3 — `Results` live-view semantics (clarified; v3.0).
- DOC-ARCH-6 — pragma footprint in `Common/` (recorded; v3.0).
- ARCH-V31-1 — retire `BaseProvider<T>`, introduce `StreamSource<T>` (refactor; v3.1).
- ARCH-V31-5 / ARCH-V31-6 — Rx.NET / `IAsyncEnumerable<T>` adapters (interop; v3.1).
- ARCH-V31-7 — standardize shared `*.Increment` kernels to ≥80% adoption (refactor; v3.1).

This ADR is the parent document those items hang off; if the dual-track decision is ever reversed, those items are invalidated together.
