# Test suite

This folder holds the library's test projects. Benchmarks live in `tools/performance/`. Load the testing-standards skill before writing or debugging a test.

| Project | Holds |
| ------- | ----- |
| `Library/Tests.Indicators.csproj` | Unit and regression tests: per-indicator tests under `Indicators/{a-b … t-z}/{Name}/`, plus `Common/`, `Precision/`, `Obsolete/` shim tests, `TestBase/`, and `TestTools/` |
| `PublicApi/Tests.PublicApi.csproj` | Public-surface tests: `Convergence.*` checks that Series, BufferList, and StreamHub agree end to end; `Customizable/` and `Guide/` cover consumer scenarios |
| `Integration/Tests.Integration.csproj` | Tests against live external services, including the SSE thread-safety suite |

`[TestCategory("Regression")]` and `[TestCategory("Integration")]` select the regression and integration tests; the `.runsettings` files filter on them.

## Commands

```bash
# Unit tests (excludes Regression and Integration categories)
dotnet test tests/Library/Tests.Indicators.csproj --settings tests/tests.unit.runsettings

# Regression baselines
dotnet test tests/Library/Tests.Indicators.csproj --settings tests/tests.regression.runsettings

# Public API and integration tests
dotnet test tests/PublicApi/Tests.PublicApi.csproj
dotnet test tests/Integration/Tests.Integration.csproj
```

## Boundaries

✅ Always verify Stream and Buffer results match Series for the same inputs

⚠️ Ask before changing regression baseline data — regenerate it with the `Run: Generate test baseline data (all)` VS Code task and review the diff

🚫 Never skip or exclude a failing test instead of fixing its root cause

🚫 Never add `[Ignore]` without a tracked issue that justifies it
