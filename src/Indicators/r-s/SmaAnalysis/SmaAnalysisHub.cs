namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents a Simple Moving Average (SMA) Analysis stream hub.
/// </summary>
public class SmaAnalysisHub
    : ChainHub<IReusable, SmaAnalysisResult>, ISma
{
    private readonly Queue<double> buffer;

    internal SmaAnalysisHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods) : base(provider)
    {
        Sma.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        buffer = new Queue<double>(lookbackPeriods);
        Name = $"SMA-ANALYSIS({lookbackPeriods})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }
    /// <inheritdoc/>
    protected override (SmaAnalysisResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // advance the rolling window of raw values and emit once it is full
        buffer.Update(LookbackPeriods, item.Value);

        double? sma = null;
        double? mad = null;
        double? mse = null;
        double? mape = null;

        double smaValue = buffer.Average(LookbackPeriods);

        if (!double.IsNaN(smaValue))
        {
            sma = smaValue;

            // analysis metrics over the same rolling window of raw values
            double sumMad = 0;
            double sumMse = 0;
            double sumMape = 0;

            foreach (double value in buffer)
            {
                double diff = value - smaValue;
                sumMad += Math.Abs(diff);
                sumMse += diff * diff;
                sumMape += value != 0 ? Math.Abs(diff) / value : double.NaN;
            }

            mad = (sumMad / LookbackPeriods).NaN2Null();
            mse = (sumMse / LookbackPeriods).NaN2Null();
            mape = (sumMape / LookbackPeriods).NaN2Null();
        }

        // candidate result
        SmaAnalysisResult r = new(
            Timestamp: item.Timestamp,
            Sma: sma,
            Mad: mad,
            Mse: mse,
            Mape: mape);

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
/// Provides methods for creating SMA Analysis hubs.
/// </summary>
public static partial class SmaAnalysis
{
    /// <summary>
    /// Converts the chain provider to an SMA Analysis hub.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>An SMA Analysis hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    public static SmaAnalysisHub ToSmaAnalysisHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods)
             => new(chainProvider, lookbackPeriods);
}
