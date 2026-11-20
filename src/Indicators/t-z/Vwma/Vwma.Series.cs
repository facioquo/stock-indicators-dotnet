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
        => bars.CalcVwma(lookbackPeriods, CandlePart.Close);

    /// <summary>
    /// Calculates VWMA using the selected component of each bar.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="candlePart">Bar component to weight by volume. Default is Close.</param>
    /// <returns>A list of VwmaResult containing the VWMA values.</returns>
    public static IReadOnlyList<VwmaResult> ToVwma(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods,
        CandlePart candlePart = CandlePart.Close)
        => bars.CalcVwma(lookbackPeriods, candlePart);

    /// <summary>
    /// Calculates the VWMA for a series of bars.
    /// </summary>
    /// <param name="bars">Source list of bars.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="candlePart">Bar component to weight by volume.</param>
    /// <returns>A list of VwmaResult containing the VWMA values.</returns>
    private static List<VwmaResult> CalcVwma(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods,
        CandlePart candlePart)
    {
        ArgumentNullException.ThrowIfNull(bars);
        Validate(lookbackPeriods);
        BarParts.Validate(candlePart);

        // initialize
        int length = bars.Count;
        List<VwmaResult> results = [with(length)];

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            IBar q = bars[i];

            double vwma;

            if (i + 1 >= lookbackPeriods)
            {
                double sumCl = 0;
                double sumVl = 0;
                for (int p = i + 1 - lookbackPeriods; p <= i; p++)
                {
                    IBar d = bars[p];
                    double c = d.ToBarPartValue(candlePart);
                    double v = (double)d.Volume;

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
