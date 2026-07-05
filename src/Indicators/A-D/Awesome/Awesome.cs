namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Awesome Oscillator indicator.
/// </summary>
public static partial class Awesome
{
    /// <summary>
    /// Calculates the Awesome Oscillator for a series of data.
    /// </summary>
    /// <param name="source">Source data.</param>
    /// <param name="fastPeriods">Number of periods for the fast moving average. Default is 5.</param>
    /// <param name="slowPeriods">Number of periods for the slow moving average. Default is 34.</param>
    /// <returns>A list of Awesome Oscillator results.</returns>
    public static IReadOnlyList<AwesomeResult> ToAwesome(
        this IReadOnlyList<IReusable> source,
        int fastPeriods = 5,
        int slowPeriods = 34)
    {
        // check parameter arguments
        Validate(fastPeriods, slowPeriods);

        // prefer HL2 when IBar
        IReadOnlyList<IReusable> values
            = source.ToPreferredList(CandlePart.HL2);

        // initialize
        int length = values.Count;
        List<AwesomeResult> results = new(length);
        double[] pr = new double[length];

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            IReusable s = values[i];
            pr[i] = s.Value;

            double? oscillator = null;
            double? normalized = null;

            if (i >= slowPeriods - 1)
            {
                double sumSlow = 0;
                double sumFast = 0;

                for (int p = i + 1 - slowPeriods; p <= i; p++)
                {
                    sumSlow += pr[p];

                    if (p >= i + 1 - fastPeriods)
                    {
                        sumFast += pr[p];
                    }
                }

                oscillator = ((sumFast / fastPeriods) - (sumSlow / slowPeriods)).NaN2Null();
                normalized = pr[i] != 0 ? 100 * oscillator / pr[i] : null;
            }

            AwesomeResult r = new(
                Timestamp: s.Timestamp,
                Oscillator: oscillator,
                Normalized: normalized);

            results.Add(r);
        }

        return results;
    }
}
