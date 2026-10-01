# Common utilities and patterns

Shared types, base classes, and utilities that every indicator implementation builds on.

- `Bars/` - `IBar` OHLCV bar types, the default `BarHub` source hub, bar aggregation, and validation
- `BufferLists/` - Incremental buffer-based indicator processing (`BufferList` base class and add interfaces)
- `Candles/` - Candlestick primitives and pattern utilities
- `Catalog/` - Indicator metadata and discovery (see [Catalog/README.md](Catalog/README.md))
- `Enums/` - Shared enumerations
- `Exceptions/` - Custom exceptions
- `Math/` - Numerical utilities (deterministic double-precision math, null-safe wrappers, and rounding/statistics helpers)
- `Pruning/` - Cache-trimming helpers and the internal self-pruning list
- `Reusable/` - Core `ISeries` / `IReusable` interfaces, `TimeValue`, and chaining extensions
- `SeekSort/` - Index-seeking and sorting extensions
- `StreamHub/` - Real-time streaming indicator base classes and utilities
- `StringFormatters/` - String output formatting utilities
- `TradeTicks/` - Trade-tick types, the default `TradeTickHub` source hub, and tick-to-bar aggregation

Framework invariants (thread safety, `RollbackState` semantics, catalog registration) live in the companion [AGENTS.md](AGENTS.md). The NaN handling policy that every calculation follows lives in [src/AGENTS.md](../AGENTS.md#nan-handling).

## Related guides

- Indicator implementation patterns for each style: the indicator-series, indicator-buffer, and indicator-stream skills in [.agents/skills/](../../.agents/skills/)
- Benchmarks: [tools/performance/benchmarking.md](../../tools/performance/benchmarking.md) for running benchmarks and spot checks, and [tools/performance/baselines/README.md](../../tools/performance/baselines/README.md) for baseline conventions
- Project principles: [docs/PRINCIPLES.md](../../docs/PRINCIPLES.md)
