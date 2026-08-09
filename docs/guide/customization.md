---
title: Creating custom indicators
description: Learn how to create your own custom technical indicators using the Stock Indicators for .NET library as a foundation. This guide shows you how to build custom indicators that integrate seamlessly with the library's existing features.
---

# Creating custom indicators

At some point in your journey, you may want to create your own custom indicators. The following guide shows you how to create custom indicators that work seamlessly with this library.

::: warning 🚩 Series (batch) style only
Custom indicators are currently supported for the **Batch (Series)** style only. Creating custom [Buffer list](/guide/styles/buffer) or [Stream hub](/guide/styles/stream) indicators is not yet supported as a first-class extension point; support is planned for a future release (buffer lists: [#2096](https://github.com/facioquo/stock-indicators-dotnet/issues/2096), stream hubs: [#2097](https://github.com/facioquo/stock-indicators-dotnet/issues/2097)). To integrate custom logic with streaming data today, see [Custom observers](/guide/custom-observers).
:::

> ✨ Working example code is available in the [CustomIndicatorsLibrary](https://github.com/facioquo/stock-indicators-dotnet/tree/main/docs/examples/CustomIndicatorsLibrary) project.

## Creating a custom indicator

### Step 1: Create the result class

Create your results class by implementing the `IReusable` interface or inheriting from existing result patterns. This allows your custom indicator to be chainable with other indicators.

```csharp
using FacioQuo.Stock.Indicators;

namespace Custom.Indicators;

// Custom results class
public record AtrWmaResult : IReusable
{
    public DateTime Timestamp { get; init; }
    public double? AtrWma { get; init; }
    
    // Identify value to propagate in chains
    double IReusable.Value => AtrWma.Null2NaN();
}
```

### Step 2: Create your custom indicator

Create your custom algorithm following the same patterns as the main library.

```csharp
using FacioQuo.Stock.Indicators;

namespace Custom.Indicators;

public static class CustomIndicators
{
    /// <summary>
    /// ATR-weighted moving average (custom indicator example)
    /// </summary>
    /// <param name="bars">Historical price bars</param>
    /// <param name="lookbackPeriods">Lookback period</param>
    /// <returns>Collection of AtrWmaResult</returns>
    public static IReadOnlyList<AtrWmaResult> ToAtrWma(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods)
    {
        // Validate parameters
        ArgumentNullException.ThrowIfNull(bars);
        
        if (lookbackPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lookbackPeriods),
                "Lookback periods must be greater than 0.");
        }

        // sort price bars (optional)
        List<IBar> barsList = bars
            .OrderBy(x => x.Timestamp)
            .ToList();

        // initialize results
        List<AtrWmaResult> results = new(barsList.Count);

        // get pre-requisite ATR values
        List<AtrResult> atrResults = barsList
            .ToAtr(lookbackPeriods)
            .ToList();

        // roll through source values
        for (int i = 0; i < barsList.Count; i++)
        {
            IBar b = barsList[i];
            double atrWma = double.NaN;

            // only do calculations after uncalculable periods
            if (i >= lookbackPeriods - 1)
            {
                double sumWma = 0;
                double sumAtr = 0;

                for (int p = i - lookbackPeriods + 1; p <= i; p++)
                {
                    double close = (double)barsList[p].Close;
                    double atr = atrResults[p].Atr ?? double.NaN;

                    sumWma += atr * close;
                    sumAtr += atr;
                }

                atrWma = sumWma / sumAtr;
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

### Step 3: Use your custom indicator

Use your custom indicator just like the built-in indicators:

```csharp
using FacioQuo.Stock.Indicators;
using Custom.Indicators;

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

### Using multiple indicator styles

You can also implement your custom indicator in other styles:

- **Buffer list style** - For incremental processing
- **Stream hub style** - For real-time data feeds

See the [Guide](/guide/) for more information about different indicator styles.

### Best practices

When creating custom indicators:

1. **Validate inputs** - Always validate parameters and bars
2. **Handle edge cases** - Check for insufficient data, null values
3. **Follow naming conventions** - Use `To{IndicatorName}` pattern
4. **Implement IReusable** - Enable chaining with other indicators
5. **Add XML documentation** - Document parameters and return values
6. **Test thoroughly** - Verify calculations against reference data

## Example projects

For complete working examples, see:

- [CustomIndicatorsLibrary](https://github.com/facioquo/stock-indicators-dotnet/tree/main/docs/examples/CustomIndicatorsLibrary) - Shows how to create custom indicators
- [CustomIndicatorsUsage](https://github.com/facioquo/stock-indicators-dotnet/tree/main/docs/examples/CustomIndicatorsUsage) - Shows how to use custom indicators

## See also

- [Custom stream hub observers](/guide/custom-observers) - React to new stream data
- [Utilities and helpers](/utilities/) - Tools for working with bars and results
- [Contributing guidelines](/contributing) - How to contribute indicators to the library
