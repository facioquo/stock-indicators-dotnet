namespace FacioQuo.Stock.Indicators;

/// <summary>
/// WMA (Weighted Moving Average) indicator.
/// </summary>
public static partial class Wma
{
    /// <summary>
    /// Calculates the WMA for a series of reusable data.
    /// </summary>
    /// <param name="source">Source list of reusable data.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A list of WmaResult containing the WMA values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than or equal to 0.</exception>
    public static IReadOnlyList<WmaResult> ToWma(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(source);
        Validate(lookbackPeriods);

        // initialize
        int length = source.Count;
        List<WmaResult> results = new(length);

        double divisor = (double)lookbackPeriods * (lookbackPeriods + 1) / 2d;

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            IReusable s = source[i];

            double wma;

            if (i >= lookbackPeriods - 1)
            {
                wma = 0;
                for (int p = i + 1 - lookbackPeriods; p <= i; p++)
                {
                    IReusable ps = source[p];
                    wma += ps.Value * (lookbackPeriods - (i + 1 - p - 1)) / divisor;
                }
            }
            else
            {
                wma = double.NaN;
            }

            results.Add(new(
                Timestamp: s.Timestamp,
                Wma: wma.NaN2Null()));
        }

        return results;
    }
}
