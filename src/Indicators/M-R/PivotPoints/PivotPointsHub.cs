namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Pivot Points using a stream hub.
/// </summary>
public class PivotPointsHub
    : StreamHub<IBar, PivotPointsResult>
{

    private int windowId;
    private bool firstWindow;
    private double windowHigh;
    private double windowLow;
    private double windowOpen;
    private double windowClose;
    private WindowPoint windowPoint;

    internal PivotPointsHub(
        IBarProvider<IBar> provider,
        BarInterval windowSize,
        PivotPointType pointType) : base(provider)
    {
        WindowSize = windowSize;
        PointType = pointType;
        Name = $"PIVOT-POINTS({windowSize},{pointType})";

        // Initialize state
        windowId = 0;
        firstWindow = true;
        windowHigh = 0;
        windowLow = 0;
        windowOpen = 0;
        windowClose = 0;
        windowPoint = new();

        // Validate cache size for warmup requirements
        ValidateCacheSize(2, Name);  // Needs at least 2 periods for pivot calculation

        Reinitialize();
    }

    /// <inheritdoc/>
    public BarInterval WindowSize { get; init; }

    /// <inheritdoc/>
    public PivotPointType PointType { get; init; }
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Reset all state
        windowId = 0;
        firstWindow = true;
        windowHigh = 0;
        windowLow = 0;
        windowOpen = 0;
        windowClose = 0;
        windowPoint = new();

        if (restoreIndex < 0)
        {
            return;
        }

        // Rebuild state up to the rollback point
        for (int p = 0; p <= restoreIndex; p++)
        {
            IBar q = ProviderCache[p];

            if (p == 0)
            {
                windowId = PivotPoints.GetWindowNumber(q.Timestamp, WindowSize);
                windowHigh = (double)q.High;
                windowLow = (double)q.Low;
                windowOpen = (double)q.Open;
                windowClose = (double)q.Close;
                continue;
            }

            UpdateWindowState(q);
        }
    }

    /// <inheritdoc/>
    protected override (PivotPointsResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // Initialize on first bar
        if (i == 0)
        {
            windowId = PivotPoints.GetWindowNumber(item.Timestamp, WindowSize);
            firstWindow = true;
            windowHigh = (double)item.High;
            windowLow = (double)item.Low;
            windowOpen = (double)item.Open;
            windowClose = (double)item.Close;
            windowPoint = new();
        }

        UpdateWindowState(item);

        // Create result
        PivotPointsResult result = !firstWindow
            ? new() {
                Timestamp = item.Timestamp,
                PP = windowPoint.PP,
                S1 = windowPoint.S1,
                S2 = windowPoint.S2,
                S3 = windowPoint.S3,
                S4 = windowPoint.S4,
                R1 = windowPoint.R1,
                R2 = windowPoint.R2,
                R3 = windowPoint.R3,
                R4 = windowPoint.R4
            }
            : new PivotPointsResult {
                Timestamp = item.Timestamp
            };

        return (result, i);
    }

    private void UpdateWindowState(IBar q)
    {
        int windowEval = PivotPoints.GetWindowNumber(q.Timestamp, WindowSize);

        if (windowEval != windowId)
        {
            windowId = windowEval;
            firstWindow = false;

            // Set new levels based on previous window
            double pivotOpen = windowOpen;
            if (PointType == PivotPointType.Woodie)
            {
                pivotOpen = (double)q.Open;
            }

            windowPoint = PivotPoints.GetPivotPoint(
                PointType, pivotOpen, windowHigh, windowLow, windowClose);

            // Reset window min/max thresholds
            windowOpen = (double)q.Open;
            windowHigh = (double)q.High;
            windowLow = (double)q.Low;
        }

        // Update window thresholds (for next iteration)
        windowHigh = (double)q.High > windowHigh ? (double)q.High : windowHigh;
        windowLow = (double)q.Low < windowLow ? (double)q.Low : windowLow;
        windowClose = (double)q.Close;
    }

}

public static partial class PivotPoints
{
    /// <summary>
    /// Creates a PivotPoints streaming hub from a bar provider.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="windowSize">Size of the window for pivot calculation. Default is <see cref="BarInterval.Month"/>.</param>
    /// <param name="pointType">Type of pivot points to calculate. Default is <see cref="PivotPointType.Standard"/>.</param>
    /// <returns>An instance of <see cref="PivotPointsHub"/>.</returns>
    public static PivotPointsHub ToPivotPointsHub(
        this IBarProvider<IBar> barProvider,
        BarInterval windowSize = BarInterval.Month,
        PivotPointType pointType = PivotPointType.Standard)
    {
        ArgumentNullException.ThrowIfNull(barProvider);
        return new(barProvider, windowSize, pointType);
    }
}
