using FacioQuo.Stock.Indicators;

namespace Custom.Stock.Indicators;

// Custom results class
// This inherits extension methods, like .RemoveWarmupPeriods()
public record AtrWmaResult
(
    DateTime Timestamp,
    double? AtrWma = null
) : IReusable
{
    /// <inheritdoc/>
    public double Value => AtrWma.Null2NaN();
}

public static class CustomIndicators
{
    // Custom ATR WMA calculation
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

        // perform pre-requisite calculations to get ATR values
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
