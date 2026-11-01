namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streams the values of two chain providers matched by timestamp, for two-series hubs to observe.
/// </summary>
/// <remarks>
/// <para>
/// The hub observes the source provider as its <c>Provider</c>, and the other provider
/// through a private observer that stages each value in this hub. A
/// <see cref="PairedValue"/> is emitted only once both sides hold its timestamp, so
/// values from different timestamps are never paired. A timestamp that only one side
/// holds produces no result; when the missing value arrives late, the usual rebuild
/// pairs it.
/// </para>
/// <para>
/// Both providers must be driven from one thread at a time, as they are when both
/// descend from one <see cref="BarHub"/>: a rebuild from either side replays the
/// source provider's cache.
/// </para>
/// </remarks>
internal sealed class PairHub
    : ChainHub<IReusable, PairedValue>
{
    private readonly IChainProvider<IReusable> _other;
    private readonly OtherObserver _otherObserver;
    private readonly PairStager _stager;
    private readonly object _otherLock = new();
    private IDisposable? _otherSubscription;

    /// <summary>
    /// Initializes a new instance of the <see cref="PairHub"/> class.
    /// </summary>
    /// <param name="source">Provider of the source series.</param>
    /// <param name="other">Provider of the other series.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="other"/> is null.</exception>
    internal PairHub(
        IChainProvider<IReusable> source,
        IChainProvider<IReusable> other) : base(source)
    {
        ArgumentNullException.ThrowIfNull(other);

        _other = other;
        _stager = new PairStager(MaxCacheSize);
        _otherObserver = new OtherObserver(this);
        Name = "PAIR";

        // follow the other side first, then stage its history: a value that
        // arrives in between is staged twice, which Set makes idempotent
        _otherSubscription = other.Subscribe(_otherObserver);

        lock (CacheLock)
        {
            foreach (IReusable item in other.Snapshot())
            {
                _stager.Set(PairSide.Other, item.Timestamp, item.Value);
            }
        }

        Reinitialize();
    }

    /// <inheritdoc/>
    public override void OnAdd(IReusable item, bool notify, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (CacheLock)
        {
            _stager.Set(PairSide.Source, item.Timestamp, item.Value);

            if (_stager.TryGetPair(item.Timestamp, out PairedValue? pair))
            {
                AppendCache(pair, notify);
            }
        }
    }

    /// <inheritdoc/>
    protected override (PairedValue result, int index)
        ToIndicator(IReusable item, int? indexHint)
        => throw new InvalidOperationException(); // not used: OnAdd pairs instead

    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // the replay re-stages the source side from the provider cache
        if (restoreIndex < 0)
        {
            _stager.RemoveFrom(PairSide.Source, DateTime.MinValue);
        }
        else
        {
            _stager.RemoveAfter(PairSide.Source, ProviderCache[restoreIndex].Timestamp);
        }
    }

    /// <inheritdoc/>
    private protected override void ReleaseLinkedProviders()
    {
        lock (_otherLock)
        {
            if (_otherObserver.IsSubscribed)
            {
                _other.Unsubscribe(_otherObserver);
            }

            _otherSubscription?.Dispose();
            _otherSubscription = null;
        }
    }

    /// <summary>
    /// Stages a value from the other provider and pairs it.
    /// </summary>
    /// <param name="item">The other provider's new value.</param>
    /// <param name="notify">Whether to notify this hub's observers.</param>
    private void OnOtherAdd(IReusable item, bool notify)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (CacheLock)
        {
            DateTime? last = _stager.Last(PairSide.Other);
            _stager.Set(PairSide.Other, item.Timestamp, item.Value);

            if (last is null || item.Timestamp > last)
            {
                if (_stager.TryGetPair(item.Timestamp, out PairedValue? pair))
                {
                    AppendCache(pair, notify);
                }

                return;
            }

            // a correction or a late value re-pairs from its timestamp
            Rebuild(item.Timestamp);
        }
    }

    /// <summary>
    /// Re-stages the other provider's values from a timestamp, then re-pairs from it.
    /// </summary>
    /// <param name="fromTimestamp">First timestamp the other provider rebuilt.</param>
    private void OnOtherRebuild(DateTime fromTimestamp)
    {
        lock (CacheLock)
        {
            _stager.RemoveFrom(PairSide.Other, fromTimestamp);

            // runs inside the other provider's notification, which holds its lock
            IReadOnlyList<IReusable> results = _other.Results;
            int index = results.IndexGte(fromTimestamp);

            if (index >= 0)
            {
                for (int i = index; i < results.Count; i++)
                {
                    IReusable item = results[i];
                    _stager.Set(PairSide.Other, item.Timestamp, item.Value);
                }
            }

            Rebuild(fromTimestamp);
        }
    }

    /// <summary>
    /// Observes the other provider on behalf of a <see cref="PairHub"/>.
    /// </summary>
    /// <param name="hub">Hub that pairs the values.</param>
    private sealed class OtherObserver(PairHub hub) : IStreamObserver<IReusable>
    {
        /// <inheritdoc/>
        public bool IsSubscribed => hub._other.HasSubscriber(this);

        /// <inheritdoc/>
        public void Unsubscribe() => hub.ReleaseLinkedProviders();

        /// <inheritdoc/>
        public void OnAdd(IReusable item, bool notify, int? indexHint)
            => hub.OnOtherAdd(item, notify);

        /// <inheritdoc/>
        public void OnRebuild(DateTime fromTimestamp)
            => hub.OnOtherRebuild(fromTimestamp);

        /// <inheritdoc/>
        /// <remarks>The staged values are bounded by the hub's cache size instead.</remarks>
        public void OnPrune(DateTime toTimestamp)
        {
            // no-op: keeping the staged values lets a later rebuild still pair them
        }

        /// <inheritdoc/>
        public void OnError(Exception exception) => hub.OnError(exception);

        /// <inheritdoc/>
        public void OnCompleted() => hub.OnCompleted();
    }
}
