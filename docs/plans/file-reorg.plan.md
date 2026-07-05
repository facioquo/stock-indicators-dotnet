# File Reorganization Plan

**Status**: Non-breaking portion implemented (v3.1); breaking renames deferred to v4
**Date**: 2025-12-28 (last substantive content); 2026-07-05 (implementation status refresh)
**Related Issue**: [#1810](https://github.com/facioquo/stock-indicators-dotnet/issues/1810) — Major File Renaming Refactor

**Related plans**: [streaming-indicators.plan.md](streaming-indicators.plan.md) is the v3.0 source of truth and tracks this work in **§J Infrastructure — deferred but listed for context** as the single deferred item. This plan is the detailed catalog of the file/directory renames and naming-convention decisions to execute when v3.1 work begins. It is intentionally **out of scope for v3.0** to avoid churning the codebase during the release-hardening window.

> This plan is out of date in specifics, but notionally correct.

## Implementation status (2026-07-05)

The **non-breaking** portion of this plan is implemented (see #1810 for the tracking issue):

- **Directories**: `src/_common/` → `src/Common/` (with `Interfaces/`, `Models/`, `Exceptions/`, `Extensions/` functional subdirectories); `src/{a-d,e-k,m-r,s-z}/` → `src/Indicators/{A-D,E-K,M-R,S-Z}/`; `tests/indicators` → `tests/Indicators` (with `Base/`, `Common/`, `Precision/`, `TestData/`, `Tools/`); `tests/integration` → `tests/Integration`; `tests/public-api` → `tests/PublicApi`.
- **File names now match contained type names**: `{Ind}.StreamHub.cs` → `{Ind}Hub.cs`, `{Ind}.BufferList.cs` → `{Ind}List.cs`, `{Ind}.Models.cs` → `{Ind}Result.cs`, `{Ind}.StaticSeries.cs` → `{Ind}.cs` (main partial-class file; `.Utilities`/`.Catalog` remain dot-notation auxiliary files).
- **Multi-type files split**: `BetaType`, `PivotPointType`, `PivotTrend`, `TradeTicks`, `IndicatorConfigExtensions`, `IInertProvider` each moved to their own file.
- **Internal-only class renames**: `internal static class StreamHub` → `StreamHubUtilities` (resolves the Task 4.2 conflict without touching public API).
- **Test classes standardized** (Task 4.11): `{Ind}SeriesTests`, `{Ind}BufferListTests`, `{Ind}HubTests`, `{Ind}CatalogTests`, `{Ind}RegressionTests`, with files renamed to match class names.

The **breaking** remainder ("completely perfect" file/type alignment) is tracked in v4-milestone sub-issues of #1810:

- [#2137](https://github.com/facioquo/stock-indicators-dotnet/issues/2137) — Add `Series` suffix to indicator series classes (Tasks 4.8/4.9)
- [#2138](https://github.com/facioquo/stock-indicators-dotnet/issues/2138) — Unify McGinley Dynamic type family naming (Task 4.10)
- [#2139](https://github.com/facioquo/stock-indicators-dotnet/issues/2139) — Rename public extension-method holder classes to `*Extensions` (Tasks 2.2–2.4, 4.1)
- [#2140](https://github.com/facioquo/stock-indicators-dotnet/issues/2140) — Align result record names with indicator family names (`CorrResult`, `HtlResult`, `MaEnvelopeResult`, `WilliamsResult`)

---

## Objective

Reorganize the Stock.Indicators library to follow .NET naming conventions where file names match class/interface/enum names, using dot notation for partial classes. The target structure will improve discoverability, maintainability, and align with Microsoft's library design guidelines.

### Target folder structure

```text
src/                                  # Source code root
├── Common/                           # Shared functionality (formerly _common)
│   ├── BufferLists/                  # Buffer list infrastructure
│   ├── Candles/                      # Candle pattern functionality
│   ├── Catalog/                      # Indicator catalog system
│   │   └── Schema/                   # Catalog schema types
│   │       └── Enums/                # Catalog-specific enums
│   ├── Enums/                        # Core library enums
│   ├── Exceptions/                   # Custom exceptions
│   ├── Extensions/                   # Extension methods
│   │   ├── Generics/                 # Generic extensions
│   │   └── Math/                     # Math extensions
│   ├── Interfaces/                   # Core interfaces
│   ├── Models/                       # Core models
│   ├── BarPart/                      # Bar part functionality
│   ├── Bars/                         # Bar types and operations
│   ├── StreamHub/                    # Streaming infrastructure
│   │   └── Providers/                # Stream providers
│   └── Validation/                   # Validation utilities
├── Indicators/                       # Indicator implementations
│   ├── A-D/                          # Indicators A-D (alphabetical)
│   ├── E-K/                          # Indicators E-K
│   ├── M-R/                          # Indicators M-R
│   └── S-Z/                          # Indicators S-Z
└── [root files]                      # Project files, global configs

tests/                                # Test projects root
├── Indicators/                       # Indicator tests (formerly indicators)
│   ├── Base/                         # Base test classes (formerly _base)
│   ├── Common/                       # Common test utilities (formerly _common)
│   ├── Precision/                    # Precision tests (formerly _precision)
│   ├── TestData/                     # Test data (formerly _testdata)
│   ├── Tools/                        # Test tools (formerly _tools)
│   ├── A-D/                          # Tests for indicators A-D
│   ├── E-K/                          # Tests for indicators E-K
│   ├── M-R/                          # Tests for indicators M-R
│   └── S-Z/                          # Tests for indicators S-Z
├── Integration/                      # Integration tests
│   ├── Common/                       # Integration test utilities
│   └── Indicators/                   # Indicator integration tests
└── PublicApi/                        # Public API tests (formerly public-api)
    ├── Customizable/                 # Customizable API tests
    └── Indicators/                   # Indicator API tests
```

---

## File naming rules

Follow these conventions for all file naming:

### 1. Match type names

- **Rule**: Each file name MUST match the primary type (class, interface, enum, record, struct) it contains
- **Example**: `public class EmaHub` → `EmaHub.cs`
- **Exception**: Partial classes use dot notation to indicate their functional area

### 2. Partial classes use dot notation

- **Rule**: Partial class files use dot notation for auxiliary files of the same partial class
- **Pattern**: Main partial class file gets base name (no extender), auxiliary files get dot extenders
- **Current examples** (to be updated per Phase 4):
  - `Ema.StaticSeries.cs` → will become `EmaSeries.cs` (main file)
  - `Ema.StreamHub.cs` → will become `EmaHub.cs` (separate class, exact match)
  - `Ema.BufferList.cs` → will become `EmaList.cs` (separate class, exact match)
  - `Ema.Catalog.cs` → will become `EmaSeries.Catalog.cs` (auxiliary partial file)
  - `Ema.Models.cs` → will become `EmaResult.cs` (separate class, exact match)
  - `Ema.Utilities.cs` → will become `EmaSeries.Utilities.cs` OR `EmaExtensions.cs`
- **Other partial class examples**:
  - `Bar.Validation.cs` - Validation methods (auxiliary partial file)
  - `Bar.Converters.cs` - Conversion methods (auxiliary partial file)

### 3. One type per file

- **Rule**: Each file contains exactly ONE public type (class, interface, enum, record, or struct)
- **Exception**: Nested types may reside in the same file as their parent
- **Action Required**: Files with multiple types (e.g., `Enums.cs`, `IIncrementFrom.cs`) must be split

### 4. Extension method classes vs. static utility classes

- **Extension method classes** (contain `this` parameter): Name with `{Type}Extensions.cs` or `{Area}Extensions.cs`
  - **Examples**: `StringExtensions.cs`, `ReusableExtensions.cs`, `EnumerableExtensions.cs`
- **Static utility classes** (no `this` parameter): Name descriptively by function, following BCL patterns
  - **Examples**: `Pruning.cs` (like `Math.cs`), `Seeking.cs`, `Sorting.cs`, `Transforms.cs`
- **Reference**: Microsoft explicitly states to avoid generic "Extensions" naming and use descriptive functional names instead

### 5. Interface naming

- **Rule**: Interface files follow standard .NET convention with `I` prefix
- **Example**: `public interface IReusable` → `IReusable.cs`
- **Multiple Related Interfaces**: When closely related, consider organizing in subdirectories
  - `StreamHub/IStreamHub.cs`
  - `StreamHub/IStreamObserver.cs`
  - `StreamHub/IStreamObservable.cs`

### 6. Enum naming

- **Rule**: Each enum gets its own file matching the enum name
- **Example**: `public enum CandlePart` → `CandlePart.cs`
- **Organization**: Group related enums in subdirectories (e.g., `Enums/`, `Catalog/Schema/Enums/`)

### 7. Exception naming

- **Rule**: Exception classes follow .NET convention with `Exception` suffix
- **Example**: `public class InvalidBarsException` → `InvalidBarsException.cs`
- **Organization**: Place in `Common/Exceptions/` directory

### 8. PascalCase for file names

- **Rule**: All file names use PascalCase (UpperCamelCase)
- **Example**: `ChainHub.cs`, `RollingWindowMax.cs`, `StringOutExtensions.cs`

### 9. Avoid generic terms

- **Rule**: Avoid overly generic file names like `Enums.cs`, `Models.cs`, `Utilities.cs`
- **Better**: Use specific names or partition by functional area
- **Exception**: `{IndicatorName}.Models.cs` is acceptable as it's scoped to the indicator

### 10. Directory names

- **Rule**: Directory names use PascalCase and are plural when containing multiple related items
- **Examples**: `Indicators/`, `Extensions/`, `Enums/`, `Interfaces/`
- **Exception**: Indicator alphabetical groupings remain as is (e.g., `A-D/`, `E-K/`)

---

## Migration plan

> Phases and tasks are prioritized based on logical implementation sequence, minimizing disruption and ensuring testability at each step.

### Phase 1: Foundation - Common infrastructure split

**Goal**: Separate multi-type files in `_common/` into individual files

**Priority**: High - Establishes the foundation for all other changes

- [ ] Task 1.1: Split `Enums.cs` into individual enum files
  - Create `Common/Enums/Act.cs` (internal enum)
  - Create `Common/Enums/CandlePart.cs`
  - Create `Common/Enums/Direction.cs`
  - Create `Common/Enums/EndType.cs`
  - Create `Common/Enums/Match.cs`
  - Create `Common/Enums/MaType.cs`
  - Create `Common/Enums/OutType.cs`
  - Create `Common/Enums/BarInterval.cs`
  - Delete `Enums.cs`
  - Update all using statements in dependent files

- [ ] Task 1.2: Split `IIncrementFrom.cs` into individual interface files
  - Create `Common/BufferLists/IIncrementFromChain.cs`
  - Create `Common/BufferLists/IIncrementFromBar.cs`
  - Delete `IIncrementFrom.cs`

- [ ] Task 1.3: Split `IStreamObservable.cs` into individual interface files
  - Create `Common/StreamHub/IChainProvider.cs`
  - Create `Common/StreamHub/IBarProvider.cs`
  - Create `Common/StreamHub/IStreamObservable.cs` (keep as single interface)
  - Delete original multi-interface file

- [ ] Task 1.4: Reorganize model files with multiple types
  - Split `Bar.Models.cs` → `IBar.cs` and `Bar.cs`
  - Split `Candles.Models.cs` → `CandleProperties.cs` and `CandleResult.cs`
  - Review all other `*.Models.cs` files for multi-type violations

- [ ] Task 1.6: Run full test suite to verify no regressions

### Phase 2: Static class naming alignment

**Goal**: Align static class naming with Microsoft .NET conventions

**Priority**: Medium - Most files already follow conventions; verify and fix inconsistencies

- [ ] Task 2.1: Audit static utility classes (no `this` parameters)
  - Verify `Pruning.cs`, `Seeking.cs`, `Sorting.cs` follow BCL patterns (like `Math.cs`, `File.cs`)
  - Rename class `Seeking` → match file name if needed
  - Keep descriptive functional names per Microsoft guidelines

- [ ] Task 2.2: Audit extension method classes (have `this` parameters)
  - Verify classes with extension methods use `{Type}Extensions` or `{Area}Extensions` pattern
  - Examples: `StringExtensions.cs`, `ReusableExtensions.cs`, `EnumerableExtensions.cs`
  - Rename `Reusable.Utilities.cs` → `ReusableExtensions.cs` if it contains extension methods

- [ ] Task 2.3: Review `StringOut` partial classes
  - Determine if `StringOut` contains extension methods or static utilities
  - Keep as `StringOut.cs` if static utility (like `Convert.cs`)
  - Rename to `StringExtensions.cs` if primarily extension methods

- [ ] Task 2.4: Review math utility classes
  - Keep `NullMath.cs`, `Numerical.cs` as static utilities (follow `Math.cs` pattern)
  - Verify `DeMath.cs` naming aligns with function
  - Only use `*Extensions` suffix if they contain extension methods

- [ ] Task 2.5: Run targeted tests for affected areas

### Phase 3: Directory reorganization

**Goal**: Rename and restructure directories for clarity and consistency

**Priority**: Medium - Improves navigation without changing functionality

- [ ] Task 3.1: Rename `_common/` → `Common/`
  - Update namespace declarations from `FacioQuo.Stock.Indicators` (stay the same)
  - Update project file references
  - Update all path references in documentation

- [ ] Task 3.2: Reorganize `Common/` subdirectories
  - Create `Common/Extensions/` - Move extension method files (with `this` parameter)
  - Create `Common/Utilities/` - Move static utility classes (NullMath, Numerical, etc.)
  - Create `Common/Interfaces/` - Move core interfaces (`ISeries.cs`, `IReusable.cs`)
  - Create `Common/Exceptions/` - Move exception files
  - Move `BinarySettings.cs` → `Common/Models/BinarySettings.cs`

- [ ] Task 3.3: Rename indicator category directories
  - Rename `a-d/` → `A-D/`
  - Rename `e-k/` → `E-K/`
  - Rename `m-r/` → `M-R/`
  - Rename `s-z/` → `S-Z/`
  - Create parent `Indicators/` directory and move all under it

- [ ] Task 3.4: Reorganize test directories
  - Rename `tests/indicators/` → `tests/Indicators/`
  - Rename `tests/indicators/_base/` → `tests/Indicators/Base/`
  - Rename `tests/indicators/_common/` → `tests/Indicators/Common/`
  - Rename `tests/indicators/_precision/` → `tests/Indicators/Precision/`
  - Rename `tests/indicators/_testdata/` → `tests/Indicators/TestData/`
  - Rename `tests/indicators/_tools/` → `tests/Indicators/Tools/`
  - Rename `tests/public-api/` → `tests/PublicApi/`
  - Update test namespaces to follow new directory structure

- [ ] Task 3.5: Run full test suite to verify directory changes

### Phase 4: Class and type renaming alignment

**Goal**: Ensure class/type names match file names per .NET conventions

**Priority**: Medium-High - Prevents confusion and follows "file name matches type name" rule

- [ ] Task 4.1: Fix class/file name mismatches in utilities
  - Rename `class Transforming` → `class Transforms` in `Transforms.cs`
  - Rename `Seek.cs` → `Seeking.cs` OR rename `class Seeking` → `class Seek`
  - Verify `Pruning.cs` contains `class Pruning` ✓ (already correct)
  - Verify `Sorting.cs` contains `class Sorting` ✓ (already correct)

- [ ] Task 4.2: Resolve StreamHub naming conflict
  - Rename `class StreamHub` in `StreamHub.Utilities.cs` → `StreamHubUtilities` to avoid conflict with generic `StreamHub<T>` class
  - Update all references to the static utility class

- [ ] Task 4.3: Document partial class plural/singular naming rationale
  - `partial class Bars` (plural) in Bar.* files - Document reason for plural
  - `partial class BarParts` (plural) in BarPart.* files - Document reason for plural
  - `partial class Catalog` (singular) in Catalog.* files - Document reason for singular
  - `partial class StringOut` in StringOut.* files - Verify naming aligns with functionality
  - Add naming guidelines to documentation

- [ ] Task 4.4: Review Candlesticks/Candles naming
  - Verify `Candles.Utilities.cs` contains `class Candlesticks` - decide on alignment
  - Either rename file to `Candlesticks.Utilities.cs` OR rename class to `Candles`
  - Ensure consistency with related types (CandlePart, CandleResult, etc.)

- [ ] Task 4.5: Audit all static classes for name matching
  - Run automated check: file name (minus extension) should match class name
  - Generate list of any remaining mismatches
  - Document intentional exceptions (if any)

- [ ] Task 4.6: Update XML documentation
  - Update summaries for renamed classes
  - Ensure IntelliSense reflects new names
  - Update any code examples in documentation

- [ ] Task 4.7: Run targeted tests for renamed classes
  - Verify all references updated correctly
  - Check for compilation errors
  - Validate functionality unchanged

- [ ] Task 4.8: Add suffix to Series indicator partial classes for consistency
  - **Problem**: Hub classes have "Hub" suffix, List classes have "List" suffix, but Series partial classes have NO suffix
  - **Current state**: `partial class Ema`, `partial class Rsi` (no suffix)
  - **Recommended**: Add suffix to Series partial classes across ~70 indicators
  - **Suffix recommendation**: Use `{Indicator}Series` (e.g., `partial class EmaSeries`, `partial class RsiSeries`)
    - **Rationale for "Series"**:
      - Mirrors the "StaticSeries" file naming pattern
      - Commonly used term in documentation (e.g., "Series-style indicators")
      - Distinguishes from StreamHub (stateful) and BufferList (incremental) variants
      - More descriptive than "Static" which could imply static methods/class
      - Aligns with industry terminology (time series, data series)
  - **Alternative considered**: `{Indicator}Static` - rejected because "Static" could be confused with static class modifier
  - **Impact**: ~70 partial class renames + all consuming code references
  - **Example transformations**:
    - `Ema.StaticSeries.cs`: `partial class Ema` → `partial class EmaSeries`
    - `Rsi.StaticSeries.cs`: `partial class Rsi` → `partial class RsiSeries`
    - `ConnorsRsi.StaticSeries.cs`: `partial class ConnorsRsi` → `partial class ConnorsRsiSeries`

- [ ] Task 4.9: Rename indicator files to match class names
  - **Rule**: File name must match class name, with dot extenders ONLY for auxiliary files of the same partial class
  - **Pattern**: Main partial class file gets base name (no extender), auxiliary partial class files get dot extenders, separate classes use descriptive names
  - **Current violations**:
    - `Ema.StaticSeries.cs` contains `partial class Ema` ❌ (file has extender but no class suffix)
    - `Ema.StreamHub.cs` contains `class EmaHub` ❌ (file "Ema" ≠ class "EmaHub")
    - `Ema.BufferList.cs` contains `class EmaList` ❌ (file "Ema" ≠ class "EmaList")
    - `Ema.Models.cs` contains `record EmaResult` ❌ (file "Ema" ≠ class "EmaResult")
  - **Correct patterns**:
    - `EmaSeries.cs` → `partial class EmaSeries` (MAIN file for partial class, no extender)
    - `EmaSeries.Utilities.cs` → `partial class EmaSeries` (auxiliary fragment IF utilities belong to same partial class)
    - `EmaSeries.Catalog.cs` → `partial class EmaSeries` (auxiliary fragment IF catalog belongs to same partial class)
    - `EmaExtensions.cs` → `static class EmaExtensions` (separate extension class per Phase 2 IF utilities are split out)
    - `EmaHub.cs` → `class EmaHub` (exact match for non-partial classes)
    - `EmaList.cs` → `class EmaList` (exact match for non-partial classes)
    - `EmaResult.cs` → `record EmaResult` (exact match for result records, shared across all styles)
    - `IEma.cs` → `interface IEma` (exact match for interfaces)
  - **Example directory structure after renaming**:

    ```text
    e-k/Ema/
    ├── EmaSeries.cs                 # partial class EmaSeries (MAIN - no extender)
    ├── EmaSeries.Utilities.cs       # partial class EmaSeries (IF keeping as partial fragment)
        OR
    ├── EmaExtensions.cs             # static class EmaExtensions (IF splitting out per Phase 2)
    ├── EmaSeries.Catalog.cs         # partial class EmaSeries (catalog typically stays with main)
    ├── EmaHub.cs                    # class EmaHub (exact name match)
    ├── EmaList.cs                   # class EmaList (exact name match)
    ├── EmaResult.cs                 # record EmaResult (exact name match, shared across all styles)
    └── IEma.cs                      # interface IEma (exact name match)
    ```

  - **Transformations** (~210 files: 70 indicators × 3 files average):
    - Series (main): `Ema.StaticSeries.cs` → `EmaSeries.cs` (MAIN file, no extender)
    - StreamHub: `Ema.StreamHub.cs` → `EmaHub.cs` (exact class name match)
    - BufferList: `Ema.BufferList.cs` → `EmaList.cs` (exact class name match)
    - Result model: `Ema.Models.cs` → `EmaResult.cs` (exact class name match, shared across all styles)
    - Utilities (partial): `Ema.Utilities.cs` → `EmaSeries.Utilities.cs` (IF `partial class EmaSeries`)
    - Utilities (separate): `Ema.Utilities.cs` → `EmaExtensions.cs` (IF `static class EmaExtensions`)
    - Catalog: `Ema.Catalog.cs` → `EmaSeries.Catalog.cs` (catalog typically stays as partial fragment)
    - Interface: `IEma.cs` → `IEma.cs` (interface matches indicator name, shared across all styles)
  - **Decision point for each indicator**: Evaluate utilities/catalog files:
    1. **Are they partial class fragments?** → Use `{Indicator}Series.{Purpose}.cs` pattern (e.g., `EmaSeries.Utilities.cs`)
    2. **Are they separate extension classes?** → Use `{Indicator}Extensions.cs` pattern per Phase 2 (e.g., `EmaExtensions.cs`)
  - **Note:** Files remain grouped in same directory (e.g., `e-k/Ema/`), only file names change to match class names

- [ ] Task 4.10: Standardize indicator partial class names for file alignment
  - Audit all indicators for file/class name alignment (Dynamic vs ConnorsRsi examples show current problems)
  - Decision: Adopt hybrid approach - directory name can be short (e.g., `Dynamic/`), but partial class and file names should match (e.g., `MgDynamicSeries.*.cs` with `partial class MgDynamicSeries`)
  - Fix indicators where directory name ≠ partial class name (like Dynamic → MgDynamic)
  - Apply "Series" suffix from Task 4.8 during this rename
  - Create mapping document for abbreviated vs. full names

- [ ] Task 4.11: Standardize test class naming conventions with CONSISTENT suffix usage
  - **Problem**: Current state mixes no-suffix (Series/Buffer) with suffixes (Hub/Catalog/Regression)
  - **Recommended standard** (consistent suffixes across ALL types):
    - StaticSeries tests: `{Indicator}SeriesTests` (e.g., `EmaSeriesTests`, `RsiSeriesTests`, `ConnorsRsiSeriesTests`)
    - BufferList tests: `{Indicator}BufferListTests` (e.g., `EmaBufferListTests`, `RsiBufferListTests`)
    - StreamHub tests: Keep `{Indicator}HubTests` (e.g., `EmaHubTests`, `RsiHubTests`)
    - Catalog tests: `{Indicator}CatalogTests` (e.g., `EmaCatalogTests`, `RsiCatalogTests`)
    - Regression tests: `{Indicator}RegressionTests` (e.g., `EmaRegressionTests`, `RsiRegressionTests`)
  - **Rationale**: Descriptive suffixes make it immediately clear what aspect is being tested, mirrors source class suffixes (Series/Hub/List)
  - **Effort**: Rename ~280 test classes (70 indicators × 4 test types average)

### Phase 5: Indicator file organization

**Goal**: Ensure consistent naming patterns for all indicators

**Priority**: Medium - Most files already follow the pattern

- [ ] Task 5.1: Audit all indicator directories for compliance
  - Verify each indicator follows `{Name}.{FunctionalArea}.cs` pattern
  - Identify any non-compliant files
  - Create inventory of files needing renaming

- [ ] Task 5.2: Standardize interface files
  - Verify all `I{IndicatorName}.cs` files exist and follow naming
  - Consolidate any standalone interfaces into proper files

- [ ] Task 5.3: Review `.Models.cs` files for single-type compliance
  - Audit all `*.Models.cs` files
  - Split files containing multiple result types
  - Example: If `Macd.Models.cs` has `MacdResult` and `MacdSettings`, split appropriately

- [ ] Task 5.4: Run indicator-specific test suites

### Phase 6: Catalog and schema organization

**Goal**: Clean up catalog system file organization

**Priority**: Low - Working well but can be improved

- [ ] Task 6.1: Review `Catalog/` directory structure
  - Verify `CatalogListingBuilder.cs` naming
  - Verify `ListingExecutionBuilder.cs` naming
  - Verify `ListingExecutor.cs` naming
  - Ensure schema files follow conventions

- [ ] Task 6.2: Verify enum organization in `Catalog/Schema/Enums/`
  - Confirm `Category.cs`, `ResultType.cs`, `Style.cs` follow naming

- [ ] Task 6.3: Run catalog-related tests

### Phase 7: Special cases and edge cases

**Goal**: Address unique situations and exceptions

**Priority**: Low - Handle after main reorganization

- [ ] Task 7.1: Review and rename remaining multi-type files
  - `Bar.StreamHub.cs` (contains `BarHub` and `BaseProvider<T>`)
  - Split if beneficial or document as acceptable exception

- [ ] Task 7.2: Review partial classes spanning multiple functional areas
  - Ensure dot notation is consistent and meaningful
  - Document any special cases in code comments

- [ ] Task 7.3: Review obsolete code organization
  - `Obsolete.V3.Indicators.cs` - acceptable as consolidated obsolete items
  - `Obsolete.V3.Other.cs` - acceptable as consolidated obsolete items
  - Consider whether to split in future major versions

- [ ] Task 7.4: Update build and tooling configurations
  - Verify `.csproj` file references
  - Update any build scripts or automation
  - Update IDE project settings if needed

### Phase 8: Documentation and finalization

**Goal**: Update all documentation to reflect new organization

**Priority**: Medium - Must be done before release

- [ ] Task 8.1: Update contributor documentation
  - Update file organization guidelines
  - Update contribution guide with new naming conventions
  - Create migration guide for contributors with pending PRs

- [ ] Task 8.2: Update API documentation
  - Verify XML documentation references are correct
  - Update any file path references in docs
  - Regenerate API documentation if auto-generated

- [ ] Task 8.3: Update build and CI/CD documentation
  - Update any scripts that reference file paths
  - Update deployment documentation
  - Verify continuous integration still works

- [ ] Task 8.4: Create migration notes for external developers
  - Document breaking changes (if any namespace changes)
  - Provide guidance for updating custom indicators
  - Update changelog with reorganization details

- [ ] Task 8.5: Final validation
  - Run complete test suite (unit, integration, regression)
  - Run performance benchmarks to verify no degradation
  - Manual smoke testing of key scenarios
  - Code review of all changes

---

## Appendix

### A.1: Current directory structure

```text
src/                                  # Source code
├── _common/                          # Common functionality
│   ├── BufferLists/
│   ├── Candles/
│   ├── Catalog/
│   │   └── Schema/
│   │       └── Enums/
│   ├── Generics/
│   ├── Math/
│   ├── BarPart/
│   ├── Bars/
│   ├── Reusable/
│   ├── StreamHub/
│   │   └── Providers/
│   └── Validation/
├── a-d/                              # Indicators A-D
│   ├── Adl/
│   ├── Adx/
│   ├── Alligator/
│   ├── [... 22 more indicators]
├── e-k/                              # Indicators E-K
│   ├── ElderRay/
│   ├── Ema/
│   ├── Epma/
│   ├── [... 12 more indicators]
├── m-r/                              # Indicators M-R
│   ├── MaEnvelopes/
│   ├── Macd/
│   ├── Mama/
│   ├── [... 14 more indicators]
└── s-z/                              # Indicators S-Z
    ├── Slope/
    ├── Sma/
    ├── SmaAnalysis/
    └── [... 24 more indicators]

tests/                                # Tests
├── indicators/                       # Indicator tests
│   ├── _base/                        # Base test classes
│   ├── _common/                      # Common test utilities
│   ├── _precision/                   # Precision tests
│   ├── _testdata/                    # Test data
│   ├── _tools/                       # Test tools
│   ├── a-d/
│   ├── e-k/
│   ├── m-r/
│   └── s-z/
├── integration/                      # Integration tests
│   ├── _common/
│   └── indicators/
└── public-api/                       # Public API tests
    ├── customizable/
    └── indicators/
```

### A.2: Current state analysis

#### What's working well

1. **Indicator Organization**: The alphabetical categorization (a-d, e-k, m-r, s-z) works well for managing 70+ indicators
2. **Partial Class Pattern**: Using dot notation for partial classes (e.g., `Ema.StaticSeries.cs`) is clear and follows .NET conventions
3. **Functional Separation**: Clear separation between series, stream, buffer, catalog, and models
4. **Test Organization**: Test structure mirrors source structure, making tests easy to locate
5. **Common Directory**: Consolidation of shared functionality in `_common/` is logical
6. **Namespace Consistency**: Single namespace `FacioQuo.Stock.Indicators` keeps public API clean

#### What's not working well

1. **Underscore Prefix**: Leading underscores in directory names (`_common/`, `_base/`) are not idiomatic in .NET
2. **Multiple Types Per File**: Files like `Enums.cs` (7 enums), `IIncrementFrom.cs` (3 interfaces) violate single-type principle
3. **Generic File Names**: Names like `Enums.cs`, `Utilities.cs` don't indicate what types they contain
4. **Inconsistent Extension Classes**: Some use class name matching file name, others don't (e.g., `Seeking` vs `Seek.cs`)
5. **Mixed Case Directories**: Test directories use lowercase (`a-d/`) while .NET convention is PascalCase
6. **Extension Method Organization**: Extension methods scattered across files named by functionality rather than as `*Extensions.cs`
7. **Interface Organization**: Related interfaces split across files could be better organized
8. **Model File Ambiguity**: `.Models.cs` suffix doesn't indicate which model(s) are in the file

### A.3: Top 10 problems to solve (prioritized)

1. **Multi-type files** (High Impact)
   - **Problem**: `Enums.cs`, `IIncrementFrom.cs`, and other files contain multiple types
   - **Impact**: Violates .NET conventions, makes types hard to find, complicates navigation
   - **Solution**: Split into one file per type

2. **Class name / file name mismatches** (High Impact)
   - **Problem**: `class Transforming` in `Transforms.cs`, `class Seeking` in `Seek.cs`, `class StreamHub` conflicts with generic `StreamHub<T>`
   - **Impact**: Confusion when navigating, violates "file name matches type name" rule
   - **Solution**: Phase 4 - Align class names with file names or vice versa

3. **Underscore Directory Prefixes** (High Impact)
   - **Problem**: `_common/`, `_base/`, `_testdata/` use leading underscores
   - **Impact**: Not idiomatic for .NET, suggests private/hidden when they're not
   - **Solution**: Rename to PascalCase without underscores (e.g., `Common/`, `Base/`)

4. **Extension Method Naming** (Medium Impact)
   - **Problem**: Extension classes don't follow `*Extensions` naming convention
   - **Impact**: Harder to identify extension methods in IntelliSense and search
   - **Solution**: Rename to `PruningExtensions.cs`, `SeekingExtensions.cs`, etc.

5. **Generic Utility File Names** (Medium Impact)
   - **Problem**: Files like `Utilities.cs` don't indicate their specific purpose
   - **Impact**: Must open file to know what's inside
   - **Solution**: Use specific names or partition by function

6. **Interface File Organization** (Low Impact)
   - **Problem**: Multiple related interfaces in single files makes individual interfaces harder to reference
   - **Impact**: Reduced discoverability, harder to link to specific interface docs
   - **Solution**: One interface per file

7. **Lower Case Directory Names** (Low Impact)
   - **Problem**: `a-d/`, `e-k/`, etc. use lowercase in tests and src
   - **Impact**: Inconsistent with .NET PascalCase convention
   - **Solution**: Rename to `A-D/`, `E-K/`, etc.

8. **Directory Organization Depth** (Low Impact)
   - **Problem**: Current structure is flat with alphabetical grouping
   - **Impact**: Could benefit from functional grouping within indicators
   - **Solution**: Consider `Indicators/` parent directory for clarity

9. **Model File Naming Ambiguity** (Low Impact)
   - **Problem**: `*.Models.cs` suffix doesn't indicate which specific types are inside
   - **Impact**: Must open file to see if it contains desired model
   - **Solution**: Split multi-model files or document naming standard

10. **Test Directory Alignment** (Low Impact)
    - **Problem**: Test directory structure uses different casing and conventions than src
    - **Impact**: Slightly harder to map test files to source files
    - **Solution**: Align test directory naming with source conventions

### A.4: File naming patterns by count

Based on analysis of 623 C# files in `src/`:

- **Pattern: `{Indicator}.StaticSeries.cs`** - ~70 files (one per indicator)
- **Pattern: `{Indicator}.StreamHub.cs`** - ~50 files (indicators with streaming)
- **Pattern: `{Indicator}.BufferList.cs`** - ~40 files (indicators with buffering)
- **Pattern: `{Indicator}.Models.cs`** - ~70 files (result models)
- **Pattern: `{Indicator}.Catalog.cs`** - ~70 files (catalog metadata)
- **Pattern: `{Indicator}.Utilities.cs`** - ~45 files (utility methods)
- **Pattern: `I{Indicator}.cs`** - ~45 files (indicator interfaces)
- **Pattern: Partial class with dots** - ~320 files total
- **Pattern: Generic utility classes** - ~20 files (need renaming)
- **Pattern: Multiple types per file** - ~12 files (need splitting)

### A.4.1: Deep dive - Naming inconsistencies across indicator implementations

Analysis of "Dynamic" (McGinley Dynamic) and "ConnorsRsi" indicators reveals systematic naming inconsistencies:

#### Dynamic (McGinley Dynamic) indicator

**Source files** (`src/a-d/Dynamic/`):

| File name | Partial class name | Other classes | Issue |
| --------- | ------------------ | ------------- | ----- |
| `Dynamic.StaticSeries.cs` | `MgDynamic` | - | ❌ File "Dynamic" ≠ Class "MgDynamic" |
| `Dynamic.Utilities.cs` | `MgDynamic` | - | ❌ File "Dynamic" ≠ Class "MgDynamic" |
| `Dynamic.Catalog.cs` | `MgDynamic` | - | ❌ File "Dynamic" ≠ Class "MgDynamic" |
| `Dynamic.Models.cs` | - | `DynamicResult` | ✓ Match |
| `Dynamic.StreamHub.cs` | - | `DynamicHub` | ✓ Match |
| `Dynamic.BufferList.cs` | - | `DynamicList` | ✓ Match |
| `IDynamic.cs` | - | `IDynamic` | ✓ Match |

**Test files** (`tests/indicators/a-d/Dynamic/`):

| File name | Test class name | Issue |
| --------- | --------------- | ----- |
| `Dynamic.StaticSeries.Tests.cs` | `McGinleyDynamic` | ❌ File "Dynamic" ≠ Class "McGinleyDynamic" |
| `Dynamic.BufferList.Tests.cs` | `MgDynamic` | ❌ File "Dynamic" ≠ Class "MgDynamic" |
| `Dynamic.StreamHub.Tests.cs` | `DynamicHubTests` | ✓ Match |
| `Dynamic.Catalog.Tests.cs` | `DynamicTests` | ✓ Match |

**Problems identified**:

1. Partial class `MgDynamic` appears in 3 files but doesn't match directory/file name "Dynamic"
2. Test class names inconsistent: `McGinleyDynamic`, `MgDynamic`, `DynamicHubTests`, `DynamicTests`
3. No clear pattern: some use full name "McGinley", some abbreviate "Mg", some use "Dynamic" only

#### ConnorsRsi indicator

**Source files** (`src/a-d/ConnorsRsi/`):

| File name | Partial class name | Other classes | Issue |
| --------- | ------------------ | ------------- | ----- |
| `ConnorsRsi.StaticSeries.cs` | `ConnorsRsi` | - | ✓ Match |
| `ConnorsRsi.Utilities.cs` | `ConnorsRsi` | - | ✓ Match |
| `ConnorsRsi.Catalog.cs` | `ConnorsRsi` | - | ✓ Match |
| `ConnorsRsi.Models.cs` | - | `ConnorsRsiResult` | ✓ Match |
| `ConnorsRsi.StreamHub.cs` | - | `ConnorsRsiHub` | ✓ Match |
| `ConnorsRsi.BufferList.cs` | - | `ConnorsRsiList` | ✓ Match |
| `IConnorsRsi.cs` | - | `IConnorsRsi` | ✓ Match |

**Test files** (`tests/indicators/a-d/ConnorsRsi/`):

| File name | Test class name | Issue |
| --------- | --------------- | ----- |
| `ConnorsRsi.StaticSeries.Tests.cs` | `ConnorsRsi` | ⚠️ No suffix (inconsistent with Hub/Catalog) |
| `ConnorsRsi.BufferList.Tests.cs` | `ConnorsRsi` | ⚠️ No suffix (inconsistent with Hub/Catalog) |
| `ConnorsRsi.StreamHub.Tests.cs` | `ConnorsRsiHubTests` | ⚠️ Has suffix (inconsistent with Series/Buffer) |
| `ConnorsRsi.Catalog.Tests.cs` | `ConnorsRsiTests` | ⚠️ Ambiguous - could be Series or Catalog |

**Problems identified**:

1. Internally consistent (file names match class names) - this is good
2. **Still exhibits suffix inconsistency** across test types - this is the core problem
3. Series/Buffer tests have no suffix while Hub/Catalog tests have suffixes
4. `ConnorsRsiTests` is ambiguous - doesn't tell you if it's testing the series or the catalog

#### Key findings from both examples

1. **Partial class naming**: Inconsistent abbreviation (Dynamic uses "MgDynamic" vs file name "Dynamic")
2. **Test class naming**: **Critical inconsistency** - mixing plain names (Series/Buffer) with suffixes (Hub/Catalog/Regression)
3. **Suffix ambiguity**: `{Indicator}Tests` could mean StaticSeries, Catalog, OR Regression tests
4. **Hub/List/Result naming**: Mostly consistent with `{Indicator}Hub`, `{Indicator}List`, `{Indicator}Result` pattern
5. **Interface naming**: Consistently `I{Indicator}` across all indicators

**Bottom line**: Both examples demonstrate the current problematic state - the recommended patterns with consistent suffixes are the TARGET state, not the current state.

#### Recommended standardization for Phase 4

**Decision point**: Should partial class name match directory name or use abbreviated/full name?

**Option A** - Match directory (simplest):

- `Dynamic.*.cs` files → `partial class Dynamic`
- Rename directory `Dynamic/` → `McGinleyDynamic/` if full name preferred
- **Pro**: File name always matches class name
- **Con**: Loses descriptive "McGinley" context in code

**Option B** - Use full descriptive name (most clear):

- Rename all `Dynamic.*` files → `McGinleyDynamic.*`
- `partial class McGinleyDynamic` (or `MgDynamic` if abbreviated)
- **Pro**: Clear what indicator is in code
- **Con**: Longer file names, more renaming work

**Option C** - Hybrid approach (contextual):

- Keep directory as `Dynamic/` (short, navigable)
- Partial class uses `MgDynamic` (abbreviated but clear)
- Rename files: `Dynamic.*.cs` → `MgDynamic.*.cs`
- **Pro**: Balanced brevity + clarity
- **Con**: Directory ≠ file name (but matches pattern like `e-k/Ema/`)

**Recommended**: Option C (Hybrid) - Matches existing successful patterns like EMA, SMA where directory name is short but class name can be slightly different.

#### Test class naming patterns - current inconsistent state

**Current observed patterns** (INCONSISTENT - needs standardization):

| Test type | Current pattern | Examples | Issue |
|-----------|-----------------|----------|-------|
| StaticSeries | Plain name, no suffix | `Wma`, `Rsi`, `ConnorsRsi` | ❌ No suffix while others have suffixes |
| BufferList | Plain name, no suffix | `Wma`, `Rsi`, `Renko` | ❌ No suffix while others have suffixes |
| StreamHub | `{Indicator}HubTests` | `WmaHubTests`, `RsiHubTests` | ✓ Descriptive suffix |
| Catalog | `{Indicator}Tests` | `WmaTests`, `RsiTests` | ⚠️ Ambiguous - could be Series or Catalog |
| Regression | `{Indicator}Tests` | `WmaTests`, `RsiTests` | ⚠️ Ambiguous - could be Series or Regression |

**Recommended consistent patterns** (ALL use descriptive suffixes mirroring source class suffixes):

| Test type | Recommended pattern | Examples | Benefit |
| --- | --- | --- | --- |
| StaticSeries | `{Indicator}SeriesTests` | `EmaSeriesTests`, `RsiSeriesTests`, `ConnorsRsiSeriesTests` | Mirrors source class suffix (EmaSeries) |
| BufferList | `{Indicator}BufferListTests` | `EmaBufferListTests`, `RsiBufferListTests` | Mirrors source class suffix (EmaList) |
| StreamHub | `{Indicator}HubTests` | `EmaHubTests`, `RsiHubTests` | Mirrors source class suffix (EmaHub) |
| Catalog | `{Indicator}CatalogTests` | `EmaCatalogTests`, `RsiCatalogTests` | Disambiguate from Series tests |
| Regression | `{Indicator}RegressionTests` | `EmaRegressionTests`, `RsiRegressionTests` | Disambiguate from Series tests |

**Dynamic indicator test fixes** (example applying recommended patterns):

| File name | Current class | Recommended class | Action needed |
| --- | --- | --- | --- |
| `Dynamic.StaticSeries.Tests.cs` | `McGinleyDynamic` | `{FinalName}SeriesTests` | Rename + add "SeriesTests" suffix |
| `Dynamic.BufferList.Tests.cs` | `MgDynamic` | `{FinalName}BufferListTests` | Rename + add "BufferListTests" suffix |
| `Dynamic.StreamHub.Tests.cs` | `DynamicHubTests` | `{FinalName}HubTests` | Rename if Task 4.9 changes base name |
| `Dynamic.Catalog.Tests.cs` | `DynamicTests` | `{FinalName}CatalogTests` | Add "Catalog" to disambiguate |

Note: `{FinalName}` determined by Task 4.9 (likely `Dynamic`, `MgDynamic`, or `McGinleyDynamic`)

<!-- markdownlint-enable MD060 -->

**ConnorsRsi example** (shows what standardized pattern looks like):

After applying recommended patterns with Series suffix, ConnorsRsi would be:

- `ConnorsRsi.StaticSeries.cs` → `partial class ConnorsRsiSeries` (add "Series" suffix to partial class)
- `ConnorsRsi.StaticSeries.Tests.cs` → class `ConnorsRsiSeriesTests` (add "SeriesTests" suffix)
- `ConnorsRsi.BufferList.Tests.cs` → class `ConnorsRsiBufferListTests` (add "BufferListTests" suffix)
- `ConnorsRsi.StreamHub.Tests.cs` → class `ConnorsRsiHubTests` ✓ (already correct)
- `ConnorsRsi.Catalog.Tests.cs` → class `ConnorsRsiCatalogTests` (add "Catalog" for clarity)

**Recommended standards (already followed by most indicators)**:

- **StaticSeries**: Plain indicator class name, no suffix (matches `partial class {Name}`)
- **BufferList**: Plain indicator class name, no suffix (matches `partial class {Name}`)
- **StreamHub**: `{Indicator}HubTests` pattern
- **Catalog**: `{Indicator}Tests` pattern
- **Regression**: `{Indicator}Tests` pattern

### A.5: Namespace considerations

**Current**: All files use `namespace FacioQuo.Stock.Indicators;`

**Recommendation**: Keep single namespace for public API simplicity

- **Pros**: Clean public API, no breaking changes, easy to use
- **Cons**: Doesn't reflect file organization
- **Decision**: Maintain current single namespace - file organization is for developer experience, not API design

**Rationale**: .NET library design guidelines prioritize clean public APIs over internal organization. File structure aids development; namespace structure aids consumption.

### A.6: Estimated effort

- **Phase 1**: 6-8 hours (Foundation split) - High complexity due to dependency updates
- **Phase 2**: 3-4 hours (Static class naming) - Medium complexity, requires careful analysis
- **Phase 3**: 8-12 hours (Directory reorganization) - High complexity, many file moves
- **Phase 4**: 24-36 hours (Class and file renaming) - Very high complexity, includes:
  - Adding "Series" suffix to ~70 indicator partial classes
  - Renaming ~210 indicator files to match class names (Hub/List/Result files)
  - Indicator partial class name alignment (Dynamic example)
  - Test class standardization (~280 test classes requiring consistent suffixes)
  - All consuming code and namespace/reference updates throughout codebase
- **Phase 5**: 4-6 hours (Indicator files) - Medium complexity, mostly verification
- **Phase 6**: 2-3 hours (Catalog) - Low complexity, already well-structured
- **Phase 7**: 2-3 hours (Special cases) - Low complexity, edge cases
- **Phase 8**: 6-9 hours (Documentation) - High complexity, comprehensive updates

**Total Estimated Effort**: 55-87 hours

**Risk Factors**:

- Test breakage from namespace or path changes
- Build script dependencies on file paths
- External developer impact if namespaces change
- Merge conflicts with active development

**Mitigation**:

- Execute in feature branch with frequent test runs
- Coordinate with core contributors
- Use automated refactoring tools where possible
- Create comprehensive migration documentation

---

## References

- [.NET Library Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/)
- [Framework Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/)
- [General Naming Conventions](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/general-naming-conventions)
- [Names of Assemblies and DLLs](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-assemblies-and-dlls)
