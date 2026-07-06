namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for reusable types.
/// </summary>
public static class Reusable
{
    /// <summary>
    /// Converts a list of bars to a list of reusable types.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="candlePart">The <see cref="CandlePart" /> element.</param>
    /// <returns>A list of reusable types.</returns>
    public static IReadOnlyList<IReusable> ToReusable(
        this IReadOnlyList<IBar> bars,
        CandlePart candlePart)

        => bars
            .OrderBy(x => x.Timestamp)
            .Select(x => x.ToReusable(candlePart))
            .ToList();

    /// <summary>
    /// Removes non-essential records containing null or NaN values.
    /// </summary>
    /// <typeparam name="T">Any reusable result type.</typeparam>
    /// <param name="results">Indicator results to evaluate.</param>
    /// <returns>Time series of indicator results, condensed.</returns>
    public static IReadOnlyList<T> Condense<T>(
        this IReadOnlyList<T> results)
        where T : IReusable
    {
        List<T> resultsList = results
            .ToList();

        resultsList
            .RemoveAll(match:
                static x => double.IsNaN(x.Value));

        return resultsList;
    }

    /// <summary>
    /// Removes the recommended quantity of results from the beginning
    /// of the results list using a reverse-engineering approach.
    /// </summary>
    /// <typeparam name="T">Any reusable result type.</typeparam>
    /// <param name="results">Indicator results to evaluate.</param>
    /// <returns>Time series of results, pruned.</returns>
    internal static IReadOnlyList<T> RemoveWarmupPeriods<T>(
        this IReadOnlyList<T> results)
        where T : IReusable
    {
        int removePeriods = results
            .FindIndex(static x => !double.IsNaN(x.Value));

        return results.Remove(removePeriods);
    }

    /// <summary>
    /// Converts a bar to a basic chainable class.
    /// </summary>
    /// <param name="q">Bar to convert.</param>
    /// <param name="candlePart">The <see cref="CandlePart" /> element.</param>
    /// <returns>A reusable type.</returns>
    internal static IReusable ToReusable(this IBar q, CandlePart candlePart)
        => q.ToBarPart(candlePart);
}
