namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Standard deviation from incremental reusable values.
/// </summary>
public class StdDevList : BufferList<StdDevResult>, IIncrementFromChain, IStdDev
{
    private readonly Queue<double> _buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="StdDevList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public StdDevList(int lookbackPeriods = 14)
    {
        StdDev.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        _buffer = new Queue<double>(lookbackPeriods);

        Name = $"STDDEV({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StdDevList"/> class with initial reusable values.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public StdDevList(int lookbackPeriods, IReadOnlyList<IReusable> values)
        : this(lookbackPeriods) => Add(values);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // Update the rolling buffer
        _buffer.Update(LookbackPeriods, value);

        // Calculate when we have enough data
        double? stdDev = null;
        double? mean = null;
        double? zScore = null;

        if (_buffer.Count == LookbackPeriods)
        {
            // Calculate mean
            double sum = 0;
            foreach (double val in _buffer)
            {
                sum += val;
            }

            mean = sum / _buffer.Count;

            // Calculate standard deviation
            double sumSq = 0;
            foreach (double val in _buffer)
            {
                sumSq += (val - mean.Value) * (val - mean.Value);
            }

            stdDev = Math.Sqrt(sumSq / LookbackPeriods);

            // Calculate Z-score
            zScore = stdDev == 0 ? double.NaN : (value - mean.Value) / stdDev.Value;
            zScore = zScore.Value.NaN2Null();
        }

        AddInternal(new StdDevResult(
            Timestamp: timestamp,
            StdDev: stdDev,
            Mean: mean,
            ZScore: zScore));
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
    }
}

public static partial class StdDev
{
    /// <summary>
    /// Creates a buffer list for Standard deviation calculations.
    /// </summary>
    /// <param name="source">Collection of input values, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    public static StdDevList ToStdDevList(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods = 14)
        => new(lookbackPeriods) { source };
}
