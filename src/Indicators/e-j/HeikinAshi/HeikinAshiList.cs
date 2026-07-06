namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Heikin-Ashi from incremental bar values.
/// </summary>
public class HeikinAshiList : BufferList<HeikinAshiResult>, IIncrementFromBar
{
    private decimal _prevOpen = decimal.MinValue;
    private decimal _prevClose = decimal.MinValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeikinAshiList"/> class.
    /// </summary>
    public HeikinAshiList()
    {
        // HeikinAshi doesn't require any parameters or initialization
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeikinAshiList"/> class with initial bars.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public HeikinAshiList(IReadOnlyList<IBar> bars)
        : this() => Add(bars);

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        // Initialize on first bar
        if (_prevOpen == decimal.MinValue)
        {
            _prevOpen = bar.Open;
            _prevClose = bar.Close;
        }

        // close
        decimal close = (bar.Open + bar.High + bar.Low + bar.Close) / 4;

        // open
        decimal open = (_prevOpen + _prevClose) / 2;

        // high
        decimal high = Math.Max(bar.High, Math.Max(open, close));

        // low
        decimal low = Math.Min(bar.Low, Math.Min(open, close));

        AddInternal(new HeikinAshiResult(
            Timestamp: bar.Timestamp,
            Open: open,
            High: high,
            Low: low,
            Close: close,
            Volume: bar.Volume));

        // save for next iteration
        _prevOpen = open;
        _prevClose = close;
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
        _prevOpen = decimal.MinValue;
        _prevClose = decimal.MinValue;
    }
}

public static partial class HeikinAshi
{
    /// <summary>
    /// Creates a buffer list for Heikin-Ashi calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public static HeikinAshiList ToHeikinAshiList(
        this IReadOnlyList<IBar> bars)
        => new() { bars };
}
