namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Smoothed Moving Average (SMMA) for a series of data indicator.
/// </summary>
public static partial class Smma
{
    /// <summary>
    /// Calculates the Smoothed Moving Average (SMMA) for a series of data.
    /// </summary>
    /// <param name="source">Source list of data.</param>
    /// <param name="lookbackPeriods">Number of periods to look back for the SMMA calculation.</param>
    /// <returns>A list of <see cref="SmmaResult"/> containing the SMMA values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than 1.</exception>
    public static IReadOnlyList<SmmaResult> ToSmma(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(source);
        Validate(lookbackPeriods);

        // initialize
        int length = source.Count;
        List<SmmaResult> results = new(length);
        double prevSmma = double.NaN;

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            IReusable s = source[i];

            // skip incalculable periods
            if (i < lookbackPeriods - 1)
            {
                results.Add(new(s.Timestamp));
                continue;
            }

            double smma;

            // when no prior SMMA, reset as SMA
            if (double.IsNaN(prevSmma))
            {
                double sum = 0;
                for (int p = i + 1 - lookbackPeriods; p <= i; p++)
                {
                    IReusable ps = source[p];
                    sum += ps.Value;
                }

                smma = sum / lookbackPeriods;
            }

            // normal SMMA
            else
            {
                smma = ((prevSmma * (lookbackPeriods - 1)) + s.Value) / lookbackPeriods;
            }

            results.Add(new SmmaResult(
                Timestamp: s.Timestamp,
                Smma: smma.NaN2Null()));

            prevSmma = smma;
        }

        return results;
    }
}
