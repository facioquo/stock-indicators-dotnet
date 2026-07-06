namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Ulcer Index indicator.
/// </summary>
public static partial class UlcerIndex
{
    /// <summary>
    /// Calculates the Ulcer Index for a series of data.
    /// </summary>
    /// <param name="source">Source list of data.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A list of UlcerIndexResult containing the Ulcer Index values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    public static IReadOnlyList<UlcerIndexResult> ToUlcerIndex(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods = 14)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(source);
        Validate(lookbackPeriods);

        // initialize
        int length = source.Count;
        List<UlcerIndexResult> results = new(length);

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            IReusable s = source[i];

            double? ui;

            if (i + 1 >= lookbackPeriods)
            {
                double sumSquared = 0;
                for (int p = i + 1 - lookbackPeriods; p <= i; p++)
                {
                    IReusable ps = source[p];
                    int dIndex = p + 1;

                    double maxClose = 0;
                    for (int z = i + 1 - lookbackPeriods; z < dIndex; z++)
                    {
                        IReusable zs = source[z];
                        if (zs.Value > maxClose)
                        {
                            maxClose = zs.Value;
                        }
                    }

                    double percentDrawdown = maxClose == 0 ? double.NaN
                        : 100 * ((ps.Value - maxClose) / maxClose);

                    sumSquared += percentDrawdown * percentDrawdown;
                }

                ui = Math.Sqrt(sumSquared / lookbackPeriods).NaN2Null();
            }
            else
            {
                ui = null;
            }

            UlcerIndexResult r = new(
                Timestamp: s.Timestamp,
                UlcerIndex: ui);
            results.Add(r);
        }

        return results;
    }
}
