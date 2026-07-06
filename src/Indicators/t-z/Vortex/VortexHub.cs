namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods for creating Vortex Indicator hubs.
/// </summary>
public class VortexHub
    : StreamHub<IBar, VortexResult>, IVortex
{

    private readonly Queue<(double Tr, double Pvm, double Nvm)> _buffer;
    private double _prevHigh;
    private double _prevLow;
    private double _prevClose;
    private bool _isInitialized;

    internal VortexHub(
        IStreamObservable<IBar> provider,
        int lookbackPeriods) : base(provider)
    {
        Vortex.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        Name = $"VORTEX({lookbackPeriods})";

        _buffer = new Queue<(double, double, double)>(lookbackPeriods);
        _isInitialized = false;

        // Validate cache size for warmup requirements
        // Vortex uses the first period for initialization and fills the buffer for LookbackPeriods more,
        // so the first valid result occurs at index LookbackPeriods (requiring LookbackPeriods + 1 items).
        ValidateCacheSize(lookbackPeriods + 1, Name);

        Reinitialize();
    }

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }
    /// <inheritdoc/>
    protected override (VortexResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        double high = (double)item.High;
        double low = (double)item.Low;
        double close = (double)item.Close;

        // Handle first period - no calculations possible
        if (!_isInitialized)
        {
            _prevHigh = high;
            _prevLow = low;
            _prevClose = close;
            _isInitialized = true;

            return (new VortexResult(item.Timestamp), i);
        }

        // Calculate trend information
        double highMinusPrevClose = Math.Abs(high - _prevClose);
        double lowMinusPrevClose = Math.Abs(low - _prevClose);

        double tr = Math.Max(high - low, Math.Max(highMinusPrevClose, lowMinusPrevClose));
        double pvm = Math.Abs(high - _prevLow);
        double nvm = Math.Abs(low - _prevHigh);

        // Update buffer with new values
        _buffer.Enqueue((tr, pvm, nvm));
        if (_buffer.Count > LookbackPeriods)
        {
            _buffer.Dequeue();
        }

        double? pvi = null;
        double? nvi = null;

        // Calculate vortex indicator when we have enough data
        if (_buffer.Count == LookbackPeriods)
        {
            double sumTr = 0;
            double sumPvm = 0;
            double sumNvm = 0;

            foreach ((double Tr, double Pvm, double Nvm) bufferItem in _buffer)
            {
                sumTr += bufferItem.Tr;
                sumPvm += bufferItem.Pvm;
                sumNvm += bufferItem.Nvm;
            }

            if (sumTr is not 0)
            {
                pvi = sumPvm / sumTr;
                nvi = sumNvm / sumTr;
            }
        }

        VortexResult result = new(
            Timestamp: item.Timestamp,
            Pvi: pvi,
            Nvi: nvi);

        _prevHigh = high;
        _prevLow = low;
        _prevClose = close;

        return (result, i);
    }

    /// <summary>
    /// Restores the buffer state up to the specified timestamp.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Clear buffer and state
        _buffer.Clear();
        _isInitialized = false;
        _prevHigh = 0;
        _prevLow = 0;
        _prevClose = 0;

        if (restoreIndex < 0)
        {
            return;
        }

        // Process first bar to initialize state
        if (restoreIndex >= 0)
        {
            IBar firstBar = ProviderCache[0];
            _prevHigh = (double)firstBar.High;
            _prevLow = (double)firstBar.Low;
            _prevClose = (double)firstBar.Close;
            _isInitialized = true;
        }

        // Rebuild buffer from bars starting at index 1
        int startIdx = Math.Max(1, restoreIndex + 1 - LookbackPeriods);
        for (int p = startIdx; p <= restoreIndex; p++)
        {
            IBar bar = ProviderCache[p];
            IBar prevBar = ProviderCache[p - 1];

            double high = (double)bar.High;
            double low = (double)bar.Low;
            double prevHigh = (double)prevBar.High;
            double prevLow = (double)prevBar.Low;
            double prevClose = (double)prevBar.Close;

            // Calculate trend information
            double highMinusPrevClose = Math.Abs(high - prevClose);
            double lowMinusPrevClose = Math.Abs(low - prevClose);

            double tr = Math.Max(high - low, Math.Max(highMinusPrevClose, lowMinusPrevClose));
            double pvm = Math.Abs(high - prevLow);
            double nvm = Math.Abs(low - prevHigh);

            _buffer.Enqueue((tr, pvm, nvm));
        }

        // Update prev values to the last processed bar
        if (restoreIndex >= 0)
        {
            IBar lastBar = ProviderCache[restoreIndex];
            _prevHigh = (double)lastBar.High;
            _prevLow = (double)lastBar.Low;
            _prevClose = (double)lastBar.Close;
        }
    }

}

public static partial class Vortex
{
    /// <summary>
    /// Converts the bar provider to a Vortex Indicator hub.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A Vortex Indicator hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bar provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are invalid.</exception>
    public static VortexHub ToVortexHub(
        this IStreamObservable<IBar> barProvider,
        int lookbackPeriods = 14)
             => new(barProvider, lookbackPeriods);
}
