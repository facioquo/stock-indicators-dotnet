namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Williams Fractal from incremental bar values.
/// </summary>
public class FractalList : BufferList<FractalResult>, IIncrementFromBar, IFractal
{
    private readonly List<FractalBuffer> _bars;

    /// <summary>
    /// Initializes a new instance of the <see cref="FractalList"/> class.
    /// </summary>
    /// <param name="windowSpan">Number of periods to look back and forward for the calculation.</param>
    /// <param name="endType">Type of price to use for the calculation.</param>
    public FractalList(int windowSpan = 2, EndType endType = EndType.HighLow)
        : this(windowSpan, windowSpan, endType)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FractalList"/> class with different left and right spans.
    /// </summary>
    /// <param name="leftSpan">Number of periods to look back for the calculation.</param>
    /// <param name="rightSpan">Number of periods to look forward for the calculation.</param>
    /// <param name="endType">Type of price to use for the calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="endType"/> is invalid.</exception>
    public FractalList(int leftSpan, int rightSpan, EndType endType = EndType.HighLow)
    {
        Fractal.Validate(Math.Min(leftSpan, rightSpan));
        LeftSpan = leftSpan;
        RightSpan = rightSpan;
        EndType = endType;

        _bars = [];

        Name = $"FRACTAL({2}, {EndType.HighLow})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FractalList"/> class with initial bars.
    /// </summary>
    /// <param name="windowSpan">Number of periods to look back and forward for the calculation.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="endType">Type of price to use for the calculation.</param>
    public FractalList(int windowSpan, IReadOnlyList<IBar> bars, EndType endType = EndType.HighLow)
        : this(windowSpan, endType) => Add(bars);

    /// <summary>
    /// Initializes a new instance of the <see cref="FractalList"/> class with different spans and initial bars.
    /// </summary>
    /// <param name="leftSpan">Number of periods to look back for the calculation.</param>
    /// <param name="rightSpan">Number of periods to look forward for the calculation.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="endType">Type of price to use for the calculation.</param>
    public FractalList(int leftSpan, int rightSpan, IReadOnlyList<IBar> bars, EndType endType = EndType.HighLow)
        : this(leftSpan, rightSpan, endType) => Add(bars);

    /// <inheritdoc />
    public int LeftSpan { get; init; }

    /// <inheritdoc />
    public int RightSpan { get; init; }

    /// <inheritdoc />
    public EndType EndType { get; init; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        // store the bar
        _bars.Add(new FractalBuffer(
            bar.High,
            bar.Low,
            bar.Close,
            bar.Timestamp));

        // always add a result (initially null)
        AddInternal(new FractalResult(bar.Timestamp, null, null));

        int length = _bars.Count;

        // Calculate fractal for the bar that just became calculable (if any)
        // We only need to calculate the newest calculable index, not recalculate a range
        // Previous fractals are already correct from earlier calls
        int lastCalculableIndex = length - RightSpan - 1;

        // Only calculate if this index is newly calculable and has sufficient context
        if (lastCalculableIndex >= 0 && lastCalculableIndex + 1 > LeftSpan)
        {
            int i = lastCalculableIndex;
            FractalBuffer center = _bars[i];
            bool isHigh = true;
            bool isLow = true;

            decimal evalHigh = EndType == EndType.Close ? center.Close : center.High;
            decimal evalLow = EndType == EndType.Close ? center.Close : center.Low;

            // compare with wings
            for (int p = i - LeftSpan; p <= i + RightSpan; p++)
            {
                // skip center
                if (p == i)
                {
                    continue;
                }

                FractalBuffer wing = _bars[p];
                decimal wingHigh = EndType == EndType.Close ? wing.Close : wing.High;
                decimal wingLow = EndType == EndType.Close ? wing.Close : wing.Low;

                if (evalHigh <= wingHigh)
                {
                    isHigh = false;
                }

                if (evalLow >= wingLow)
                {
                    isLow = false;
                }
            }

            decimal? fractalBear = isHigh ? evalHigh : null;
            decimal? fractalBull = isLow ? evalLow : null;

            // update the result at index i
            UpdateInternal(i, new FractalResult(center.Timestamp, fractalBear, fractalBull));
        }
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
        _bars.Clear();
    }
    /// <inheritdoc />
    protected override void PruneList()
    {
        int overflow = Count - MaxListSize;

        if (overflow <= 0)
        {
            return;
        }

        // remove from results
        base.PruneList();

        // also remove from bars
        _bars.RemoveRange(0, overflow);
    }

    internal class FractalBuffer(
        decimal high,
        decimal low,
        decimal close,
        DateTime timestamp)
    {
        internal decimal High { get; init; } = high;
        internal decimal Low { get; init; } = low;
        internal decimal Close { get; init; } = close;
        internal DateTime Timestamp { get; init; } = timestamp;
    }
}

public static partial class Fractal
{
    /// <summary>
    /// Creates a buffer list for Williams Fractal calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="windowSpan">Number of periods to look back and forward for the calculation.</param>
    /// <param name="endType">Type of price to use for the calculation.</param>
    /// <returns>An initialized <see cref="FractalList" />.</returns>
    public static FractalList ToFractalList(
        this IReadOnlyList<IBar> bars,
        int windowSpan = 2,
        EndType endType = EndType.HighLow)
        => new(windowSpan, bars, endType);

    /// <summary>
    /// Creates a buffer list for Williams Fractal calculations with different left and right spans.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="leftSpan">Number of periods to look back for the calculation.</param>
    /// <param name="rightSpan">Number of periods to look forward for the calculation.</param>
    /// <param name="endType">Type of price to use for the calculation.</param>
    /// <returns>An initialized <see cref="FractalList" />.</returns>
    public static FractalList ToFractalList(
        this IReadOnlyList<IBar> bars,
        int leftSpan,
        int rightSpan,
        EndType endType = EndType.HighLow)
        => new(leftSpan, rightSpan, bars, endType);
}
