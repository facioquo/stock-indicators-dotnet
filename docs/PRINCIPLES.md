# Stock Indicators project principles

Six core principles govern engineering decisions ([Discussion #648](https://github.com/facioquo/stock-indicators-dotnet/discussions/648)).

## Core principles

### 1. Mathematical precision (NON‑NEGOTIABLE)

All indicators MUST be mathematically correct, deterministic, and reproducible across net8.0, net9.0, net10.0.

**Rules:**

- No silent rounding or implicit unit conversions; use explicit casting when required.
- Default numeric type is double; decimal ONLY when price-sensitive rounding would materially change output.
- Results MUST match validated reference data (published examples, academic definitions, or vetted calculators).
- All implementation styles (Series/batch, BufferList/streaming, StreamHub/streaming) MUST produce bit-for-bit identical final values for deterministic calculations; Series serves as the validated baseline and streaming implementations must match it precisely.
- Any new indicator requires a written spec (math definition + parameter constraints) before implementation.
- Breaking mathematical behavior changes require a MAJOR library version bump and release note callout.

**Formula sourcing hierarchy:**

1. **Primary**: Manual calculation spreadsheets in `tests/Library/` (all implementations MUST match exactly)
2. **Authoritative**: Original creator publications, expert textbooks, reputable institutional specs, cited Wikipedia
3. **Acceptable**: Third-party implementations citing authoritative sources, community-validated calculators
4. **Prohibited**: TradingView ([#801](https://github.com/facioquo/stock-indicators-dotnet/discussions/801)), uncited calculators, sources without formula references

**Documentation requirements:**

`docs/indicators/*.md` SHOULD link to creator's original publication and reputable secondary sources.

**NaN handling**: Use non-nullable `double` with IEEE 754 NaN propagation. See [src/AGENTS.md](../src/AGENTS.md) NaN handling policy section for implementation guidelines.

**Reputation criteria** ([#1024](https://github.com/facioquo/stock-indicators-dotnet/discussions/1024)):

Indicators MUST be created by recognized experts, published in established venues, and time-tested in the financial industry.

### 2. Performance first (CRITICAL)

Low allocation, cache-friendly, single-pass O(n) computations.

**Rules:**

- No per-iteration heap allocations in hot loops; avoid LINQ, prefer `for` loops and `Span<T>`/`ReadOnlySpan<T>`
- Benchmarks MUST accompany algorithmic changes; regressions >2% mean or >5% p95 require justification or revert
- Prove optimizations via BenchmarkDotNet before merging
- Memory growth bounded by input length; no unbounded collections
- Streaming MUST not exceed batch complexity or add >5% overhead

### 3. Comprehensive validation

**Rules:**

- Guard against null inputs, empty sequences, insufficient history with clear `ArgumentException` messages
- Enforce and document parameter constraints (min periods, non-negative lengths)
- Streaming state MUST NOT leak or mutate shared buffers
- Document invariants in XML comments and tests
- Fail fast before allocating result buffers

### 4. Test‑driven quality

**Rules:**

- New indicators: write unit + edge + streaming parity tests before implementation (TDD)
- Bug fixes MUST add regression test proving defect and fix
- Cover public surface behavior (parameters, defaults, warmup); update tests when changed
- Performance baselines updated only when intentional
- CI runs unit, integration, performance benchmarks before release

### 5. Documentation excellence

**Rules:**

- Every public type/member has XML docs (`inheritdoc` for inherited semantics)
- Update `docs/indicators/*.md` when parameters, formulas, warmup, or examples change
- Release notes enumerate breaking changes with migration guidance
- Examples MUST compile and reflect current API

### 6. Scope & stewardship

Source: [Discussion #648](https://github.com/facioquo/stock-indicators-dotnet/discussions/648)

**Rules:**

- **Ease of use**: Clarity over convenience; zero hidden magic
- **Unopinionated**: Implement reputable formulas without reinterpretation
- **Single responsibility**: bars + parameters → results; no signals, data fetching, trading logic, or storage
- **Encapsulation**: No external service calls or third-party packages
- **Broad instruments**: Support equities, commodities, forex, crypto (extreme scales, fractional volume)
- **Simplicity**: Decline features diluting core purpose
- **Community**: Reviewable, traceable, standards-aligned; fast bug/security resolution

## Pull request requirements

- Title: Conventional Commits format; link spec/issue
- Resolve all analyzer warnings or suppress with justification
- New/changed indicators: warmup guidance, parameter constraints, example snippet
- Performance changes: benchmark delta summary (mean, alloc)
- Streaming indicators: prove batch vs streaming parity in tests

Quality gates are enforced via the code-completion skill - agents execute all gates before yielding.

## Governance

- **Authority**: Supersedes ad-hoc conventions
- **Review**: Quarterly (Jan/Apr/Jul/Oct); deviations >1 release require escalation
- **Enforcement**: PR reviewers cite specific principles; violations require immediate follow-up
