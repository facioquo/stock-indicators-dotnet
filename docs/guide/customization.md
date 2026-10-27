---
title: Creating custom indicators
description: Learn how to create your own custom technical indicators using the Stock Indicators for .NET library as a foundation. This guide shows you how to build custom indicators that integrate seamlessly with the library's existing features.
---

# Creating custom indicators

At some point in your journey, you may want to create your own custom indicators. The following guide shows you how to create custom indicators that work seamlessly with this library.

::: warning 🚩 Series (batch) style only
Custom indicators are currently supported for the **Batch (Series)** style only. Creating custom [Buffer list](/guide/styles/buffer) or [Stream hub](/guide/styles/stream) indicators is not yet supported as a first-class extension point; support is planned for a future release (buffer lists: [#2096](https://github.com/facioquo/stock-indicators-dotnet/issues/2096), stream hubs: [#2097](https://github.com/facioquo/stock-indicators-dotnet/issues/2097)). To integrate custom logic with streaming data today, see [Custom observers](/guide/custom-observers).
:::

::: tip Working examples
The [CustomIndicatorsLibrary](https://github.com/facioquo/stock-indicators-dotnet/tree/main/docs/examples/CustomIndicatorsLibrary) project includes an ATR-weighted moving average and alternative Pivot Points with custom fourth- and fifth-level bars added.
:::

## Creating a custom indicator

### Step 1: Create the result class

Create a result record that implements `IReusable` when one property is the natural value for chaining. Use an explicit interface property to map that value.

```csharp
using FacioQuo.Stock.Indicators;

namespace Custom.Stock.Indicators;

// Custom results class
public record AtrWmaResult(
    DateTime Timestamp,
    double? AtrWma = null) : IReusable
{
    double IReusable.Value => AtrWma.Null2NaN();
}
```

### Step 2: Create your custom indicator

Create your custom algorithm following the same patterns as the main library.

```csharp
using FacioQuo.Stock.Indicators;

namespace Custom.Stock.Indicators;

public static class CustomIndicators
{
    public static IReadOnlyList<AtrWmaResult> ToAtrWma(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods)
    {
        // Validate parameters
        ArgumentNullException.ThrowIfNull(bars);

        if (lookbackPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lookbackPeriods),
                "Lookback periods must be greater than 1.");
        }

        // initialize results
        List<AtrWmaResult> results = new(bars.Count);

        // get pre-requisite ATR values
        IReadOnlyList<AtrResult> atrResults = bars.ToAtr(lookbackPeriods);

        // roll through source values
        for (int i = 0; i < bars.Count; i++)
        {
            IBar b = bars[i];
            double atrWma = double.NaN;

            // only do calculations after uncalculable periods
            if (i >= lookbackPeriods - 1)
            {
                double sumWma = 0;
                double sumAtr = 0;

                for (int p = i - lookbackPeriods + 1; p <= i; p++)
                {
                    double close = (double)bars[p].Close;
                    double atr = atrResults[p].Atr ?? double.NaN;

                    sumWma += atr * close;
                    sumAtr += atr;
                }

                atrWma = sumAtr != 0 ? sumWma / sumAtr : double.NaN;
            }

            // add record to results
            results.Add(new AtrWmaResult(
                Timestamp: b.Timestamp,
                AtrWma: atrWma.NaN2Null()));
        }

        return results;
    }
}
```

The method validates arguments, preserves the input order, returns one result per input bar, and represents unavailable values as `null` at the result boundary. It composes the built-in ATR calculation instead of duplicating it.

### Composing an existing indicator

A custom indicator can also adapt built-in results. `ToExtendedPivotPoints()` starts with [Pivot Points](/indicators/pivot-points), retains its existing levels, and derives custom `R4`, `R5`, `S4`, and `S5` bars from the same pivot range:

```csharp
IReadOnlyList<PivotPointsResult> pivots = bars.ToPivotPoints(
    windowSize,
    PivotPointType.Standard);

// p is a warmed-up PivotPointsResult
double r4 = (3 * p.R1.Value) - p.S1.Value - p.PP.Value;
double r5 = (4 * p.R1.Value) - p.S1.Value - (2 * p.PP.Value);
double s4 = (3 * p.S1.Value) - p.R1.Value - p.PP.Value;
double s5 = (4 * p.S1.Value) - p.R1.Value - (2 * p.PP.Value);
```

The complete implementation in [`ExtendedPivotPoints.cs`](https://github.com/facioquo/stock-indicators-dotnet/blob/main/docs/examples/CustomIndicatorsLibrary/ExtendedPivotPoints.cs) handles warmup periods and returns a chainable result record. This pattern is useful for bespoke formulas that belong in application code rather than the shared package.

### Step 3: Use your custom indicator

Use your custom indicator just like the built-in indicators:

```csharp
using Custom.Stock.Indicators;
using FacioQuo.Stock.Indicators;

// Get historical price bars
IReadOnlyList<Bar> bars = GetBarsFromFeed("MSFT");

// Calculate custom indicator
IReadOnlyList<AtrWmaResult> results = bars.ToAtrWma(10);

// Use results
foreach (AtrWmaResult r in results)
{
    Console.WriteLine(
        $"ATR WMA on {r.Timestamp:d} was {r.AtrWma:N4}");
}
```

## Advanced patterns

### Chainable indicators

By implementing `IReusable`, your custom indicator can be chained with other indicators:

```csharp
// Chain your custom indicator with RSI
var rsiOfAtrWma = bars
    .ToAtrWma(10)
    .ToRsi(14);
```

### Best practices

When creating custom indicators:

1. **Validate inputs** — throw `ArgumentNullException` for a null source and `ArgumentOutOfRangeException` for invalid numeric parameters.
2. **Preserve alignment** — return one result per input and keep the caller's ordering.
3. **Represent warmup consistently** — calculate with `double.NaN` internally and convert unavailable result values to `null`.
4. **Guard variable denominators** — return an unavailable value when division is undefined.
5. **Follow naming conventions** — use `To{IndicatorName}` for the extension method and `{IndicatorName}Result` for its result.
6. **Enable chaining deliberately** — implement `IReusable` only when the result has one natural reusable value.
7. **Verify the formula independently** — test against manually calculated values or an authoritative publication.

## Example projects

For complete working examples, see:

- [CustomIndicatorsLibrary](https://github.com/facioquo/stock-indicators-dotnet/tree/main/docs/examples/CustomIndicatorsLibrary) — create custom indicators by composing library calculations
- [CustomIndicatorsUsage](https://github.com/facioquo/stock-indicators-dotnet/tree/main/docs/examples/CustomIndicatorsUsage) — call custom indicators from an application

## See also

- [Custom stream hub observers](/guide/custom-observers) — react to new stream data
- [Utilities and helpers](/utilities/) — tools for working with bars and results
- [Contributing guidelines](/contributing) — contribute broadly reusable indicators to the library
