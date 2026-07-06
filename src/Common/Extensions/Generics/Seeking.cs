namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides extension methods for seeking and finding elements in a series.
/// </summary>
public static class Seeking
{
    /// <summary>
    /// Finds an element in the series by its timestamp.
    /// </summary>
    /// <typeparam name="T">Type of the elements in the series, which must implement <see cref="ISeries"/>.</typeparam>
    /// <param name="series">Series of elements to search.</param>
    /// <param name="lookupDate">Timestamp to look for.</param>
    /// <returns>Element with the matching timestamp, or the default value if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the series is null.</exception>
    public static T? Find<T>(
        this IReadOnlyList<T> series,
        DateTime lookupDate)
        where T : ISeries
    {
        ArgumentNullException.ThrowIfNull(series);

        int low = 0;
        int high = series.Count - 1;

        while (low <= high)
        {
            int mid = (low + high) >> 1;
            DateTime midTimestamp = series[mid].Timestamp;

            if (midTimestamp == lookupDate)
            {
                return series[mid]; // found
            }
            else if (midTimestamp < lookupDate)
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        // not found
        return default;
    }
}
