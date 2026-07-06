namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides streaming hub for Slope and linear regression calculations.
/// </summary>
/// <remarks>
/// The Slope indicator exhibits legitimate historical repaint behavior where Line values
/// for the last lookbackPeriods results are recalculated using the most recent slope/intercept.
/// This matches the Series implementation's behavior.
/// <para>
/// <see cref="CalculateStatistics"/> retains the two-pass deviation form for the same
/// numerical-stability reason documented on <see cref="SlopeList"/> — see those remarks.
/// </para>
/// </remarks>
public class SlopeHub
    : ChainHub<IReusable, SlopeResult>, ISlope
{
    private readonly Queue<double> buffer;

    // Track how many items have been removed from the beginning (like BufferList)
    private int globalIndexOffset;

    // Track how many unique items have been added (for deduplication)
    private int itemsAdded;
    private DateTime? lastSeenTimestamp;

    // Track cache size to detect pruning
    private int lastCacheSize;

    // Cache latest slope/intercept to avoid cache lookups in OnAdd
    private double? currentSlope;
    private double? currentIntercept;

    /// <summary>
    /// Initializes a new instance of the <see cref="SlopeHub"/> class.
    /// </summary>
    /// <param name="provider">Chain provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    internal SlopeHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods) : base(provider)
    {
        Slope.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        Name = $"SLOPE({lookbackPeriods})";
        buffer = new Queue<double>(lookbackPeriods);

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods, Name);

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
    public override void OnAdd(IReusable item, bool notify, int? indexHint)
    {
        // Call base to add the result to cache
        base.OnAdd(item, notify, indexHint);

        // Track cache size for pruning detection
        lastCacheSize = Cache.Count;

        // Update Line values for the last lookbackPeriods results
        // This is done after the result is in the cache
        int currentIndex = Cache.Count - 1;
        if (currentIndex >= LookbackPeriods - 1)
        {
            UpdateLineValues(currentIndex, currentSlope, currentIntercept);
        }
    }

    /// <inheritdoc/>
    protected override (SlopeResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // Increment items added counter for new unique items only
        // Duplicates (same timestamp) don't increment the counter
        bool isNewItem = item.Timestamp != lastSeenTimestamp;
        if (isNewItem)
        {
            lastSeenTimestamp = item.Timestamp;
            itemsAdded++;
        }

        // Update buffer
        buffer.Update(LookbackPeriods, item.Value);

        // Skip initialization period
        if (i < LookbackPeriods - 1)
        {
            return (new SlopeResult(item.Timestamp), i);
        }

        // Calculate the global index (absolute position accounting for pruned items)
        int globalIndex = globalIndexOffset + i;

        // Calculate slope, intercept, and statistics using global index
        (double? slope, double? intercept, double? stdDev, double? rSquared)
            = CalculateStatistics(globalIndex);

        // Cache slope and intercept for use in OnAdd
        currentSlope = slope;
        currentIntercept = intercept;

        // Create result (Line will be updated in OnAdd after this is added to Cache)
        SlopeResult r = new(
            Timestamp: item.Timestamp,
            Slope: slope,
            Intercept: intercept,
            StdDev: stdDev,
            RSquared: rSquared,
            Line: null);

        return (r, i);
    }

    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Clear buffer
        buffer.Clear();

        // Clear cached slope/intercept
        currentSlope = null;
        currentIntercept = null;

        // Reset counters to match the restore point
        // NOTE: Do NOT reset globalIndexOffset - it tracks provider pruning and must persist across rollbacks
        itemsAdded = restoreIndex + 1;
        lastSeenTimestamp = restoreIndex >= 0 ? ProviderCache[restoreIndex].Timestamp : null;

        if (restoreIndex < LookbackPeriods - 1)
        {
            return;
        }

        // Rebuild buffer from cache up to restoreIndex
        int startIdx = Math.Max(0, restoreIndex - LookbackPeriods + 1);

        for (int p = startIdx; p <= restoreIndex; p++)
        {
            buffer.Enqueue(ProviderCache[p].Value);
        }
    }

    /// <summary>
    /// Called when items are pruned from the cache.
    /// Update globalIndexOffset to track removed items (like BufferList).
    /// </summary>
    /// <param name="toTimestamp">Timestamp of last item being removed.</param>
    protected override void PruneState(DateTime toTimestamp)
    {
        // Calculate how many items were removed by comparing cache sizes
        // This is called AFTER items are removed from Cache
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

    /// <summary>
    /// Calculates slope, intercept, standard deviation, and R-squared using global X values.
    /// </summary>
    /// <param name="globalIndex">Global index (0-based absolute position) of the current item.</param>
    private (double? slope, double? intercept, double? stdDev, double? rSquared)
        CalculateStatistics(int globalIndex)
    {
        // Calculate global X values mathematically (like BufferList for precision)
        // X values are: (globalIndex - lookbackPeriods + 2) to (globalIndex + 1)
        // This matches Series which uses 1-based X values: (index + 1)
        double firstX = globalIndex - LookbackPeriods + 2d;
        double sumX = (LookbackPeriods * firstX) + (LookbackPeriods * (LookbackPeriods - 1) / 2.0);
        double avgX = sumX / LookbackPeriods;

        // Calculate avgY
        double sumY = 0;
        foreach (double bufferValue in buffer)
        {
            sumY += bufferValue;
        }

        double avgY = sumY / LookbackPeriods;

        // Calculate deviations and their products
        double sumSqX = 0;
        double sumSqY = 0;
        double sumSqXy = 0;
        int relativeIndex = 0;

        foreach (double bufferValue in buffer)
        {
            double xValue = firstX + relativeIndex;
            double devX = xValue - avgX;
            double devY = bufferValue - avgY;

            sumSqX += devX * devX;
            sumSqY += devY * devY;
            sumSqXy += devX * devY;
            relativeIndex++;
        }

        // Calculate slope and intercept directly using global X values
        double? slope = (sumSqXy / sumSqX).NaN2Null();
        double? intercept = (avgY - (slope * avgX)).NaN2Null();

        // Calculate Standard deviation and R-squared
        double stdDevX = Math.Sqrt(sumSqX / LookbackPeriods);
        double stdDevY = Math.Sqrt(sumSqY / LookbackPeriods);

        double? rSquared = null;

        if (stdDevX * stdDevY != 0)
        {
            double arrr = sumSqXy / (stdDevX * stdDevY) / LookbackPeriods;
            rSquared = (arrr * arrr).NaN2Null();
        }

        return (slope, intercept, stdDevY.NaN2Null(), rSquared);
    }

    /// <summary>
    /// Updates Line values for the last lookbackPeriods results using the current slope/intercept.
    /// This is legitimate historical repaint behavior matching the Series implementation.
    /// </summary>
    /// <param name="currentIndex">Current index in the cache.</param>
    /// <param name="slope">Calculated slope value.</param>
    /// <param name="intercept">Calculated global intercept value.</param>
    private void UpdateLineValues(int currentIndex, double? slope, double? intercept)
    {
        // Calculate the range of indices that should have Line values
        int startIndex = currentIndex - LookbackPeriods + 1;

        // Nullify the SINGLE Line value that just exited the window (if any)
        int exitedIndex = startIndex - 1;
        if (exitedIndex >= 0)
        {
            SlopeResult exited = Cache[exitedIndex];
            // Only update if Line was previously set (optimization)
            if (exited.Line is not null)
            {
                Cache[exitedIndex] = exited with { Line = null };
            }
        }

        // Update Line values using global X values: line = slope × X + intercept
        // where X = globalIndex + 1 (1-based, matching Series)
        for (int p = startIndex; p <= currentIndex; p++)
        {
            SlopeResult existing = Cache[p];

            // Calculate global index for this position
            int globalIndex = globalIndexOffset + p;

            // Calculate Line using global X value (1-based)
            decimal? line = (decimal?)((slope * (globalIndex + 1)) + intercept).NaN2Null();

            Cache[p] = existing with { Line = line };
        }
    }
}

public static partial class Slope
{
    /// <summary>
    /// Creates a Slope streaming hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A Slope hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    public static SlopeHub ToSlopeHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods = 14)
        => new(chainProvider, lookbackPeriods);
}
