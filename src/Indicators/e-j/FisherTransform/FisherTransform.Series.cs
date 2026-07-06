namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Fisher Transform indicator.
/// </summary>
public static partial class FisherTransform
{
    /// <summary>
    /// Converts a list of source data to Fisher Transform results.
    /// </summary>
    /// <param name="source">List of source data.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A list of Fisher Transform results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public static IReadOnlyList<FisherTransformResult> ToFisherTransform(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods = 10)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        Validate(lookbackPeriods);

        // prefer HL2 when IBar
        IReadOnlyList<IReusable> values
            = source.ToPreferredList(CandlePart.HL2);

        // initialize
        int length = values.Count;
        double[] pr = new double[length]; // median price
        double[] xv = new double[length]; // price transform "value"
        List<FisherTransformResult> results = new(length);

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            IReusable s = values[i];
            pr[i] = s.Value;

            double minPrice = pr[i];
            double maxPrice = pr[i];

            for (int p = Math.Max(i - lookbackPeriods + 1, 0); p <= i; p++)
            {
                minPrice = Math.Min(pr[p], minPrice);
                maxPrice = Math.Max(pr[p], maxPrice);
            }

            double fisher;
            double? trigger = null;

            if (i > 0)
            {
                xv[i] = maxPrice - minPrice != 0
                    ? (0.33 * 2 * (((pr[i] - minPrice) / (maxPrice - minPrice)) - 0.5))
                          + (0.67 * xv[i - 1])
                    : 0d;

                // limit xv to prevent log issues
                xv[i] = xv[i] > 0.99 ? 0.999 : xv[i];
                xv[i] = xv[i] < -0.99 ? -0.999 : xv[i];

                fisher = DeMath.Atanh(xv[i]) + (0.5d * results[i - 1].Fisher);

                trigger = results[i - 1].Fisher;
            }
            else
            {
                xv[i] = 0;
                fisher = 0;
            }

            results.Add(new(
                Timestamp: s.Timestamp,
                Fisher: fisher,
                Trigger: trigger));
        }

        return results;
    }
}
