namespace FacioQuo.Stock.Indicators;

/// <summary>
/// the Arnaud Legoux Moving Average (ALMA) indicator.
/// </summary>
public static partial class Alma
{
    /// <summary>
    /// Calculates the Arnaud Legoux Moving Average (ALMA) for a series of data.
    /// </summary>
    /// <param name="source">Source series.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="offset">Offset for the ALMA calculation.</param>
    /// <param name="sigma">Sigma for the ALMA calculation.</param>
    /// <returns>A list of ALMA results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the source series is null.</exception>
    public static IReadOnlyList<AlmaResult> ToAlma(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods = 9,
        double offset = 0.85,
        double sigma = 6)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(source);
        Validate(lookbackPeriods, offset, sigma);

        // initialize
        int length = source.Count;
        List<AlmaResult> results = new(length);

        // determine price weight constants
        double m = offset * (lookbackPeriods - 1);
        double s = lookbackPeriods / sigma;

        double[] weight = new double[lookbackPeriods];
        double norm = 0;

        for (int i = 0; i < lookbackPeriods; i++)
        {
            double wt = Math.Exp(-((i - m) * (i - m)) / (2 * s * s));
            weight[i] = wt;
            norm += wt;
        }

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            double alma = double.NaN;

            if (i + 1 >= lookbackPeriods)
            {
                double weightedSum = 0;
                int n = 0;

                for (int p = i + 1 - lookbackPeriods; p <= i; p++)
                {
                    IReusable ps = source[p];
                    weightedSum += weight[n] * ps.Value;
                    n++;
                }

                alma = weightedSum / norm;
            }

            results.Add(
            new(Timestamp: source[i].Timestamp,
                Alma: alma.NaN2Null()));
        }

        return results;
    }
}
