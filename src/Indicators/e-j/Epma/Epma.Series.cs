namespace FacioQuo.Stock.Indicators;

/// <summary>
/// EPMA (Endpoint Moving Average) indicator.
/// </summary>
public static partial class Epma
{
    /// <summary>
    /// Converts a list of source data to EPMA results.
    /// </summary>
    /// <param name="source">List of source data.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A list of EPMA results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public static IReadOnlyList<EpmaResult> ToEpma(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods = 10)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(source);
        Validate(lookbackPeriods);

        // initialize
        int length = source.Count;
        List<EpmaResult> results = new(length);

        IReadOnlyList<SlopeResult> slope
            = source.ToSlope(lookbackPeriods);

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            results.Add(new EpmaResult(
                Timestamp: slope[i].Timestamp,
                Epma: ((slope[i].Slope * (i + 1)) + slope[i].Intercept).NaN2Null()));
        }

        return results;
    }
}
