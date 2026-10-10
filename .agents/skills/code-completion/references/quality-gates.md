# Quality gates reference

Use this when a single gate fails, or you need the VS Code task behind one gate. Each task's exact command is its entry in `.vscode/tasks.json`, and the test runner's options are in `tests/README.md`.

## Gates

| Gate | VS Code task |
| ---- | ------------ |
| Restore .NET tools | `Install: .NET tools` |
| Roslynator | `Lint: .NET Roslynator (fix)`, `Lint: .NET Roslynator (analyze)` |
| Format | `Lint: .NET format (fix)`, `Lint: .NET format` |
| Markdown | `Lint: Markdown (fix)`, `Lint: Markdown` |
| All fixers | `Lint: All (fix)` |
| All linters | `Lint: All` |
| Build | `Build: .NET Solution (incremental)` |
| Unit tests | `Test: Unit tests` |
| Regression tests | `Test: Regression tests` |
| Integration tests | `Test: Integration` |
| StreamHub audit | none; run `tools/scripts/audit-streamhub.sh` |
| Build, lint, unit test | `Verify: Quick checks (build + lint + unit test)` |

## Configuration files

| Concern | Location |
| ------- | -------- |
| Analyzer level and mode | `AnalysisLevel` / `AnalysisMode` in each `.csproj` (e.g., `src/Indicators.csproj`) |
| Rule severities, formatting, Roslynator suppressions | `.editorconfig` |
| Roslynator CLI version | `dotnet-tools.json` |
| Roslynator analyzer package version | `src/Directory.Packages.props` |
| Code-level suppressions | `src/GlobalSuppressions.cs`, `src/Obsolete/Obsolete.V3.Suppressions.cs` |
| Markdown rules | `.markdownlint-cli2.jsonc`, with `docs/.markdownlint-cli2.jsonc` for the docs site |
| Solution | `Stock.Indicators.sln` |
| Package versions | `Directory.Packages.props` in `src/`, `tests/`, and `tools/` |
| CI jobs and steps | `.github/workflows/ci.yml` |
| Test filters | `tests/tests.unit.runsettings`, `tests/tests.regression.runsettings`, `tests/tests.integration.runsettings` |
