namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Money Flow Index (MFI).
/// </summary>
public class MfiHub : ChainHub<IBar, MfiResult>, IMfi
{
    private readonly Queue<(double TruePrice, double MoneyFlow, int Direction)> _buffer;
    private double? _prevTruePrice;

    internal MfiHub(
        IBarProvider<IBar> provider,
        int lookbackPeriods)
        : base(provider)
    {
        Mfi.Validate(lookbackPeriods);

        LookbackPeriods = lookbackPeriods;
        Name = $"MFI({lookbackPeriods})";
        _buffer = new Queue<(double, double, int)>(lookbackPeriods);

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods + 1, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }
    /// <inheritdoc/>
    protected override (MfiResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // Calculate true price and money flow
        double truePrice = ((double)item.High + (double)item.Low + (double)item.Close) / 3;
        double moneyFlow = truePrice * (double)item.Volume;

        // Update buffer with new data
        UpdateBuffer(truePrice, moneyFlow);

        // Calculate MFI when we have enough data
        double? mfi = i >= LookbackPeriods ? CalculateMfi() : null;

        MfiResult r = new(
            Timestamp: item.Timestamp,
            Mfi: mfi);

        return (r, i);
    }

    private void UpdateBuffer(double truePrice, double moneyFlow)
    {
        // Determine direction
        int direction = _prevTruePrice == null || truePrice == _prevTruePrice
            ? 0
            : truePrice > _prevTruePrice ? 1 : -1;

        // If buffer is full, remove oldest item
        if (_buffer.Count == LookbackPeriods)
        {
            _buffer.Dequeue();
        }

        // Add new item to buffer
        _buffer.Enqueue((truePrice, moneyFlow, direction));

        // Update previous true price
        _prevTruePrice = truePrice;
    }

    private double CalculateMfi()
    {
        // Recalculate sums from buffer to avoid floating point accumulation errors
        double sumPosMFs = 0;
        double sumNegMFs = 0;

        foreach ((double tp, double mf, int dir) in _buffer)
        {
            if (dir == 1)
            {
                sumPosMFs += mf;
            }
            else if (dir == -1)
            {
                sumNegMFs += mf;
            }
        }

        if (sumNegMFs == 0)
        {
            return 100;
        }

        double mfRatio = sumPosMFs / sumNegMFs;
        return 100 - (100 / (1 + mfRatio));
    }

    /// <summary>
    /// Restores the buffer state up to the specified timestamp.
    /// Clears and rebuilds buffer from ProviderCache for Add/Remove operations.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Clear state
        _buffer.Clear();
        _prevTruePrice = null;

        if (restoreIndex < 0)
        {
            return;
        }

        // Rebuild buffer from ProviderCache
        // We need to start from restoreIndex - LookbackPeriods to rebuild the buffer
        int startIdx = Math.Max(0, restoreIndex + 1 - LookbackPeriods);

        for (int p = startIdx; p <= restoreIndex; p++)
        {
            IBar bar = ProviderCache[p];

            // Calculate true price
            double truePrice = ((double)bar.High + (double)bar.Low + (double)bar.Close) / 3;

            // Calculate raw money flow
            double moneyFlow = truePrice * (double)bar.Volume;

            // Determine direction
            int direction = _prevTruePrice == null || truePrice == _prevTruePrice
                ? 0
                : truePrice > _prevTruePrice ? 1 : -1;

            // Add to buffer (no need to dequeue since we're rebuilding)
            _buffer.Enqueue((truePrice, moneyFlow, direction));

            // Update previous true price
            _prevTruePrice = truePrice;
        }
    }
}

public static partial class Mfi
{
    /// <summary>
    /// Converts the bar provider to an MFI hub.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="lookbackPeriods">Number of lookback periods. Default is 14.</param>
    /// <returns>An MFI hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bar provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    public static MfiHub ToMfiHub(
        this IBarProvider<IBar> barProvider,
        int lookbackPeriods = 14)
    {
        ArgumentNullException.ThrowIfNull(barProvider);
        return new(barProvider, lookbackPeriods);
    }
}
