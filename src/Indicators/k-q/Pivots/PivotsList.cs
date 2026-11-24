namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Pivot Points from incremental bars.
/// </summary>
public class PivotsList : BufferList<PivotsResult>, IIncrementFromBar
{
    private readonly int _leftSpan;
    private readonly int _rightSpan;
    private readonly int _maxTrendPeriods;
    private readonly EndType _endType;
    private readonly List<BarBuffer> _barBuffer;

    private int? _lastHighIndex;
    private decimal? _lastHighValue;
    private int? _lastLowIndex;
    private decimal? _lastLowValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="PivotsList"/> class.
    /// </summary>
    /// <param name="leftSpan">Number of periods to the left of the pivot point.</param>
    /// <param name="rightSpan">Number of periods to the right of the pivot point.</param>
    /// <param name="maxTrendPeriods">Maximum number of periods for trend calculation.</param>
    /// <param name="endType">Type of end point for the pivot calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="endType"/> is invalid.</exception>
    public PivotsList(
        int leftSpan = 2,
        int rightSpan = 2,
        int maxTrendPeriods = 20,
        EndType endType = EndType.HighLow)
    {
        Pivots.Validate(leftSpan, rightSpan, maxTrendPeriods);
        LeftSpan = leftSpan;
        RightSpan = rightSpan;
        MaxTrendPeriods = maxTrendPeriods;
        EndType = endType;

        _leftSpan = leftSpan;
        _rightSpan = rightSpan;
        _maxTrendPeriods = maxTrendPeriods;
        _endType = endType;

        // Buffer needs to hold enough bars to compute fractals and track trends
        _barBuffer = [with(leftSpan + rightSpan + maxTrendPeriods + 1)];

        Name = $"PIVOTS({leftSpan}, {rightSpan}, {maxTrendPeriods}, {endType})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PivotsList"/> class with initial bars.
    /// </summary>
    /// <param name="leftSpan">Number of periods to the left of the pivot point.</param>
    /// <param name="rightSpan">Number of periods to the right of the pivot point.</param>
    /// <param name="maxTrendPeriods">Maximum number of periods for trend calculation.</param>
    /// <param name="endType">Type of end point for the pivot calculation.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public PivotsList(
        int leftSpan,
        int rightSpan,
        int maxTrendPeriods,
        EndType endType,
        IReadOnlyList<IBar> bars)
        : this(leftSpan, rightSpan, maxTrendPeriods, endType) => Add(bars);

    /// <inheritdoc />
    public int LeftSpan { get; init; }

    /// <inheritdoc />
    public int RightSpan { get; init; }

    /// <inheritdoc />
    public int MaxTrendPeriods { get; init; }

    /// <inheritdoc />
    public EndType EndType { get; init; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        DateTime timestamp = bar.Timestamp;

        // Add bar to buffer
        _barBuffer.Add(new BarBuffer(
            bar.Timestamp,
            bar.High,
            bar.Low,
            bar.Close));

        // Trim buffer if it exceeds maximum needed size
        int maxBufferSize = _leftSpan + _rightSpan + _maxTrendPeriods + 1;
        while (_barBuffer.Count > maxBufferSize && TotalCount > maxBufferSize)
        {
            _barBuffer.RemoveAt(0);
        }

        // Initialize result with no values
        PivotsResult result = new(
            Timestamp: timestamp,
            HighPoint: null,
            LowPoint: null,
            HighLine: null,
            LowLine: null,
            HighTrend: null,
            LowTrend: null);

        // Add result to list
        AddInternal(result);

        // Need at least leftSpan + rightSpan + 1 bars to identify fractals
        if (_barBuffer.Count < _leftSpan + _rightSpan + 1)
        {
            return;
        }

        // Compute fractal for bar at position (TotalCount - rightSpan - 1),
        // the bar that now has rightSpan bars after it; positions count pruned results
        int fractalCheckIndex = TotalCount - _rightSpan - 1;
        if (fractalCheckIndex < _leftSpan)
        {
            return;
        }

        int bufferCheckIndex = _barBuffer.Count - _rightSpan - 1;
        if (bufferCheckIndex < _leftSpan)
        {
            return;
        }

        ComputeFractalAndUpdatePivots(fractalCheckIndex, bufferCheckIndex);
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
        _barBuffer.Clear();
        _lastHighIndex = null;
        _lastHighValue = null;
        _lastLowIndex = null;
        _lastLowValue = null;
    }
    private void ComputeFractalAndUpdatePivots(int resultIndex, int bufferIndex)
    {
        BarBuffer evalBar = _barBuffer[bufferIndex];
        bool isHighFractal = true;
        bool isLowFractal = true;

        decimal evalHigh = _endType == EndType.Close ? evalBar.Close : evalBar.High;
        decimal evalLow = _endType == EndType.Close ? evalBar.Close : evalBar.Low;

        // Check left and right wings
        for (int p = bufferIndex - _leftSpan; p <= bufferIndex + _rightSpan; p++)
        {
            if (p == bufferIndex)
            {
                continue;
            }

            BarBuffer wing = _barBuffer[p];
            decimal wingHigh = _endType == EndType.Close ? wing.Close : wing.High;
            decimal wingLow = _endType == EndType.Close ? wing.Close : wing.Low;

            if (evalHigh <= wingHigh)
            {
                isHighFractal = false;
            }

            if (evalLow >= wingLow)
            {
                isLowFractal = false;
            }
        }

        decimal? highPoint = isHighFractal ? evalHigh : null;
        decimal? lowPoint = isLowFractal ? evalLow : null;

        // Update the result at resultIndex with fractal points
        int index = RetainedIndex(resultIndex);
        if (index >= 0)
        {
            UpdateInternal(index, this[index] with {
                HighPoint = highPoint,
                LowPoint = lowPoint
            });
        }

        // Reset expired indexes
        if (_lastHighIndex < resultIndex - _maxTrendPeriods)
        {
            _lastHighIndex = null;
            _lastHighValue = null;
        }

        if (_lastLowIndex < resultIndex - _maxTrendPeriods)
        {
            _lastLowIndex = null;
            _lastLowValue = null;
        }

        // Update high trend if new high fractal found
        if (highPoint != null)
        {
            if (_lastHighIndex != null && highPoint != _lastHighValue && resultIndex != _lastHighIndex)
            {
                PivotTrend trend = highPoint > _lastHighValue
                    ? PivotTrend.Hh
                    : PivotTrend.Lh;

                // Set the line value at the last high index
                index = RetainedIndex((int)_lastHighIndex);
                if (index >= 0)
                {
                    UpdateInternal(index, this[index] with { HighLine = _lastHighValue });
                }

                decimal? incr = (highPoint - _lastHighValue) / (resultIndex - _lastHighIndex);

                // Repaint trend line from last high + 1 to current high
                for (int t = (int)_lastHighIndex + 1; t <= resultIndex; t++)
                {
                    index = RetainedIndex(t);
                    if (index >= 0)
                    {
                        UpdateInternal(index, this[index] with {
                            HighLine = highPoint + (incr * (t - resultIndex)),
                            HighTrend = trend
                        });
                    }
                }
            }

            _lastHighIndex = resultIndex;
            _lastHighValue = highPoint;
        }

        // Update low trend if new low fractal found
        if (lowPoint != null)
        {
            if (_lastLowIndex != null && lowPoint != _lastLowValue && resultIndex != _lastLowIndex)
            {
                PivotTrend trend = lowPoint > _lastLowValue
                    ? PivotTrend.Hl
                    : PivotTrend.Ll;

                // Set the line value at the last low index
                index = RetainedIndex((int)_lastLowIndex);
                if (index >= 0)
                {
                    UpdateInternal(index, this[index] with { LowLine = _lastLowValue });
                }

                decimal? incr = (lowPoint - _lastLowValue) / (resultIndex - _lastLowIndex);

                // Repaint trend line from last low + 1 to current low
                for (int t = (int)_lastLowIndex + 1; t <= resultIndex; t++)
                {
                    index = RetainedIndex(t);
                    if (index >= 0)
                    {
                        UpdateInternal(index, this[index] with {
                            LowLine = lowPoint + (incr * (t - resultIndex)),
                            LowTrend = trend
                        });
                    }
                }
            }

            _lastLowIndex = resultIndex;
            _lastLowValue = lowPoint;
        }
    }

    /// <summary>
    /// Converts an absolute position, which counts pruned results, to a list index.
    /// </summary>
    /// <param name="position">Absolute position of the result.</param>
    /// <returns>The list index, or -1 when the result was pruned.</returns>
    private int RetainedIndex(int position)
    {
        int index = position - PrunedCount;
        return index >= 0 && index < Count ? index : -1;
    }

    private sealed record BarBuffer(DateTime Timestamp, decimal High, decimal Low, decimal Close);
}

public static partial class Pivots
{
    /// <summary>
    /// Creates a buffer list for Pivot Points calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="leftSpan">Number of periods to the left of the pivot point.</param>
    /// <param name="rightSpan">Number of periods to the right of the pivot point.</param>
    /// <param name="maxTrendPeriods">Maximum number of periods for trend calculation.</param>
    /// <param name="endType">Type of end point for the pivot calculation.</param>
    /// <returns>A new <see cref="PivotsList"/> instance.</returns>
    public static PivotsList ToPivotsList(
        this IReadOnlyList<IBar> bars,
        int leftSpan = 2,
        int rightSpan = 2,
        int maxTrendPeriods = 20,
        EndType endType = EndType.HighLow)
        => new(leftSpan, rightSpan, maxTrendPeriods, endType) { bars };
}
