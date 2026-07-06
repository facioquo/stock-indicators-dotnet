# Testing

Tests are split into different projects for isolation of purpose.

```bash
# runs all unit
# and integration tests
dotnet test
```

> When developing locally, we recommend that you normally _unload_ the external test projects shown below, except when testing externalities.

## Unit tests

> `Indicators/Tests.Indicators.csproj` unit tests library

Our primary full unit test project covers the entire utility of the library.  In most IDE, you can [manually select](https://learn.microsoft.com/en-us/visualstudio/test/configure-unit-tests-by-using-a-dot-runsettings-file?view=vs-2022#manually-select-the-run-settings-file) the `tests/tests.unit.runsettings` for isolation for local IDE dev/test efficiency, or use the _unload_ approach described above.

```bash
# CLI equivalent
dotnet test --settings tests/tests.unit.runsettings
```

## Performance tests

> `tools/performance/Tests.Performance.csproj` benchmark tests

Running the `Tests.Performance` console application in `Release` mode will produce [benchmark performance data](https://dotnet.stockindicators.dev/performance/) that we include on our documentation site.

```bash
# run all performance benchmarks (~15-20 minutes)
dotnet run -c Release

# run individual performance benchmark
dotnet run -c Release --filter *.ToAdx

# run cohorts of performance benchmarks
dotnet run -c Release --filter **
```

```bash
# to see all cohorts
dotnet run --list
...
# Available Benchmarks:
  #0 BufferLists
  #1 SeriesIndicators
  #2 StreamExternal
  #3 StreamIndicators
  #4 Utility
  #5 UtilityMaths

# to see all tests
dotnet run --list flat
```

## External tests

All external integration and API tests can be run with one CLI

```bash
# CLI equivalent
dotnet test --settings tests/tests.integration.runsettings
```

Since we assume tests are non-integration tests by default, set the category attribute on any new test classes that contain integration tests.  This can be applied uniquely to `[TestMethod]` as well.

```csharp
[TestClass, TestCategory("Integration")]
public class MyIntegrationTests : TestBase
...
```

### Public API tests

> `external/tests.Indicators.csproj` exercises real-world scenarios against a directly loaded package.

### Integration tests

- `Indicators/Tests.Indicators.csproj` unit tests the main NuGet library
