namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Choppiness Index (CHOP) on a series of bars indicator.
/// </summary>
public static partial class Chop
{
    /// <summary>
    /// Calculates the Choppiness Index (CHOP) for a series of bars.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A read-only list of <see cref="ChopResult"/> containing the CHOP calculation results.</returns>
    public static IReadOnlyList<ChopResult> ToChop(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14)
        => bars
            .ToBarDList()
            .CalcChop(lookbackPeriods);

    /// <summary>
    /// Calculates the Choppiness Index (CHOP) for a series of bars.
    /// </summary>
    /// <param name="bars">Source list of bars.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A list of <see cref="ChopResult"/> containing the CHOP calculation results.</returns>
    private static List<ChopResult> CalcChop(
        this List<BarD> bars,
        int lookbackPeriods)
    {
        // check parameter arguments
        Validate(lookbackPeriods);

        // initialize
        int length = bars.Count;
        List<ChopResult> results = new(length);
        double[] trueHigh = new double[length];
        double[] trueLow = new double[length];
        double[] trueRange = new double[length];

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            double? chop = null;

            if (i > 0)
            {
                trueHigh[i] = Math.Max(bars[i].High, bars[i - 1].Close);
                trueLow[i] = Math.Min(bars[i].Low, bars[i - 1].Close);
                trueRange[i] = trueHigh[i] - trueLow[i];

                // calculate CHOP

                if (i >= lookbackPeriods)
                {
                    // reset measurements
                    double sum = trueRange[i];
                    double high = trueHigh[i];
                    double low = trueLow[i];

                    // iterate over lookback window
                    for (int j = 1; j < lookbackPeriods; j++)
                    {
                        sum += trueRange[i - j];
                        high = Math.Max(high, trueHigh[i - j]);
                        low = Math.Min(low, trueLow[i - j]);
                    }

                    double range = high - low;

                    // calculate CHOP
                    if (range != 0)
                    {
                        chop = 100 * (Math.Log(sum / range) / Math.Log(lookbackPeriods));
                    }
                }
            }

            results.Add(new(
                Timestamp: bars[i].Timestamp,
                Chop: chop));
        }

        return results;
    }
}
