namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Exponential Moving Average (EMA) from incremental reusable values.
/// </summary>
public class EmaList : BufferList<EmaResult>, IIncrementFromChain, IEma
{
    private readonly Queue<double> _buffer;
    private double _bufferSum;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmaList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public EmaList(int lookbackPeriods)
    {
        Ema.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        K = 2d / (lookbackPeriods + 1);

        _buffer = new Queue<double>(lookbackPeriods);
        _bufferSum = 0;

        Name = $"EMA({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmaList"/> class with initial values.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public EmaList(
        int lookbackPeriods,
        IReadOnlyList<IReusable> values)
        : this(lookbackPeriods) => Add(values);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public double K { get; private init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // update buffer and track dequeued value for sum maintenance using extension method
        double? dequeuedValue = _buffer.UpdateWithDequeue(LookbackPeriods, value);

        // update running sum
        if (_buffer.Count == LookbackPeriods && dequeuedValue.HasValue)
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
            AddInternal(new EmaResult(timestamp));
            return;
        }

        double? lastEma = this[^1].Ema;

        // re/initialize as SMA
        if (lastEma is null)
        {
            AddInternal(new EmaResult(
                timestamp,
                _bufferSum / LookbackPeriods));
            return;
        }

        // calculate EMA normally
        AddInternal(new EmaResult(
            timestamp,
            Ema.Increment(K, lastEma.Value, value)));
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
    }
}

public static partial class Ema
{
    /// <summary>
    /// Creates a buffer list for Exponential Moving Average (EMA) calculations.
    /// </summary>
    /// <param name="source">Collection of input values, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    public static EmaList ToEmaList(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
        => new(lookbackPeriods) { source };
}
