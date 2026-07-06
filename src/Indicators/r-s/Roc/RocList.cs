namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Rate of Change (ROC) from incremental reusable values.
/// </summary>
public class RocList : BufferList<RocResult>, IIncrementFromChain, IRoc
{
    private readonly Queue<double> buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="RocList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public RocList(int lookbackPeriods)
    {
        Roc.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        buffer = new Queue<double>(lookbackPeriods + 1);

        Name = $"ROC({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RocList"/> class with initial reusable values.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public RocList(int lookbackPeriods, IReadOnlyList<IReusable> values)
        : this(lookbackPeriods) => Add(values);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // Use BufferListUtilities extension method for consistent buffer management
        buffer.Update(LookbackPeriods + 1, value);

        double roc;
        double momentum;

        // Calculate ROC when we have enough values
        if (buffer.Count > LookbackPeriods)
        {
            double backValue = buffer.Peek();
            momentum = value - backValue;

            roc = backValue == 0
                ? double.NaN
                : 100d * momentum / backValue;
        }
        else
        {
            momentum = double.NaN;
            roc = double.NaN;
        }

        AddInternal(new RocResult(
            Timestamp: timestamp,
            Momentum: momentum.NaN2Null(),
            Roc: roc.NaN2Null()));
    }

    /// <summary>
    /// Adds a new reusable value to the ROC list.
    /// </summary>
    /// <param name="value">Reusable value to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    public void Add(IReusable value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Add(value.Timestamp, value.Value);
    }

    /// <summary>
    /// Adds a list of reusable values to the ROC list.
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
        buffer.Clear();
    }
}

public static partial class Roc
{
    /// <summary>
    /// Creates a buffer list for Rate of Change (ROC) calculations.
    /// </summary>
    /// <param name="source">Collection of input values, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    public static RocList ToRocList(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
        => new(lookbackPeriods) { source };
}
