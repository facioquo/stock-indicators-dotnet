# Stock Indicators for .NET

This repository is the production source for the [FacioQuo.Stock.Indicators](https://www.nuget.org/packages/FacioQuo.Stock.Indicators) NuGet package: financial market technical analysis indicators built for accuracy, performance, and ergonomics.

- Multi-targets `net10.0`, `net9.0`, and `net8.0`, with analyzers enforcing strict code quality.
- Every indicator ships in up to three styles: Series (batch), BufferList (incremental), and StreamHub (real-time).
- The `docs/` folder is the source for [dotnet.stockindicators.dev](https://dotnet.stockindicators.dev).

## Primary directive

Provide financial market developers with mathematically exact, high-performance technical analysis indicators they can depend on in production — so they can build trading and analytics applications with full confidence in numerical correctness.

## Secondary directives

1. Surface every indicator's behavior, limitations, and integration path through clear documentation and ergonomic APIs, so developers can discover and use any indicator without guesswork (not as important as primary)
2. Protect downstream users from silent numerical errors through comprehensive validation, deterministic warmup, and ≥ 98% test coverage — ensuring every deployment succeeds (not as important as #1)
3. Ensure all three indicator styles — Series, Buffer, Stream — produce identical results, so developers can switch between batch and real-time APIs without concern for numerical differences (not as important as #2)

## Operating model

Agents are the primary contributors; humans set direction, make architectural decisions, and review outcomes.

- Infer and proceed autonomously; state the assumptions you made.
- Deliver code, tests, and docs together in one pass, and run the quality gates before yielding.

[PRINCIPLES.md](docs/PRINCIPLES.md) holds the six project principles and their rationale; this file holds the operating rules.

## Repository layout

```plaintext
.
├── src/                    # Library source
│   ├── Common/             # Streaming framework, catalog, shared types
│   ├── Indicators/         # Indicators in a-b, c-d, e-j, k-q, r-s, t-z folders
│   ├── Obsolete/           # Migration shims for renamed or removed APIs
│   └── Indicators.csproj
├── tests/                  # Unit, regression, public-API, integration tests
├── tools/                  # Benchmarks, baselines, simulators, scripts
├── docs/                   # Documentation site (VitePress)
├── .agents/skills/         # Agent skills (.claude/skills links here)
└── Stock.Indicators.sln
```

## Commands

```bash
# Build
dotnet build "Stock.Indicators.sln" -v minimal --nologo

# Unit tests (excludes regression and integration categories)
dotnet test tests/Library/Tests.Indicators.csproj --no-restore --nologo --settings tests/tests.unit.runsettings

# Lint and fix (run markdownlint from the repo root so its config loads)
dotnet format --severity info --no-restore
npx markdownlint-cli2 --fix
```

The code-completion skill holds the full quality-gate sequence, Roslynator commands, and VS Code task names.

## Skills

Skills live in `.agents/skills/`; `.claude/skills` is a tracked symlink to that folder. Load the matching skill before working in its area.

| Skill | Load when |
| ----- | --------- |
| code-completion | Finishing any implementation, bug fix, or refactor |
| documentation | Adding or editing a page on the documentation site |
| indicator-buffer | Implementing or changing a BufferList (`{Name}List`) indicator |
| indicator-catalog | Creating or registering catalog listings (`{Name}.Catalog.cs`) |
| indicator-series | Implementing or changing a Series indicator, or adding any new indicator |
| indicator-stream | Implementing or changing a StreamHub (`{Name}Hub`) indicator |
| markdown | Creating or editing any Markdown file |
| performance-testing | Adding benchmarks or investigating performance regressions |
| testing-standards | Writing or debugging tests |
| vitepress | Changing the docs site's VitePress config, theme, or components |

## Folder guidance

Read a folder's AGENTS.md before working in it:

- [src/AGENTS.md](src/AGENTS.md) — implementation constraints, NaN policy, result and facade conventions
- [src/Common/AGENTS.md](src/Common/AGENTS.md) — streaming, buffer, and catalog framework invariants
- [tests/AGENTS.md](tests/AGENTS.md) — test projects and commands
- [docs/AGENTS.md](docs/AGENTS.md) — documentation site
- [.agents/skills/AGENTS.md](.agents/skills/AGENTS.md) — skill authoring
- [.github/AGENTS.md](.github/AGENTS.md) — workflows and GitHub configuration

## Tools

- The `csharp-ls` language server (enabled through the `csharp-lsp` plugin) indexes every project in `Stock.Indicators.sln`. Use it for callers, implementations, and resolved types; use text search for literals, naming sweeps, and non-`.cs` files. It reports structure, not whether the code compiles.
  - Install it globally with `dotnet tool install --global csharp-ls`; language server clients spawn the bare binary from PATH, so `dotnet-tools.json` cannot supply it.
- MCP servers in `.mcp.json`: microsoft-learn for .NET and C# guidance, context7 for third-party library docs, codacy for this repository's static analysis and coverage findings.
- Use the `gh` CLI for workflow runs, pull requests, and issues.

## Pull requests

PR titles follow Conventional Commits: `type: Subject`, ≤ 65 characters, subject starting uppercase — for example `feat: Add RSI indicator`.

Types: feat, fix, docs, style, refactor, perf, test, build, ci, chore, revert, plan.

Do not add `Co-authored-by` trailers to commit messages.

## Boundaries

✅ Always run the quality gates (format, build, test, markdownlint) before marking work complete

✅ Always keep Series results as canonical truth — fix Stream or Buffer to match, never the reverse

⚠️ Ask before renaming or removing any public API member — removal is a MAJOR-version change, and a rename keeps an `[Obsolete]` shim

⚠️ Ask before suppressing any compiler or linter warning — treat every warning as an error

🚫 Never implement beyond what was asked — no tests of tests, no docs describing state that does not exist

🚫 Never copy an indicator calculation from another implementation — cite the authoritative reference and implement from it

🚫 Never merge without every quality gate passing, including for "minor" changes

🚫 Never put ephemeral plan-item IDs (e.g. `TC001`, `T203`) in source, tests, comments, commit messages, or PR titles — reference the GitHub Issue instead
