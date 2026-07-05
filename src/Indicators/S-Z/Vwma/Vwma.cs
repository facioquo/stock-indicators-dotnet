namespace FacioQuo.Stock.Indicators;

/// <summary>
/// VWMA (Volume Weighted Moving Average) indicator.
/// </summary>
public static partial class Vwma
{
    /// <summary>
    /// Calculates the VWMA for a series of bars.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A list of VwmaResult containing the VWMA values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bars"/> is null.</exception>
    public static IReadOnlyList<VwmaResult> ToVwma(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods)
        => bars
            .ToBarDList()
            .CalcVwma(lookbackPeriods);

    /// <summary>
    /// Calculates the VWMA for a series of bars.
    /// </summary>
    /// <param name="bars">Source list of bars.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A list of VwmaResult containing the VWMA values.</returns>
    private static List<VwmaResult> CalcVwma(
        this List<BarD> bars,
        int lookbackPeriods)
    {
        // check parameter arguments
        Validate(lookbackPeriods);

        // initialize
        int length = bars.Count;
        List<VwmaResult> results = new(length);

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            BarD q = bars[i];

            double vwma;

            if (i + 1 >= lookbackPeriods)
            {
                double sumCl = 0;
                double sumVl = 0;
                for (int p = i + 1 - lookbackPeriods; p <= i; p++)
                {
                    BarD d = bars[p];
                    double c = d.Close;
                    double v = d.Volume;

                    sumCl += c * v;
                    sumVl += v;
                }

                vwma = sumVl != 0 ? sumCl / sumVl : double.NaN;
            }
            else
            {
                vwma = double.NaN;
            }

            results.Add(new(
                Timestamp: q.Timestamp,
                Vwma: vwma.NaN2Null()));
        }

        return results;
    }
}
