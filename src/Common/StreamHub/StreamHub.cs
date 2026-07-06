namespace FacioQuo.Stock.Indicators;

// STREAM HUB (BASE/CACHE)

/// <inheritdoc cref="IStreamHub{TIn, TOut}"/>
public abstract partial class StreamHub<TIn, TOut> : IStreamHub<TIn, TOut>
    where TIn : ISeries
    where TOut : ISeries
{
    /// <summary>
    /// Gets the lock object for thread-safe cache operations during rebuild/rollback.
    /// This protects against concurrent cache modifications when out-of-order
    /// data triggers rebuild operations.
    /// </summary>
    private protected object CacheLock { get; } = new();

    /// <summary>
    /// Prevents self-recursion: during rebuild, OnAdd calls AppendCache,
    /// which must not trigger another rebuild on the same hub.
    /// Cascading to observers is still allowed and desired.
    /// </summary>
    private bool _isRebuilding;

    /// <summary>
    /// Set once the hub has completed its construction-time
    /// <see cref="Reinitialize"/>. A later <see cref="Reinitialize"/> on a
    /// non-root hub is a consumer call and is rejected.
    /// </summary>
    private bool _initialized;

    private protected StreamHub(IStreamObservable<TIn> provider)
    {
        // store provider reference
        Provider = provider;

        // provider's Cache reference (wrapped for safety)
        ProviderCache = provider.Results;

        // inherit settings (reinstantiate struct on heap)
        Properties = Properties.Combine(provider.Properties);

        // inherit max cache size from provider
        MaxCacheSize = provider.MaxCacheSize;

        // start the cache empty and let growth amortize: a fixed
        // pre-allocation (formerly 800 slots, ~6.4KB per hub) dominates the
        // footprint of multi-symbol deployments with thousands of hubs,
        // while growth-on-demand costs only a handful of one-time copies.
        // PruningList gives O(1) amortized front pruning once capped.
        Cache = new PruningList<TOut>();

        // build read-only cache reference
        Results = Cache.AsReadOnly();
    }

    /// <summary>
    /// Validates that the inherited MaxCacheSize is sufficient for the indicator's warmup requirements.
    /// </summary>
    /// <param name="requiredWarmupPeriods">Minimum number of periods required for indicator warmup.</param>
    /// <param name="indicatorName">Name of the indicator (for error messaging).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when MaxCacheSize is less than required warmup periods.</exception>
    protected void ValidateCacheSize(int requiredWarmupPeriods, string indicatorName)
    {
        if (MaxCacheSize < requiredWarmupPeriods)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requiredWarmupPeriods),
                requiredWarmupPeriods,
                $"Insufficient cache size for {indicatorName}. " +
                $"Requires at least {requiredWarmupPeriods} periods for proper initialization, " +
                $"but inherited MaxCacheSize is {MaxCacheSize}. " +
                $"Increase the provider's MaxCacheSize to at least {requiredWarmupPeriods}.");
        }

        // Set MinCacheSize to the required warmup periods
        SetMinCacheSize(requiredWarmupPeriods);
    }

    /// <summary>
    /// Sets the minimum cache size for this hub based on warmup requirements.
    /// This should be called in derived class constructors to specify the minimum
    /// number of periods required for the indicator to function correctly.
    /// </summary>
    /// <param name="requiredWarmupPeriods">Minimum number of periods required for indicator warmup.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    protected void SetMinCacheSize(int requiredWarmupPeriods)
    {
        // Validate parameter is within acceptable range
        if (requiredWarmupPeriods < 0 || requiredWarmupPeriods > MaxCacheSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requiredWarmupPeriods),
                requiredWarmupPeriods,
                $"Required warmup periods must be between 0 and {MaxCacheSize} (MaxCacheSize). Got {requiredWarmupPeriods}.");
        }

        // Update the baseline requirement for this hub
        _minCacheSizeBaseline = Math.Max(_minCacheSizeBaseline, requiredWarmupPeriods);

        // Update MinCacheSize to reflect the new baseline and any subscribers
        UpdateMinCacheSize();
    }

    // PROPERTIES

    /// <inheritdoc/>
    public string Name { get; private protected init; } = string.Empty;

    /// <inheritdoc/>
    public IReadOnlyList<TOut> Results { get; }

    /// <inheritdoc/>
    public bool IsFaulted { get; private set; }

    /// <summary>
    /// Gets the cache of stored values (base).
    /// </summary>
    internal PruningList<TOut> Cache { get; }

    /// <summary>
    /// Gets the current count of repeated caching attempts.
    /// An overflow condition is triggered after 100.
    /// </summary>
    internal byte OverflowCount { get; private set; }

    /// <summary>
    /// Gets or sets the most recent item saved to cache.
    /// </summary>
    private TOut? LastItem { get; set; }

    /// <summary>
    /// Gets a value indicating whether this hub is a <em>root</em> hub — one
    /// that owns its own input timeline because it has no upstream provider
    /// (its provider is the inert placeholder). Only a <see cref="BarHub"/>
    /// or <see cref="TradeTickHub"/> created without a provider is a root; every hub
    /// that subscribes to a provider is non-root and is driven by that provider.
    /// </summary>
    private protected bool IsRootHub => Provider is IInertProvider;

    /// <summary>
    /// Rejects a direct, consumer-initiated mutation on a non-root
    /// (subscribed/chained) hub. Such a hub is driven by its provider, so
    /// mutating it directly would desynchronize it from that provider and a
    /// later rebuild could produce wrong results. Feed and correct data through
    /// the root hub instead.
    /// </summary>
    /// <param name="caller">Name of the rejected mutating method.</param>
    /// <exception cref="InvalidOperationException">
    /// This hub is subscribed to a provider (non-root).
    /// </exception>
    private protected void ThrowIfNotRootHub([CallerMemberName] string? caller = null)
    {
        if (!IsRootHub)
        {
            throw new InvalidOperationException(
                $"'{caller}' can only be called on a root hub (a standalone BarHub or TradeTickHub). "
              + "This hub is subscribed to a provider and is driven by it; "
              + "mutate the root hub instead so the chain stays synchronized.");
        }
    }

    // METHODS

    /// <summary>
    /// Resets the fault flag and condition.
    /// </summary>
    /// <inheritdoc/>
    public void ResetFault()
    {
        OverflowCount = 0;
        IsFaulted = false;
    }

    /// <summary>
    /// Returns a detached, read-only point-in-time copy of the cache, taken
    /// under the cache lock so it is consistent even if a rebuild runs
    /// concurrently. Hand this to another thread rather than the live
    /// <see cref="Results"/> view.
    /// </summary>
    /// <returns>A detached, read-only copy of the current cache.</returns>
    public IReadOnlyList<TOut> Snapshot()
    {
        lock (CacheLock)
        {
            // detached, read-only copy (cannot be cast back to a mutable array)
            return Cache.ToArray().AsReadOnly();
        }
    }

    /// <summary>
    /// Adds a new item to the stream.
    /// </summary>
    /// <param name="newIn">New item to add.</param>
    /// <exception cref="InvalidOperationException">
    /// Called on a subscribed (non-root) hub. Add to the root hub instead.
    /// </exception>
    public void Add(TIn newIn)
    {
        ThrowIfNotRootHub();
        OnAdd(newIn, notify: true, null);
    }

    /// <summary>
    /// Adds a batch of new items to the stream.
    /// </summary>
    /// <param name="batchIn">Batch of new items to add.</param>
    /// <exception cref="InvalidOperationException">
    /// Called on a subscribed (non-root) hub. Add to the root hub instead.
    /// </exception>
    public void Add(IEnumerable<TIn> batchIn)
    {
        ThrowIfNotRootHub();

        // fast path: streaming batches are almost always pre-sorted, so
        // skip the O(n) sort allocation (keys + map + buffer) when a single
        // scan confirms chronological order; OrderBy is kept for the
        // unsorted case because its stable sort preserves same-timestamp
        // arrival order
        List<TIn> items = batchIn as List<TIn> ?? [.. batchIn];

        bool isSorted = true;
        for (int i = 1; i < items.Count; i++)
        {
            if (items[i].Timestamp < items[i - 1].Timestamp)
            {
                isSorted = false;
                break;
            }
        }

        if (isSorted)
        {
            foreach (TIn newIn in items)
            {
                OnAdd(newIn, notify: true, null);
            }

            return;
        }

        foreach (TIn newIn in items.OrderBy(static x => x.Timestamp))
        {
            OnAdd(newIn, notify: true, null);
        }
    }

    /// <summary>
    /// Removes a cached item at a specific index position.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">
    /// Called on a subscribed (non-root) hub. Remove from the root hub instead.
    /// </exception>
    public void RemoveAt(int cacheIndex)
    {
        ThrowIfNotRootHub();
        lock (CacheLock)
        {
            RemoveAtCore(cacheIndex);
        }
    }

    /// <summary>
    /// Removes the cached item at <paramref name="cacheIndex"/> and notifies
    /// observers. The caller must already hold <see cref="CacheLock"/>; this lets
    /// a find-then-remove (e.g. <c>Remove(IBar)</c>) be atomic under one lock.
    /// </summary>
    /// <param name="cacheIndex">Cache index to remove.</param>
    private protected void RemoveAtCore(int cacheIndex)
    {
        TOut cachedItem = Cache[cacheIndex];
        DateTime timestamp = cachedItem.Timestamp;
        Cache.RemoveAt(cacheIndex);

        // notify observers (inside lock to ensure cache consistency)
        NotifyObserversOnRebuild(timestamp);
    }

    /// <summary>
    /// Removes a range of cached items from a specific timestamp.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">
    /// Called on a subscribed (non-root) hub. Remove from the root hub instead.
    /// </exception>
    public void RemoveRange(DateTime fromTimestamp, bool notify)
    {
        ThrowIfNotRootHub();
        lock (CacheLock)
        {
            RemoveRangeFrom(fromTimestamp, notify);
        }
    }

    /// <summary>
    /// Removes cached records from a timestamp (inclusive) without the
    /// root-hub guard. The caller must hold <see cref="CacheLock"/>: this runs
    /// under the lock taken by <see cref="Rebuild(DateTime)"/> (every hub) and
    /// by the public <see cref="RemoveRange(DateTime, bool)"/> wrapper.
    /// </summary>
    /// <param name="fromTimestamp">Inclusive lower bound to remove from.</param>
    /// <param name="notify">Notify subscribers of the delete point.</param>
    private void RemoveRangeFrom(DateTime fromTimestamp, bool notify)
    {
        // compute restore index: last ProviderCache entry before rollback point
        int gteIndex = ProviderCache.IndexGte(fromTimestamp);
        int restoreIndex = (gteIndex == -1 ? ProviderCache.Count : gteIndex) - 1;

        // rollback internal state
        RollbackState(restoreIndex);

        // remove cache entries: the cache is chronological, so a binary
        // search + RemoveRange avoids the full predicate scan and the
        // per-call closure allocation of List.RemoveAll
        int cacheGteIndex = Cache.IndexGte(fromTimestamp);
        if (cacheGteIndex >= 0)
        {
            Cache.RemoveRange(cacheGteIndex, Cache.Count - cacheGteIndex);
        }

        // reset LastItem to prevent IsOverflowing from incorrectly detecting
        // duplicates when rebuilding (the new items may have same timestamp
        // and equal values as the old LastItem, especially during warmup periods)
        LastItem = Cache.Count > 0 ? Cache[^1] : default;

        // notify observers
        if (notify)
        {
            NotifyObserversOnRebuild(fromTimestamp);
        }
    }

    /// <summary>
    /// Removes a range of cached items from a specific index.
    /// </summary>
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">
    /// Called on a subscribed (non-root) hub. Remove from the root hub instead.
    /// </exception>
    public void RemoveRange(int fromIndex, bool notify)
    {
        ThrowIfNotRootHub();
        lock (CacheLock)
        {
            // nothing to do
            if (Cache.Count == 0 || fromIndex >= Cache.Count)
            {
                return;
            }

            // remove cache entries
            DateTime fromTimestamp = fromIndex <= 0
                ? DateTime.MinValue
                : Cache[fromIndex].Timestamp;

            RemoveRangeFrom(fromTimestamp, notify);
        }
    }

    /// <summary>
    /// Fully resets the stream hub.
    /// </summary>
    /// <inheritdoc/>
    public void Reinitialize()
    {
        // The construction-time call (from a derived hub's constructor)
        // initializes the hub and is always permitted. Any later call is a
        // consumer re-init, which is only valid on a root hub — a subscribed
        // hub is driven by its provider and must not be reset independently.
        if (_initialized)
        {
            ThrowIfNotRootHub();
        }

        Unsubscribe();
        ResetFault();
        Rebuild();

        // A root hub's provider is the inert placeholder, which rejects
        // subscriptions; only a non-root hub re-establishes its subscription.
        // This also makes a consumer Reinitialize() on a root hub a clean
        // rebuild instead of throwing partway from the inert provider.
        if (!IsRootHub)
        {
            // Close the rebuild->subscribe gap: an item the provider appends
            // between the rebuild above and the subscription becoming active would
            // otherwise be missing here (it was not replayed, and we were not yet
            // subscribed to receive it). Detect provider growth across Subscribe and
            // re-run a catch-up rebuild so it is folded in. From this point new
            // items arrive via OnAdd. In the common (quiescent) case the count is
            // unchanged and no second rebuild runs.
            int providerCountBeforeSubscribe = ProviderCache.Count;
            Subscription = Provider.Subscribe(this);

            if (ProviderCache.Count != providerCountBeforeSubscribe)
            {
                Rebuild();
            }
        }

        _initialized = true;

        // TODO: make reinitialization abstract,
        // and build initial Cache from faster static method
    }

    /// <summary>
    /// Rebuilds the cache.
    /// </summary>
    /// <inheritdoc/>
    public void Rebuild() => Rebuild(DateTime.MinValue);

    /// <summary>
    /// Rebuilds the cache from a specific timestamp.
    /// </summary>
    /// <inheritdoc/>
    public virtual void Rebuild(DateTime fromTimestamp)
    {
        // Lock to prevent concurrent cache access during rebuild.
        // Out-of-order data triggers rebuild, which clears and repopulates
        // the cache. Without locking, ToIndicator may access Cache[i-1]
        // while the cache is in an inconsistent state.
        // NOTE: Observer notification must happen INSIDE the lock to prevent
        // race conditions where new items are added before rebuild notification
        // reaches observers, causing cache desynchronization.
        lock (CacheLock)
        {
            // Set flag to prevent self-recursion in AppendCache
            _isRebuilding = true;

            try
            {
                // clear cache (internal path — bypasses the root-hub guard
                // because Rebuild runs on every hub, root or not)
                RemoveRangeFrom(fromTimestamp, notify: false);

                // get provider position
                int provIndex = ProviderCache.IndexGte(fromTimestamp);

                // rebuild
                if (provIndex >= 0)
                {
                    int cacheSize = ProviderCache.Count;
                    for (int i = provIndex; i < cacheSize; i++)
                    {
                        OnAdd(ProviderCache[i], notify: false, i);
                    }
                }

                // notify observers (inside lock to ensure cache consistency
                // before any new items can be added)
                NotifyObserversOnRebuild(fromTimestamp);
            }
            finally
            {
                // Clear flag
                _isRebuilding = false;
            }
        }
    }

    /// <summary>
    /// Rebuilds the cache from a specific index.
    /// </summary>
    /// <inheritdoc/>
    public void Rebuild(int fromIndex)
    {
        // find timestamp
        DateTime fromTimestamp = fromIndex <= 0 || Cache.Count == 0
            ? DateTime.MinValue
            : Cache[fromIndex].Timestamp;

        // rebuild & notify
        Rebuild(fromTimestamp);
    }

    /// <inheritdoc/>
    public override string ToString() => Name;

    /// <summary>
    /// Converts incremental value into an indicator candidate and cache position.
    /// </summary>
    /// <param name="item">New item from provider.</param>
    /// <param name="indexHint">Provider index hint.</param>
    /// <returns>Cacheable item candidate and index hint.</returns>
    protected abstract (TOut result, int index) ToIndicator(TIn item, int? indexHint);

    /// <summary>
    /// Performs appropriate caching action after analysis.
    /// It will add if new, ignore if duplicate, or rebuild if late-arrival.
    /// </summary>
    /// <param name="result">Item to cache.</param>
    /// <param name="notify">Notify subscribers of change (send to observers). This is disabled for bulk operations like rebuild.</param>
    /// <exception cref="InvalidOperationException">Thrown when the operation is invalid for the current state</exception>
    protected void AppendCache(TOut result, bool notify)
    {
        // check overflow/duplicates
        if (IsOverflowing(result))
        {
            return;
        }

        bool bypassRebuild = Properties[1]; // forced add/caching w/o rebuild

        // Prevent self-recursion: during rebuild, OnAdd processes provider items
        // and AppendCache must not trigger another rebuild on this same hub.
        // Observer cascading (NotifyObserversOnRebuild) is separate and still occurs.
        if (_isRebuilding)
        {
            bypassRebuild = true;
        }

        // consider timeline
        Act act = bypassRebuild || Cache.Count == 0 || result.Timestamp > Cache[^1].Timestamp
            ? Act.Add
            : Act.Rebuild;

        // fulfill action
        switch (act)
        {
            // add to cache
            case Act.Add:
                Add(result, notify);
                break;

            // rebuild cache
            case Act.Rebuild:
                Rebuild(result.Timestamp);
                break;

            // would never happen
            default:
                throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Restores internal state to match entries through <paramref name="restoreIndex"/>.
    /// </summary>
    /// <remarks>
    /// Override when an indicator maintains stateful fields outside the
    /// result cache (rolling sums, EMAs, deques, last-seen timestamps,
    /// etc.).
    /// <para>
    /// Contract:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <paramref name="restoreIndex"/> is the last
    /// <see cref="ProviderCache"/> index whose state must be retained.
    /// Implementations should rebuild their internal state to be
    /// equivalent to having processed inputs <c>[0..restoreIndex]</c>
    /// in order, so the next item at <c>restoreIndex + 1</c> can be
    /// computed correctly.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>-1</c> signals that no entries precede the rollback point;
    /// reset all internal state to its initial post-construction values.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Called by the base <em>before</em> result-cache entries beyond
    /// <c>restoreIndex</c> are removed and <em>before</em> the replay
    /// loop re-emits items at <c>restoreIndex + 1</c> onward.
    /// Implementations may safely read <see cref="ProviderCache"/>
    /// entries <c>[0..restoreIndex]</c> while rebuilding internal
    /// state. Do not re-emit or mutate the result cache from this
    /// method; the base handles that immediately after this call returns.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <param name="restoreIndex">
    /// Last ProviderCache index to retain, or -1 to reset to initial
    /// state.
    /// </param>
    protected virtual void RollbackState(int restoreIndex)
    {
        // default: do nothing
    }

    /// <summary>
    /// Prunes the cache to the maximum size.
    /// </summary>
    private void PruneCache()
    {
        if (Cache.Count < MaxCacheSize)
        {
            return;
        }

        // calculate how many items to remove
        int count = Cache.Count - MaxCacheSize + 1;

        // store timestamp of last item being removed
        DateTime toTimestamp = Cache[count - 1].Timestamp;

        // remove all items in one operation
        Cache.RemoveRange(0, count);

        NotifyObserversOnPrune(toTimestamp);
    }

    /// <summary>
    /// Adds an item to cache and notifies observers.
    /// </summary>
    /// <param name="item">Item to add to end of cache.</param>
    /// <param name="notify">Inherited notification instructions.</param>
    private void Add(TOut item, bool notify)
    {
        // notes:
        // 1. Should only be called from AppendCache()
        // 2. Notify has to be disabled for bulk operations, like rebuild.
        // 3. Forced caching (rebuild analysis bypass) is inherited property.

        // add to cache
        Cache.Add(item);
        IsFaulted = false;

        // notify subscribers
        if (notify)
        {
            NotifyObserversOnAdd(item, Cache.Count - 1);
        }
    }

    /// <summary>
    /// Inserts an item without rebuilding this hub.
    /// </summary>
    /// <param name="item">Item to insert.</param>
    /// <param name="index">Cache index to insert at.</param>
    /// <param name="notify">Notify observers of rebuild.</param>
    protected void InsertWithoutRebuild(TOut item, int index, bool notify)
    {
        if (index < 0 || index > Cache.Count)
        {
            AppendCache(item, notify);
            return;
        }

        int countBefore = Cache.Count;
        bool midInsert = index < countBefore;
        TOut? lastBefore = LastItem;
        byte overflowBefore = OverflowCount;

        // Check if this is a tail replacement (replacing the last element with same timestamp)
        bool isTailReplacement = index == Cache.Count - 1
            && index < Cache.Count
            && Cache[index].Timestamp == item.Timestamp;

        if (IsOverflowing(item))
        {
            if (midInsert && !isTailReplacement)
            {
                LastItem = lastBefore;
                OverflowCount = overflowBefore;
            }

            return;
        }

        int removed = countBefore - Cache.Count;
        if (removed > 0)
        {
            index -= removed;
            if (index < 0)
            {
                if (midInsert && !isTailReplacement)
                {
                    LastItem = lastBefore;
                    OverflowCount = overflowBefore;
                }

                return;
            }
        }

        if (index < Cache.Count && Cache[index].Timestamp == item.Timestamp)
        {
            if (Cache[index].Equals(item))
            {
                if (midInsert && !isTailReplacement)
                {
                    LastItem = lastBefore;
                    OverflowCount = overflowBefore;
                }

                return;
            }

            Cache[index] = item;
        }
        else
        {
            Cache.Insert(index, item);
        }

        if (midInsert && !isTailReplacement)
        {
            LastItem = lastBefore;
            OverflowCount = overflowBefore;
        }

        if (notify)
        {
            NotifyObserversOnRebuild(item.Timestamp);
        }
    }

    /// <summary>
    /// Validates outbound item and compares to prior cached item,
    /// to gracefully manage and prevent overflow conditions.
    /// </summary>
    /// <param name="item">Cacheable time-series object.</param>
    /// <returns>True if item is repeating and duplicate was suppressed.</returns>
    /// <exception cref="OverflowException">Too many sequential duplicates were detected.</exception>
    private bool IsOverflowing(TOut item)
    {
        // skip first arrival
        if (LastItem is null)
        {
            LastItem = item;
            return false;
        }

        // track/check for overflow condition
        if (item.Timestamp == LastItem.Timestamp && item.Equals(LastItem))
        {
            // ^^ using progressive check to avoid Equals() on every item

            OverflowCount++;

            // handle overflow
            if (OverflowCount > 100)
            {
                const string msg = """
                    A repeated stream update exceeded the 100 attempt threshold.
                    Check and remove circular chains or check your stream provider.
                    Provider terminated.
                    """;

                IsFaulted = true;

                // emit error
                OverflowException exception = new(msg);
                NotifyObserversOnError(exception);
                throw exception;
            }

            // bypass duplicate prevention
            // when forced caching is enabled
            return !Properties[1];
        }

        // maintenance pruning
        PruneCache();

        // not repeating
        OverflowCount = 0;
        LastItem = item;
        return false;
    }



}
