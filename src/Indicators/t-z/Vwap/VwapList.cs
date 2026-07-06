namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Volume Weighted Average Price (VWAP) from incremental bars.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="VwapList"/> class.
/// </remarks>
/// <param name="startDate">Start date for VWAP calculation. If null, auto-anchors to first bar.</param>
public class VwapList(DateTime? startDate = null) : BufferList<VwapResult>, IIncrementFromBar
{
    private readonly bool _autoAnchor = (startDate ?? default) == default;
    private double _cumVolume;
    private double _cumVolumeTp;

    /// <summary>
    /// Initializes a new instance of the <see cref="VwapList"/> class with initial bars.
    /// </summary>
    /// <param name="startDate">Start date for VWAP calculation. If null, auto-anchors to first bar.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public VwapList(DateTime? startDate, IReadOnlyList<IBar> bars)
        : this(startDate) => Add(bars);

    /// <inheritdoc />
    public DateTime? StartDate { get; private set; } = startDate;

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        // Set start date to first bar's timestamp if auto-anchoring
        if (StartDate == null)
        {
            StartDate = bar.Timestamp;
        }

        double volume = (double)bar.Volume;
        double high = (double)bar.High;
        double low = (double)bar.Low;
        double close = (double)bar.Close;

        double? vwap;

        if (bar.Timestamp >= StartDate.Value)
        {
            _cumVolume += volume;
            _cumVolumeTp += volume * (high + low + close) / 3;

            vwap = _cumVolume != 0 ? _cumVolumeTp / _cumVolume : null;
        }
        else
        {
            vwap = null;
        }

        AddInternal(new VwapResult(
            Timestamp: bar.Timestamp,
            Vwap: vwap));
    }

    /// <inheritdoc />
    public void Add(IReadOnlyList<IBar> bars)
    {
        ArgumentNullException.ThrowIfNull(bars);

        for (int i = 0; i < bars.Count; i++)
        {
            Add(bars[i]);
        }
    }

    /// <inheritdoc />
    public override void Clear()
    {
        base.Clear();
        _cumVolume = 0;
        _cumVolumeTp = 0;

        // Reset _startDate to null if in auto-anchor mode
        if (_autoAnchor)
        {
            StartDate = null;
        }
    }
}

public static partial class Vwap
{
    /// <summary>
    /// Creates a buffer list for VWAP calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="startDate">Starting date for calculation</param>
    public static VwapList ToVwapList(
        this IReadOnlyList<IBar> bars,
        DateTime startDate)
        => new(startDate) { bars };

    /// <summary>
    /// Creates a buffer list for VWAP calculations starting from the first bar.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public static VwapList ToVwapList(
        this IReadOnlyList<IBar> bars)
        => new(null) { bars };
}
