namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Utility for removing and pruning series data.
/// </summary>
public static class Pruning
{
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

        if (removePeriods <= 0)
        {
            return seriesList;
        }

        for (int i = 0; i < removePeriods; i++)
        {
            seriesList.RemoveAt(0);
        }

        return seriesList;
    }
}
