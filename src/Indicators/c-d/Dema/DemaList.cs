namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Double Exponential Moving Average (DEMA) from incremental reusable values.
/// </summary>
public class DemaList : BufferList<DemaResult>, IIncrementFromChain, IDema
{
    private readonly Queue<double> _buffer;
    private double _bufferSum;
    private double _lastEma1 = double.NaN;
    private double _lastEma2 = double.NaN;

    /// <summary>
    /// Initializes a new instance of the <see cref="DemaList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public DemaList(
        int lookbackPeriods
    )
    {
        Dema.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        K = 2d / (lookbackPeriods + 1);

        _buffer = new Queue<double>(lookbackPeriods);
        _bufferSum = 0;

        Name = $"DEMA({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DemaList"/> class with initial reusable values.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    public DemaList(
        int lookbackPeriods,
        IReadOnlyList<IReusable> values
    )
        : this(lookbackPeriods) => Add(values);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public double K { get; private init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // Use BufferListUtilities extension method with dequeue tracking for running sum maintenance
        double? dequeuedValue = _buffer.UpdateWithDequeue(LookbackPeriods, value);

        // Update running sum efficiently
        if (dequeuedValue.HasValue)
        {
            _bufferSum = _bufferSum - dequeuedValue.Value + value;
        }
        else
        {
            _bufferSum += value;
        }

        // add nulls for incalculable periods
        if (Count < LookbackPeriods - 1)
        {
            AddInternal(new DemaResult(timestamp));
            return;
        }

        double ema1;
        double ema2;

        // when no prior EMA, reset as SMA
        if (double.IsNaN(_lastEma2))
        {
            ema1 = ema2 = _bufferSum / LookbackPeriods;
        }
        else
        {
            // normal DEMA calculation
            ema1 = Ema.Increment(K, _lastEma1, value);
            ema2 = Ema.Increment(K, _lastEma2, ema1);
        }

        // store state for next iteration
        _lastEma1 = ema1;
        _lastEma2 = ema2;

        // calculate and store DEMA result
        AddInternal(new DemaResult(
            timestamp,
            Dema.Calculate(ema1, ema2)));
    }

    /// <inheritdoc />
    public void Add(IReusable value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Add(value.Timestamp, value.Value);
    }

    /// <inheritdoc />
    public void Add(IReadOnlyList<IReusable> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        for (int i = 0; i < values.Count; i++)
        {
            Add(values[i].Timestamp, values[i].Value);
        }
    }

    /// <inheritdoc />
    public override void Clear()
    {
        base.Clear();
        _buffer.Clear();
        _bufferSum = 0;
        _lastEma1 = double.NaN;
        _lastEma2 = double.NaN;
    }
}
