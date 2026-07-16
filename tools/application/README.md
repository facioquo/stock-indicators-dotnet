# Test Application for Skender.Stock.Indicators v2

## Overview

This test application is designed to exercise all public interfaces of the **Skender.Stock.Indicators v2** NuGet package. `FacioQuo.Stock.Indicators` v3 has already shipped; this app preserves a working v2 baseline so its API surface can be compared against the current release (see [Comparing against v3+](#comparing-against-v3) below).

## Purpose

The primary purposes of this application are:

1. **API Coverage Testing** - Calls every public indicator method and utility function available in v2
2. **Migration Baseline** - Provides a known-good v2 baseline for comparing against v3+ breaking changes
3. **Documentation** - Serves as a living example of v2 API usage patterns
4. **Regression Detection** - Helps identify unexpected behavior changes between versions

## Project Structure

```text
tools/application/
├── Program.cs                    # Main test application with all API calls
├── GlobalUsings.cs              # Global using directives
├── GlobalSuppressions.cs        # Code analysis suppressions
├── Test.Application.csproj      # Project file (references v2 package)
├── Directory.Packages.props     # Package management configuration
└── README.md                    # This file
```

## What It Tests

### Indicators (All Categories)

The application tests all 200+ technical indicators organized alphabetically:

- **A-D Indicators**: ADL, ADX, Alligator, ALMA, Aroon, ATR, ATR Stop, Awesome, Beta, Bollinger Bands, BOP, Candles (Doji, Marubozu), CCI, Chaikin Oscillator, Chandelier, Chop, CMF, CMO, Connors RSI, Correlation, DEMA, Donchian, DPO, Dynamic
- **E-K Indicators**: Elder Ray, EMA, EPMA, FCB, Fisher Transform, Force Index, Fractal, Gator, Heikin-Ashi, HMA, HT Trendline, Hurst, Ichimoku, KAMA, Keltner, KVO
- **M-P Indicators**: MACD, MA Envelopes, MAMA, MFI, OBV, Parabolic SAR, Pivot Points, Pivots, PMO, PRS, PVO
- **R-S Indicators**: Renko, Renko ATR, ROC, ROC with Bands, Rolling Pivots, RSI, Slope, SMA, SMA Analysis, SMI, SMMA, STARC Bands, STC, StdDev Channels, StdDev, Stochastic, Stochastic RSI, SuperTrend
- **T-Z Indicators**: T3, TEMA, TRIX, True Range, TSI, Ulcer Index, Ultimate, Volatility Stop, Vortex, VWAP, VWMA, Williams %R, WMA, ZigZag

### Indicator Variations

Tests include:

- **Default parameters** - Using standard/recommended settings
- **Custom parameters** - Testing with non-default values
- **Quote inputs** - Standard OHLCV data
- **Tuple inputs** - Simplified (DateTime, double) pairs where applicable
- **Enum parameters** - All enumeration types (EndType, BetaType, ChandelierType, MaType, PeriodSize, PivotPointType)

### Utility Methods

- **Quote Utilities**:
  - `Use()` - Extract candle part values
  - `ToSortedCollection()` - Sort and convert quotes
  - `ToTupleCollection()` - Convert to tuples
  - `Aggregate()` - Aggregate to larger periods
  - `Validate()` - Validate quote data
  - `ToCandle()` / `ToCandles()` - Convert to candle properties
  - `Find()` - Find specific quote by date

- **Result Utilities**:
  - `ToTupleChainable()` - Convert reusable results to tuples
  - `ToTupleNaN()` - Convert with NaN handling
  - `Find()` - Find specific result by date

## Test Data Generation

The application generates synthetic test data programmatically:

- **108 trading days** of OHLCV data (skips weekends)
- **Realistic price patterns** with controlled volatility
- **Sufficient history** for indicators requiring 100+ periods
- **Consistent date sequences** for correlation/comparison indicators

## Running the Application

### Prerequisites

- .NET 9.0 SDK or later
- NuGet package: Skender.Stock.Indicators v2

### Build and Run

```bash
# Navigate to the application directory
cd tools/application

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

### Expected Output

When run successfully, the application displays:

```text
===========================================
Testing Skender.Stock.Indicators v2
===========================================

Generating test data...
Generated 108 quotes for testing

Testing indicators...
✓ All indicators tested successfully

Testing utilities...
✓ All utilities tested successfully

Testing result utilities...
  - ToTupleChainable: 89 items
  - ToTupleNaN: 108 items
✓ All result utilities tested successfully

===========================================
All tests completed successfully!
===========================================
```

## Development Notes

### Code Organization

The application is organized into logical methods:

- `GenerateTestData()` - Creates synthetic OHLCV data
- `TestIndicators()` - Exercises all indicator methods
- `TestUtilities()` - Tests quote utility functions
- `TestResultUtilities()` - Tests result transformation methods

### Error Handling

Result utilities (ToTupleChainable, ToTupleNaN) include try-catch blocks to gracefully handle any version-specific behavior differences.

### Analyzer Configuration

The project has:

- **Analyzers enabled** - Uses Roslynator for code quality
- **Warnings NOT treated as errors** - Obsolete warnings are expected during migration testing
- **Code style enforcement** - Follows repository standards

## Comparing against v3+

`FacioQuo.Stock.Indicators` v3 has already shipped (the package was renamed from `Skender.Stock.Indicators` at the v3.0 cutover). To compare v2 baseline behavior against the current release:

1. **Create a comparison branch** from this working v2 version
2. **Update package reference** to `FacioQuo.Stock.Indicators` (current version) in `Test.Application.csproj`
3. **Build and identify errors** - Note all breaking changes
4. **Update code** - Fix breaking changes based on [the v3 migration guide](https://dotnet.stockindicators.dev/migration/v3)
5. **Compare output** - Ensure results remain mathematically equivalent
6. **Document differences** - Track API changes for user communication

## Additional Resources

- [Stock Indicators Documentation](https://dotnet.stockindicators.dev)
- [v2 NuGet Package](https://www.nuget.org/packages/Skender.Stock.Indicators/2.7.3)
- [GitHub Repository](https://github.com/facioquo/stock-indicators-dotnet)
- [Migration Guides](https://github.com/facioquo/stock-indicators-dotnet/discussions)

## Contributing

This test application should be kept in sync with the v2 package. If you discover missing API coverage:

1. Add the missing method call to `Program.cs`
2. Update this README if needed
3. Verify the application runs cleanly
4. Submit a pull request
