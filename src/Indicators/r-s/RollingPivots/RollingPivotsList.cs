namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Rolling Pivot Points from incremental bars.
/// </summary>
public class RollingPivotsList : BufferList<RollingPivotsResult>, IIncrementFromBar
{
    private readonly Queue<IBar> _buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="RollingPivotsList"/> class.
    /// </summary>
    /// <param name="windowPeriods">Number of periods in the rolling window.</param>
    /// <param name="offsetPeriods">Number of periods to offset the window.</param>
    /// <param name="pointType">Type of pivot point calculation to use.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="pointType"/> is invalid.</exception>
    public RollingPivotsList(
        int windowPeriods = 20,
        int offsetPeriods = 0,
        PivotPointType pointType = PivotPointType.Standard)
    {
        RollingPivots.Validate(windowPeriods, offsetPeriods);
        WindowPeriods = windowPeriods;
        OffsetPeriods = offsetPeriods;
        PointType = pointType;
        _buffer = new Queue<IBar>(windowPeriods + offsetPeriods + 1);

        Name = $"ROLLINGPIVOTS({20}, {0}, {PivotPointType.Standard})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RollingPivotsList"/> class with initial bars.
    /// </summary>
    /// <param name="windowPeriods">Number of periods in the rolling window.</param>
    /// <param name="offsetPeriods">Number of periods to offset the window.</param>
    /// <param name="pointType">Type of pivot point calculation to use.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public RollingPivotsList(
        int windowPeriods,
        int offsetPeriods,
        PivotPointType pointType,
        IReadOnlyList<IBar> bars)
        : this(windowPeriods, offsetPeriods, pointType) => Add(bars);

    /// <inheritdoc />
    public int WindowPeriods { get; }

    /// <inheritdoc />
    public int OffsetPeriods { get; }

    /// <inheritdoc />
    public PivotPointType PointType { get; }

    /// <summary>
    /// Gets the deterministic warmup period (WindowPeriods + OffsetPeriods), i.e., the number
    /// of initial items that produce null results before the first valid pivot point.
    /// </summary>
    public int LookbackPeriods => WindowPeriods + OffsetPeriods;

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        // Update buffer with new bar
        _buffer.Update(WindowPeriods + OffsetPeriods + 1, bar);

        RollingPivotsResult result;

        // Check if we have enough data to calculate pivots
        // Need buffer to contain windowPeriods + offsetPeriods + 1 bars
        if (_buffer.Count > WindowPeriods + OffsetPeriods)
        {
            // Get the window data from buffer
            // The buffer contains the last (windowPeriods + offsetPeriods + 1) bars
            // Current bar is the last element (bufferCount - 1)
            // Window ends at (bufferCount - 1 - 1 - offsetPeriods) = (bufferCount - 2 - offsetPeriods)
            // because the current bar itself is not part of the window when offsetPeriods = 0

            int bufferCount = _buffer.Count;

            // Window ends offsetPeriods + 1 positions before the end (the "+1" accounts for the current bar)
            int windowEndIndex = bufferCount - 2 - OffsetPeriods;
            int windowStartIndex = windowEndIndex - WindowPeriods + 1;

            double windowHigh = double.MinValue;
            double windowLow = double.MaxValue;
            double windowClose = double.NaN;

            // scan the window in place (queue enumerates front-to-back) without copying the buffer
            int index = 0;
            foreach (IBar d in _buffer)
            {
                if (index > windowEndIndex)
                {
                    break;
                }

                if (index >= windowStartIndex)
                {
                    windowHigh = (double)d.High > windowHigh ? (double)d.High : windowHigh;
                    windowLow = (double)d.Low < windowLow ? (double)d.Low : windowLow;

                    if (index == windowEndIndex)
                    {
                        windowClose = (double)d.Close;
                    }
                }

                index++;
            }

            // Calculate pivot points
            WindowPoint wp = PivotPoints.GetPivotPoint(
                PointType, (double)bar.Open, windowHigh, windowLow, windowClose);

            result = new RollingPivotsResult {
                Timestamp = bar.Timestamp,
                PP = wp.PP,
                S1 = wp.S1,
                S2 = wp.S2,
                S3 = wp.S3,
                S4 = wp.S4,
                R1 = wp.R1,
                R2 = wp.R2,
                R3 = wp.R3,
                R4 = wp.R4
            };
        }
        else
        {
            result = new RollingPivotsResult { Timestamp = bar.Timestamp };
        }

        AddInternal(result);
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
        _buffer.Clear();
    }
}

public static partial class RollingPivots
{
    /// <summary>
    /// Creates a buffer list for Rolling Pivot Points calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="windowPeriods">Number of periods in the rolling window</param>
    /// <param name="offsetPeriods">Number of periods to offset</param>
    /// <param name="pointType">Type of pivot point calculation</param>
    public static RollingPivotsList ToRollingPivotsList(
        this IReadOnlyList<IBar> bars,
        int windowPeriods = 20,
        int offsetPeriods = 0,
        PivotPointType pointType = PivotPointType.Standard)
        => new(windowPeriods, offsetPeriods, pointType) { bars };
}
