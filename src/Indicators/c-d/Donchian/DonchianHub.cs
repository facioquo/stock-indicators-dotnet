namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Donchian Channels using a stream hub.
/// </summary>
public static partial class Donchian
{
    /// <summary>
    /// Creates a Donchian Channels streaming hub from a bars provider.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>An instance of <see cref="DonchianHub"/>.</returns>
    public static DonchianHub ToDonchianHub(
        this IBarProvider<IBar> barProvider,
        int lookbackPeriods = 20)
        => new(barProvider, lookbackPeriods);
}

/// <summary>
/// Streaming hub for Donchian Channels.
/// </summary>
public class DonchianHub
    : StreamHub<IBar, DonchianResult>, IDonchian
{
    private CircularDoubleBuffer _highBuffer;
    private CircularDoubleBuffer _lowBuffer;

    internal DonchianHub(
        IBarProvider<IBar> provider,
        int lookbackPeriods) : base(provider)
    {
        Donchian.Validate(lookbackPeriods);

        LookbackPeriods = lookbackPeriods;
        Name = $"DONCHIAN({lookbackPeriods})";
        _highBuffer = new CircularDoubleBuffer(lookbackPeriods);
        _lowBuffer = new CircularDoubleBuffer(lookbackPeriods);

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }
    /// <inheritdoc/>
    protected override (DonchianResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // handle warmup periods
        // Note: Donchian looks at PRIOR periods (not including current)
        // We need LookbackPeriods items in the window before we can calculate
        if (i < LookbackPeriods)
        {
            _highBuffer.Add((double)item.High);
            _lowBuffer.Add((double)item.Low);
            return (new DonchianResult(item.Timestamp), i);
        }

        double upperBand = _highBuffer.GetMax();
        double lowerBand = _lowBuffer.GetMin();
        double centerline = (upperBand + lowerBand) / 2d;
        double? width = centerline == 0 ? null : (upperBand - lowerBand) / centerline;

        _highBuffer.Add((double)item.High);
        _lowBuffer.Add((double)item.Low);

        DonchianResult r = new(
            Timestamp: item.Timestamp,
            UpperBand: upperBand,
            Centerline: centerline,
            LowerBand: lowerBand,
            Width: width);

        return (r, i);
    }

    /// <summary>
    /// Restores the rolling window state up to the specified timestamp.
    /// Clears and rebuilds rolling windows from ProviderCache for Add/Remove operations.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Clear circular buffers
        _highBuffer.Clear();
        _lowBuffer.Clear();

        if (restoreIndex < 0)
        {
            return;
        }

        // Rebuild rolling windows from ProviderCache
        int startIdx = Math.Max(0, restoreIndex + 1 - LookbackPeriods);

        for (int p = startIdx; p <= restoreIndex; p++)
        {
            IBar bar = ProviderCache[p];
            _highBuffer.Add((double)bar.High);
            _lowBuffer.Add((double)bar.Low);
        }
    }
}
