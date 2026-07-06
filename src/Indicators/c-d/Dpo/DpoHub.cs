namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Detrended Price Oscillator (DPO) using a stream hub.
/// </summary>
/// <remarks>
/// DPO calculation at any position relies on data values before and after it in the timeline.
/// Therefore, it can only be calculated after sufficient subsequent data arrives and is
/// retroactively updated. For real-time processing, DPO values are initially null
/// (incalculable) and are updated as enough data becomes available with an offset delay.
/// Results maintain 1:1 correspondence with inputs.
/// </remarks>
public class DpoHub
    : ChainHub<IReusable, DpoResult>, IDpo
{
    internal DpoHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods) : base(provider)
    {
        Dpo.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        Offset = (lookbackPeriods / 2) + 1;
        Name = $"DPO({lookbackPeriods})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc/>
    public int Offset { get; init; }

    /// <remarks>
    /// DPO at any position requires an offset number of subsequent values for calculation.
    /// Emits results for all positions from Cache.Count to current index, calculating DPO when
    /// sufficient subsequent data is available, otherwise emitting null values.
    /// As new data arrives, retroactively recalculates earlier positions that now have sufficient data.
    /// Maintains 1:1 correspondence between inputs and outputs.
    /// </remarks>
    /// <inheritdoc />
    public override void OnAdd(IReusable item, bool notify, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (CacheLock)
        {
            int i = indexHint ?? ProviderCache.IndexOf(item, true);

            // DPO at position dpoIndex requires SMA at position (dpoIndex + offset)
            // Calculate the range of positions that now have sufficient lookahead data
            int maxCalculableIndex = i - Offset;

            // Determine the starting position for emission
            int startIndex = Cache.Count;

            // Check if we need to recalculate any existing results due to new lookahead data
            if (notify && startIndex > 0 && maxCalculableIndex >= 0)
            {
                // Check if there are positions in our cache that have null values
                // but now have sufficient lookahead data for calculation
                // This happens during normal streaming when bars arrive in order

                // Look backwards from maxCalculableIndex to find positions with null DPO
                for (int checkIndex = Math.Max(0, maxCalculableIndex - LookbackPeriods);
                    checkIndex <= Math.Min(maxCalculableIndex, startIndex - 1);
                    checkIndex++)
                {
                    if (checkIndex >= 0 && checkIndex < Cache.Count && checkIndex < ProviderCache.Count)
                    {
                        DpoResult existingResult = Cache[checkIndex];
                        if (!existingResult.Dpo.HasValue)
                        {
                            // Found a null position that now has lookahead data
                            // Trigger rebuild from this position
                            Rebuild(ProviderCache[checkIndex].Timestamp);
                            return;
                        }
                    }
                }
            }

            // Emit all results from current cache size up to current provider index
            // to maintain 1:1 correspondence (some will be null if insufficient lookahead)
            for (int dpoIndex = startIndex; dpoIndex <= i; dpoIndex++)
            {
                if (dpoIndex < 0 || dpoIndex >= ProviderCache.Count)
                {
                    continue;
                }

                DpoResult result = CalculateDpoAtIndex(dpoIndex);
                AppendCache(result, notify);
            }
        }
    }

    /// <summary>
    /// Calculates DPO result for a specific index position.
    /// </summary>
    /// <param name="dpoIndex">DPO value to calculate (index position).</param>
    private DpoResult CalculateDpoAtIndex(int dpoIndex)
    {
        IReusable dpoTargetItem = ProviderCache[dpoIndex];
        int smaIndex = dpoIndex + Offset;

        // Check if we can calculate DPO for this position
        int firstValidDpoIndex = LookbackPeriods - Offset - 1;

        // Need sufficient historical data AND lookahead data
        if (dpoIndex >= firstValidDpoIndex
            && smaIndex < ProviderCache.Count
            && smaIndex >= LookbackPeriods - 1)
        {
            // Calculate SMA at the lookahead position using Sma.Increment utility
            double smaValue = Sma.Increment(ProviderCache, LookbackPeriods, smaIndex);
            double? sma = smaValue.NaN2Null();
            double? dpoVal = sma.HasValue ? dpoTargetItem.Value - sma.Value : null;

            return new DpoResult(dpoTargetItem.Timestamp, dpoVal, sma);
        }

        // Return empty result if we can't calculate
        return new DpoResult(dpoTargetItem.Timestamp);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// ToIndicator is used during explicit calls and must handle lookahead correctly.
    /// This implementation delegates to CalculateDpoAtIndex for consistency.
    /// </remarks>
    protected override (DpoResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);
        DpoResult result = CalculateDpoAtIndex(i);
        return (result, i);
    }

    /// <summary>
    /// Restores the DPO state and adjusts cache for retroactive calculation requirements.
    /// </summary>
    /// <param name="restoreIndex">Last ProviderCache index to preserve, or -1 to reset.</param>
    /// <remarks>
    /// DPO calculation at any position relies on data values with an offset: DPO[i] = Value[i] - SMA[i + offset].
    /// When a bar is inserted/removed at position p, all positions from (p - offset) onward
    /// are affected because their calculations depend on SMA values that include the mutation position.
    /// This override clears cache entries from the earlier affected position to ensure
    /// all offset-dependent positions are retroactively recalculated during rebuild.
    /// </remarks>
    protected override void RollbackState(int restoreIndex)
    {
        // Derive mutation index (first entry being removed)
        int mutationIndex = restoreIndex + 1;
        if (mutationIndex >= ProviderCache.Count)
        {
            return;
        }

        // Calculate first affected index accounting for lookahead
        // Positions as early as (mutationIndex - Offset) are affected
        // because they need SMA values that include the mutation position
        int firstAffectedIndex = Math.Max(0, mutationIndex - Offset);

        // Remove cache entries from the earlier affected position
        // This ensures all lookahead-dependent positions are recalculated
        if (firstAffectedIndex < mutationIndex && Cache.Count > firstAffectedIndex)
        {
            DateTime adjustedTimestamp = ProviderCache[firstAffectedIndex].Timestamp;
            Cache.RemoveAll(c => c.Timestamp >= adjustedTimestamp);
        }

        // No internal state to restore for DPO (stateless indicator)
    }

    /// <summary>
    /// Rebuilds the cache from a specific timestamp, adjusting for DPO's backward offset.
    /// </summary>
    /// <param name="fromTimestamp">Point in time to rebuild from.</param>
    /// <remarks>
    /// DPO requires lookahead data for calculation: DPO[i] = Value[i] - SMA[i + offset].
    /// When provider history is mutated (Add/Remove), downstream observers need to be
    /// notified from the adjusted position that accounts for the backward offset.
    /// This ensures chained observers recalculate all affected positions.
    /// </remarks>
    public override void Rebuild(DateTime fromTimestamp)
    {
        // Calculate adjusted timestamp accounting for backward offset
        int mutationIndex = ProviderCache.IndexGte(fromTimestamp);
        if (mutationIndex >= 0)
        {
            int firstAffectedIndex = Math.Max(0, mutationIndex - Offset);
            if (firstAffectedIndex < mutationIndex)
            {
                fromTimestamp = ProviderCache[firstAffectedIndex].Timestamp;
            }
        }

        // Call base implementation with adjusted timestamp
        base.Rebuild(fromTimestamp);
    }
}

/// <summary>
/// Provides methods for creating DPO hubs.
/// </summary>
public static partial class Dpo
{
    /// <summary>
    /// Converts the chain provider to a DPO hub.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A DPO hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    public static DpoHub ToDpoHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods = 14)
             => new(chainProvider, lookbackPeriods);
}
