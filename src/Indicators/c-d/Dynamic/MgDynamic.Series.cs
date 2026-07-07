namespace FacioQuo.Stock.Indicators;

/// <summary>
/// McGinley Dynamic indicator.
/// </summary>
public static partial class MgDynamic
{
    /// <summary>
    /// Converts a list of source data to McGinley Dynamic results.
    /// </summary>
    /// <param name="source">List of source data.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="kFactor">Smoothing factor for the calculation.</param>
    /// <returns>A list of McGinley Dynamic results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods or kFactor are invalid.</exception>
    public static IReadOnlyList<DynamicResult> ToDynamic(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods,
        double kFactor = 0.6)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(source);
        Validate(lookbackPeriods, kFactor);

        // initialize
        int length = source.Count;
        List<DynamicResult> results = new(length);

        // skip first period
        if (length > 0)
        {
            results.Add(new(source[0].Timestamp, null));
        }

        // roll through source values
        for (int i = 1; i < length; i++)
        {
            double? dyn = Increment(
                lookbackPeriods,
                kFactor,
                newVal: source[i].Value,
                prevDyn: results[i - 1].Dynamic ?? source[i - 1].Value
                ).NaN2Null();

            results.Add(new DynamicResult(
                Timestamp: source[i].Timestamp,
                Dynamic: dyn));
        }

        return results;
    }
}
