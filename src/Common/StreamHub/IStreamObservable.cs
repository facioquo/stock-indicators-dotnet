namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provider of data + management of and notification to observing subscribers.
/// </summary>
/// <typeparam name="T">
/// The object that provides notification information.
/// </typeparam>
public interface IStreamObservable<out T>
{
    /// <summary>
    /// Gets the hub observable properties and behaviors.
    /// </summary>
    /// <remarks>
    /// This <see cref="BinarySettings"/> struct holds cumulative overrides for
    /// a streaming hub.  Observer hubs inherit these values cumulatively when
    /// instantiated as a <see cref="StreamHub{TIn, TOut}"/>, except where masked.
    /// <para>
    /// Default settings are a binary set of 0b00000000, where 1 values represent
    /// exceptional (atypical) behaviors.
    /// </para>
    /// <list type="table">
    /// <item>
    ///   <term>0</term>
    ///   <description>
    ///   Disable observer: a non-observing observable (e.g. base provider).
    ///   </description>
    /// </item>
    /// <item>
    ///   <term>1</term>
    ///   <description>
    ///   Allow duplicates:
    ///   bypass rebuild analysis and duplicate prevention when caching new results.
    ///   </description>
    /// </item>
    /// <item>
    ///   <term>2-7</term>
    ///   <description>[unused positions]</description>
    /// </item>
    /// </list>
    /// </remarks>
    BinarySettings Properties { get; }

    /// <summary>
    /// Read-only list of the stored cache values.
    /// </summary>
    /// <remarks>
    /// This is a <strong>live read-only view</strong> over the internal
    /// <see cref="StreamHub{TIn, TOut}.Cache"/>, not an immutable snapshot.
    /// Mutation is forbidden, but enumeration during a concurrent <c>Add</c>
    /// or <c>Rebuild</c> on the source hub will throw
    /// <see cref="InvalidOperationException"/>.
    /// <para>
    /// Consumers that iterate while upstream may emit must snapshot first —
    /// prefer <see cref="Snapshot"/> for a detached copy (taken atomically
    /// under the cache lock on a <see cref="StreamHub{TIn, TOut}"/>).
    /// </para>
    /// </remarks>
    IReadOnlyList<T> Results { get; }

    /// <summary>
    /// Returns a detached, read-only point-in-time copy of the cached values.
    /// </summary>
    /// <remarks>
    /// Unlike the live <see cref="Results"/> view, the returned list is a
    /// separate copy: it is safe to enumerate on another thread and will not
    /// throw if the source hub mutates afterwards. A
    /// <see cref="StreamHub{TIn, TOut}"/> takes the copy under its cache lock,
    /// so the snapshot is internally consistent even when taken concurrently
    /// with a rebuild; this default implementation is best-effort for other
    /// providers (it copies <see cref="Results"/> without a lock).
    /// </remarks>
    /// <returns>A detached, read-only copy of the current cache.</returns>
    IReadOnlyList<T> Snapshot() => Results.ToArray().AsReadOnly();

    /// <summary>
    /// Gets the maximum size of the Cache list.
    /// </summary>
    int MaxCacheSize { get; }

    /// <summary>
    /// Gets the minimum cache size required by this hub or its subscribers.
    /// This value represents the maximum minimum cache size requirement from all subscribers.
    /// Additions that would modify index values before this position are rejected to prevent
    /// corrupted rebuilds.
    /// </summary>
    /// <remarks>Default implementation returns 0 (no minimum requirement).</remarks>
    int MinCacheSize => 0;

    /// <summary>
    /// Gets the current number of subscribers.
    /// </summary>
    int ObserverCount { get; }

    /// <summary>
    /// Gets a value indicating whether the provider currently has subscribers.
    /// </summary>
    bool HasObservers { get; }

    /// <summary>
    /// Checks if a specific observer is subscribed.
    /// </summary>
    /// <param name="observer">
    /// Subscriber <c>IStreamObserver</c> reference.
    /// </param>
    /// <returns>True if subscribed/registered.</returns>
    bool HasSubscriber(IStreamObserver<T> observer);

    /// <summary>
    /// Notifies the provider that an observer is to receive notifications.
    /// </summary>
    /// <param name="observer">
    /// The object that is to receive notifications.
    /// </param>
    /// <returns>
    /// A reference to an interface that allows observers
    /// to stop receiving notifications before the provider
    /// has finished sending them.
    /// </returns>
    IDisposable Subscribe(IStreamObserver<T> observer);

    /// <summary>
    /// Unsubscribe from the data provider.
    /// </summary>
    /// <inheritdoc cref="HashSet{T}.Remove(T)"/>
    bool Unsubscribe(IStreamObserver<T> observer);

    /// <summary>
    /// Unsubscribe all observers (subscribers).
    /// </summary>
    void EndTransmission();
}
