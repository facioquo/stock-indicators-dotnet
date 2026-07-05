namespace FacioQuo.Stock.Indicators;

/// <summary>
/// T3 moving average indicator.
/// </summary>
public static partial class T3
{
    /// <summary>
    /// Calculates the T3 moving average for a series of data.
    /// </summary>
    /// <param name="source">Source list of data.</param>
    /// <param name="lookbackPeriods">Number of lookback periods.</param>
    /// <param name="volumeFactor">Volume factor.</param>
    /// <returns>A list of T3Result containing the T3 moving average values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    public static IReadOnlyList<T3Result> ToT3(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods = 5,
        double volumeFactor = 0.7)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(source);
        Validate(lookbackPeriods, volumeFactor);

        // initialize
        int length = source.Count;
        List<T3Result> results = new(length);

        double k = 2d / (lookbackPeriods + 1);
        double a = volumeFactor;

        double c1 = -a * a * a;
        double c2 = (3 * a * a) + (3 * a * a * a);
        double c3 = (-6 * a * a) - (3 * a) - (3 * a * a * a);
        double c4 = 1 + (3 * a) + (a * a * a) + (3 * a * a);

        double e1 = double.NaN;
        double e2 = double.NaN;
        double e3 = double.NaN;
        double e4 = double.NaN;
        double e5 = double.NaN;
        double e6 = double.NaN;

        // roll through remaining bars
        for (int i = 0; i < length; i++)
        {
            IReusable s = source[i];

            // re/seed values
            if (double.IsNaN(e6))
            {
                e1 = e2 = e3 = e4 = e5 = e6 = s.Value;
            }

            // first smoothing
            e1 += k * (s.Value - e1);
            e2 += k * (e1 - e2);
            e3 += k * (e2 - e3);
            e4 += k * (e3 - e4);
            e5 += k * (e4 - e5);
            e6 += k * (e5 - e6);

            // T3 moving average
            results.Add(new(
                Timestamp: s.Timestamp,
                T3: ((c1 * e6) + (c2 * e5) + (c3 * e4) + (c4 * e3)).NaN2Null()));
        }

        return results;
    }
}
