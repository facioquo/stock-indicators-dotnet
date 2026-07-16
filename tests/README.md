# Testing

Tests are split into different projects for isolation of purpose.

```bash
# runs all unit
# and integration tests
dotnet test
```

> When developing locally, we recommend that you normally _unload_ the external test projects shown below, except when testing externalities.

## Unit tests

> `Library/Tests.Indicators.csproj` unit tests library

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
  #0 BufferList
  #1 SeriesIndicators
  #2 StreamExternal
  #3 StreamIndicators
  #4 Utility
  #5 UtilityMaths

# to see all tests
dotnet run --list flat
```

## Integration tests

All integration tests are marked with `[TestCategory("Integration")]` and are located in separate test projects (`Tests.Integration` and `Tests.PublicApi`).

To run just the integration tests, you must explicitly use its configuration:

```bash
# CLI equivalent
dotnet test --settings tests/tests.integration.runsettings
```

Since we assume tests are non-integration tests by default, set the category attribute on any new test classes that contain integration tests. This can be applied uniquely to `[TestMethod]` as well.

```csharp
[TestClass, TestCategory("Integration")]
public class MyIntegrationTests : TestBase
...
```

### Public API tests

> `PublicApi/Tests.PublicApi.csproj` E2E library external tests

### Live integration tests

> `Integration/Tests.Integration.csproj` connected to Live 3rd-party APIs

## Linting and code analyzers

### Roslynator

The project uses Roslynator for code analysis and formatting. All analyzer rules and suppressions are configured in the root `.editorconfig` file.

To run Roslynator analysis:

```bash
dotnet tool run roslynator analyze \
--properties TargetFramework=net10.0 \
--verbosity normal \
--severity-level hidden \
--language csharp
```

To apply automatic fixes:

```bash
dotnet tool run roslynator fix \
--properties TargetFramework=net10.0 \
--verbosity normal \
--severity-level hidden \
--language csharp
```

For more information, see:

- [Roslynator CLI documentation](https://josefpihrt.github.io/docs/roslynator/cli/commands/analyze)
- [.NET code style rules](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules)
