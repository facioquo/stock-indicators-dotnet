namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Pivot Points from incremental bars.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PivotPointsList"/> class.
/// </remarks>
/// <param name="windowSize">Size of the window for pivot point calculation.</param>
/// <param name="pointType">Type of pivot point calculation to use.</param>
public class PivotPointsList(
    BarInterval windowSize = BarInterval.Month,
    PivotPointType pointType = PivotPointType.Standard) : BufferList<PivotPointsResult>, IIncrementFromBar, IPivotPoints
{
    private int _windowId = int.MinValue;
    private bool _firstWindow = true;
    private double _windowHigh;
    private double _windowLow;
    private double _windowOpen;
    private double _windowClose;
    private WindowPoint _windowPoint = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PivotPointsList"/> class with initial bars.
    /// </summary>
    /// <param name="windowSize">Size of the window for pivot point calculation.</param>
    /// <param name="pointType">Type of pivot point calculation to use.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public PivotPointsList(
        BarInterval windowSize,
        PivotPointType pointType,
        IReadOnlyList<IBar> bars)
        : this(windowSize, pointType) => Add(bars);

    /// <inheritdoc />
    public BarInterval WindowSize { get; init; } = windowSize;

    /// <inheritdoc />
    public PivotPointType PointType { get; init; } = pointType;

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        DateTime timestamp = bar.Timestamp;

        // Initialize window tracking on first bar
        if (_windowId == int.MinValue)
        {
            _windowId = GetWindowNumber(timestamp, WindowSize);
            _windowHigh = (double)bar.High;
            _windowLow = (double)bar.Low;
            _windowOpen = (double)bar.Open;
            _windowClose = (double)bar.Close;
        }

        // Check for new window
        int windowEval = GetWindowNumber(timestamp, WindowSize);

        if (windowEval != _windowId)
        {
            _windowId = windowEval;
            _firstWindow = false;

            // Set new levels
            if (PointType == PivotPointType.Woodie)
            {
                _windowOpen = (double)bar.Open;
            }

            _windowPoint = GetPivotPoint(
                PointType, _windowOpen, _windowHigh, _windowLow, _windowClose);

            // Reset window min/max thresholds
            _windowOpen = (double)bar.Open;
            _windowHigh = (double)bar.High;
            _windowLow = (double)bar.Low;
        }

        // Add levels
        PivotPointsResult result = !_firstWindow
            ? new() {
                Timestamp = timestamp,
                PP = _windowPoint.PP,
                S1 = _windowPoint.S1,
                S2 = _windowPoint.S2,
                S3 = _windowPoint.S3,
                S4 = _windowPoint.S4,
                R1 = _windowPoint.R1,
                R2 = _windowPoint.R2,
                R3 = _windowPoint.R3,
                R4 = _windowPoint.R4
            }
            : new PivotPointsResult {
                Timestamp = timestamp
            };

        AddInternal(result);

        // Capture window thresholds (for next iteration)
        _windowHigh = (double)bar.High > _windowHigh ? (double)bar.High : _windowHigh;
        _windowLow = (double)bar.Low < _windowLow ? (double)bar.Low : _windowLow;
        _windowClose = (double)bar.Close;
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
        _windowId = int.MinValue;
        _firstWindow = true;
        _windowHigh = 0;
        _windowLow = 0;
        _windowOpen = 0;
        _windowClose = 0;
        _windowPoint = new();
    }

    private static int GetWindowNumber(DateTime d, BarInterval windowSize)
        => PivotPoints.GetWindowNumber(d, windowSize);

    private static WindowPoint GetPivotPoint(
        PivotPointType pointType, double open, double high, double low, double close)
        => PivotPoints.GetPivotPoint(pointType, open, high, low, close);
}

public static partial class PivotPoints
{
    /// <summary>
    /// Creates a buffer list for Pivot Points calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="windowSize">Size of the window for pivot point calculation.</param>
    /// <param name="pointType">Type of pivot point calculation to use.</param>
    /// <returns>A new <see cref="PivotPointsList"/> instance.</returns>
    public static PivotPointsList ToPivotPointsList(
        this IReadOnlyList<IBar> bars,
        BarInterval windowSize = BarInterval.Month,
        PivotPointType pointType = PivotPointType.Standard)
        => new(windowSize, pointType) { bars };
}
