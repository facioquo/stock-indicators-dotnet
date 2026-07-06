namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Balance of Power (BOP) on a series of bars indicator.
/// </summary>
public static partial class Bop
{
    /// <summary>
    /// Calculates the Balance of Power (BOP) for a series of bars.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="smoothPeriods">Number of periods to use for smoothing. Default is 14.</param>
    /// <returns>A read-only list of <see cref="BopResult"/> containing the BOP calculation results.</returns>
    public static IReadOnlyList<BopResult> ToBop(
        this IReadOnlyList<IBar> bars,
        int smoothPeriods = 14)
        => bars
            .ToBarDList()
            .CalcBop(smoothPeriods);

    /// <summary>
    /// Calculates the Balance of Power (BOP) for a series of bars.
    /// </summary>
    /// <param name="bars">Source list of bars.</param>
    /// <param name="smoothPeriods">Number of periods to use for smoothing.</param>
    /// <returns>A list of <see cref="BopResult"/> containing the BOP calculation results.</returns>
    private static List<BopResult> CalcBop(
        this List<BarD> bars,
        int smoothPeriods)
    {
        // check parameter arguments
        Validate(smoothPeriods);

        // initialize
        int length = bars.Count;
        List<BopResult> results = new(length);

        double[] raw = bars
            .Select(static x => x.High - x.Low != 0 ?
                (x.Close - x.Open) / (x.High - x.Low) : double.NaN)
            .ToArray();

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            double bop = double.NaN;

            if (i >= smoothPeriods - 1)
            {
                double sum = 0;
                for (int p = i - smoothPeriods + 1; p <= i; p++)
                {
                    sum += raw[p];
                }

                bop = sum / smoothPeriods;
            }

            results.Add(new(
                Timestamp: bars[i].Timestamp,
                Bop: bop.NaN2Null()));
        }

        return results;
    }
}
