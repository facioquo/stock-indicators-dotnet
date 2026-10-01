# Library source

This folder holds the library source. Load the matching skill from the root AGENTS.md before changing an indicator; read [Common/AGENTS.md](Common/AGENTS.md) before changing the streaming, buffer, or catalog framework.

## Technical constraints

- **Targets** — net10.0, net9.0, and net8.0 must all build and pass tests.
- **Complexity** — single-pass O(n) unless mathematically impossible.
- **Warmup** — the count of null warmup results is deterministic; when it is not obvious from the parameters, expose it as a static `WarmupPeriod(...)` helper in `{Name}.Utilities.cs`, as `Hma` and `StochRsi` do.
- **Precision** — use `double` for speed; escalate to `decimal` only when rounding affects financial correctness.
- **Allocation** — the result list plus minimal working buffers only.
- **Thread safety** — Series calculations are stateless; each hub and list isolates its own state.
- **Compatibility** — removing a public member or changing a default is a breaking, MAJOR-version change. A rename keeps the old name as an `[Obsolete]` shim in `Obsolete/` until the next major version.

## Errors

- Throw `ArgumentOutOfRangeException` for an invalid numeric parameter and `ArgumentException` for semantic misuse such as insufficient history.
- Include the parameter name and the offending value in the message.
- Never swallow an exception; wrap one only to add context.

## NaN handling

Calculations use non-nullable `double` internally and let NaN propagate, as IEEE 754 defines.

- Use `double.NaN` for a value that cannot be calculated and for uninitialized state — never a sentinel such as 0 or -1, and never `double?`.
- Accept NaN inputs and let them propagate. Bar validation rejects null or missing bars, never NaN property values.
- Convert NaN to `null` with `.NaN2Null()` only at the result boundary; a chainable `Value` converts back with `.Null2NaN()`.
- Guard a variable denominator with a ternary (`denom != 0 ? num / denom : double.NaN`), choosing NaN, 0, or null by mathematical meaning.
- Compare to zero exactly (`== 0`, `!= 0`); never use an epsilon.

## Series is canonical

Series results come from authoritative publications and manually verified calculations. BufferList and StreamHub results must match Series exactly once warmed up; on a mismatch, fix Buffer or Stream unless the Series and its reference data are verifiably wrong.

## Result types

Mirror `Indicators/e-j/Ema/EmaResult.cs`: a `[Serializable] public record` with positional parameters, `Timestamp` first, `double?` values that are null during warmup, and one-line XML docs per parameter.

- A result meant to chain implements `IReusable` through a calculated `[JsonIgnore] Value` property (not a constructor parameter) that calls `.Null2NaN()`.
- A multi-output result maps exactly one property to `Value` — the one its catalog listing flags `isReusable: true`.

## Per-indicator facade class

`public static partial class {Name}` is the indicator's facade, not its Series implementation. Its partial files hold every style's entry point (`To{Name}`, `To{Name}List`, `To{Name}Hub`), the shared utilities, and the catalog listings.

- Style-specific types carry a suffix: `{Name}Hub`, `{Name}List`, `{Name}Result`, and the `I{Name}` parameter interface.
- Never give the facade a style suffix; an `EmaSeries` class would hold `ToEmaList` and `ToEmaHub`.

## Size budget for a streamable indicator

Ceremony around the math stays near these Ema sizes. A file far over budget without algorithmic cause signals a missing shared kernel — reuse one such as `Ema.Increment`, `Sma.Increment`, `Tr.Increment`, or `Atr.Increment`.

| File | Budget |
| ---- | ------ |
| `I{Name}.cs` | ~15 lines |
| `{Name}Result.cs` | ~20 lines |
| `{Name}.Utilities.cs` | ~80 lines |
| `{Name}.Series.cs` | ~60 lines |
| `{Name}List.cs` | ~120 lines |
| `{Name}Hub.cs` | ~75 lines |
| `{Name}.Catalog.cs` | ~45 lines |

## Boundaries

✅ Always keep warmup length deterministic for given parameters

⚠️ Ask before changing a public API member's name, signature, or default value

🚫 Never use epsilon comparisons

🚫 Never use `double?` for internal state
