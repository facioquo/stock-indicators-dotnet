---
name: code-completion
description: Quality gates checklist for completing code work before finishing implementation cycles
---

# Code completion checklist

Run before finishing any implementation cycle.

## Workflow

### Step 0: Remove dead code

Before running quality gates:

- Delete obsolete or commented-out code
- Remove unused imports, variables, helper methods
- Strip debugging aids (`Console.WriteLine`, breakpoints)
- Remove scratch files (`.bak`, `.new`, `.debug.*`)

### Step 1: Run linters

```bash
dotnet format --severity info --no-restore
npx markdownlint-cli2 --fix
```

Zero errors and zero warnings required. Individual checks:

```bash
dotnet tool run roslynator fix --properties TargetFramework=net10.0 --severity-level hidden --verbosity normal
dotnet format --severity info --no-restore
npx markdownlint-cli2 --fix
```

VS Code task: `Lint: All (fix)`

### Step 2: Build

```bash
dotnet build "Stock.Indicators.sln" -v minimal --nologo
```

VS Code task: `Build: .NET Solution (incremental)`

### Step 3: Test

```bash
dotnet test "Stock.Indicators.sln" --no-restore --nologo
```

VS Code task: `Test: Unit tests`

### Step 4: Update documentation

When changing indicators or public APIs:

- Update XML documentation for changed public APIs
- Update `docs/indicators/{IndicatorName}.md`
- Update `docs/migration/v3.md` for notable and breaking changes from v2
- Update `src/Obsolete.V3.*.cs` for deprecated APIs

### Step 5: Verify

```bash
dotnet format --verify-no-changes --severity info --no-restore
dotnet build "Stock.Indicators.sln" -v minimal --nologo
dotnet test "Stock.Indicators.sln" --no-build --nologo
npx markdownlint-cli2
```

## Quality standards

- Zero linting errors and zero warnings
- Zero build warnings and errors
- All tests pass (coverage ≥ 98% validated in CI/CD)
- Documentation updated for public API changes
- Migration bridges updated for breaking changes

Do not ignore, defer, or suppress warnings.

## Indicator components

New or updated indicators require:

- Series: main `{Indicator}.cs` partial class file
- Catalog: `*.Catalog.cs` + registration
- Tests: `*Tests.cs` with full coverage
- Docs: `docs/indicators/{Name}.md`
- Regression baseline (if algorithm changed)
- Performance benchmark (complex indicators)

## Migration bridge

When changing public APIs:

- Add `[Obsolete]` with migration message
- Update `docs/migration/v3.md`
- Update `src/Obsolete.V3.Indicators.cs` and `src/Obsolete.V3.Other.cs`

See [references/quality-gates.md](references/quality-gates.md) for the quick reference table of all commands and configuration file locations.
