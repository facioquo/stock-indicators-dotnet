namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for managing bars.
/// </summary>
public class BarHub
    : BarProvider<IBar, IBar>
{
    /// <summary>
    /// Absolute maximum cache size to prevent overflow.
    /// </summary>
    private const int absoluteMaxCacheSize = (int)(0.8 * int.MaxValue);

    /// <summary>
    /// Initializes a new instance of the <see cref="BarHub"/> base, without its own provider.
    /// </summary>
    /// <param name="maxCacheSize">Maximum in-memory cache size.</param>
    public BarHub(int? maxCacheSize = null)
        : base(new BaseProvider<IBar>(ValidateAndGetMaxCacheSize(maxCacheSize)))
    { }

    /// <summary>
    /// Validates and returns the max cache size.
    /// </summary>
    /// <param name="maxCacheSize">Maximum in-memory cache size.</param>
    /// <returns>Validated max cache size.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private static int ValidateAndGetMaxCacheSize(int? maxCacheSize)
    {
        const int maxCacheSizeDefault = 100_000;

        if (maxCacheSize is (not null and <= 0) or > absoluteMaxCacheSize)
        {
            string message
                = $"'{nameof(maxCacheSize)}' must be greater than 0 and not over {absoluteMaxCacheSize}.";

            throw new ArgumentOutOfRangeException(
                nameof(maxCacheSize), maxCacheSize, message);
        }

        return maxCacheSize ?? maxCacheSizeDefault;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BarHub"/> class with a specified provider.
    /// </summary>
    /// <param name="provider">Bar provider.</param>
    public BarHub(
        IBarProvider<IBar> provider)
        : base(provider)
        => Reinitialize();

    /// <inheritdoc/>
    protected override (IBar result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        // append fast path: live feeds almost always deliver new bars,
        // so skip the binary search when the item extends the timeline
        if (indexHint is null
            && (Cache.Count == 0 || item.Timestamp > Cache[^1].Timestamp))
        {
            return (item, Cache.Count);
        }

        int index = indexHint
            ?? Cache.IndexGte(item.Timestamp);

        return (item, index == -1 ? Cache.Count : index);
    }

    /// <inheritdoc/>
    public override string ToString()
        => $"BARS<{nameof(IBar)}>: {Bars.Count} items";

    /// <summary>
    /// Handles adding a new bar with special handling for same-timestamp updates
    /// when BarHub is standalone (no external provider).
    /// </summary>
    /// <inheritdoc/>
    public override void OnAdd(IBar item, bool notify, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        // Reject additions that precede the current cache timeline
        // (applies to both standalone and non-standalone BarHub)
        lock (CacheLock)
        {
            if (Cache.Count > 0 && item.Timestamp < Cache[0].Timestamp)
            {
                // Silently ignore - this prevents indeterminate gaps in the timeline
                return;
            }
        }

        // for non-root BarHub, use standard behavior (which handles locking)
        if (!IsRootHub)
        {
            base.OnAdd(item, notify, indexHint);
            return;
        }

        // Lock for standalone BarHub operations
        lock (CacheLock)
        {

            // get result and position
            (IBar result, int index) = ToIndicator(item, indexHint);

            // check if this is a same-timestamp update (not a new item at the end)
            if (Cache.Count > 0 && index < Cache.Count && Cache[index].Timestamp == result.Timestamp)
            {
                // check if this is an exact duplicate (same values)
                // if so, defer to AppendCache for overflow tracking
                if (Cache[index].Equals(result))
                {
                    AppendCache(result, notify);
                    return;
                }

                // replace existing item at this position (different values, same timestamp)
                Cache[index] = result;

                // notify observers (inside lock to ensure cache consistency)
                if (notify)
                {
                    NotifyObserversOnRebuild(item.Timestamp);
                }
            }
            else
            {
                // if out-of-order insert, insert and trigger rebuild
                if (index >= 0 && index < Cache.Count)
                {
                    InsertWithoutRebuild(result, index, notify);
                    return;
                }

                // standard add behavior for new items
                AppendCache(result, notify);
            }
        }
    }

    /// <summary>
    /// Rebuilds the cache from a specific timestamp.
    /// For standalone BarHub, preserves cache and notifies observers.
    /// </summary>
    /// <inheritdoc/>
    public override void Rebuild(DateTime fromTimestamp)
    {
        // for a root BarHub (no external provider),
        // we cannot rebuild from an empty provider cache
        // instead, just notify observers to rebuild from this hub's cache
        if (IsRootHub)
        {
            lock (CacheLock)
            {
                // rollback internal state
                int gte = Cache.IndexGte(fromTimestamp);
                int restoreIndex = gte == -1
                    ? Cache.Count - 1
                    : gte - 1;
                RollbackState(restoreIndex);

                // notify observers to rebuild from this hub (inside lock
                // to ensure cache consistency before any new items are added)
                NotifyObserversOnRebuild(fromTimestamp);
            }

            return;
        }

        // standard rebuild for BarHub with external provider
        base.Rebuild(fromTimestamp);
    }

    /// <summary>
    /// Removes the cached bar whose timestamp matches the supplied bar,
    /// cascading the resulting rebuild to every dependent hub. This is a
    /// convenience over <see cref="StreamHub{TIn, TOut}.RemoveAt(int)"/> that
    /// locates the entry by timestamp.
    /// </summary>
    /// <remarks>
    /// The match is by timestamp, not by value, so a bar carrying only the
    /// timestamp identifies the entry. If more than one cached bar shares the
    /// timestamp, an unspecified one is removed; identify the entry positionally
    /// via <see cref="StreamHub{TIn, TOut}.RemoveAt(int)"/> when that matters.
    /// </remarks>
    /// <param name="bar">
    /// Bar whose <see cref="ISeries.Timestamp"/> identifies the cache entry
    /// to remove.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="bar"/> is null.</exception>
    /// <exception cref="ArgumentException">No cached bar matches the timestamp.</exception>
    /// <exception cref="InvalidOperationException">
    /// Called on a subscribed (non-root) hub. Remove from the root hub instead.
    /// </exception>
    public void Remove(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);
        ThrowIfNotRootHub();

        // find-then-remove under one lock so the index can't shift in between
        lock (CacheLock)
        {
            int index = Cache.IndexOf(bar.Timestamp, throwOnFail: false);

            if (index < 0)
            {
                throw new ArgumentException(
                    $"No cached bar was found at timestamp {bar.Timestamp:O}.",
                    nameof(bar));
            }

            RemoveAtCore(index);
        }
    }
}

public static partial class Bars
{
    /// <summary>
    /// Creates a BarHub that is subscribed to an IBarProvider.
    /// </summary>
    /// <param name="barProvider">Bar provider to convert.</param>
    /// <returns>A new instance of BarHub.</returns>
    public static BarHub ToBarHub(
        this IBarProvider<IBar> barProvider)
        => new(barProvider);
}
