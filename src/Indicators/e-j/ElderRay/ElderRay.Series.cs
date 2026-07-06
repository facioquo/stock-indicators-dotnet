namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Elder Ray indicator.
/// </summary>
public static partial class ElderRay
{
    /// <summary>
    /// Converts a list of bars to Elder Ray results.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A list of Elder Ray results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bars list is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public static IReadOnlyList<ElderRayResult> ToElderRay(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 13)
        => bars
            .ToBarDList()
            .CalcElderRay(lookbackPeriods);

    /// <summary>
    /// Calculates the Elder Ray indicator.
    /// </summary>
    /// <param name="bars">Source list of bars.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A list of Elder Ray results.</returns>
    private static List<ElderRayResult> CalcElderRay(
        this List<BarD> bars,
        int lookbackPeriods)
    {
        // check parameter arguments
        Validate(lookbackPeriods);

        // initialize
        int length = bars.Count;
        List<ElderRayResult> results = new(length);

        // EMA
        IReadOnlyList<EmaResult> emaResults
            = bars.ToEma(lookbackPeriods);

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            BarD q = bars[i];
            EmaResult e = emaResults[i];

            results.Add(new(
                Timestamp: e.Timestamp,
                Ema: e.Ema,
                BullPower: q.High - e.Ema,
                BearPower: q.Low - e.Ema));
        }

        return results;
    }
}
