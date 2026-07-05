namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Aroon Oscillator indicator.
/// </summary>
public static partial class Aroon
{
    /// <summary>
    /// Calculates the Aroon Oscillator from a series of bars.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A list of Aroon results.</returns>
    public static IReadOnlyList<AroonResult> ToAroon(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 25)
        => bars
            .ToBarDList()
            .CalcAroon(lookbackPeriods);

    /// <summary>
    /// Calculates the Aroon Oscillator for the given source data.
    /// </summary>
    /// <param name="bars">Source data.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A list of Aroon results.</returns>
    private static List<AroonResult> CalcAroon(
        this List<BarD> bars,
        int lookbackPeriods)
    {
        // check parameter arguments
        Validate(lookbackPeriods);

        // initialize
        int length = bars.Count;
        List<AroonResult> results = new(length);

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            BarD q = bars[i];
            double? aroonUp = null;
            double? aroonDown = null;

            // add aroons
            if (i + 1 > lookbackPeriods)
            {
                double? lastHighPrice = 0;
                double? lastLowPrice = double.MaxValue;
                int lastHighIndex = 0;
                int lastLowIndex = 0;

                for (int p = i + 1 - lookbackPeriods - 1; p <= i; p++)
                {
                    BarD d = bars[p];

                    if (d.High > lastHighPrice)
                    {
                        lastHighPrice = d.High;
                        lastHighIndex = p + 1;
                    }

                    if (d.Low < lastLowPrice)
                    {
                        lastLowPrice = d.Low;
                        lastLowIndex = p + 1;
                    }
                }

                aroonUp = 100d * (lookbackPeriods - (i + 1 - lastHighIndex)) / lookbackPeriods;
                aroonDown = 100d * (lookbackPeriods - (i + 1 - lastLowIndex)) / lookbackPeriods;
            }

            AroonResult r = new(
                Timestamp: q.Timestamp,
                AroonUp: aroonUp,
                AroonDown: aroonDown,
                Oscillator: aroonUp - aroonDown);

            results.Add(r);

        }

        return results;
    }
}
