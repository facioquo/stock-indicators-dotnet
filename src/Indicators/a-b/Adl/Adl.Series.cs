namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Accumulation/Distribution Line (ADL) indicator.
/// </summary>
public static partial class Adl
{
    /// <summary>
    /// Calculates the Accumulation/Distribution Line (ADL) from a series of bars.
    /// </summary>
    /// <param name="source">Source list of bars.</param>
    /// <returns>A read-only list of ADL results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    public static IReadOnlyList<AdlResult> ToAdl(this IReadOnlyList<IBar> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        // initialize
        int length = source.Count;
        List<AdlResult> results = new(length);

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            AdlResult r = Increment(
                source[i].Timestamp,
                source[i].High,
                source[i].Low,
                source[i].Close,
                source[i].Volume,
                i > 0 ? results[i - 1].Adl : 0);

            results.Add(r);
        }

        return results;
    }
}
