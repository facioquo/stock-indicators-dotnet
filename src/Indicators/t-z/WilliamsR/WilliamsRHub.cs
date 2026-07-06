namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents a Williams %R stream hub.
/// </summary>
public class WilliamsRHub
    : StreamHub<IBar, WilliamsResult>, IWilliamsR
{
    #region constructors

    private CircularDoubleBuffer _highBuffer;
    private CircularDoubleBuffer _lowBuffer;

    internal WilliamsRHub(
        IStreamObservable<IBar> provider,
        int lookbackPeriods) : base(provider)
    {
        WilliamsR.Validate(lookbackPeriods);

        LookbackPeriods = lookbackPeriods;
        _highBuffer = new CircularDoubleBuffer(lookbackPeriods);
        _lowBuffer = new CircularDoubleBuffer(lookbackPeriods);

        Name = $"WILLR({lookbackPeriods})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods, Name);

        Reinitialize();
    }

    #endregion constructors

    #region properties

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    #endregion properties

    #region methods
    /// <inheritdoc/>
    protected override (WilliamsResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // Add current high/low to rolling windows (only require High/Low, not Close)
        bool hasHL = !double.IsNaN((double)item.High) &&
                     !double.IsNaN((double)item.Low);
        bool hasClose = !double.IsNaN((double)item.Close);

        if (hasHL)
        {
            _highBuffer.Add((double)item.High);
            _lowBuffer.Add((double)item.Low);
        }

        // Calculate Williams %R
        double williamsR = double.NaN;
        if (i >= LookbackPeriods - 1 && hasHL && hasClose)
        {
            // Get highest high and lowest low from circular buffers
            double highHigh = _highBuffer.GetMax();
            double lowLow = _lowBuffer.GetMin();

            // Apply boundary clamping to ensure -100 ≤ WilliamsR ≤ 0
            // This prevents floating-point precision errors at boundaries
            if (highHigh == lowLow)
            {
                williamsR = -100.0;
            }
            else if ((double)item.Close >= highHigh)
            {
                williamsR = 0.0;
            }
            else if ((double)item.Close <= lowLow)
            {
                williamsR = -100.0;
            }
            else
            {
                williamsR = (100 * ((double)item.Close - lowLow) / (highHigh - lowLow)) - 100;
            }
        }

        WilliamsResult result = new(
            Timestamp: item.Timestamp,
            WilliamsR: williamsR.NaN2Null());

        return (result, i);
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

            if (!double.IsNaN((double)bar.High) &&
                !double.IsNaN((double)bar.Low))
            {
                _highBuffer.Add((double)bar.High);
                _lowBuffer.Add((double)bar.Low);
            }
        }
    }

    #endregion methods

}

public static partial class WilliamsR
{
    /// <summary>
    /// Converts the bar provider to a Williams %R hub.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A Williams %R hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bar provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are invalid.</exception>
    public static WilliamsRHub ToWilliamsRHub(
        this IStreamObservable<IBar> barProvider,
        int lookbackPeriods = 14)
             => new(barProvider, lookbackPeriods);
}
