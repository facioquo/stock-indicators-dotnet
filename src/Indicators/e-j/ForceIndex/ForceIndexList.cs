namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Force Index from incremental bar values.
/// </summary>
public class ForceIndexList : BufferList<ForceIndexResult>, IIncrementFromBar, IForceIndex
{
    private readonly Queue<double> _rawFiBuffer;
    private double _sumRawFi;
    private double? _previousFi;
    private double? _previousClose;
    private readonly double _k;

    /// <summary>
    /// Initializes a new instance of the <see cref="ForceIndexList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Number of periods to look back for the calculation. Default is 2.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public ForceIndexList(
        int lookbackPeriods = 2
    )
    {
        ForceIndex.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;

        _rawFiBuffer = new Queue<double>(lookbackPeriods);
        _sumRawFi = 0;
        _previousFi = null;
        _previousClose = null;
        _k = 2d / (lookbackPeriods + 1);

        Name = $"FORCEINDEX({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForceIndexList"/> class with initial bars.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public ForceIndexList(
        int lookbackPeriods,
        IReadOnlyList<IBar> bars
    )
        : this(lookbackPeriods) => Add(bars);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        // skip first period
        if (Count == 0)
        {
            _previousClose = (double)bar.Close;
            AddInternal(new ForceIndexResult(bar.Timestamp));
            return;
        }

        double? fi = null;

        // calculate raw Force Index
        double? rawFi = (double)bar.Volume * ((double)bar.Close - _previousClose);

        if (rawFi.HasValue)
        {
            // update buffer
            double? dequeuedValue = _rawFiBuffer.UpdateWithDequeue(LookbackPeriods, rawFi.Value);

            if (_rawFiBuffer.Count == LookbackPeriods && dequeuedValue.HasValue)
            {
                _sumRawFi = _sumRawFi - dequeuedValue.Value + rawFi.Value;
            }
            else
            {
                _sumRawFi += rawFi.Value;
            }

            // calculate EMA
            if (Count >= LookbackPeriods)
            {
                if (!_previousFi.HasValue)
                {
                    // initialization period - first EMA value
                    fi = _sumRawFi / LookbackPeriods;
                }
                else
                {
                    fi = _previousFi + (_k * (rawFi - _previousFi));
                }
            }
        }

        _previousFi = fi;
        _previousClose = (double)bar.Close;

        AddInternal(new ForceIndexResult(
            Timestamp: bar.Timestamp,
            ForceIndex: fi));
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
        _rawFiBuffer.Clear();
        _sumRawFi = 0;
        _previousFi = null;
        _previousClose = null;
    }
}

public static partial class ForceIndex
{
    /// <summary>
    /// Creates a buffer list for Force Index calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    public static ForceIndexList ToForceIndexList(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 2)
        => new(lookbackPeriods) { bars };
}
