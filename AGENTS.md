# Stock Indicators for .NET

This repository hosts **Stock Indicators for .NET**, the production source for the <a href="https://www.nuget.org/packages/FacioQuo.Stock.Indicators">FacioQuo.Stock.Indicators</a> NuGet package. The library offers financial market technical analysis indicators with a focus on accuracy, performance, and ergonomics for financial analytics.

- Multi-targets `net10.0`, `net9.0`, and `net8.0` with analyzers enabled for strict code quality.
- Active development expands streaming indicator support — see [src/Common/AGENTS.md](src/Common/AGENTS.md) for the framework guidance before modifying stateful pipelines (BufferList or StreamHub).
- Documentation at <a href="https://dotnet.stockindicators.dev">dotnet.stockindicators.dev</a> is sourced from the `docs/` content in this repository.

## Primary directive

Provide financial market developers with mathematically exact, high-performance technical analysis indicators they can depend on in production — so they can build trading and analytics applications with full confidence in numerical correctness.

## Secondary directives

1. Surface every indicator's behavior, limitations, and integration path through clear documentation and ergonomic APIs, so developers can discover and use any indicator without guesswork (not as important as primary)
2. Protect downstream users from silent numerical errors through comprehensive validation, deterministic warmup, and ≥ 98% test coverage — ensuring every deployment succeeds (not as important as #1)
3. Ensure all three indicator styles — Series, Buffer, Stream — produce identical results, so developers can switch between batch and real-time APIs without concern for numerical differences (not as important as #2)

## AI-first development model

This repository is optimized for AI agent contributions. Agents are the primary contributors; humans set direction, review, and validate outcomes.

- **Agents**: infer and proceed autonomously, generate complete implementations (code + tests + docs) in one pass, execute quality gates before yielding, document assumptions
- **Humans**: set strategic direction, make architectural decisions, review agent output, validate outcomes — not micromanage implementation
- **Voice**: imperative, present tense; no historical context; outcome-focused

## Guiding principles

This library follows six core principles that balance usability, performance, precision, and security: **Mathematical Precision** (non-negotiable), **Performance First** (critical), **Comprehensive Validation**, **Test-Driven Quality**, **Documentation Excellence**, and **Scope & Stewardship**.

See [PRINCIPLES.md](docs/PRINCIPLES.md) for constitutional philosophy and rationale. This file (AGENTS.md) provides operational implementation guidance.

## Repository layout

```plaintext
(root)
├── src/                   # Library source code
│    ├── Common/           # Shared utilities, base classes, and common types
│    ├── Indicators/       # Indicator implementations (a-b … t-z groups)
│    └── Indicators.csproj # Main project file
├── tests/                 # Unit, integration, performance, and simulation suites
├── tools/                 # Performance benchmarks, baselines, and analysis scripts
├── docs/                  # Public documentation site (VitePress)
└── Stock.Indicators.sln   # Primary solution for src + tests
```

## Commands

```bash
# Build
dotnet build "Stock.Indicators.sln" -v minimal --nologo

# Test (unit; integration tests run in CI only)
dotnet test "Stock.Indicators.sln" --no-restore --nologo

# Lint: .NET format
dotnet format --severity info --no-restore

# Lint: Markdown
npx markdownlint-cli2 --fix

# All quality gates (abbreviated)
dotnet format --no-restore && dotnet build && dotnet test --no-restore && npx markdownlint-cli2
```

See the code-completion skill for the complete quality gates checklist, Roslynator commands, and VS Code task equivalents.

## Code navigation

A C# language server (`csharp-ls`) indexes the whole `Stock.Indicators.sln` — `src/`, `tests/`, and `tools/`. Agents with language server support resolve symbols through it rather than inferring structure from text search.

Prerequisite: `dotnet tool install --global csharp-ls` — installed globally, not from `dotnet-tools.json`, because language server clients spawn the bare binary from PATH. Run the `Install: .NET tools` VS Code task or `.devcontainer/post-create.sh` and it is handled.

| Use the language server for | Use text search for |
| --------------------------- | ------------------- |
| Every caller of a public API member, before renaming or removing it | Literal strings, numeric constants, and test data |
| Implementations of `IStreamHub`, `IBufferList`, and the `StreamHub/Providers/` base classes | Markdown, YAML, JSON, and `.csproj` content |
| The Series, Buffer, and Stream surface for one indicator | Naming-convention sweeps across many files |
| Resolved types and XML documentation at a call site | Anything outside `.cs` files |

The language server reports what the solution *is*, not whether it compiles. `dotnet format`, `dotnet build`, Roslynator, and `dotnet test` remain the only correctness gates.

## Skills for development

This repository uses Agent Skills (.agents/skills/) for domain-specific guidance. Skills are automatically loaded when relevant:

| Skill | Description | When to use |
| ----- | ----------- | ----------- |
| indicator-series | Series indicator development - mathematical precision, validation patterns, test coverage | Implementing new Series indicators, validating calculations, structuring tests |
| indicator-buffer | BufferList indicator development - incremental processing, interface selection, buffer management | Implementing BufferList indicators, choosing interfaces, managing state efficiently |
| indicator-stream | StreamHub indicator development - implementation patterns, provider selection, state management | Implementing StreamHub indicators, choosing provider base classes, optimizing real-time processing |
| indicator-catalog | Catalog entry creation and registration | Creating indicator catalog entries for automation |
| performance-testing | Benchmarking with BenchmarkDotNet, regression detection | Adding performance tests, optimizing indicator performance |
| code-completion | Quality gates checklist for completing code work | Before finishing any implementation, bug fix, or refactoring |
| testing-standards | Test naming, FluentAssertions, Series parity | Writing comprehensive tests, debugging test failures |
| documentation | Writing and updating VitePress indicator documentation pages | Adding a new indicator doc page, updating an existing one, structural consistency audits |
| vitepress | VitePress documentation site development - configuration, routing, theme, components | Working on the docs/ site, VitePress config, or custom theme |
| markdown | Markdown authoring, linting workflow, formatting rules, validation checklist | Creating or modifying any Markdown file |

Skills are defined in .agents/skills/ following the Agent Skills specification. `.claude/skills` is a tracked symlink to that folder, so Claude Code loads the same skills from its own conventional path. Edit skills in `.agents/skills/` only; never add files under `.claude/skills/`.

## Folder-specific guidance

Subfolder AGENTS.md files provide domain-specific context:

- Documentation site: docs/AGENTS.md for VitePress development
- Source code: src/AGENTS.md for implementation constraints
- Test suite: tests/AGENTS.md for test organization

## MCP tools guidance

MCP servers configured in .mcp.json provide research and documentation lookup:

- microsoft-learn/* - C# coding conventions, .NET best practices, performance optimization
- context7/* - NuGet package dependencies and external libraries
- codacy/* - static analysis findings and coverage for this repository

Use the `gh` CLI for CI workflow runs, pull requests, and issues.

Use MCP tools for external research, the language server for local symbol navigation, and direct file operations for local edits.

## Pull request guidelines

PR titles follow Conventional Commits format: `type: Subject` (≤65 characters, subject starts uppercase).

Types: feat, fix, docs, style, refactor, perf, test, build, ci, chore, revert, plan.

Examples: `feat: Add RSI indicator`, `fix: Resolve MACD calculation error`, `docs: Update API documentation`

Do not add "Co-authored-by" trailers to commit messages.

## Boundaries

✅ Always run quality gates (format + build + test + markdownlint) before marking work complete

✅ Always load the relevant skill before working in a domain area

✅ Always keep Series results as canonical truth — fix Stream/Buffer to match, not the reverse

✅ Always run a language server reference search before renaming or removing a public API member

⚠️ Ask before renaming or removing any public API member — requires a MAJOR version bump

⚠️ Ask before suppressing any compiler or linting warning — treat all warnings as errors

🚫 Never over-extrapolate an implementation — for example, never write tests of tests or write about non-existant state

🚫 Never duplicate indicator calculations from authoritative sources — cite and implement from reference

🚫 Never merge without all quality gates passing — no exceptions for "minor" changes

🚫 Never embed transient or ephemeral plan-item IDs (e.g. `TC001`, `T203`, `G005`, `RG002`) in source files, tests, comments, commit messages, or PR titles. Track that kind of work in GitHub Issues instead — reference the issue number, not an internal ID.
