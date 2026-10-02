# v2 test application

`Test.Application` calls the public API of the [`Skender.Stock.Indicators` 2.7.3](https://www.nuget.org/packages/Skender.Stock.Indicators/2.7.3) NuGet package. The package is `FacioQuo.Stock.Indicators` from v3 on, so this app is a working v2 baseline for comparing API surface and results against the current release.

## Running

```bash
dotnet run --project tools/application
```

The VS Code task `Run: Test application` runs the same command. The project targets .NET 10 and references the package directly, outside central package management (`Directory.Packages.props` turns it off for this folder). `TreatWarningsAsErrors` is off because the v2 API raises `[Obsolete]` warnings.

`Program.cs` generates 108 weekday quotes of synthetic OHLCV data, then runs three passes, each printing a success line:

- `TestIndicators()` calls each v2 `Get{Name}()` indicator, with default and custom parameters, on quote and `(DateTime, double)` tuple inputs
- `TestUtilities()` calls the quote utilities: `Use()`, `ToSortedCollection()`, `ToTupleCollection()`, `Aggregate()`, `Validate()`, `ToCandle()`, `ToCandles()`, and `Find()`
- `TestResultUtilities()` calls `Find()`, `ToTupleChainable()`, and `ToTupleNaN()` on results; the last two print a failure line instead of throwing

## Comparing against v3

1. On a branch, replace the package reference in `Test.Application.csproj` with the current `FacioQuo.Stock.Indicators` version and update `GlobalUsings.cs` to the new namespace.
2. Build, then fix each break using the [v3 migration guide](https://dotnet.stockindicators.dev/migration/v3).
3. Compare the output with the v2 run; results should match numerically.

## Keeping coverage current

When a v2 public method is missing from `Program.cs`, add the call and confirm the app still runs cleanly.
