namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Chande Momentum Oscillator (CMO) calculations.
/// </summary>
public class CmoHub
    : ChainHub<IReusable, CmoResult>, ICmo
{
    private readonly Queue<(bool? isUp, double value)> _tickBuffer;

    internal CmoHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods) : base(provider)
    {
        Cmo.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        Name = $"CMO({lookbackPeriods})";
        _tickBuffer = new Queue<(bool? isUp, double value)>(lookbackPeriods);

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods + 1, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }
    /// <inheritdoc/>
    protected override (CmoResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        double? cmo = null;

        // Build up the tick buffer (starting from index 1, since we need previous value)
        if (i > 0)
        {
            double prevValue = ProviderCache[i - 1].Value;
            double currValue = item.Value;

            double tickValue = Math.Abs(currValue - prevValue);
            bool? isUp = double.IsNaN(tickValue) || currValue == prevValue
                ? null
                : currValue > prevValue;

            // Update buffer using universal buffer utilities
            _tickBuffer.Update(LookbackPeriods, (isUp, tickValue));

            // Calculate CMO only when we have enough data
            if (i >= LookbackPeriods)
            {
                cmo = Cmo.PeriodCalculation(_tickBuffer);
            }
        }

        // candidate result
        CmoResult r = new(
            Timestamp: item.Timestamp,
            Cmo: cmo);

        return (r, i);
    }

    /// <summary>
    /// Restores the tick buffer state up to the specified timestamp.
    /// Clears and rebuilds _tickBuffer from ProviderCache for Add/Remove operations.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Clear tick buffer
        _tickBuffer.Clear();

        if (restoreIndex < 0)
        {
            return;
        }

        // Need at least LookbackPeriods items to rebuild buffer
        // Start from restoreIndex - LookbackPeriods + 1, but not before index 1 (we need i-1 for prevValue)
        int startIdx = Math.Max(1, restoreIndex + 1 - LookbackPeriods);

        // Rebuild tick buffer from ProviderCache
        for (int p = startIdx; p <= restoreIndex; p++)
        {
            double prevValue = ProviderCache[p - 1].Value;
            double currValue = ProviderCache[p].Value;

            double tickValue = Math.Abs(currValue - prevValue);
            bool? isUp = double.IsNaN(tickValue) || currValue == prevValue
                ? null
                : currValue > prevValue;

            _tickBuffer.Enqueue((isUp, tickValue));
        }
    }

}

public static partial class Cmo
{
    /// <summary>
    /// Creates a CMO streaming hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A CMO hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    public static CmoHub ToCmoHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods = 14)
        => new(chainProvider, lookbackPeriods);
}
