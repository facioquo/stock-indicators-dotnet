# Common utilities and patterns

This directory contains shared utilities, base classes, and common patterns used across all indicator implementations in the Stock Indicators library.

## Purpose

This directory provides foundational infrastructure for all indicator implementations:

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

Framework invariants and contributor guidance (thread-safety contract, `RollbackState` semantics, catalog registration conventions) live in the companion [AGENTS.md](AGENTS.md).

## Development guidelines

For detailed implementation guidance, see the skills:

- StreamHub development: `.agents/skills/indicator-stream/SKILL.md`
  - Performance patterns (O(1) state updates, avoiding O(n²) recalculation)
  - State management best practices
  - Testing requirements and regression validation
- BufferList development: `.agents/skills/indicator-buffer/SKILL.md`
  - Buffer management patterns
  - Incremental processing techniques
  - Interface selection guide
- Series development: `.agents/skills/indicator-series/SKILL.md`
  - Batch processing patterns (canonical reference implementations)
- General requirements: [AGENTS.md](AGENTS.md) and the parent [src/AGENTS.md](../AGENTS.md)
  - Catalog registration
  - Documentation standards
  - Migration guide updates

## NaN handling policy

The library follows IEEE 754 floating-point standard for NaN (Not-a-Number) handling:

### Core principles

1. Natural propagation - NaN values propagate naturally through calculations (e.g., any operation with NaN produces NaN)
2. Internal representation - Use `double.NaN` internally when a value cannot be calculated
3. External representation - Convert NaN to `null` (via `.NaN2Null()`) only at the final result boundary
4. No rejection - Never reject NaN inputs with validation; allow them to flow through the system

### Implementation guidelines

- Division by zero - MUST guard variable denominators with ternary checks (e.g., `denom != 0 ? num / denom : double.NaN`); choose appropriate fallback (NaN, 0, or null) based on mathematical meaning
- NaN propagation - Accept NaN inputs and allow natural propagation; never reject NaN values in calculations
- RollingWindow utilities - Accept NaN values and return NaN for Min/Max when NaN is present in the window
- Bar validation - Only validate for null/missing bars, not for NaN values in bar properties (High/Low/Close/etc.)
- State initialization - Use `double.NaN` for uninitialized state instead of sentinel values (0, -1)

### Principles alignment

This approach aligns with §1 Mathematical precision in [docs/PRINCIPLES.md](../../docs/PRINCIPLES.md):

- Maintains numerical correctness (NaN is mathematically correct for undefined values)
- Prevents silent data corruption from substituting invalid placeholders
- Follows established IEEE 754 standard

## Performance optimization

For streaming and buffer indicators experiencing performance issues, consult:

- Benchmarking guide: [tools/performance/benchmarking.md](../../tools/performance/benchmarking.md) - Running benchmarks, spot checks, and baseline refresh workflow
- Baselines guide: [tools/performance/baselines/README.md](../../tools/performance/baselines/README.md) - Baseline file conventions and regression checks
- Open streaming work: [GitHub Issues milestone v3.1/v3.2](https://github.com/facioquo/stock-indicators-dotnet/issues?q=is%3Aopen+is%3Aissue+milestone%3Av3.1) - release gates, test hardening, performance verification
- Project principles: [docs/PRINCIPLES.md](../../docs/PRINCIPLES.md) - Performance First principles
