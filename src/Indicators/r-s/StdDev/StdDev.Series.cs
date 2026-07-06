namespace FacioQuo.Stock.Indicators;

/// <summary>
/// standard deviation on a series of data indicator.
/// </summary>
public static partial class StdDev
{
    /// <summary>
    /// Calculates the standard deviation for a series of data.
    /// </summary>
    /// <param name="source">Source data series.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A list of StdDevResult containing the standard deviation, mean, and Z-score for each data point.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when lookbackPeriods is less than 1.</exception>
    public static IReadOnlyList<StdDevResult> ToStdDev(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods = 14)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(source);
        Validate(lookbackPeriods);

        // initialize
        int length = source.Count;
        List<StdDevResult> results = new(length);

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            IReusable s = source[i];

            double mean;
            double stdDev;
            double zScore;

            if (i >= lookbackPeriods - 1)
            {
                double[] values = new double[lookbackPeriods];
                double sum = 0;
                int n = 0;

                for (int p = i + 1 - lookbackPeriods; p <= i; p++)
                {
                    IReusable ps = source[p];
                    values[n] = ps.Value;
                    sum += ps.Value;
                    n++;
                }

                mean = sum / lookbackPeriods;

                stdDev = values.StdDev();

                zScore = stdDev == 0 ? double.NaN
                    : (s.Value - mean) / stdDev;
            }
            else
            {
                mean = double.NaN;
                stdDev = double.NaN;
                zScore = double.NaN;
            }

            StdDevResult r = new(
                Timestamp: s.Timestamp,
                StdDev: stdDev.NaN2Null(),
                Mean: mean.NaN2Null(),
                ZScore: zScore.NaN2Null());

            results.Add(r);
        }

        return results;
    }
}
