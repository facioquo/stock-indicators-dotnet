---
name: indicator-series
description: Implement Series-style batch indicators with mathematical precision. Use for new StaticSeries implementations or optimization. Series results are the canonical reference—all other styles must match exactly. Focus on cross-cutting requirements and performance optimization decisions.
---

# Series indicator development

## File structure

All files live in `src/Indicators/{category}/{Indicator}/`:

| File | Purpose |
| ---- | ------- |
| `{Indicator}.cs` | Static partial class (main file) — `To{Indicator}()` series entry point |
| `{Indicator}Hub.cs` | Hub class (internal ctor) + `To{Indicator}Hub()` extension |
| `{Indicator}List.cs` | List class + `To{Indicator}List()` extension |
| `{Indicator}.Catalog.cs` | `CommonListing`, `SeriesListing`, `StreamListing`, `BufferListing` |
| `{Indicator}Result.cs` | Result record |
| `{Indicator}.Utilities.cs` | `Validate()` (internal), `Increment()` (public), `RemoveWarmupPeriods()` |
| `I{Indicator}.cs` | Parameter interface (parameter properties only; NOT result properties) |

Test files mirror in `tests/Indicators/{category}/{Indicator}/`:

- `{Indicator}SeriesTests.cs`
- `{Indicator}BufferListTests.cs`
- `{Indicator}HubTests.cs`
- `{Indicator}CatalogTests.cs`
- `{Indicator}RegressionTests.cs`

Category folders: `A-D`, `E-K`, `M-R`, `S-Z` (alphabetical)

## Performance optimization

Array allocation pattern (use for predictable result counts; benchmark first):

```csharp
TResult[] results = new TResult[length];
// ... assign results[i] = new TResult(...);
return new List<TResult>(results);  // NOT results.ToList()
```

Some indicators (e.g., ADL) are faster with `List.Add()` — benchmark both.

## Required implementation

Beyond the main `{Indicator}.cs` series file, ensure:

- [ ] **Catalog registration**: Create `src/**/{Indicator}.Catalog.cs` and register in `Catalog.Listings.cs`
- [ ] **Interface file**: Create `src/**/{Indicator}/I{Indicator}.cs` with parameter properties (NOT result properties)
- [ ] **Unit tests**: Create `tests/Indicators/**/{Indicator}SeriesTests.cs`
  - Inherit from `StaticSeriesTestBase`
  - Verify against manually calculated reference values; assert documented value ranges with `IsBetween` if applicable
- [ ] **Performance benchmark**: Add to `tools/performance/Perf.Series.cs`
- [ ] **Public documentation**: Update `docs/indicators/{Indicator}.md`
- [ ] **Regression baseline tests**: Add to `tests/Indicators/**/{Indicator}RegressionTests.cs` inheriting from `RegressionTestBase<TResult>` with `[TestCategory("Regression")]` on the class — these compare the full result set to a frozen `*.standard.json` baseline so it can be filtered via `--filter TestCategory=Regression`
- [ ] **Migration guide**: Update `docs/migration/v3.md` for notable and breaking changes from v2

## Precision testing

- Store reference data in `{Indicator}.Data.cs` at maximum precision
- Regression: compare full dataset using Money10-Money12
- Spot checks: use Money4
- Document when precision must be lowered due to accumulated floating-point error

## Examples

- Simple: `src/Indicators/S-Z/Sma/Sma.cs`
- Exponential smoothing: `src/Indicators/E-K/Ema/Ema.cs`
- Complex multi-stage: `src/Indicators/A-D/Adx/Adx.cs`
- Multi-value results: `src/Indicators/A-D/Alligator/Alligator.cs`

See [references/decision-tree.md](references/decision-tree.md) for result interface selection.

## Constraints

- Series is canonical truth — BufferList and StreamHub MUST match exactly
- Verify algorithms against authoritative reference publications only
- Never reject NaN inputs; guard against division by zero
- Fix formulas, not symptoms — see src/AGENTS.md
