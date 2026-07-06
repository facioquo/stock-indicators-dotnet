namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Balance of Power (BOP) from incremental bars.
/// </summary>
public class BopList : BufferList<BopResult>, IIncrementFromBar, IBop
{
    private readonly Queue<double> _buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="BopList"/> class.
    /// </summary>
    /// <param name="smoothPeriods">Number of periods to use for smoothing.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="smoothPeriods"/> is invalid.</exception>
    public BopList(int smoothPeriods)
    {
        Bop.Validate(smoothPeriods);
        SmoothPeriods = smoothPeriods;
        _buffer = new Queue<double>(smoothPeriods);

        Name = $"BOP({smoothPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BopList"/> class with initial bars.
    /// </summary>
    /// <param name="smoothPeriods">Number of periods to use for smoothing.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public BopList(int smoothPeriods, IReadOnlyList<IBar> bars)
        : this(smoothPeriods) => Add(bars);

    /// <inheritdoc />
    public int SmoothPeriods { get; init; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        DateTime timestamp = bar.Timestamp;

        // Calculate raw BOP for this bar
        double range = (double)bar.High - (double)bar.Low;
        double rawBop = range != 0
            ? ((double)bar.Close - (double)bar.Open) / range
            : double.NaN;

        // Update buffer with raw BOP value
        _buffer.Update(SmoothPeriods, rawBop);

        // Calculate smoothed BOP
        double? bop = null;
        if (_buffer.Count == SmoothPeriods)
        {
            // Average buffered values (sum then divide, same as LINQ Average)
            double sum = 0;
            foreach (double val in _buffer)
            {
                sum += val;
            }

            bop = (sum / _buffer.Count).NaN2Null();
        }

        AddInternal(new BopResult(timestamp, bop));
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

public static partial class Bop
{
    /// <summary>
    /// Creates a buffer list for Balance of Power (BOP) calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="smoothPeriods">Number of periods for smoothing</param>
    public static BopList ToBopList(
        this IReadOnlyList<IBar> bars,
        int smoothPeriods = 14)
        => new(smoothPeriods) { bars };
}
