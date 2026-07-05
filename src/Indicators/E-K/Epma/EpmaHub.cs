namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Endpoint Moving Average (EPMA)
/// </summary>
public class EpmaHub
    : ChainHub<SlopeResult, EpmaResult>, IEpma
{
    // Track how many items have been removed from the beginning
    private int globalIndexOffset;

    // Track how many unique items have been added (for deduplication)
    private int itemsAdded;
    private DateTime? lastSeenTimestamp;
    private int lastCacheSize;

    internal EpmaHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods)
        : this(provider.ToSlopeHub(lookbackPeriods), lookbackPeriods)
    { }

    internal EpmaHub(
        SlopeHub slopeHub,
        int lookbackPeriods) : base(slopeHub)
    {
        Epma.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        Name = $"EPMA({lookbackPeriods})";

        // Initialize counters
        globalIndexOffset = 0;
        itemsAdded = 0;
        lastSeenTimestamp = null;
        lastCacheSize = 0;

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc/>
    public override void OnAdd(SlopeResult item, bool notify, int? indexHint)
    {
        // Call base to add the result to cache
        base.OnAdd(item, notify, indexHint);

        // Track cache size for pruning detection
        lastCacheSize = Cache.Count;
    }

    /// <inheritdoc/>
    protected override (EpmaResult result, int index)
        ToIndicator(SlopeResult item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // Increment items added counter for new unique items only
        bool isNewItem = item.Timestamp != lastSeenTimestamp;
        if (isNewItem)
        {
            lastSeenTimestamp = item.Timestamp;
            itemsAdded++;
        }

        // Calculate the global index (absolute position accounting for pruned items)
        int globalIndex = globalIndexOffset + i;

        // Calculate EPMA using the endpoint formula
        // EPMA = slope × endpointX + intercept
        // where endpointX = globalIndex + 1 (1-based, matching Slope's X values)
        // Match Series calculation exactly: ((slope * (index + 1)) + intercept).NaN2Null()
        double? epma = ((item.Slope * (globalIndex + 1)) + item.Intercept).NaN2Null();

        EpmaResult r = new(
            Timestamp: item.Timestamp,
            Epma: epma);

        return (r, i);
    }

    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Reset counters to match the restore point
        // NOTE: Do NOT reset globalIndexOffset - it tracks provider pruning and must persist across rollbacks
        itemsAdded = restoreIndex + 1;
        lastSeenTimestamp = restoreIndex >= 0 ? ProviderCache[restoreIndex].Timestamp : null;
    }

    /// <inheritdoc/>
    protected override void PruneState(DateTime toTimestamp)
    {
        // Calculate how many items were removed by comparing cache sizes
        int currentCacheSize = Cache.Count;
        int removedCount = lastCacheSize - currentCacheSize;

        if (removedCount > 0)
        {
            // Update global index offset to maintain absolute positions
            globalIndexOffset += removedCount;
        }

        // Update lastCacheSize for next pruning event
        lastCacheSize = currentCacheSize;
    }
}

public static partial class Epma
{
    /// <summary>
    /// Converts the chain provider to an EPMA hub.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>An EPMA hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    public static EpmaHub ToEpmaHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods)
             => new(chainProvider, lookbackPeriods);
}
