# Result and input interface selection

Use this when writing a `{Name}Result.cs` record or choosing the `this` parameter type of `To{Name}()`.

## Result interface

Every result is a `[Serializable] public record` with positional parameters, `Timestamp` first, and nullable values (`double?` for numbers) that are `null` during warmup. `src/Indicators/e-j/Ema/EmaResult.cs` is the canonical shape.

| Result shape | Interface | `Value` maps to |
| ------------ | --------- | --------------- |
| One output | `IReusable` | That output |
| Several outputs, one of them the natural chaining value | `IReusable` | The primary output (MACD line, not Signal or Histogram) |
| Several outputs, none a natural chaining value | `ISeries` | No `Value` property |

`Value` is a computed property, never a constructor parameter, marked `[JsonIgnore]` and returning `.Null2NaN()` so chained NaN propagation behaves predictably:

```csharp
[Serializable]
public record EmaResult
(
    DateTime Timestamp,
    double? Ema = null
) : IReusable
{
    [JsonIgnore]
    public double Value => Ema.Null2NaN();
}
```

```csharp
[Serializable]
public record AlligatorResult
(
    DateTime Timestamp,
    double? Jaw,
    double? Teeth,
    double? Lips
) : ISeries;
```

The property that `Value` returns is the one result flagged `isReusable: true` in the catalog listing; an `ISeries` result flags none.

## Input type

`IBar` extends `IReusable`, so an indicator taking `IReadOnlyList<IReusable>` also accepts bars and chains from any `IReusable` result.

| The calculation needs | `this` parameter | Examples |
| --------------------- | ---------------- | -------- |
| One value per period | `IReadOnlyList<IReusable> source` | EMA, SMA, RSI, MACD |
| Open, high, low, or volume | `IReadOnlyList<IBar> bars` | ATR, ADX, Stochastic, Chandelier |

Choose `IReusable` whenever one value per period is enough; taking `IBar` without needing its fields blocks chaining for no gain. The Series input type fixes the BufferList increment interface and the StreamHub provider type.
