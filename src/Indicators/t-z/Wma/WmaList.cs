namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Weighted Moving Average (WMA) from incremental reusable values.
/// </summary>
public class WmaList : BufferList<WmaResult>, IIncrementFromChain, IWma
{
    private readonly Queue<double> _buffer;
    private readonly double _divisor;
    private double _sum;

    /// <summary>
    /// Initializes a new instance of the <see cref="WmaList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public WmaList(int lookbackPeriods)
    {
        Wma.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;

        // Pre-calculate divisor for WMA: n * (n + 1) / 2
        _divisor = (double)lookbackPeriods * (lookbackPeriods + 1) / 2d;

        _buffer = new Queue<double>(lookbackPeriods);

        Name = $"WMA({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WmaList"/> class with initial reusable values.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public WmaList(int lookbackPeriods, IReadOnlyList<IReusable> values)
        : this(lookbackPeriods) => Add(values);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        double sumBefore = _sum;
        double? dequeuedValue = _buffer.UpdateWithDequeue(LookbackPeriods, value);

        _sum = dequeuedValue.HasValue
            ? sumBefore - dequeuedValue.Value + value
            : sumBefore + value;

        double? wma = Wma.ComputeWeightedMovingAverage(
            _buffer,
            LookbackPeriods,
            _divisor);

        AddInternal(new WmaResult(timestamp, wma));
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
        _sum = 0d;
    }
}

public static partial class Wma
{
    /// <summary>
    /// Creates a buffer list for Weighted Moving Average (WMA) calculations.
    /// </summary>
    /// <param name="source">Collection of input values, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    public static WmaList ToWmaList(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
        => new(lookbackPeriods) { source };
}
