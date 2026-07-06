namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Force Index indicator.
/// </summary>
public static partial class ForceIndex
{
    /// <summary>
    /// Converts a list of bars to Force Index results.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Number of periods to look back for the calculation. Default is 2.</param>
    /// <returns>A list of Force Index results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bars list is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public static IReadOnlyList<ForceIndexResult> ToForceIndex(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 2)
        => bars
            .ToBarDList()
            .CalcForceIndex(lookbackPeriods);

    /// <summary>
    /// Calculates the Force Index for a list of bars.
    /// </summary>
    /// <param name="bars">Source list of bars.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A list of Force Index results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bars"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    private static List<ForceIndexResult> CalcForceIndex(
        this List<BarD> bars,
        int lookbackPeriods)
    {
        // check parameter arguments
        Validate(lookbackPeriods);

        // initialize
        int length = bars.Count;
        List<ForceIndexResult> results = new(length);
        double? prevFi = null;
        double? sumRawFi = 0;
        double k = 2d / (lookbackPeriods + 1);

        // skip first period
        if (length > 0)
        {
            results.Add(new(bars[0].Timestamp));
        }

        // roll through source values
        for (int i = 1; i < length; i++)
        {
            BarD q = bars[i];
            double? fi = null;

            // raw Force Index
            double? rawFi = q.Volume * (q.Close - bars[i - 1].Close);

            // calculate EMA
            if (i >= lookbackPeriods)
            {
                if (prevFi is null)
                {
                    // first EMA value
                    sumRawFi += rawFi;
                    fi = sumRawFi / lookbackPeriods;
                }
                else
                {
                    fi = prevFi + (k * (rawFi - prevFi));
                }
            }

            // initialization period
            else
            {
                sumRawFi += rawFi;
            }

            results.Add(new ForceIndexResult(
                Timestamp: q.Timestamp,
                ForceIndex: fi));

            prevFi = fi;
        }

        return results;
    }
}
