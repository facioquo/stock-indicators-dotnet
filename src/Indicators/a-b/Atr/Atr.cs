namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods for calculating the
/// Average True Range (ATR) from a series of bars.
/// </summary>
public static partial class Atr
{
    /// <summary>
    /// Calculates Average True Range (ATR) for a series of bars.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A read-only list of ATR results.</returns>
    public static IReadOnlyList<AtrResult> ToAtr(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14)
        => bars
            .ToBarDList()
            .CalcAtr(lookbackPeriods);

    /// <summary>
    /// Calculates the Average True Range (ATR) for a list of bars.
    /// </summary>
    /// <param name="bars">List of bars.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A list of ATR results.</returns>
    internal static List<AtrResult> CalcAtr(
        this List<BarD> bars,
        int lookbackPeriods)
    {
        // check parameter arguments
        Validate(lookbackPeriods);

        // initialize
        int length = bars.Count;
        List<AtrResult> results = new(length);
        double prevAtr = double.NaN;
        double prevClose = double.NaN;
        double sumTr = 0;

        // skip first period
        if (length > 0)
        {
            BarD q = bars[0];
            results.Add(new(Timestamp: q.Timestamp));
            prevClose = q.Close;
        }

        // roll through source values
        for (int i = 1; i < length; i++)
        {
            BarD q = bars[i];

            double hmpc = Math.Abs(q.High - prevClose);
            double lmpc = Math.Abs(q.Low - prevClose);

            double tr = Math.Max(q.High - q.Low, Math.Max(hmpc, lmpc));

            double atr;
            double? atrp;

            if (i >= lookbackPeriods)
            {
                if (double.IsNaN(prevAtr))
                {
                    // initialize ATR
                    sumTr += tr;
                    atr = sumTr / lookbackPeriods;
                }
                else
                {
                    // calculate ATR
                    atr = ((prevAtr * (lookbackPeriods - 1)) + tr) / lookbackPeriods;
                }

                atrp = q.Close == 0 ? null : atr / q.Close * 100;
                prevAtr = atr;
            }

            // only used for initialization periods
            else
            {
                sumTr += tr;

                atr = double.NaN;
                atrp = null;
            }

            AtrResult r = new(
                Timestamp: q.Timestamp,
                Tr: tr.NaN2Null(),
                Atr: atr.NaN2Null(),
                Atrp: atrp);

            results.Add(r);

            prevClose = q.Close;
        }

        return results;
    }
}
