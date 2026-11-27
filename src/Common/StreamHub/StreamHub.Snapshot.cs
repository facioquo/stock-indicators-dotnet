namespace FacioQuo.Stock.Indicators;

public abstract partial class StreamHub<TIn, TOut>
{
    /// <summary>
    /// Encodes the hub's internal indicator state as of its newest cached item.
    /// </summary>
    /// <returns>The encoded state, or <c>null</c> when the hub does not support snapshots.</returns>
    private protected virtual byte[]? CaptureSnapshotState() => null;

    /// <summary>
    /// Restores internal indicator state written by <see cref="CaptureSnapshotState"/>.
    /// </summary>
    /// <param name="state">Encoded state.</param>
    /// <exception cref="NotSupportedException">The hub does not support snapshots.</exception>
    private protected virtual void RestoreSnapshotState(ReadOnlySpan<byte> state)
        => throw new NotSupportedException($"{Name} does not support state snapshots.");

    /// <summary>
    /// Captures the result cache and internal state, so a later process can resume
    /// streaming with <see cref="RestoreSnapshot"/> instead of replaying the source history.
    /// </summary>
    /// <returns>A snapshot that does not change as the hub keeps running.</returns>
    /// <exception cref="NotSupportedException">The hub does not support snapshots.</exception>
    public HubSnapshot<TOut> CaptureSnapshot()
    {
        lock (CacheLock)
        {
            byte[] state = CaptureSnapshotState()
                ?? throw new NotSupportedException($"{Name} does not support state snapshots.");

            return new HubSnapshot<TOut>(
                HubSnapshot.FormatVersion,
                Name,
                PrunedThrough,
                Cache.ToArray().AsReadOnly(),
                state);
        }
    }

    /// <summary>
    /// Replaces the result cache and internal state with a snapshot's, without notifying observers.
    /// </summary>
    /// <remarks>
    /// Restore from the root hub outward: a chained hub's provider must already hold the
    /// inputs its restored results were computed from. Subscribers are not told, so
    /// restore each of them the same way; one that does not support snapshots keeps its
    /// old cache, and needs <c>Rebuild()</c> after its provider is restored.
    /// A hub keeps the later of its own and the snapshot's pruned boundary, so restore into
    /// a fresh hub: a used hub that pruned further refuses bars the snapshot's hub accepted.
    /// </remarks>
    /// <param name="snapshot">Snapshot captured from a hub with the same name.</param>
    /// <exception cref="InvalidDataException">
    /// The snapshot has another format version, another hub name, more results than the cache holds,
    /// or results that do not line up with the provider's inputs, or its state has a version or
    /// length the hub cannot read.
    /// </exception>
    /// <exception cref="NotSupportedException">The hub does not support snapshots.</exception>
    public void RestoreSnapshot(HubSnapshot<TOut> snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (snapshot.FormatVersion != HubSnapshot.FormatVersion)
        {
            throw new InvalidDataException(
                $"Snapshot format {snapshot.FormatVersion} is not supported; expected {HubSnapshot.FormatVersion}.");
        }

        if (!string.Equals(snapshot.HubName, Name, StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Snapshot is for {snapshot.HubName}, not {Name}.");
        }

        if (snapshot.Results.Count > MaxCacheSize)
        {
            throw new InvalidDataException(
                $"Snapshot holds {snapshot.Results.Count} results, more than the cache limit of {MaxCacheSize}.");
        }

        lock (CacheLock)
        {
            // a chained hub caches one result per provider item
            if (!IsRootHub
                && (snapshot.Results.Count != ProviderCache.Count
                    || (snapshot.Results.Count > 0
                        && (snapshot.Results[0].Timestamp != ProviderCache[0].Timestamp
                            || snapshot.Results[^1].Timestamp != ProviderCache[^1].Timestamp))))
            {
                throw new InvalidDataException(
                    $"{Name} results do not line up with its provider; restore the provider first.");
            }

            RestoreSnapshotState(snapshot.State.Span);

            Cache.Clear();

            foreach (TOut result in snapshot.Results)
            {
                Cache.Add(result);
            }

            LastItem = Cache.Count > 0 ? Cache[^1] : default;
            // pruning is irreversible, so a restore can only widen the refused span
            if (snapshot.PrunedThrough is DateTime prunedThrough)
            {
                MarkPruned(prunedThrough);
            }

            ResetFault();
        }
    }
}
