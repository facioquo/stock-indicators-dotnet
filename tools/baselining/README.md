# Baseline Generator

A console application for generating regression baseline files for Stock Indicators StaticSeries indicators.

## Overview

The Baseline Generator executes all StaticSeries indicators using standard test data and captures their outputs as JSON baseline files. These baseline files are used by regression tests to detect unintended behavioral changes in indicator calculations.

## Usage

### Generate all baselines

```bash
dotnet run --project tools/baselining -- --all
```

This will:

- Discover all StaticSeries indicators from the catalog (currently 84 indicators)
- Execute each indicator with default parameters using standard test data
- Generate baseline JSON files colocated with indicator tests
- Display progress and summary statistics
- Complete in approximately 1-2 minutes

### Generate baseline for a single indicator

```bash
dotnet run --project tools/baselining -- --indicator <name>
```

Example:

```bash
dotnet run --project tools/baselining -- --indicator Sma
```

### Display help

```bash
dotnet run --project tools/baselining -- --help
```

## Baseline File Format

Baseline files are stored as `{IndicatorName}.Baseline.json` in the same directory as the indicator's test files.

Example location: `tests/Indicators/S-Z/Sma/Sma.Baseline.json`

Format:

- Direct JSON array of indicator result objects
- camelCase property naming
- Explicit null values during warmup periods
- Full double precision for numeric values
- Indented for human readability

For additional guidance on regression baselines and how they are used in this repository, see the "Regression baseline testing" section in the contributing guide.

Related reading in regression testing section of the [contributing guidelines](../../docs/CONTRIBUTING.md).

## When to Regenerate Baselines

Baselines should be regenerated when:

1. **Intentional algorithm changes** - After modifying indicator calculation logic
2. **.NET version upgrades** - When upgrading target frameworks
3. **Test data changes** - When modifying standard test datasets
4. **New indicators** - When adding new StaticSeries indicators

## Troubleshooting

### "Indicator not found in catalog"

- Verify the indicator name matches the UIID in the catalog (case-insensitive)
- Check that the indicator has a SeriesListing in its Catalog.cs file

### "Method not found"

- Verify the indicator has a MethodName specified in its catalog listing
- Check that the method is a generic extension method with IBar constraint

### "Parameter count mismatch"

- Some indicators have complex parameter signatures not yet supported
- Known cases: PIVOT-POINTS, PIVOTS (use DateOnly parameters)
- These will be addressed in future updates

### "SeriesParameter not supported"

- Indicators requiring IReusable lists are currently skipped
- Known cases: BETA, CORR, PRS
- These require special handling and will be supported in future updates

### "Type conversion error"

- Some indicators require decimal parameters instead of double
- Known cases: RENKO, ZIGZAG-CLOSE
- These require type-aware parameter preparation

## Performance

- Full baseline generation: ~1-2 minutes for 76+ indicators
- Single indicator: <5 seconds
- Parallel execution with `Parallel.ForEach`
- Memory efficient - processes one indicator at a time

## Exit Codes

- `0` - Success (all indicators processed successfully or skipped gracefully)
- `1` - Failure (one or more indicators failed to generate baselines)

## Contributing

### Adding Support for New Indicator Types

To support indicators with special parameter types:

1. Update `IndicatorExecutor.PrepareParameters()` to handle the parameter type
2. Add type detection logic based on `IndicatorParam.DataType`
3. Provide appropriate test data or parameter values
4. Update this README with the new supported types

### Testing Changes

After modifying the generator:

1. Test with single indicator: `--indicator Sma`
2. Test with batch generation: `--all`
3. Verify baseline files are correctly formatted
4. Check that existing regression tests still pass
