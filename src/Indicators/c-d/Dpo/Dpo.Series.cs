namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Detrended Price Oscillator (DPO) indicator.
/// </summary>
public static partial class Dpo
{
    /* REPAINTING INDICATOR
     * The value of DPO at any given point relies on future data points.
     * Therefore, it can only be calculated X number of periods later and retroactively updated.
     * For batch processing in series, this is not an issue since all data is available.
     * For incremental and real-time processing, this means that DPO values are initially
     * 'null' (incalculable) and are updated retroactively as enough data becomes available.
     */

    /// <summary>
    /// Converts a list of source data to Detrended Price Oscillator (DPO) results.
    /// </summary>
    /// <param name="source">List of source data.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A list of DPO results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public static IReadOnlyList<DpoResult> ToDpo(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods = 14)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(source);
        Validate(lookbackPeriods);

        // initialize
        int length = source.Count;
        List<DpoResult> results = new(length);

        int offset = (lookbackPeriods / 2) + 1;

        IReadOnlyList<SmaResult> sma
            = source.ToSma(lookbackPeriods);

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            IReusable src = source[i];

            double? dpoSma = null;
            double? dpoVal = null;

            if (i >= lookbackPeriods - offset - 1 && i < length - offset)
            {
                SmaResult s = sma[i + offset];
                dpoSma = s.Sma;
                dpoVal = s.Sma is null ? null : src.Value - s.Sma;
            }

            DpoResult r = new(
                Timestamp: src.Timestamp,
                Dpo: dpoVal,
                Sma: dpoSma);

            results.Add(r);
        }

        return results;
    }
}
