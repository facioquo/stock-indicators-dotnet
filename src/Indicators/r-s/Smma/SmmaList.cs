namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Smoothed Moving Average (SMMA) from incremental reusable values.
/// </summary>
public class SmmaList : BufferList<SmmaResult>, IIncrementFromChain, ISmma
{
    private double? _previousSmma;
    private readonly Queue<double> _buffer;
    private double _bufferSum;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmmaList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public SmmaList(
        int lookbackPeriods
    )
    {
        Smma.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;

        _buffer = new Queue<double>(lookbackPeriods);
        _bufferSum = 0;
        _previousSmma = null;

        Name = $"SMMA({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmmaList"/> class with initial reusable values.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    public SmmaList(
        int lookbackPeriods,
        IReadOnlyList<IReusable> values
    )
        : this(lookbackPeriods) => Add(values);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // update buffer for SMA initialization using buffer utilities
        double? dequeuedValue = _buffer.UpdateWithDequeue(LookbackPeriods, value);

        // update running sum efficiently
        if (_buffer.Count == LookbackPeriods && dequeuedValue.HasValue)
        {
            _bufferSum = _bufferSum - dequeuedValue.Value + value;
        }
        else
        {
            _bufferSum += value;
        }

        // add nulls for incalculable periods
        if (_buffer.Count < LookbackPeriods)
        {
            AddInternal(new SmmaResult(timestamp));
            return;
        }

        double smma = _previousSmma is null
            ? _bufferSum / LookbackPeriods
            // normal SMMA calculation
            : ((_previousSmma.Value * (LookbackPeriods - 1)) + value) / LookbackPeriods;

        // when no prior SMMA, reset as SMA

        SmmaResult result = new(timestamp, smma.NaN2Null());
        AddInternal(result);
        _previousSmma = smma;
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
        _previousSmma = null;
    }
}

public static partial class Smma
{
    /// <summary>
    /// Creates a buffer list for Smoothed Moving Average (SMMA) calculations.
    /// </summary>
    /// <param name="source">Collection of input values, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    public static SmmaList ToSmmaList(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
        => new(lookbackPeriods) { source };
}
