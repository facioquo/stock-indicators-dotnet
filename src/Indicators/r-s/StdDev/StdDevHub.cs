namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents a Standard deviation stream hub.
/// </summary>
public class StdDevHub
    : ChainHub<IReusable, StdDevResult>, IStdDev
{
    private readonly Queue<double> buffer;

    internal StdDevHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods) : base(provider)
    {
        StdDev.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        buffer = new Queue<double>(lookbackPeriods);
        Name = $"STDDEV({lookbackPeriods})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }
    /// <inheritdoc/>
    protected override (StdDevResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // advance the rolling window of raw values and emit once it is full
        buffer.Update(LookbackPeriods, item.Value);

        // Two-pass algorithm over the rolling buffer: a fresh window mean
        // followed by the sum of squared deviations. The fresh-sum pass is
        // required for exact Series parity (a maintained running sum drifts).
        double? stdDev = null;
        double? mean = null;
        double? zScore = null;

        double meanValue = buffer.Average(LookbackPeriods);

        if (!double.IsNaN(meanValue))
        {
            mean = meanValue;

            // Calculate sum of squared deviations (numerically stable method)
            double sumSqDev = 0;
            foreach (double value in buffer)
            {
                double deviation = value - meanValue;
                sumSqDev += deviation * deviation;
            }

            // Calculate standard deviation
            stdDev = Math.Sqrt(sumSqDev / LookbackPeriods);

            // Calculate Z-score
            zScore = stdDev == 0 ? double.NaN : (item.Value - meanValue) / stdDev.Value;
        }

        // candidate result
        StdDevResult r = new(
            Timestamp: item.Timestamp,
            StdDev: stdDev,
            Mean: mean,
            ZScore: zScore.NaN2Null());

        return (r, i);
    }

    /// <summary>
    /// Restores the buffer state up to the specified timestamp.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        buffer.Clear();

        if (restoreIndex < 0)
        {
            return;
        }

        // rebuild from the last LookbackPeriods preserved values; the item at
        // restoreIndex is preserved (not replayed through ToIndicator), so it
        // is included here
        int startIdx = Math.Max(0, restoreIndex + 1 - LookbackPeriods);

        for (int p = startIdx; p <= restoreIndex; p++)
        {
            buffer.Update(LookbackPeriods, ProviderCache[p].Value);
        }
    }
}

/// <summary>
/// Provides methods for creating StdDev hubs.
/// </summary>
public static partial class StdDev
{
    /// <summary>
    /// Converts the chain provider to a StdDev hub.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A StdDev hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    public static StdDevHub ToStdDevHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods = 14)
             => new(chainProvider, lookbackPeriods);
}
