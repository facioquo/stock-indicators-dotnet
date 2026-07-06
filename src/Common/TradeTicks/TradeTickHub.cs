namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for managing raw tick data.
/// </summary>
public class TradeTickHub
    : StreamHub<ITradeTick, ITradeTick>, IStreamObservable<ITradeTick>
{
    /// <summary>
    /// Absolute maximum cache size to prevent overflow.
    /// </summary>
    private const int absoluteMaxCacheSize = (int)(0.8 * int.MaxValue);

    /// <summary>
    /// Initializes a new instance of the <see cref="TradeTickHub"/> class without its own provider.
    /// </summary>
    /// <param name="maxCacheSize">Maximum in-memory cache size.</param>
    public TradeTickHub(int? maxCacheSize = null)
        : base(new BaseProvider<ITradeTick>(ValidateAndGetMaxCacheSize(maxCacheSize)))
            => Name = "TRADE-TICK-HUB";

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
                = $"'{nameof(maxCacheSize)}' must be greater than 0 and not over {absoluteMaxCacheSize}; was {maxCacheSize}.";

            throw new ArgumentOutOfRangeException(
                nameof(maxCacheSize), maxCacheSize, message);
        }

        return maxCacheSize ?? maxCacheSizeDefault;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TradeTickHub"/> class with a specified provider.
    /// </summary>
    /// <param name="provider">The tick provider.</param>
    public TradeTickHub(
        IStreamObservable<ITradeTick> provider)
        : base(provider ?? throw new ArgumentNullException(nameof(provider)))
    {
        Name = "TRADE-TICK-HUB";
        Reinitialize();
    }

    /// <inheritdoc/>
    protected override (ITradeTick result, int index)
        ToIndicator(ITradeTick item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        // append fast path: live feeds almost always deliver new ticks,
        // so skip the binary search when the item extends the timeline
        lock (CacheLock)
        {
            if (indexHint is null
                && (Cache.Count == 0 || item.Timestamp > Cache[^1].Timestamp))
            {
                return (item, Cache.Count);
            }

            int index = indexHint
                ?? Cache.IndexGte(item.Timestamp);

            return (item, index == -1 ? Cache.Count : index);
        }
    }

    /// <inheritdoc/>
    public override string ToString()
        => $"TICKS: {Cache.Count} items";

    /// <summary>
    /// Handles adding a new tick with special handling for same-timestamp updates
    /// when TradeTickHub is standalone (no external provider).
    /// </summary>
    /// <inheritdoc/>
    public override void OnAdd(ITradeTick item, bool notify, int? indexHint)
    {
        // for non-root TradeTickHub, use standard behavior
        if (!IsRootHub)
        {
            base.OnAdd(item, notify, indexHint);
            return;
        }

        // get result and position
        (ITradeTick result, int index) = ToIndicator(item, indexHint);

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

            // determine execution ID presence for same-timestamp replacement logic
            bool hasExecutionId = !string.IsNullOrEmpty(result.ExecutionId);
            bool hasCachedExecutionId = !string.IsNullOrEmpty(Cache[index].ExecutionId);

            if (hasExecutionId && hasCachedExecutionId && Cache[index].ExecutionId != result.ExecutionId)
            {
                // Different execution IDs at same timestamp - both are valid trades;
                // replace the cached entry and notify as a new addition so aggregators process it
                Cache[index] = result;

                if (notify)
                {
                    NotifyObserversOnAdd(result, index);
                }

                return;
            }

            // For ticks without execution IDs or same execution ID, replace in cache
            Cache[index] = result;

            // Notify appropriately: same ExecutionId = correction (rebuild),
            // no ExecutionIds = treat as new addition for aggregators
            if (notify)
            {
                if (hasExecutionId && hasCachedExecutionId)
                {
                    // Same execution ID - this is an update/correction
                    NotifyObserversOnRebuild(result.Timestamp);
                }
                else
                {
                    // No execution IDs - notify as addition so aggregators can process
                    NotifyObserversOnAdd(result, index);
                }
            }

            return;
        }

        // late-arrival: insert tick at correct sorted position, then rebuild observers
        if (Cache.Count > 0 && result.Timestamp < Cache[^1].Timestamp)
        {
            Cache.Insert(index, result);

            if (notify)
            {
                NotifyObserversOnRebuild(result.Timestamp);
            }

            return;
        }

        // standard add behavior for new items
        AppendCache(result, notify);
    }

    /// <summary>
    /// Rebuilds the cache from a specific timestamp.
    /// For standalone TradeTickHub, preserves cache and notifies observers.
    /// </summary>
    /// <inheritdoc/>
    public override void Rebuild(DateTime fromTimestamp)
    {
        // for a root TradeTickHub (no external provider),
        // we cannot rebuild from an empty provider cache
        // instead, just notify observers to rebuild from this hub's cache
        if (IsRootHub)
        {
            // compute restore index from cache
            int gte = Cache.IndexGte(fromTimestamp);
            int restoreIndex = gte == -1
                ? Cache.Count - 1
                : gte - 1;

            // rollback internal state
            RollbackState(restoreIndex);

            // notify observers to rebuild from this hub
            NotifyObserversOnRebuild(fromTimestamp);
            return;
        }

        // standard rebuild for TradeTickHub with external provider
        base.Rebuild(fromTimestamp);
    }

    /// <summary>
    /// Removes the cached tick whose timestamp matches the supplied tick,
    /// cascading the resulting rebuild to every dependent hub. This is a
    /// convenience over <see cref="StreamHub{TIn, TOut}.RemoveAt(int)"/> that
    /// locates the entry by timestamp.
    /// </summary>
    /// <remarks>
    /// The match is by timestamp. For root hubs, the cache holds at most one
    /// entry per timestamp — when a new tick is added with the same timestamp as
    /// an existing entry, the earlier entry is replaced — so <c>Remove</c>
    /// unambiguously removes that single entry. If a custom derived hub stores
    /// multiple entries per timestamp, use
    /// <see cref="StreamHub{TIn, TOut}.RemoveAt(int)"/> to target a specific
    /// positional entry.
    /// </remarks>
    /// <param name="tick">
    /// TradeTick whose <see cref="ISeries.Timestamp"/> identifies the cache entry
    /// to remove.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="tick"/> is null.</exception>
    /// <exception cref="ArgumentException">No cached tick matches the timestamp.</exception>
    /// <exception cref="InvalidOperationException">
    /// Called on a subscribed (non-root) hub. Remove from the root hub instead.
    /// </exception>
    public void Remove(ITradeTick tick)
    {
        ArgumentNullException.ThrowIfNull(tick);
        ThrowIfNotRootHub();

        // find-then-remove under one lock so the index can't shift in between
        lock (CacheLock)
        {
            int index = Cache.IndexOf(tick.Timestamp, throwOnFail: false);

            if (index < 0)
            {
                throw new ArgumentException(
                    $"No cached tick was found at timestamp {tick.Timestamp:O}.",
                    nameof(tick));
            }

            RemoveAtCore(index);
        }
    }
}
