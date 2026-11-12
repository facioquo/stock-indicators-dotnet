namespace FacioQuo.Stock.Indicators;

// STREAM HUB (OBSERVER)

public abstract partial class StreamHub<TIn, TOut> : IStreamObserver<TIn>, IReleasableHub
{
    // PROPERTIES

    /// <inheritdoc/>
    public bool IsSubscribed => Provider.HasSubscriber(this);

    /// <summary>
    /// Data provider that this observer subscribes to.
    /// </summary>
    protected IStreamObservable<TIn> Provider { get; }

    /// <summary>
    /// Data provider's internal cache (read-only).
    /// </summary>
    protected IReadOnlyList<TIn> ProviderCache { get; }

    /// <summary>
    /// Subscription token for managing the subscription lifecycle.
    /// </summary>
    private IDisposable? Subscription { get; set; }

    /// <summary>
    /// Lock object to ensure thread safety during unsubscription.
    /// </summary>
    private readonly Lock _unsubscribeLock = new();

    /// <summary>
    /// Set when this hub built its own provider, so no caller holds a reference to it.
    /// </summary>
    private bool _ownsProvider;

    // METHODS

    /// <summary>
    /// Marks the provider as built by this hub, so <see cref="Unsubscribe()"/> releases it too.
    /// </summary>
    /// <remarks>
    /// Call this only from a constructor or factory that created the provider itself.
    /// A hub the caller supplied stays subscribed, because the caller may still be using it.
    /// </remarks>
    internal void OwnProvider() => _ownsProvider = true;

    /// <inheritdoc/>
    public void Unsubscribe()
    {
        Detach();
        ReleaseLinkedProviders();

        // an owned provider left with no observers would keep computing every update
        // for nothing; outside the lock so no two hubs' locks are ever held together
        if (_ownsProvider
            && Provider is IReleasableHub owned
            && !Provider.HasObservers)
        {
            owned.Unsubscribe();
        }
    }

    /// <summary>
    /// Releases subscriptions this hub holds beyond its <see cref="Provider"/>.
    /// </summary>
    /// <remarks>
    /// A hub that observes a second provider overrides this to leave it,
    /// so unsubscribing the hub stops both inputs.
    /// </remarks>
    private protected virtual void ReleaseLinkedProviders()
    {
        // no-op by default: most hubs observe only their Provider
    }

    /// <summary>
    /// Leaves the provider without releasing an owned one, so a reset can resubscribe.
    /// </summary>
    private protected void Detach()
    {
        // Ensure thread-safety for EndTransmission > OnCompleted-type race conditions
        // see https://learn.microsoft.com/en-us/dotnet/standard/events/observer-design-pattern-best-practices

        lock (_unsubscribeLock)
        {
            if (IsSubscribed)
            {
                Provider.Unsubscribe(this);
            }

            Subscription?.Dispose();
            Subscription = null; // ensure the ref is cleared
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Override this method if the input and output types are not indexed 1:1.
    /// </remarks>
    public virtual void OnAdd(TIn item, bool notify, int? indexHint)
    {
        // Lock to prevent concurrent cache access.
        // ToIndicator and AppendCache access Cache, which may be modified
        // by concurrent Rebuild operations on other threads.
        // .NET locks are reentrant, so this works when called from within Rebuild.
        lock (CacheLock)
        {
            (TOut result, int _) = ToIndicator(item, indexHint);
            AppendCache(result, notify);
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Override this method if complex state rollback or rebuild logic is required.
    /// </remarks>
    public virtual void OnRebuild(DateTime fromTimestamp)
        => Rebuild(fromTimestamp);

    /// <summary>
    /// Gets a value indicating whether this hub should prune its cache when the provider prunes.
    /// Transformation hubs (like Renko) that don't have 1:1 timestamp alignment with inputs
    /// should override this to return false.
    /// </summary>
    protected virtual bool ShouldPruneOnProviderPrune => true;

    /// <inheritdoc/>
    public void OnPrune(DateTime toTimestamp)
    {
        lock (CacheLock)
        {
            int removedCount = 0;

            // Only prune if this hub participates in provider-driven pruning
            if (ShouldPruneOnProviderPrune)
            {
                // count, then remove in one operation: per-item RemoveAt(0)
                // shifts the whole backing array on every removal
                while (removedCount < Cache.Count
                    && Cache[removedCount].Timestamp <= toTimestamp)
                {
                    removedCount++;
                }

                if (removedCount > 0)
                {
                    Cache.RemoveRange(0, removedCount);

                    // provider-driven prune drops history here too
                    MarkPruned(toTimestamp);
                }
            }

            // Call PruneState only when this hub participates in pruning and items were removed
            if (ShouldPruneOnProviderPrune && removedCount > 0)
            {
                PruneState(toTimestamp);
            }
            else if (!ShouldPruneOnProviderPrune)
            {
                // Call OnProviderPrune for hubs that don't prune their own cache
                // but need to track upstream pruning (e.g., Slope tracking itemsProcessed)
                OnProviderPrune(toTimestamp);
            }

            // notify observers (inside lock to ensure cache consistency)
            NotifyObserversOnPrune(toTimestamp);
        }
    }

    /// <summary>
    /// Called when the provider prunes, even if this hub doesn't prune its own cache.
    /// Override this to track upstream pruning events (e.g., Slope updating itemsProcessed).
    /// </summary>
    /// <param name="toTimestamp">Timestamp of last item being removed from provider.</param>
    protected virtual void OnProviderPrune(DateTime toTimestamp)
    {
        // No-op by default. Override in derived classes that need to track provider pruning.
    }

    /// <summary>
    /// Called when items are pruned from the beginning of the cache.
    /// Override this method to prune internal state arrays in sync with the cache.
    /// Prune all state items with timestamps &lt;= toTimestamp.
    /// </summary>
    /// <param name="toTimestamp">Prune all state items with timestamps less than or equal to this timestamp.</param>
    protected virtual void PruneState(DateTime toTimestamp)
    {
        // No-op by default. Override in derived classes with internal state arrays.
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Cascades the provider's error to this hub's own subscribers rather than
    /// rethrowing into the provider's notification loop, so one faulted link does
    /// not abort the provider's fan-out to its other observers.
    /// </remarks>
    public void OnError(Exception exception)
        => NotifyObserversOnError(exception);

    /// <inheritdoc/>
    /// <remarks>
    /// Completes this hub's own subscribers after leaving the provider, so
    /// the end of a transmission reaches every hub chained below it.
    /// </remarks>
    public void OnCompleted()
    {
        Unsubscribe();
        EndTransmission();
    }

}

/// <summary>
/// A hub another hub can release without knowing its input type.
/// </summary>
internal interface IReleasableHub
{
    /// <summary>
    /// Unsubscribes the hub from its provider.
    /// </summary>
    void Unsubscribe();
}
