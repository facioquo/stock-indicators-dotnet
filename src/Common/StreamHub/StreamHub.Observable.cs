namespace FacioQuo.Stock.Indicators;

// STREAM HUB (OBSERVABLE)

public abstract partial class StreamHub<TIn, TOut> : IStreamObservable<TOut>
{
    private readonly HashSet<IStreamObserver<TOut>> _observers = [];

    /// <summary>
    /// Guards every read and write of <see cref="_observers"/>. Subscribe and
    /// Unsubscribe can run on a different thread than the notify fan-out (a
    /// downstream hub is constructed while the provider is mid-<c>Add</c>), and a
    /// <see cref="HashSet{T}"/> is not safe for concurrent mutation and
    /// enumeration. The lock is held only to mutate or to take a snapshot — never
    /// while invoking an observer callback — so callbacks can re-enter
    /// Subscribe/Unsubscribe without deadlocking.
    /// </summary>
    private readonly object _observersLock = new();

    /// <summary>
    /// Cached point-in-time copy of <see cref="_observers"/>, rebuilt lazily by
    /// <see cref="SnapshotObservers"/> and invalidated (under the lock) whenever
    /// the subscriber set changes. Notification fan-out runs once per cached
    /// item across four notify paths, so reusing the snapshot avoids an array
    /// allocation per event when subscriptions are stable (the common case).
    /// </summary>
    private IStreamObserver<TOut>[]? _observersSnapshot;

    /// <summary>
    /// Baseline minimum cache size requirement for this hub (set by ValidateCacheSize).
    /// </summary>
    private int _minCacheSizeBaseline;

    // PROPERTIES

    /// <inheritdoc/>
    public virtual BinarySettings Properties { get; init; } = new(0); // default 0b00000000

    /// <inheritdoc/>
    public int MaxCacheSize { get; private set; }

    /// <inheritdoc/>
    public int MinCacheSize { get; private set; }

    /// <inheritdoc/>
    public int ObserverCount
    {
        get { lock (_observersLock) { return _observers.Count; } }
    }

    /// <inheritdoc/>
    public bool HasObservers
    {
        get { lock (_observersLock) { return _observers.Count > 0; } }
    }

    // METHODS

    /// <inheritdoc/>
    public bool HasSubscriber(IStreamObserver<TOut> observer)
    {
        lock (_observersLock) { return _observers.Contains(observer); }
    }

    /// <summary>
    /// Takes a point-in-time copy of the subscribers under the lock. Callers
    /// iterate the copy outside the lock so observer callbacks never run while
    /// the lock is held.
    /// </summary>
    private IStreamObserver<TOut>[] SnapshotObservers()
    {
        lock (_observersLock)
        {
            return _observersSnapshot ??=
                _observers.Count == 0 ? [] : [.. _observers];
        }
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IStreamObserver<TOut> observer)
    {
        lock (_observersLock)
        {
            if (_observers.Add(observer))
            {
                _observersSnapshot = null;
            }
        }

        // Update MinCacheSize to the maximum of all subscribers
        UpdateMinCacheSize();

        return new Unsubscriber(() => Unsubscribe(observer));
    }

    /// <inheritdoc/>
    public bool Unsubscribe(IStreamObserver<TOut> observer)
    {
        bool removed;
        lock (_observersLock)
        {
            removed = _observers.Remove(observer);
            if (removed)
            {
                _observersSnapshot = null;
            }
        }

        // Re-evaluate MinCacheSize after unsubscribing
        if (removed)
        {
            UpdateMinCacheSize();
        }

        return removed;
    }

    /// <summary>
    /// Updates the MinCacheSize to the maximum of this hub's baseline requirement
    /// and all subscribers' MinCacheSize values.
    /// </summary>
    private void UpdateMinCacheSize()
    {
        // Start from the hub's own baseline requirement
        int maxMinCacheSize = _minCacheSizeBaseline;

        foreach (IStreamObserver<TOut> observer in SnapshotObservers())
        {
            if (observer is IStreamObservable<ISeries> observable)
            {
                maxMinCacheSize = Math.Max(maxMinCacheSize, observable.MinCacheSize);
            }
        }

        MinCacheSize = maxMinCacheSize;
    }

    /// <inheritdoc/>
    public void EndTransmission()
    {
        IStreamObserver<TOut>[] snapshot;
        lock (_observersLock)
        {
            if (_observers.Count == 0)
            {
                return;
            }

            snapshot = [.. _observers];
            _observers.Clear();
            _observersSnapshot = null;

            // Reset to baseline when all subscribers are removed
            MinCacheSize = _minCacheSizeBaseline;
        }

        // notify outside the lock; each subscriber removes itself (now a no-op)
        foreach (IStreamObserver<TOut> o in snapshot)
        {
            o.OnCompleted();
        }
    }

    /// <summary>
    /// A disposable subscription token. Disposing it removes the observer from
    /// its provider exactly once via the synchronized
    /// <see cref="Unsubscribe(IStreamObserver{TOut})"/>. A delegate (not a hub
    /// reference) is held so the token does not appear to own the hub's lifetime.
    /// </summary>
    /// <param name="unsubscribe">
    /// Synchronized removal callback. Invoked at most once.
    /// </param>
    private sealed class Unsubscriber(Action unsubscribe) : IDisposable
    {
        private Action? _unsubscribe = unsubscribe;

        /// <summary>
        /// Remove the single observer (idempotent).
        /// </summary>
        public void Dispose()
            => Interlocked.Exchange(ref _unsubscribe, null)?.Invoke();
    }

    // Observer isolation: a faulting subscriber must not abort notification to
    // its siblings. Each callback below is guarded; a thrown exception is routed
    // to that subscriber's own OnError and the fan-out continues. A failure
    // inside OnError itself is intentionally swallowed (it cannot be routed
    // anywhere without recursing). Catching the general Exception type and the
    // deliberate swallow are required for an isolation boundary, hence the
    // CA1031 + RCS1075 suppressions scoped to this region.
#pragma warning disable CA1031, RCS1075 // intentional catch-all at an observer-isolation boundary

    /// <summary>
    /// Sends new <c>TSeries</c> item to subscribers.
    /// </summary>
    /// <param name="item"><c>TSeries</c> item to send.</param>
    /// <param name="indexHint">Provider index hint.</param>
    private protected void NotifyObserversOnAdd(TOut item, int? indexHint)
    {
        foreach (IStreamObserver<TOut> o in SnapshotObservers())
        {
            try
            {
                o.OnAdd(item, notify: true, indexHint);
            }
            catch (Exception ex)
            {
                IsolateObserverFault(o, ex);
            }
        }
    }

    /// <summary>
    /// Sends rebuilds point in time to all subscribers.
    /// </summary>
    /// <param name="fromTimestamp">Rebuild starting date.</param>
    private protected void NotifyObserversOnRebuild(DateTime fromTimestamp)
    {
        foreach (IStreamObserver<TOut> o in SnapshotObservers())
        {
            try
            {
                o.OnRebuild(fromTimestamp);
            }
            catch (Exception ex)
            {
                IsolateObserverFault(o, ex);
            }
        }
    }

    /// <summary>
    /// Sends prune notification to all subscribers.
    /// </summary>
    /// <param name="toTimestamp">Prune ending date.</param>
    private void NotifyObserversOnPrune(DateTime toTimestamp)
    {
        foreach (IStreamObserver<TOut> o in SnapshotObservers())
        {
            try
            {
                o.OnPrune(toTimestamp);
            }
            catch (Exception ex)
            {
                IsolateObserverFault(o, ex);
            }
        }
    }

    /// <summary>
    /// Sends error (exception) to all subscribers.
    /// </summary>
    /// <param name="exception">Exception to send.</param>
    private void NotifyObserversOnError(Exception exception)
    {
        foreach (IStreamObserver<TOut> o in SnapshotObservers())
        {
            try
            {
                o.OnError(exception);
            }
            catch (Exception)
            {
                // The subscriber's own error handler threw; suppress so the
                // remaining subscribers are still notified of the fault.
            }
        }
    }

    /// <summary>
    /// Routes a faulting observer callback to that observer's own
    /// <see cref="IStreamObserver{T}.OnError"/>, isolated so a failure inside the
    /// handler cannot abort notification to sibling observers.
    /// </summary>
    /// <param name="observer">The observer whose callback threw.</param>
    /// <param name="exception">The exception thrown by the callback.</param>
    private static void IsolateObserverFault(IStreamObserver<TOut> observer, Exception exception)
    {
        try
        {
            observer.OnError(exception);
        }
        catch (Exception)
        {
            // The faulting observer's error handler also threw; suppress to keep
            // sibling observers isolated from it.
        }
    }

#pragma warning restore CA1031, RCS1075 // intentional catch-all at an observer-isolation boundary

}
