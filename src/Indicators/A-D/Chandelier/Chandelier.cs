namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Chandelier Exit on a series of bars indicator.
/// </summary>
public static partial class Chandelier
{
    /// <summary>
    /// Calculates the Chandelier Exit for a series of bars.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="multiplier">Multiplier to apply to the ATR.</param>
    /// <param name="type">Type of Chandelier Exit to calculate (Long or Short).</param>
    /// <returns>A read-only list of <see cref="ChandelierResult"/> containing the Chandelier Exit calculation results.</returns>
    public static IReadOnlyList<ChandelierResult> ToChandelier(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 22,
        double multiplier = 3,
        Direction type = Direction.Long)
        => bars
            .ToBarDList()
            .CalcChandelier(lookbackPeriods, multiplier, type);

    /// <summary>
    /// Calculates the Chandelier Exit for a series of bars.
    /// </summary>
    /// <param name="bars">Source list of bars.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="multiplier">Multiplier to apply to the ATR.</param>
    /// <param name="type">Type of Chandelier Exit to calculate (Long or Short).</param>
    /// <returns>A list of <see cref="ChandelierResult"/> containing the Chandelier Exit calculation results.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a parameter is out of the valid range</exception>
    private static List<ChandelierResult> CalcChandelier(
        this List<BarD> bars,
        int lookbackPeriods,
        double multiplier,
        Direction type)
    {
        // check parameter arguments
        Validate(lookbackPeriods, multiplier);

        // initialize
        int length = bars.Count;
        List<ChandelierResult> results = new(length);

        List<AtrResult> atrResult
            = bars.CalcAtr(lookbackPeriods);

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            BarD q = bars[i];

            double? exit = null;

            // add exit values
            if (i >= lookbackPeriods)
            {
                double? atr = atrResult[i].Atr;

                switch (type)
                {
                    case Direction.Long:

                        double maxHigh = 0;
                        for (int p = i + 1 - lookbackPeriods; p <= i; p++)
                        {
                            BarD d = bars[p];
                            if (d.High > maxHigh)
                            {
                                maxHigh = d.High;
                            }
                        }

                        exit = maxHigh - (atr * multiplier);
                        break;

                    case Direction.Short:

                        double minLow = double.MaxValue;
                        for (int p = i + 1 - lookbackPeriods; p <= i; p++)
                        {
                            BarD d = bars[p];
                            if (d.Low < minLow)
                            {
                                minLow = d.Low;
                            }
                        }

                        exit = minLow + (atr * multiplier);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(type));
                }
            }

            results.Add(new(
                Timestamp: q.Timestamp,
                ChandelierExit: exit));
        }

        return results;
    }
}
