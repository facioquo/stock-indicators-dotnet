namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Volume Weighted Average Price (VWAP).
/// </summary>
public class VwapHub : ChainHub<IBar, VwapResult>
{
    private readonly bool _autoAnchor;
    private readonly Queue<(DateTime Timestamp, double Volume, double VolumeTp)> _stateQueue = new();
    private double _cumVolume;
    private double _cumVolumeTp;
    private double _prunedVolume;
    private double _prunedVolumeTp;
    private DateTime? _autoAnchorDate;

    internal VwapHub(
        IBarProvider<IBar> provider,
        DateTime? startDate = null)
        : base(provider)
    {
        StartDate = startDate;
        _autoAnchor = (startDate ?? default) == default;
        Name = "VWAP";
        // Validate cache size for warmup requirements
        ValidateCacheSize(1, Name);  // Requires at least 1 period

        Reinitialize();
    }

    /// <inheritdoc/>
    public DateTime? StartDate { get; private set; }

    /// <inheritdoc/>
    protected override (VwapResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // Set start date to first bar's timestamp if auto-anchoring
        if (StartDate == null)
        {
            StartDate = item.Timestamp;
            _autoAnchorDate ??= StartDate;
        }

        double volume = (double)item.Volume;
        double high = (double)item.High;
        double low = (double)item.Low;
        double close = (double)item.Close;

        double? vwap;

        if (item.Timestamp >= StartDate.Value)
        {
            _cumVolume += volume;
            _cumVolumeTp += volume * (high + low + close) / 3;

            vwap = _cumVolume != 0 ? _cumVolumeTp / _cumVolume : null;
            _stateQueue.Enqueue((item.Timestamp, volume, volume * (high + low + close) / 3));
        }
        else
        {
            vwap = null;
        }

        VwapResult r = new(
            Timestamp: item.Timestamp,
            Vwap: vwap);

        return (r, i);
    }

    /// <summary>
    /// Restores the cumulative state up to the specified timestamp.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Clear cumulative state
        _cumVolume = _prunedVolume;
        _cumVolumeTp = _prunedVolumeTp;
        _stateQueue.Clear();

        // Reset start date if auto-anchoring
        if (_autoAnchor)
        {
            StartDate = _autoAnchorDate;
        }

        if (restoreIndex < 0)
        {
            return;
        }

        // Rebuild cumulative state from ProviderCache
        for (int p = 0; p <= restoreIndex; p++)
        {
            IBar bar = ProviderCache[p];

            // Set start date to first bar if auto-anchoring
            if (StartDate == null)
            {
                StartDate = bar.Timestamp;
                _autoAnchorDate ??= StartDate;
            }

            if (bar.Timestamp >= StartDate.Value)
            {
                double volume = (double)bar.Volume;
                double high = (double)bar.High;
                double low = (double)bar.Low;
                double close = (double)bar.Close;

                _cumVolume += volume;
                _cumVolumeTp += volume * (high + low + close) / 3;
                _stateQueue.Enqueue((bar.Timestamp, volume, volume * (high + low + close) / 3));
            }
        }
    }

    /// <inheritdoc/>
    protected override void PruneState(DateTime toTimestamp)
    {
        while (_stateQueue.Count > 0 && _stateQueue.Peek().Timestamp <= toTimestamp)
        {
            (DateTime _, double volume, double volumeTp) = _stateQueue.Dequeue();
            _prunedVolume += volume;
            _prunedVolumeTp += volumeTp;
        }
    }

    /// <inheritdoc/>
    public override string ToString() =>
        StartDate.HasValue ? $"VWAP({StartDate.Value:d})" : "VWAP";
}

/// <summary>
/// Provides extension methods for creating VWAP hubs.
/// </summary>
public static partial class Vwap
{
    /// <summary>
    /// Creates a VWAP streaming hub from a bars provider.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="startDate">Start date for VWAP calculation. If null, auto-anchors to first bar.</param>
    /// <returns>A VWAP streaming hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bar provider is null.</exception>
    public static VwapHub ToVwapHub(
        this IBarProvider<IBar> barProvider,
        DateTime? startDate = null)
    {
        ArgumentNullException.ThrowIfNull(barProvider);
        return new VwapHub(barProvider, startDate);
    }
}
