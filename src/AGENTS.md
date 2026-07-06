# .NET source code development

This folder contains the Stock Indicators library source code.

## Implementation guidance

Load the relevant skill before working in this folder. See the skills index in the root [AGENTS.md](../AGENTS.md#skills-for-development).

For the streaming framework and shared types under `Common/` (StreamHub, BufferLists, Catalog, Bars, aggregator hubs, thread-safety contract, `RollbackState` semantics), see [Common/AGENTS.md](Common/AGENTS.md).

## Technical constraints

**Performance & compatibility:**

- Targets: net10.0, net9.0, net8.0 (all must build and pass tests)
- Complexity: Single-pass O(n) unless mathematically impossible
- Warmup: Provide deterministic WarmupPeriod helper for each indicator
- Precision: Use double for speed; escalate to decimal only when rounding affects financial correctness
- Allocation: Result list + minimal working buffers only
- Thread safety: Stateless calculations are thread-safe; streaming hubs isolate instance state
- Backward compatibility: Renaming public members or altering defaults requires MAJOR version bump

**Error conventions:**

- Use ArgumentOutOfRangeException for invalid numeric parameter ranges
- Use ArgumentException for semantic misuse (e.g., insufficient history)
- Never swallow exceptions; wrap only to add context
- Messages MUST include parameter name and offending value when relevant

## NaN handling policy

This library uses non-nullable double types internally for performance, with intentional NaN propagation:

**Core principles:**

1. Natural propagation - NaN values propagate through calculations (any operation with NaN produces NaN)
2. Internal representation - Use double.NaN internally when a value cannot be calculated
3. External representation - Convert NaN to null (via .NaN2Null()) only at final result boundary
4. No rejection - Never reject NaN inputs; allow them to flow through the system
5. Performance first - Non-nullable double provides significant performance gains

**Implementation guidelines:**

- Division by zero - Guard variable denominators with ternary checks (e.g., `denom != 0 ? num / denom : double.NaN`)
- No epsilon comparisons - Use exact zero comparison (!= 0 or == 0), never epsilon values
- NaN propagation - Accept NaN inputs and allow natural propagation
- State initialization - Use double.NaN for uninitialized state instead of sentinel values

See Common/README.md for complete policy documentation.

## Series as the canonical reference

- Series indicators are the canonical source of truth for numerical correctness
- Series results are based on authoritative publications and manually verified calculations
- Stream and Buffer implementations must match Series results for same inputs once warmed up
- For discrepancies, fix Stream/Buffer unless there is verified issue with Series and reference data

## Result type convention

Indicator result types are `public record` declarations with positional parameters, `Timestamp` first, nullable `double?` for warmup-period values, and `IReusable` implementation when the result is intended to chain into downstream indicators. The reusable value projection is a calculated `Value` property (not a constructor parameter) that calls `.Null2NaN()` so chained NaN propagation behaves predictably.

Canonical reference: `src/Indicators/e-j/Ema/EmaResult.cs` (`EmaResult`). When adding a new indicator, mirror this shape — positional record, `Timestamp` first, `[Serializable]` attribute, `[JsonIgnore]` on the chainable `Value` projection, single-line xmldoc per parameter. Multi-output indicators (e.g. Bollinger Bands, MACD) follow the same skeleton with additional positional parameters; only one property maps to `Value` and that property is the one flagged `isReusable: true` in the catalog listing.

## Cost of a new streamable indicator

A new fully-streamable indicator costs **seven files plus a documentation page**, with ceremony excluding the math itself bounded to roughly 300–400 lines:

| File | Purpose | Size guideline (Ema baseline) |
| ---- | ------- | ------------------------------ |
| `I{Name}.cs` | Public interface | ~15 LOC |
| `{Name}.Models.cs` | Result `record` | ~20 LOC |
| `{Name}.Utilities.cs` | Validation + helpers | ~80 LOC |
| `{Name}.StaticSeries.cs` | Canonical batch implementation | ~60 LOC |
| `{Name}.BufferList.cs` | Incremental `BufferList` form | ~120 LOC |
| `{Name}.StreamHub.cs` | Live `StreamHub` form | ~75 LOC |
| `{Name}.Catalog.cs` | Catalog listing builders (Common/Series/Buffer/Stream) | ~45 LOC |

If a new indicator exceeds these guidelines by a wide margin without algorithmic justification, treat the excess as accidental complexity and look for a missing shared kernel (see `Ema.Increment`, `Sma.Average`, `Tr.Increment`, `Atr.Increment` in `Common/`-adjacent siblings). Documentation under `docs/indicators/{Name}.md` and a test set under `tests/Indicators/Indicators/{a-b|c-d|e-j|k-q|r-s|t-z}/{Name}/*Tests.cs` are required and have their own budgets.

## Boundaries

✅ Always use Series results as the canonical numerical reference — Stream/Buffer must match exactly

✅ Always provide a deterministic `WarmupPeriod` property for every indicator

⚠️ Ask before changing any public API member name, signature, or default value — requires MAJOR version bump

🚫 Never use epsilon comparisons — use exact zero checks (`!= 0`, `== 0`)

🚫 Never swallow exceptions; wrap only to add context

🚫 Never use nullable `double?` internally for performance — use `double.NaN` for uninitialized state
