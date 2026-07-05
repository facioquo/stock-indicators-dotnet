namespace FacioQuo.Stock.Indicators;

/// <summary>
/// On-Balance Volume (OBV) from incremental bar values.
/// </summary>
public class ObvList : BufferList<ObvResult>, IIncrementFromBar
{
    private double _previousClose = double.NaN;
    private double _obvValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObvList"/> class.
    /// </summary>
    public ObvList() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObvList"/> class with initial bars.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public ObvList(IReadOnlyList<IBar> bars)
        : this() => Add(bars);

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        // Handle volume direction changes in streaming mode
        if (!double.IsNaN(_previousClose))
        {
            if ((double)bar.Close > _previousClose)
            {
                _obvValue += (double)bar.Volume;
            }
            else if ((double)bar.Close < _previousClose)
            {
                _obvValue -= (double)bar.Volume;
            }
            // No change if bar.Close == _previousClose
        }

        // Add result with current OBV value
        AddInternal(new ObvResult(
            Timestamp: bar.Timestamp,
            Obv: _obvValue));

        // Update previous close for next iteration
        _previousClose = (double)bar.Close;
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
        _previousClose = double.NaN;
        _obvValue = 0;
    }
}

public static partial class Obv
{
    /// <summary>
    /// Creates a buffer list for On-Balance Volume (OBV) calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public static ObvList ToObvList(
        this IReadOnlyList<IBar> bars)
        => new() { bars };
}
