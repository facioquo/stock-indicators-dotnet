# Baseline generator

`Test.DataGenerator` writes the regression baseline files that the regression tests in `tests/Library` compare indicator output against. For each Series indicator in the catalog, it runs the indicator with its catalog default parameters on the standard test bars and saves the results as JSON.

## Usage

```bash
# every Series indicator in the catalog, in parallel
dotnet run --project tools/baselining -- --all

# one indicator, by catalog UIID (case-insensitive)
dotnet run --project tools/baselining -- --indicator Sma

dotnet run --project tools/baselining -- --help
```

The VS Code task `Run: Generate test baseline data (all)` runs `--all`. The [Regenerate baselines workflow](../../.github/workflows/regenerate-baselines.yml) runs `--all` in GitHub Actions and opens a pull request with the changes.

The tool exits `1` if any indicator fails, and `0` when every indicator succeeds or is skipped.

## Baseline files

Each file is `tests/Library/TestData/results/{uiid-lowercase}.standard.json`, such as `sma.standard.json`. It holds a JSON array of result objects with camelCase property names, explicit `null` values during warmup, and full double precision, indented for review.

Regenerate baselines after an intentional calculation change, a target framework upgrade, a change to the standard test data, or when adding a Series indicator, then review the diff. See the regression baseline section of the [contributing guide](../../docs/CONTRIBUTING.md#regression-baseline-testing) for how to review baseline changes in a pull request.

## Troubleshooting

- `Indicator '<name>' not found in catalog`: the name must match a catalog UIID with a Series listing in the indicator's `Catalog.cs`.
- `Method name not specified` or `Method '<name>' not found`: the Series listing needs a method name that matches a public static method in the library.

To support a new parameter type, extend `IndicatorExecutor.PrepareParameters()`, which maps each catalog `IndicatorParam` to a method argument.
