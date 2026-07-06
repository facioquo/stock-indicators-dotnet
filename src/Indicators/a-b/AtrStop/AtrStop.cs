namespace FacioQuo.Stock.Indicators;

/// <summary>
/// ATR Trailing Stop indicator.
/// </summary>
public static partial class AtrStop
{
    /// <summary>
    /// Calculates the ATR Trailing Stop (High/Low offset) from a series of bars.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="multiplier">Multiplier for the ATR.</param>
    /// <param name="endType">Candle threshold point to use for reversals.</param>
    /// <returns>A list of ATR Trailing Stop results.</returns>
    public static IReadOnlyList<AtrStopResult> ToAtrStop(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 21,
        double multiplier = 3,
        EndType endType = EndType.Close)
        => bars
            .ToBarDList()
            .CalcAtrStop(lookbackPeriods, multiplier, endType);

    private static List<AtrStopResult> CalcAtrStop(
        this List<BarD> bars,
        int lookbackPeriods,
        double multiplier,
        EndType endType)
    {
        // check parameter arguments
        Validate(lookbackPeriods, multiplier);

        // initialize
        int length = bars.Count;
        List<AtrStopResult> results = new(length);
        List<AtrResult> atrResults = bars.CalcAtr(lookbackPeriods);

        // prevailing direction and bands
        bool isBullish = true;
        double upperBand = double.NaN;
        double lowerBand = double.NaN;

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            // handle warmup periods
            if (i < lookbackPeriods)
            {
                results.Add(new(Timestamp: bars[i].Timestamp));
                continue;
            }

            BarD q = bars[i];
            BarD p = bars[i - 1];

            // evaluate bands
            double upperEval;
            double lowerEval;
            double atr = atrResults[i].Atr ?? double.NaN;

            // potential bands for CLOSE
            if (endType == EndType.Close)
            {
                upperEval = q.Close + (multiplier * atr);
                lowerEval = q.Close - (multiplier * atr);
            }

            // potential bands for HIGH/LOW
            else
            {
                upperEval = q.High + (multiplier * atr);
                lowerEval = q.Low - (multiplier * atr);
            }

            // initialize direction and bands on first evaluation
            if (double.IsNaN(upperBand))
            {
                isBullish = q.Close >= p.Close;
                upperBand = upperEval;
                lowerBand = lowerEval;
            }

            // new upper band: can only go down, or reverse
            if (upperEval < upperBand || p.Close > upperBand)
            {
                upperBand = upperEval;
            }

            // new lower band: can only go up, or reverse
            if (lowerEval > lowerBand || p.Close < lowerBand)
            {
                lowerBand = lowerEval;
            }

            // trailing stop: based on direction

            AtrStopResult r;

            // the upper band (short / buy-to-stop)
            if (q.Close <= (isBullish ? lowerBand : upperBand))
            {
                isBullish = false;

                r = new(
                    Timestamp: q.Timestamp,
                    AtrStop: upperBand,
                    BuyStop: upperBand,
                    SellStop: null,
                    Atr: atr);
            }

            // the lower band (long / sell-to-stop)
            else
            {
                isBullish = true;

                r = new(
                    Timestamp: q.Timestamp,
                    AtrStop: lowerBand,
                    BuyStop: null,
                    SellStop: lowerBand,
                    Atr: atr);
            }

            results.Add(r);
        }

        return results;
    }
}
