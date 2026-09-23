namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides extension methods for removing and pruning series data.
/// </summary>
public static class PruningExtensions
{
    /// <summary>
    /// Removes the leading results that have no calculated value, up to the first one that does.
    /// Indicators whose values converge after that point declare their own overload.
    /// </summary>
    /// <typeparam name="T">Reusable result type.</typeparam>
    /// <param name="results">Indicator results to evaluate.</param>
    /// <returns>Results from the first calculated value onward; empty when none has a value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when results is null.</exception>
    public static IReadOnlyList<T> RemoveWarmupPeriods<T>(
        this IReadOnlyList<T> results)
        where T : IReusable
        => results.RemoveBeforeFirstValue(static x => !double.IsNaN(x.Value));

    /// <summary>
    /// Removes a specified number of warmup periods from the beginning of the series.
    /// </summary>
    /// <typeparam name="T">Type of elements in the series.</typeparam>
    /// <param name="series">Series from which to remove warmup periods.</param>
    /// <param name="removePeriods">Number of periods to remove.</param>
    /// <returns>A new series with the specified number of warmup periods removed.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when removePeriods is less than 0.</exception>
    public static IReadOnlyList<T> RemoveWarmupPeriods<T>(
        this IReadOnlyList<T> series,
        int removePeriods)
        => removePeriods < 0
            ? throw new ArgumentOutOfRangeException(nameof(removePeriods), removePeriods,
                "If specified, the Remove Periods value must be greater than or equal to 0.")
            : series.Remove(removePeriods);

    /// <summary>
    /// Removes the results before the first one with a calculated value, plus any further
    /// periods an indicator needs for its values to converge.
    /// </summary>
    /// <typeparam name="T">Type of elements in the series.</typeparam>
    /// <param name="results">Indicator results to evaluate.</param>
    /// <param name="hasValue">Whether a result has a calculated value.</param>
    /// <param name="removePeriods">
    /// Periods to remove, given the index of the first calculated value; defaults to that index.
    /// </param>
    /// <returns>The remaining results; empty when none has a calculated value.</returns>
    internal static IReadOnlyList<T> RemoveBeforeFirstValue<T>(
        this IReadOnlyList<T> results,
        Func<T, bool> hasValue,
        Func<int, int>? removePeriods = null)
    {
        ArgumentNullException.ThrowIfNull(results);

        int firstValue = results.FindIndex(hasValue);

        return firstValue < 0
            ? []
            : results.Remove(removePeriods?.Invoke(firstValue) ?? firstValue);
    }

    /// <summary>
    /// Finds the index of the first element that matches the specified predicate.
    /// </summary>
    /// <typeparam name="T">Type of elements in the series.</typeparam>
    /// <param name="series">Series to search.</param>
    /// <param name="match">Predicate that defines the conditions of the element to search for.</param>
    /// <returns>Zero-based index of the first occurrence of an element that matches the conditions defined by match, if found; otherwise, -1.</returns>
    internal static int FindIndex<T>(
        this IReadOnlyList<T> series,
        Func<T, bool> match)
    {
        for (int i = 0; i < series.Count; i++)
        {
            if (match(series[i]))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Removes a specified number of periods from the beginning of the series.
    /// </summary>
    /// <typeparam name="T">Type of elements in the series.</typeparam>
    /// <param name="series">Series from which to remove periods.</param>
    /// <param name="removePeriods">Number of periods to remove.</param>
    /// <returns>A new list with the specified number of periods removed.</returns>
    internal static List<T> Remove<T>(
        this IReadOnlyList<T> series,
        int removePeriods)
    {
        List<T> seriesList = series.ToList();

        if (seriesList.Count <= removePeriods)
        {
            return [];
        }

        if (removePeriods > 0)
        {
            seriesList.RemoveRange(0, removePeriods);
        }

        return seriesList;
    }
}
