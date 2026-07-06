namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Simple Moving Average (SMA) indicator.
/// </summary>
public static partial class Sma
{
    /// <summary>
    /// Calculates the Simple Moving Average (SMA) for a given source list and lookback period.
    /// </summary>
    /// <param name="source">Source list to analyze.</param>
    /// <param name="lookbackPeriods">Number of periods to look back for the SMA calculation.</param>
    /// <returns>A read-only list of SMA results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback period is less than 1.</exception>
    public static IReadOnlyList<SmaResult> ToSma(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(source);
        Validate(lookbackPeriods);

        // initialize
        int length = source.Count;
        SmaResult[] results = new SmaResult[length];
        Queue<double> buffer = new(lookbackPeriods);

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            IReusable s = source[i];

            // advance the rolling window and emit once it is full
            buffer.Update(lookbackPeriods, s.Value);

            results[i] = new SmaResult(
                Timestamp: s.Timestamp,
                Sma: buffer.Average(lookbackPeriods).NaN2Null());
        }

        return results;
    }
}
