namespace FacioQuo.Stock.Indicators;

/// <summary>
/// SuperTrend indicator calculation.
/// </summary>
public static partial class SuperTrend
{
    /// <summary>
    /// Converts a list of bars to a list of SuperTrend results.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="multiplier">Multiplier for the ATR.</param>
    /// <returns>A list of SuperTrend results.</returns>
    public static IReadOnlyList<SuperTrendResult> ToSuperTrend(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 10,
        double multiplier = 3)
        => bars
            .ToBarDList()
            .CalcSuperTrend(lookbackPeriods, multiplier);

    /// <summary>
    /// Calculates the SuperTrend indicator.
    /// </summary>
    /// <param name="bars">Source list of bars.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="multiplier">Multiplier for the ATR.</param>
    /// <returns>A list of SuperTrend results.</returns>
    private static List<SuperTrendResult> CalcSuperTrend(
        this List<BarD> bars,
        int lookbackPeriods,
        double multiplier)
    {
        // check parameter arguments
        Validate(lookbackPeriods, multiplier);

        // initialize
        int length = bars.Count;
        List<SuperTrendResult> results = new(length);
        List<AtrResult> atrResults = bars.CalcAtr(lookbackPeriods);

        bool isBullish = true;
        double? upperBand = null;
        double? lowerBand = null;

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            BarD q = bars[i];

            double? superTrend;
            double? upperOnly;
            double? lowerOnly;

            if (i >= lookbackPeriods)
            {
                double? mid = (q.High + q.Low) / 2;
                double? atr = atrResults[i].Atr;
                double? prevClose = bars[i - 1].Close;

                // potential bands
                double? upperEval = mid + (multiplier * atr);
                double? lowerEval = mid - (multiplier * atr);

                // initial values
                if (upperBand is null)
                {
                    isBullish = q.Close >= mid;

                    upperBand = upperEval;
                    lowerBand = lowerEval;
                }

                // new upper band
                if (upperEval < upperBand || prevClose > upperBand)
                {
                    upperBand = upperEval;
                }

                // new lower band
                if (lowerEval > lowerBand || prevClose < lowerBand)
                {
                    lowerBand = lowerEval;
                }

                // supertrend
                if (q.Close <= (isBullish ? lowerBand : upperBand))
                {
                    superTrend = upperBand;
                    upperOnly = upperBand;
                    lowerOnly = null;
                    isBullish = false;
                }
                else
                {
                    superTrend = lowerBand;
                    lowerOnly = lowerBand;
                    upperOnly = null;
                    isBullish = true;
                }
            }
            else
            {
                superTrend = null;
                upperOnly = null;
                lowerOnly = null;
            }

            results.Add(new(
                Timestamp: q.Timestamp,
                SuperTrend: (decimal?)superTrend,
                UpperBand: (decimal?)upperOnly,
                LowerBand: (decimal?)lowerOnly));
        }

        return results;
    }
}
