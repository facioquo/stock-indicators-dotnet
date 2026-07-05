# Common utilities and patterns

This directory contains shared utilities, base classes, and common patterns used across all indicator implementations in the Stock Indicators library.

## Directory structure

```text
Common/
├── README.md                       # This file
├── BarPart/                        # CandlePart extraction (Open/High/Low/Close/Volume/HL2/...)
│   ├── IBarPart.cs
│   ├── BarPartHub.cs               # BarPartHub streaming hub + ToBarPartHub()
│   ├── BarPartList.cs              # BarPartList incremental list + ToBarPartList()
│   ├── BarParts.cs                 # Static partial class (main) — ToBarPart() series
│   ├── BarParts.Catalog.cs         # Catalog listings (partial)
│   └── BarParts.Utilities.cs       # Validation and helpers (partial)
├── Bars/                           # IBar OHLCV bar types, aggregator hub, validation
│   ├── IBar.cs
│   ├── Bar.cs                      # Default IBar record
│   ├── BarD.cs                     # Double-precision internal bar
│   ├── BarAggregatorHub.cs         # Streaming bar→bar quantization
│   ├── BarHub.cs                   # BarHub: self-rooted source hub
│   ├── Bars.Aggregates.cs          # Series-side aggregation (.Aggregate(BarInterval))
│   ├── Bars.Converters.cs          # Conversions to/from IReusable etc.
│   └── Bars.Validation.cs
├── BufferLists/                    # BufferList incremental processing
│   ├── BufferList.cs               # Abstract base; MaxListSize pruning; field-keyword usage
│   ├── BufferListUtilities.cs      # Rolling Queue.Update / UpdateWithDequeue extensions
│   ├── IIncrementFromChain.cs      # Add(DateTime, double), Add(IReusable), Add(batch)
│   └── IIncrementFromBar.cs        # Add(IBar), Add(IReadOnlyList<IBar>)
├── Candles/                        # Candlestick pattern utilities
│   ├── CandleProperties.cs
│   ├── CandleResult.cs
│   └── Candlesticks.cs             # Candlestick utility methods
├── Catalog/                        # Indicator catalog and metadata (see Catalog/README.md)
│   ├── Catalog.cs                  # Public API: Catalog.Get(), Catalog.Listings
│   ├── Catalog.Listings.cs         # PopulateCatalog() — _listings.Add(...) registrations
│   ├── CatalogListingBuilder.cs    # Fluent builder for IndicatorListing
│   ├── ListingExecutionBuilder.cs  # Reflection-based execution glue
│   ├── ListingExecutionBuilderExtensions.cs  # Fluent extension helpers
│   ├── ListingExecutor.cs
│   └── Schema/                     # IndicatorListing, IndicatorParam, IndicatorResult records
│       └── Enums/                  # Catalog-specific enums (Style, Category, etc.)
├── Enums/                          # Shared enumerations
│   ├── Act.cs                      # Add / Rebuild action discriminator
│   ├── BarInterval.cs              # Aggregation interval keys
│   ├── BarIntervals.cs             # BarInterval ↔ string-code map (ToCode/ToBarInterval)
│   ├── CandlePart.cs               # Open/High/Low/Close/Volume/HL2/HLC3/OC2/OHL3/OHLC4
│   ├── Direction.cs
│   ├── EndType.cs
│   ├── Match.cs
│   ├── MaType.cs
│   └── OutType.cs
├── Exceptions/                     # Custom exceptions
│   └── InvalidBarsException.cs
├── Extensions/                     # Extension methods and static utilities
│   ├── Reusable.cs                 # ToReusable, generic RemoveWarmupPeriods, etc.
│   ├── Generics/                   # Generic utilities
│   │   ├── Pruning.cs              # Cache trimming helpers
│   │   ├── PruningList.cs          # Internal self-pruning list
│   │   ├── Seeking.cs              # IndexOf / IndexGte / IndexBefore extensions
│   │   ├── Sorting.cs
│   │   ├── StringOut.List.cs
│   │   └── StringOut.Type.cs
│   └── Math/                       # Numerical utilities
│       ├── DeMath.cs               # Decimal math helpers
│       ├── NullMath.cs             # Null-safe math (NaN2Null, etc.)
│       └── Numerical.cs            # Abs, Round, double / decimal conversions
├── Interfaces/                     # Core series interfaces
│   ├── IReusable.cs                # Single-value chainable record interface
│   └── ISeries.cs                  # Time-stamped record interface
├── Models/                         # Core models
│   ├── BinarySettings.cs           # Bitfield settings carried between hubs (Properties)
│   └── TimeValue.cs                # Concrete IReusable (Timestamp + Value)
├── StreamHub/                      # StreamHub base classes and streaming utilities
│   ├── IStreamHub.cs               # Public hub interface (Add, RemoveAt, RemoveRange, Rebuild, Reinitialize)
│   ├── IStreamObservable.cs        # Push-side: Subscribe / Unsubscribe / Results
│   ├── IStreamObserver.cs          # Pull-side: OnAdd / OnRebuild / OnPrune / OnError
│   ├── IChainProvider.cs           # Marker for IReusable producers
│   ├── IBarProvider.cs             # Marker for IBar producers
│   ├── StreamHub.cs                # Abstract base: Cache, CacheLock, _isRebuilding, RollbackState, ToIndicator
│   ├── StreamHub.Observable.cs     # Observable side: Subscribe, NotifyObserversOn* methods
│   ├── StreamHub.Observer.cs       # Observer side: OnAdd, OnRebuild, OnPrune entry points
│   ├── StreamHubUtilities.cs       # Cache size validation, IndexBefore helpers
│   ├── CircularDoubleBuffer.cs     # Fixed-size circular buffer for rolling-window indicators
│   ├── HubCollection.cs            # Observer list with thread-safe enumeration
│   ├── RollbackRing.cs             # Rollback state ring buffer
│   └── Providers/                  # Specialized base classes
│       ├── BaseProvider.cs         # Inert sentinel for self-rooted hubs (TODO: rename or remove)
│       ├── IInertProvider.cs       # Inert provider marker interface
│       ├── ChainHub.cs             # IReusable-output chainable hub
│       └── BarProvider.cs          # IBar-output source/transformer
├── TradeTicks/                     # ITradeTick raw trade-tick types and hubs
│   ├── ITradeTick.cs
│   ├── TradeTick.cs                # Default ITradeTick record
│   ├── TradeTickAggregatorHub.cs   # Streaming tick→bar quantization
│   ├── TradeTickHub.cs             # TradeTickHub: self-rooted tick source
│   └── TradeTicks.cs               # ToTradeTickHub / aggregation extensions
└── Validation/
    └── UrlSafeAttribute.cs
```

## Purpose

This directory provides foundational infrastructure for all indicator implementations:

- BufferLists/ - Incremental buffer-based indicator processing
- Candles/ - Candlestick pattern recognition utilities
- Catalog/ - Indicator metadata and discovery
- Extensions/ - Extension methods, generic and numerical utilities
- StreamHub/ - Real-time streaming indicator base classes
- Validation/ - Input validation and error handling

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
- General requirements: `AGENTS.md`
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

### Constitutional alignment

This approach aligns with Constitution §1: Mathematical Precision:

- Maintains numerical correctness (NaN is mathematically correct for undefined values)
- Prevents silent data corruption from substituting invalid placeholders
- Follows established IEEE 754 standard

## Performance optimization

For streaming and buffer indicators experiencing performance issues, consult:

- Benchmarking guide: [tools/performance/benchmarking.md](../../tools/performance/benchmarking.md) - Running benchmarks, spot checks, and baseline refresh workflow
- Baselines guide: [tools/performance/baselines/README.md](../../tools/performance/baselines/README.md) - Baseline file conventions and regression checks
- Active plan: [docs/plans/streaming-indicators.plan.md](../../docs/plans/streaming-indicators.plan.md) - Streaming indicators plan (release gates, test hardening, performance verification)
- Project principles: [docs/PRINCIPLES.md](../../docs/PRINCIPLES.md) - Performance First principles
