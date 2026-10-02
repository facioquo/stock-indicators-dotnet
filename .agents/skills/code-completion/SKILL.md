---
name: code-completion
description: Quality gates for finishing work in this repository — dead-code cleanup, Roslynator and dotnet format fixes, markdownlint, build, unit tests, documentation, and Obsolete migration shims — with the exact commands and VS Code task names CI mirrors. Use before reporting any implementation, bug fix, or refactor as done, before committing, and when a CI quick-check, lint, or build step fails.
---

# Code completion

Run these gates before reporting any implementation cycle as done. Every command, CI-equivalent, VS Code task label, and configuration file location is in the [quality gates reference](references/quality-gates.md); load it when a gate fails or you need a single-gate command.

CI builds `src/Indicators.csproj` and `tests/Library/Tests.Indicators.csproj` with `-warnAsError`, so a warning that passes a local build fails CI.

## Workflow

### Step 1: Remove dead code

- Delete commented-out code, unused usings, variables, and helpers.
- Strip debugging aids such as `Console.WriteLine` calls.
- Delete scratch files (`.bak`, `.new`, `.debug.*`).

### Step 2: Apply fixers

Run the fixers:

```bash
dotnet tool run roslynator fix --properties TargetFramework=net10.0 --severity-level hidden --verbosity normal
dotnet format --severity info --no-restore
echo y | npx markdownlint-cli2 --fix
```

VS Code task: `Lint: All (fix)` (it builds first). The Roslynator CLI comes from `dotnet-tools.json`; run `dotnet tool restore` first if `roslynator` is not found.

### Step 3: Build

```bash
dotnet build "Stock.Indicators.sln" -v minimal --nologo
```

VS Code task: `Build: .NET Solution (incremental)`. Zero warnings and zero errors.

### Step 4: Run unit tests

```bash
dotnet test tests/Library/Tests.Indicators.csproj --no-restore --nologo --settings tests/tests.unit.runsettings
```

VS Code task: `Test: Unit tests`. The runsettings filter excludes the `Regression` and `Integration` test categories. Run `Test: Regression tests` as well when an indicator's calculation changed.

### Step 5: Update documentation and migration shims

When a public API or indicator behavior changes:

- Update XML documentation on every changed public member.
- Update the indicator page `docs/indicators/{slug}.md` — the documentation skill owns its structure.
- Rename or remove a public v3 API only behind a delegating shim — see [migration shims](#migration-shims).

### Step 6: Verify

Run the read-only checks CI runs, and confirm each exits clean:

```bash
dotnet tool run roslynator analyze --properties TargetFramework=net10.0 --severity-level hidden --verbosity normal
dotnet format --verify-no-changes --severity info --no-restore
echo y | npx markdownlint-cli2
```

VS Code task: `Verify: Quick checks (build + lint + unit test)` covers build, Roslynator, markdownlint, and unit tests; it does not run `dotnet format --verify-no-changes` (`Lint: .NET format` does). When a `{Name}Hub.cs` or its test changed, also run `bash tools/scripts/audit-streamhub.sh`, which CI runs in the quick check.

## Migration shims

Obsolete members live in `src/Obsolete/`, each `[Obsolete]` with a message naming the replacement and `[ExcludeFromCodeCoverage]`:

| File | Holds |
| ---- | ----- |
| `Obsolete.V3.Indicators.cs` | v2 → v3 indicator method renames (`GetX` → `ToX`) |
| `Obsolete.V3.Other.cs` | Other v2 → v3 type and utility bridges |
| `Obsolete.V3.Suppressions.cs` | Assembly-level analyzer suppressions the shims need |
| `Obsolete.V4.cs` | Non-breaking shims for public APIs renamed within v3; removed in v4 |

- A v2 → v3 change also updates `docs/migration/v3.md`.
- A rename within v3 adds a delegating member to `Obsolete.V4.cs` with `[Obsolete("Use … instead.", false)]`, so existing callers compile with a warning.

## Indicator work

A new or changed indicator has a per-style file set (series, buffer, stream, catalog, tests, regression baseline, benchmark, docs page). The indicator-series skill owns that checklist; this skill adds only the gates above.

## Quality bar

- Zero lint findings, build warnings, and build errors.
- All unit tests pass; the repository targets ≥ 98% line coverage, reported to Codacy from CI.
- Public API changes carry XML docs, an updated docs page, and a migration shim.

## Do not do these

- Do not suppress, ignore, or defer a warning to get a gate green; fix the cause.
- Do not run `dotnet test "Stock.Indicators.sln"` as the unit gate. It also runs the regression and integration categories, which are slow and spawn the SSE test server process.
- Do not report work as done from a build alone; the unit tests and the read-only lint checks are part of the bar.
