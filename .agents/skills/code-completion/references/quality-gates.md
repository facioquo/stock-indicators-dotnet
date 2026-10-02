# Quality gates reference

Use this when a single gate fails, or you need the exact command, VS Code task, or CI step for one gate. Run commands from the repository root.

## Gates

| Gate | Command | VS Code task |
| ---- | ------- | ------------ |
| Restore .NET tools | `dotnet tool restore` | `Install: .NET tools` |
| Roslynator (fix) | `dotnet tool run roslynator fix --properties TargetFramework=net10.0 --severity-level hidden --verbosity normal` | `Lint: .NET Roslynator (fix)` |
| Roslynator (verify) | `dotnet tool run roslynator analyze --properties TargetFramework=net10.0 --severity-level hidden --verbosity normal` | `Lint: .NET Roslynator (analyze)` |
| Format (fix) | `dotnet format --severity info --no-restore` | `Lint: .NET format (fix)` |
| Format (verify) | `dotnet format --verify-no-changes --severity info --no-restore` | `Lint: .NET format` |
| Markdown (fix) | `echo y \| npx markdownlint-cli2 --fix` | `Lint: Markdown (fix)` |
| Markdown (verify) | `echo y \| npx markdownlint-cli2` | `Lint: Markdown` |
| All fixers | Build, then the three fix commands above | `Lint: All (fix)` |
| All linters | Build, then Roslynator, format, and markdown verify | `Lint: All` |
| Build | `dotnet build "Stock.Indicators.sln" -v minimal --nologo` | `Build: .NET Solution (incremental)` |
| Unit tests | `dotnet test tests/Library/Tests.Indicators.csproj --no-restore --nologo --settings tests/tests.unit.runsettings` | `Test: Unit tests` |
| Regression tests | `dotnet test tests/Library/Tests.Indicators.csproj --no-restore --nologo --settings tests/tests.regression.runsettings` | `Test: Regression tests` |
| Integration tests | `dotnet test` on `tests/Integration/Tests.Integration.csproj`, then `tests/PublicApi/Tests.PublicApi.csproj` | `Test: Integration` |
| StreamHub audit | `bash tools/scripts/audit-streamhub.sh` | none |
| Build, lint, unit test | — | `Verify: Quick checks (build + lint + unit test)` |

The VS Code markdown tasks run `pnpm dlx markdownlint-cli2`; CI runs `npx markdownlint-cli2`. Both resolve the same package and read the same configuration.

## What CI runs

`.github/workflows/ci.yml`:

- **quick check** — Release build of `src/Indicators.csproj` with `-warnAsError`, Roslynator analyze on that project, markdownlint, the StreamHub audit, and a Release build of the test project with `-warnAsError`.
- **lint and test** — unit tests with coverage, reported to Codacy.
- **regression tests** — the `Regression` test category.
- **gate** — full solution build, Roslynator analyze, and `dotnet format --verify-no-changes`.

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
| Test filters | `tests/tests.unit.runsettings`, `tests/tests.regression.runsettings`, `tests/tests.integration.runsettings` |
