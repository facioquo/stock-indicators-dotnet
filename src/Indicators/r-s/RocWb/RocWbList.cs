namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Rate of Change with Bands (RocWb) from incremental reusable values.
/// </summary>
public class RocWbList : BufferList<RocWbResult>, IIncrementFromChain, IRocWb
{
    private readonly Queue<double> _rocBuffer;
    private readonly Queue<double> _rocSqBuffer;
    private readonly Queue<double> _rocEmaInitBuffer;
    private double prevEma = double.NaN;
    private readonly double k;

    /// <summary>
    /// Initializes a new instance of the <see cref="RocWbList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="emaPeriods">Number of periods for the exponential moving average calculation.</param>
    /// <param name="stdDevPeriods">Number of periods for the standard deviation calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="stdDevPeriods"/> is invalid.</exception>
    public RocWbList(int lookbackPeriods, int emaPeriods, int stdDevPeriods)
    {
        RocWb.Validate(lookbackPeriods, emaPeriods, stdDevPeriods);
        LookbackPeriods = lookbackPeriods;
        EmaPeriods = emaPeriods;
        StdDevPeriods = stdDevPeriods;
        _rocBuffer = new Queue<double>(lookbackPeriods + 1);
        _rocSqBuffer = new Queue<double>(stdDevPeriods);
        _rocEmaInitBuffer = new Queue<double>(emaPeriods);
        k = 2d / (emaPeriods + 1);

        Name = $"ROCWB({lookbackPeriods}, {emaPeriods}, {stdDevPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RocWbList"/> class with initial reusable values.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="emaPeriods">Number of periods for the exponential moving average calculation.</param>
    /// <param name="stdDevPeriods">Number of periods for the standard deviation calculation.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    public RocWbList(int lookbackPeriods, int emaPeriods, int stdDevPeriods, IReadOnlyList<IReusable> values)
        : this(lookbackPeriods, emaPeriods, stdDevPeriods) => Add(values);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public int EmaPeriods { get; init; }

    /// <inheritdoc />
    public int StdDevPeriods { get; init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // Update the ROC buffer
        _rocBuffer.Update(LookbackPeriods + 1, value);

        // Calculate ROC
        double roc;
        if (_rocBuffer.Count > LookbackPeriods)
        {
            double backValue = _rocBuffer.Peek();
            roc = backValue == 0 ? double.NaN : 100d * (value - backValue) / backValue;
        }
        else
        {
            roc = double.NaN;
        }

        // Update the ROC buffer for squared values (RMS calculation)
        double rocSq = roc * roc;
        if (!double.IsNaN(roc))
        {
            _rocSqBuffer.Update(StdDevPeriods, rocSq);
        }

        // Calculate EMA of ROC
        double rocEma;
        if (double.IsNaN(prevEma))
        {
            // Accumulate ROC values for initial EMA calculation
            if (!double.IsNaN(roc))
            {
                _rocEmaInitBuffer.Update(EmaPeriods, roc);
            }

            // Initialize EMA with SMA when we have enough values
            if (_rocEmaInitBuffer.Count >= EmaPeriods)
            {
                double sum = 0;
                foreach (double val in _rocEmaInitBuffer)
                {
                    sum += val;
                }

                rocEma = sum / _rocEmaInitBuffer.Count;
            }
            else
            {
                rocEma = double.NaN;
            }
        }
        else
        {
            // Normal EMA calculation
            rocEma = Ema.Increment(k, prevEma, roc);
        }

        prevEma = rocEma;

        // Calculate RMS deviation bands
        double? rocDev = null;
        if (_rocSqBuffer.Count >= StdDevPeriods && !double.IsNaN(roc))
        {
            double sumSq = 0;
            foreach (double val in _rocSqBuffer)
            {
                sumSq += val;
            }

            rocDev = Math.Sqrt(sumSq / StdDevPeriods).NaN2Null();
        }

        AddInternal(new RocWbResult(
            Timestamp: timestamp,
            Roc: roc.NaN2Null(),
            RocEma: rocEma.NaN2Null(),
            UpperBand: rocDev,
            LowerBand: rocDev.HasValue ? -rocDev.Value : null));
    }

    /// <summary>
    /// Adds a new reusable value to the RocWb list.
    /// </summary>
    /// <param name="value">Reusable value to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    public void Add(IReusable value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Add(value.Timestamp, value.Value);
    }

    /// <summary>
    /// Adds a list of reusable values to the RocWb list.
    /// </summary>
    /// <param name="values">List of reusable values to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the values list is null.</exception>
    public void Add(IReadOnlyList<IReusable> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        for (int i = 0; i < values.Count; i++)
        {
            Add(values[i].Timestamp, values[i].Value);
        }
    }

    /// <summary>
    /// Clears the list and resets internal buffers so the instance can be reused.
    /// </summary>
    public override void Clear()
    {
        base.Clear();
        _rocBuffer.Clear();
        _rocSqBuffer.Clear();
        _rocEmaInitBuffer.Clear();
        prevEma = double.NaN;
    }
}

public static partial class RocWb
{
    /// <summary>
    /// Creates a buffer list for Rate of Change with Bands (RocWb) calculations.
    /// </summary>
    /// <param name="source">Collection of input values, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="emaPeriods">Number of periods for the exponential moving average</param>
    /// <param name="stdDevPeriods">Number of periods for the standard deviation calculation</param>
    public static RocWbList ToRocWbList(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods = 20,
        int emaPeriods = 5,
        int stdDevPeriods = 5)
        => new(lookbackPeriods, emaPeriods, stdDevPeriods) { source };
}
