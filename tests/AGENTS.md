# Test suite

This folder contains unit tests, public-API convergence tests, and integration tests. Performance benchmarks live separately under `tools/performance/`.

## Test organization

- `Library/` — unit test project (`Tests.Indicators.csproj`); `Indicators/` holds per-indicator tests (Series + BufferList + StreamHub + Catalog + Regression), plus `Common/` (utility and cross-cutting tests), `Precision/`, `TestBase/`, and `TestTools/`
- `Integration/` — end-to-end integration tests including `StreamHub.ThreadSafety.Sse.Tests.cs` and `Tests.Integration.csproj`; runs via `tests.integration.runsettings`
- `PublicApi/` — public-API surface tests (`Convergence.*` ensure Series / BufferList / StreamHub agree end-to-end on the public method names; `Customizable/`, `Guide/` cover consumer-facing scenarios)

## Commands

```bash
# Unit tests only
dotnet test tests/Library/ --settings tests/tests.unit.runsettings

# Regression tests only
dotnet test tests/Library/ --settings tests/tests.regression.runsettings

# Integration tests only
dotnet test tests/Integration/ --settings tests/tests.integration.runsettings

# All tests
dotnet test
```

Load #skill:testing-standards for test naming conventions, FluentAssertions patterns, precision requirements, and test base class selection.

## Boundaries

✅ Always write tests before marking an indicator implementation complete

✅ Always verify Stream/Buffer results match Series results for the same inputs

⚠️ Ask before changing baseline test data — run the baseline generation task and review the diff

🚫 Never skip or exclude a failing test without fixing the root cause

🚫 Never add `[Ignore]` attributes without a tracked issue and justification

🚫 Never embed transient plan-item IDs (e.g. `TC001`, `T203`, `G005`, `RG002`) in source, tests, comments, or PR titles. Plan IDs are short-lived working-memory pointers — they get removed when items ship and the section is pruned, leaving dead references behind. Encode the *intent* (test name, comment about a non-obvious constraint) instead. The PR description can reference the plan ID once for traceability, but the committed code should not.
