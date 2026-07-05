namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Management of observing + processing of streamed inbound data.
/// </summary>
/// <typeparam name="T">
/// The object that provides notification information.
/// </typeparam>
public interface IStreamObserver<in T>
{
    // PROPERTIES

    /// <summary>
    /// Current state of subscription to provider.
    /// </summary>
    bool IsSubscribed { get; }

    // METHODS

    /// <summary>
    /// Unsubscribe from the data provider.
    /// </summary>
    void Unsubscribe();

    /// <summary>
    /// Receives and reacts to new input data.
    /// Typically, input data will be converted and cached.
    /// </summary>
    /// <param name="item">
    /// The current notification information (input data).
    /// </param>
    /// <param name="notify">
    /// Instruction to notify subscribers of the new item.
    /// </param>
    /// <param name="indexHint">
    /// Provider index hint, if known.
    /// </param>
    void OnAdd(T item, bool notify, int? indexHint);

    /// <summary>
    /// Provides the observer with starting point in timeline
    /// to rebuild and cascade to all its own subscribers.
    /// </summary>
    /// <param name="fromTimestamp">
    /// Starting point in timeline to rebuild.
    /// </param>
    void OnRebuild(DateTime fromTimestamp);

    /// <summary>
    /// Provides the observer with notification to prune data that
    /// exceeds <see cref="IStreamObservable{T}.MaxCacheSize"/>."/>
    /// </summary>
    /// <param name="toTimestamp">
    /// Ending point in timeline to prune.
    /// </param>
    void OnPrune(DateTime toTimestamp);

    /// <summary>
    /// Provides the observer with errors from the provider
    /// that have produced a faulted state.
    /// </summary>
    /// <param name="exception">
    /// An exception with additional information about the error.
    /// </param>
    void OnError(Exception exception);

    /// <summary>
    /// Provides the observer with final notice that the data
    /// provider has finished sending push-based notifications.
    /// </summary>
    /// <remarks>
    /// Completion indicates that publisher will never send
    /// additional data. This is only used for finite data
    /// streams; and is different from faulted <see cref="OnError(Exception)"/>.
    /// </remarks>
    void OnCompleted();

}
