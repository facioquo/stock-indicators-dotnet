namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Simple Moving Average (SMA) from incremental reusable values.
/// </summary>
public class SmaList : BufferList<SmaResult>, IIncrementFromChain, ISma
{
    private readonly Queue<double> buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmaList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public SmaList(int lookbackPeriods)
    {
        Sma.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        buffer = new Queue<double>(lookbackPeriods);

        Name = $"SMA({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmaList"/> class with initial reusable values.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public SmaList(int lookbackPeriods, IReadOnlyList<IReusable> values)
        : this(lookbackPeriods) => Add(values);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // advance the rolling window and emit once it is full
        buffer.Update(LookbackPeriods, value);

        AddInternal(new SmaResult(timestamp, buffer.Average(LookbackPeriods).NaN2Null()));
    }

    /// <summary>
    /// Adds a new reusable value to the SMA list.
    /// </summary>
    /// <param name="value">Reusable value to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    public void Add(IReusable value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Add(value.Timestamp, value.Value);
    }

    /// <summary>
    /// Adds a list of reusable values to the SMA list.
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

public static partial class Sma
{
    /// <summary>
    /// Creates a buffer list for Simple Moving Average (SMA) calculations.
    /// </summary>
    /// <param name="source">Collection of input values, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    public static SmaList ToSmaList(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
        => new(lookbackPeriods) { source };
}
