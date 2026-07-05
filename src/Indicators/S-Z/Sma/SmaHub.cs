namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Simple Moving Average (SMA).
/// </summary>
public class SmaHub
    : ChainHub<IReusable, SmaResult>, ISma
{
    private readonly Queue<double> buffer;

    internal SmaHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods) : base(provider)
    {
        Sma.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        buffer = new Queue<double>(lookbackPeriods);
        Name = $"SMA({lookbackPeriods})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc/>
    protected override (SmaResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // advance the rolling window and emit once it is full
        buffer.Update(LookbackPeriods, item.Value);

        // candidate result
        SmaResult r = new(
            Timestamp: item.Timestamp,
            Sma: buffer.Average(LookbackPeriods).NaN2Null());

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

public static partial class Sma
{
    /// <summary>
    /// Creates an SMA streaming hub with a chain provider source.
    /// </summary>
    /// <remarks>If providers contain historical data, this hub will fast-forward its cache.</remarks>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A chain-sourced instance of <see cref="SmaHub"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chainProvider"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public static SmaHub ToSmaHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods)
             => new(chainProvider, lookbackPeriods);
}
