namespace FacioQuo.Stock.Indicators;

/// <summary>
/// True Range (TR) from incremental bars.
/// </summary>
public class TrList : BufferList<TrResult>, IIncrementFromBar  // TR has no interface members
{
    private readonly Queue<(double High, double Low, double Close)> _buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrList"/> class.
    /// </summary>
    public TrList() => _buffer = new Queue<(double, double, double)>(2); // Only need current and previous

    /// <summary>
    /// Initializes a new instance of the <see cref="TrList"/> class with initial bars.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public TrList(IReadOnlyList<IBar> bars)
        : this() => Add(bars);

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        DateTime timestamp = bar.Timestamp;

        (double High, double Low, double Close) curr = (
            (double)bar.High,
            (double)bar.Low,
            (double)bar.Close);

        // skip first period
        if (Count == 0)
        {
            _buffer.Update(2, curr);
            AddInternal(new TrResult(timestamp, null));
            return;
        }

        // get previous, then add current using extension method
        (double _, double _, double PrevClose) = _buffer.Last();
        _buffer.Update(2, curr);

        // calculate True Range
        double tr = Tr.Increment(curr.High, curr.Low, PrevClose);

        AddInternal(new TrResult(timestamp, tr));
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

public static partial class Tr
{
    /// <summary>
    /// Creates a buffer list for True Range calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public static TrList ToTrList(
        this IReadOnlyList<IBar> bars)
        => new() { bars };
}
