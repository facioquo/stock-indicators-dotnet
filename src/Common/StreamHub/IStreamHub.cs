namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub: management of observer
/// and observable indicator data
/// </summary>
/// <typeparam name="TIn">
/// Type of inbound provider data.
/// </typeparam>
/// <typeparam name="TOut">
/// Type of outbound indicator data.
/// </typeparam>
public interface IStreamHub<in TIn, out TOut> : IStreamObserver<TIn>, IStreamObservable<TOut>
    where TIn : ISeries
{
    /// <summary>
    /// Name of this hub instance.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The cache and provider failed and is no longer operational.
    /// </summary>
    /// <remarks>
    /// This occurs when there is an overflow condition
    /// from a circular chain or
    /// when there were too many sequential duplicates.
    /// <para>
    /// Use <see cref="ResetFault()"/> to remove this flag.
    /// </para>
    /// </remarks>
    bool IsFaulted { get; }

    /// <summary>
    /// Resets the <see cref="IsFaulted"/> flag and overflow counter.
    /// Use this after recovering from an error.
    /// </summary>
    /// <remarks>
    /// You may also need to
    /// <see cref="Reinitialize()"/>, or
    /// <see cref="Rebuild()"/>.
    /// </remarks>
    void ResetFault();

    /// <summary>
    /// Add a single new item.
    /// We'll determine if it's new or an update.
    /// </summary>
    /// <param name="newIn">
    /// New item to add
    /// </param>
    void Add(TIn newIn);

    /// <summary>
    /// Add a batch of new items.
    /// We'll determine if they're new or updated.
    /// </summary>
    /// <param name="batchIn">
    /// Batch of new items to add
    /// </param>
    void Add(IEnumerable<TIn> batchIn);

    /// <summary>
    /// Delete an item from the cache, from a specific position.
    /// </summary>
    /// <param name="cacheIndex">Position in cache to delete</param>
    /// <exception cref="ArgumentOutOfRangeException"/>
    void RemoveAt(int cacheIndex);

    /// <summary>
    /// Deletes newer cached records from point in time (inclusive).
    /// </summary>
    /// <remarks>
    /// For observers, if your intention is to rebuild from a provider,
    /// use alternate <see cref="Rebuild(DateTime)"/>.
    /// </remarks>
    /// <param name="fromTimestamp">
    /// All periods (inclusive) after this DateTime will be removed.
    /// </param>
    /// <param name="notify">
    /// Notify subscribers of the delete point.
    /// </param>
    void RemoveRange(DateTime fromTimestamp, bool notify);

    /// <summary>
    /// Deletes newer cached records from an index position (inclusive).
    /// </summary>
    /// <remarks>
    /// For observers, if your intention is to rebuild from a provider,
    /// use alternate <see cref="Rebuild(int)"/>.
    /// </remarks>
    /// <param name="fromIndex">From index, inclusive</param>
    /// <param name="notify">
    /// Notify subscribers of the delete position.
    /// </param>
    void RemoveRange(int fromIndex, bool notify);

    /// <summary>
    /// Full reset of the provider subscription.
    /// </summary>
    /// <remarks>
    /// This unsubscribes from the provider,
    /// rebuilds the cache, resets faulted states,
    /// and then re-subscribes to the provider.
    /// <para>
    /// This is done automatically on hub
    /// instantiation, so it's only needed if you
    /// want to manually reset the hub.
    /// </para>
    /// <para>
    /// If you only need to rebuild the cache,
    /// use <see cref="Rebuild()"/> instead.
    /// </para>
    /// </remarks>
    void Reinitialize();

    /// <summary>
    /// Resets the entire results cache
    /// and rebuilds it from provider sources,
    /// with cascading updates to subscribers.
    /// </summary>
    /// <remarks>
    /// This is different from <see cref="Reinitialize()"/>.
    /// It does not reset the provider subscription.
    /// </remarks>
    void Rebuild();

    /// <summary>
    /// Resets the results cache from a point in time
    /// and rebuilds it from provider sources,
    /// with cascading updates to subscribers.
    /// </summary>
    /// <param name="fromTimestamp">
    /// All periods (inclusive) after this date/time
    /// will be removed and recalculated.
    /// </param>
    void Rebuild(DateTime fromTimestamp);

    /// <summary>
    /// Resets the results cache from an index position
    /// and rebuilds it from provider sources,
    /// with cascading updates to subscribers.
    /// </summary>
    /// <param name="fromIndex">
    /// All periods (inclusive) after this index position
    /// will be removed and recalculated.
    /// </param>
    void Rebuild(int fromIndex);

    /// <summary>
    /// Returns a short text label for the hub
    /// with parameter values, e.g. "EMA(10)"
    /// </summary>
    /// <returns>String label</returns>
    string ToString();
}
