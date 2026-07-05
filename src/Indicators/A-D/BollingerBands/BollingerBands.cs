namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Bollinger Bands indicator.
/// </summary>
public static partial class BollingerBands
{
    /// <summary>
    /// Calculates the Bollinger Bands for a series of data.
    /// </summary>
    /// <param name="source">Source list of data.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="standardDeviations">Number of standard deviations to use for the bands.</param>
    /// <returns>A read-only list of <see cref="BollingerBandsResult"/> containing the Bollinger Bands calculation results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    public static IReadOnlyList<BollingerBandsResult> ToBollingerBands(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods = 20,
        double standardDeviations = 2)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(source);
        Validate(lookbackPeriods, standardDeviations);

        // initialize
        int length = source.Count;
        List<BollingerBandsResult> results = new(length);

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            IReusable s = source[i];

            if (i >= lookbackPeriods - 1)
            {
                double[] window = new double[lookbackPeriods];
                double sum = 0;
                int n = 0;

                for (int p = i + 1 - lookbackPeriods; p <= i; p++)
                {
                    IReusable ps = source[p];
                    window[n] = ps.Value;
                    sum += ps.Value;
                    n++;
                }

                double? sma = (sum / lookbackPeriods).NaN2Null();
                double? stdDev = window.StdDev().NaN2Null();

                double? upperBand = sma + (standardDeviations * stdDev);
                double? lowerBand = sma - (standardDeviations * stdDev);

                results.Add(new BollingerBandsResult(

                    Timestamp: s.Timestamp,

                    Sma: sma,
                    UpperBand: upperBand,
                    LowerBand: lowerBand,

                    PercentB: upperBand == lowerBand ? null
                        : (s.Value - lowerBand) / (upperBand - lowerBand),

                    ZScore: stdDev == 0 ? null : (s.Value - sma) / stdDev,
                    Width: sma == 0 ? null : (upperBand - lowerBand) / sma
                ));
            }

            // initization period
            else
            {
                results.Add(new(s.Timestamp));
            }
        }

        return results;
    }
}
