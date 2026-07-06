namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Commodity Channel Index (CCI) from incremental bar values.
/// </summary>
public class CciList : BufferList<CciResult>, IIncrementFromBar, ICci
{
    private readonly Queue<double> _buffer; // true price

    /// <summary>
    /// Initializes a new instance of the <see cref="CciList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public CciList(int lookbackPeriods)
    {
        Cci.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;

        _buffer = new Queue<double>(lookbackPeriods);

        Name = $"CCI({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CciList"/> class with initial bars.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public CciList(int lookbackPeriods, IReadOnlyList<IBar> bars)
        : this(lookbackPeriods) => Add(bars);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        // Calculate typical price
        double tp = ((double)bar.High + (double)bar.Low + (double)bar.Close) / 3d;

        // Update buffer using universal buffer utilities
        _buffer.Update(LookbackPeriods, tp);

        double? cci = null;

        // Calculate CCI when we have enough data
        if (_buffer.Count == LookbackPeriods)
        {
            // Calculate average TP over lookback (sum then divide, same as LINQ Average)
            double sumTp = 0;
            foreach (double tpValue in _buffer)
            {
                sumTp += tpValue;
            }

            double avgTp = sumTp / _buffer.Count;

            // Calculate average Deviation over lookback
            double avgDv = 0;
            foreach (double tpValue in _buffer)
            {
                avgDv += Math.Abs(avgTp - tpValue);
            }

            avgDv /= LookbackPeriods;

            // Calculate CCI
            cci = avgDv == 0 ? null
                : ((tp - avgTp) / (0.015 * avgDv)).NaN2Null();
        }

        AddInternal(new CciResult(bar.Timestamp, cci));
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

public static partial class Cci
{
    /// <summary>
    /// Creates a buffer list for Commodity Channel Index (CCI).
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public static CciList ToCciList(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 20)
        => new(lookbackPeriods) { bars };
}
