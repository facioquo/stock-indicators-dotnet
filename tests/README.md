# Testing

Tests are split into three projects. The `.runsettings` files select tests by `[TestCategory]`.

The projects run on [Microsoft.Testing.Platform](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-intro), selected in `global.json`. `dotnet test` takes the MSTest runner's options, so VSTest-only options such as `--nologo`, `--logger`, and `--collect` fail with exit code 5; use `--report-trx` and `--coverage` instead.

| Project | Holds | Command | VS Code task |
| ------- | ----- | ------- | ------------ |
| `Library/Tests.Indicators.csproj` | Unit tests | `dotnet test tests/Library/Tests.Indicators.csproj --settings tests/tests.unit.runsettings` | `Test: Unit tests` |
| `Library/Tests.Indicators.csproj` | Regression tests (`[TestCategory("Regression")]`) | `dotnet test tests/Library/Tests.Indicators.csproj --settings tests/tests.regression.runsettings` | `Test: Regression tests` |
| `PublicApi/Tests.PublicApi.csproj` | End-to-end tests of the public API surface | `dotnet test tests/PublicApi/Tests.PublicApi.csproj` | `Test: Integration` |
| `Integration/Tests.Integration.csproj` | Tests against live external services, including the SSE thread-safety suite | `dotnet test tests/Integration/Tests.Integration.csproj` | `Test: Integration` |

The library test project targets .NET LTS by default, so the local unit and regression commands run on .NET 10. The CI matrix runs a matrix on other supported target frameworks.

`Test: All (library)` runs unit, regression, and integration tests in sequence. In an IDE, [select a `.runsettings` file](https://learn.microsoft.com/en-us/visualstudio/test/configure-unit-tests-by-using-a-dot-runsettings-file#manually-select-the-run-settings-file) such as `tests/tests.unit.runsettings` to keep test runs to unit tests.

## Integration tests

Mark any test class that calls an external service, so the unit run excludes it. The attribute also works on a single `[TestMethod]`.

```csharp
[TestClass, TestCategory("Integration")]
public class MyIntegrationTests : TestBase
```

Tests that fetch live bars from Alpaca read `ALPACA_KEY` and `ALPACA_SECRET` from the environment and report inconclusive when either is missing.

## Regression baselines

Regression tests compare indicator output with the JSON files in `Library/TestData/results/`. To regenerate them, see the [baseline generator](../tools/baselining/README.md).

## Performance benchmarks

Benchmarks live in `tools/performance`, not in this folder. See the [benchmarking guide](../tools/performance/benchmarking.md).

## Code analysis

Roslynator and the .NET analyzers run against rules in the root `.editorconfig`. Run them with the `Lint: .NET Roslynator (analyze)` and `Lint: .NET Roslynator (fix)` VS Code tasks; the [code-completion skill](../.agents/skills/code-completion/SKILL.md) lists the equivalent CLI commands.
