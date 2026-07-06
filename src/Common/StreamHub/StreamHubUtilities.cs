namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides static utility methods for stream hub operations.
/// </summary>
internal static class StreamHubUtilities
{
    /// <summary>
    /// Try to find index position of the provided timestamp.
    /// </summary>
    /// <typeparam name="T">Type of the items in the cache, must implement ISeries.</typeparam>
    /// <param name="cache">Cache to search.</param>
    /// <param name="timestamp">Timestamp to seek.</param>
    /// <param name="index">Index of timestamp or -1 when not found.</param>
    /// <returns>True if found.</returns>
    internal static bool TryFindIndex<T>(
        this IReadOnlyList<T> cache,
        DateTime timestamp,
        out int index)
        where T : ISeries
    {
        index = cache.IndexOf(timestamp, false);
        return index != -1;
    }

    /// <summary>
    /// Get the cache index based on item equality.
    /// </summary>
    /// <typeparam name="T">Type of the items in the cache, must implement ISeries.</typeparam>
    /// <param name="cache">Cache to search.</param>
    /// <param name="cachedItem">Time-series object to find in cache.</param>
    /// <param name="throwOnFail">Throw exception when item is not found.</param>
    /// <returns>Index position.</returns>
    /// <exception cref="ArgumentException">When item is not found (should never happen).</exception>
    internal static int IndexOf<T>(
        this IReadOnlyList<T> cache,
        T cachedItem,
        bool throwOnFail)
        where T : ISeries
    {
        int low = 0;
        int high = cache.Count - 1;
        int firstMatchIndex = -1;
        DateTime targetTimestamp = cachedItem.Timestamp;

        while (low <= high)
        {
            int mid = (low + high) >> 1;
            int comparison = cache[mid].Timestamp.CompareTo(targetTimestamp);

            if (comparison == 0)
            {
                // Found a match by Timestamp,
                // store the index of the first match
                if (firstMatchIndex == -1)
                {
                    firstMatchIndex = mid;
                }

                // Verify with Equals for an exact match
                if (cache[mid].Equals(cachedItem))
                {
                    return mid; // exact match found
                }

                high = mid - 1; // continue searching to the left
            }
            else if (comparison < 0)
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        // If a timestamp match was found but no exact
        // match, try to find an exact match in the range
        // of duplicate timestamps (e.g. Renko bricks),
        // biased towards later duplicates.
        if (firstMatchIndex != -1)
        {
            // Find the last occurrence of the matching timestamp
            for (int i = cache.Count - 1; i >= firstMatchIndex; i--)
            {
                if (cache[i].Timestamp == targetTimestamp
                 && cache[i].Equals(cachedItem))
                {
                    return i; // exact match found among duplicates
                }
            }
        }

        // not found
        return throwOnFail
            ? throw new ArgumentException(
                "Matching source history not found.", nameof(cachedItem))
            : -1;
    }

    /// <summary>
    /// Get the cache index based on a timestamp.
    /// </summary>
    /// <remarks>
    /// Only use this when you are looking for a point in time
    /// without a matching item for context. In most cases
    /// <see cref="IndexOf{T}(IReadOnlyList{T},T,bool)"/> is more appropriate.
    /// </remarks>
    /// <typeparam name="T">Type of the items in the cache, must implement ISeries.</typeparam>
    /// <param name="cache">Cache to search.</param>
    /// <param name="timestamp">Timestamp of cached item.</param>
    /// <param name="throwOnFail">Throw exception when timestamp is not found.</param>
    /// <returns>Index position.</returns>
    /// <exception cref="ArgumentException">When timestamp is not found (should never happen).</exception>
    internal static int IndexOf<T>(
        this IReadOnlyList<T> cache,
        DateTime timestamp,
        bool throwOnFail)
        where T : ISeries
    {
        int low = 0;
        int high = cache.Count - 1;

        while (low <= high)
        {
            int mid = (low + high) >> 1;
            DateTime midTimestamp = cache[mid].Timestamp;

            if (midTimestamp == timestamp)
            {
                return mid;
            }
            else if (midTimestamp < timestamp)
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        // not found
        return throwOnFail
            ? throw new ArgumentException(
                "Matching source history not found.", nameof(timestamp))
            : -1;
    }

    /// <summary>
    /// Get the first cache index on or after a timestamp.
    /// </summary>
    /// <remarks>
    /// Only use this when you are looking for a point in time
    /// without a matching item for context. In most cases
    /// <see cref="IndexOf{T}(IReadOnlyList{T},T,bool)"/> is more appropriate.
    /// </remarks>
    /// <typeparam name="T">Type of the items in the cache, must implement ISeries.</typeparam>
    /// <param name="cache">Cache to search.</param>
    /// <param name="timestamp">Timestamp of cached item.</param>
    /// <returns>First index position or -1 if not found.</returns>
    internal static int IndexGte<T>(
        this IReadOnlyList<T> cache,
        DateTime timestamp)
        where T : ISeries
    {
        int low = 0;
        int high = cache.Count;
        while (low < high)
        {
            int mid = low + ((high - low) / 2);
            if (cache[mid].Timestamp < timestamp)
            {
                low = mid + 1;
            }
            else
            {
                high = mid;
            }
        }

        // At this point, low is the index of the first
        // element that is greater than or equal to timestamp
        // or Cache.Count if all elements are less than timestamp.
        // If low is equal to Cache.Count, it means there are
        // no elements greater than or equal to timestamp.
        return low < cache.Count ? low : -1;
    }
}
