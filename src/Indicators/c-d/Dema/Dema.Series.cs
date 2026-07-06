namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Double Exponential Moving Average (DEMA) on a series of data indicator.
/// </summary>
public static partial class Dema
{
    /// <summary>
    /// Calculates the Double Exponential Moving Average (DEMA) for a series of data.
    /// </summary>
    /// <param name="source">Source list of data points.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A list of <see cref="DemaResult"/> containing the DEMA values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are not valid.</exception>
    public static IReadOnlyList<DemaResult> ToDema(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods = 14)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(source);
        Validate(lookbackPeriods);

        // initialize
        int length = source.Count;
        List<DemaResult> results = new(length);

        double k = 2d / (lookbackPeriods + 1);
        double lastEma1 = double.NaN;
        double lastEma2 = double.NaN;

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

            double ema1;
            double ema2;

            // when no prior EMA, reset as SMA
            if (double.IsNaN(lastEma2))
            {
                double sum = 0;
                for (int p = i - lookbackPeriods + 1; p <= i; p++)
                {
                    IReusable ps = source[p];
                    sum += ps.Value;
                }

                ema1 = ema2 = sum / lookbackPeriods;
            }

            // normal DEMA
            else
            {
                ema1 = lastEma1 + (k * (s.Value - lastEma1));
                ema2 = lastEma2 + (k * (ema1 - lastEma2));
            }

            results.Add(new DemaResult(
                Timestamp: s.Timestamp,
                Dema: ((2d * ema1) - ema2).NaN2Null()));

            lastEma1 = ema1;
            lastEma2 = ema2;
        }

        return results;
    }

    /// <summary>
    /// Creates a DEMA buffer list from a series of data points.
    /// </summary>
    /// <param name="source">Source list of data points.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A <see cref="DemaList"/> containing the DEMA calculations.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are not valid.</exception>
    public static DemaList ToDemaList(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods = 14)
        => new(lookbackPeriods) { source };
}
