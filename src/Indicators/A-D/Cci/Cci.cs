namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Commodity Channel Index (CCI) on a series of bars indicator.
/// </summary>
public static partial class Cci
{
    /// <summary>
    /// Calculates the Commodity Channel Index (CCI) for a series of bars.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A read-only list of <see cref="CciResult"/> containing the CCI calculation results.</returns>
    public static IReadOnlyList<CciResult> ToCci(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 20)
        => bars
            .ToBarDList()
            .CalcCci(lookbackPeriods);

    /// <summary>
    /// Calculates the Commodity Channel Index (CCI) for a series of bars.
    /// </summary>
    /// <param name="bars">Source list of bars.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A list of <see cref="CciResult"/> containing the CCI calculation results.</returns>
    private static List<CciResult> CalcCci(
        this List<BarD> bars,
        int lookbackPeriods)
    {
        // check parameter arguments
        Validate(lookbackPeriods);

        // initialize
        int length = bars.Count;
        List<CciResult> results = new(length);
        double[] tp = new double[length];

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            BarD q = bars[i];
            tp[i] = (q.High + q.Low + q.Close) / 3d;

            double? cci = null;

            if (i + 1 >= lookbackPeriods)
            {
                // average TP over lookback
                double avgTp = 0;
                for (int p = i + 1 - lookbackPeriods; p <= i; p++)
                {
                    avgTp += tp[p];
                }

                avgTp /= lookbackPeriods;

                // average Deviation over lookback
                double avgDv = 0;
                for (int p = i + 1 - lookbackPeriods; p <= i; p++)
                {
                    avgDv += Math.Abs(avgTp - tp[p]);
                }

                avgDv /= lookbackPeriods;

                cci = avgDv == 0 ? null
                    : ((tp[i] - avgTp) / (0.015 * avgDv)).NaN2Null();
            }

            results.Add(new(
                Timestamp: q.Timestamp,
                Cci: cci));
        }

        return results;
    }
}
