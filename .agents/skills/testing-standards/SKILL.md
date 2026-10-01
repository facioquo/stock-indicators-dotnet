---
name: testing-standards
description: Test conventions for the indicator library — base class and test interface per style, required test methods, `MethodName_StateUnderTest_ExpectedBehavior` naming, shared bar fixtures, FluentAssertions and `IsExactly` parity, and `Money*` precision constants. Use when creating or editing any file under `tests/Library/`, when a test class fails to compile for a missing abstract or interface method, or when choosing an assertion tolerance.
---

# Testing standards

Tests use MSTest with FluentAssertions. Base classes and test interfaces live in `tests/Library/TestBase/`; assertion helpers live in `tests/Library/TestTools/TestAssert.cs`.

Load [required test methods and precision constants](references/patterns.md) before writing a new test class or picking a `Money*` tolerance.

## Base class per test file

| Test file | Namespace | Base class | Test interfaces |
| --------- | --------- | ---------- | --------------- |
| `{Name}SeriesTests.cs` | `StaticSeries` | `StaticSeriesTestBase` | none |
| `{Name}BufferListTests.cs` | `BufferLists` | `BufferListTestBase` | `ITestChainBufferList` or `ITestBarBufferList`, plus `ITestCustomBufferListCache` for a custom cache |
| `{Name}HubTests.cs` | `StreamHubs` | `StreamHubTestBase` | `ITestChainObserver` or `ITestBarObserver`, plus `ITestChainProvider` when the hub feeds other hubs |
| `{Name}RegressionTests.cs` | `Regression` | `RegressionTestBase<TResult>` | none |
| `{Name}CatalogTests.cs` | `Catalogging` | `TestBase` | none |

`StaticSeriesTestBase` and `StreamHubTestBase` derive from `TestBaseWithPrecision`, which supplies the `Money*` constants. `BufferListTestBase` and `RegressionTestBase<TResult>` derive from `TestBase` and do not.

## Naming

Name every test method `MethodName_StateUnderTest_ExpectedBehavior`, for example `Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException`.

## Fixtures

Use the static bar sets on `TestBase` instead of loading data: `Bars` (the 502-bar default set from `Data.GetDefault()`), `BadBars`, `Nobars`, `Onebar`, `OtherBars`, `BigBars`, `LongishBars`, `ZeroesBars`, and others. `StreamHubTestBase.RevisedBars` is `Bars` with index 495 removed, for late-arrival and removal tests.

## Assertions

- Assert style parity with `actuals.IsExactly(expected)`, which compares every member in strict order. Never assert parity with `Should().Be()` or a bare `BeEquivalentTo()`.
- Assert manually calculated spot values with `BeApproximately(expected, Money4)` or the tightest `Money*` constant the reference data supports.
- Assert a bounded indicator's documented range with `sut.IsBetween(static x => x.WilliamsR, -100, 0)`, which skips null and NaN values.
- Assert exceptions with `FluentActions.Invoking(() => ...).Should().ThrowExactly<ArgumentOutOfRangeException>()`.

## Running

Unit runs use `tests/tests.unit.runsettings`, which excludes the `Regression` and `Integration` categories. Run regression tests with `--settings tests/tests.regression.runsettings`. The code-completion skill owns the full quality-gate commands and VS Code task names.

## Do not do these

- Do not widen a `Money*` tolerance to absorb floating-point drift between styles; parity is `IsExactly`, and a mismatch is a bug.
- Do not edit a `*.standard.json` regression baseline by hand; regenerate it with `dotnet run --project tools/baselining -- --indicator {UIID}` after an intended calculation change.
- Do not skip a required abstract or interface method by omitting the interface; every new test class declares the interfaces for its style.
