namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Rolling highest value from incremental reusable values.
/// </summary>
public class RollingMaxList : BufferList<TimeValue>, IIncrementFromChain
{
    private readonly RollingExtremeWindow _window;

    /// <summary>
    /// Initializes a new instance of the <see cref="RollingMaxList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Number of periods in the window, including the current one.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than 1.</exception>
    public RollingMaxList(int lookbackPeriods)
    {
        _window = new RollingExtremeWindow(lookbackPeriods, isMax: true);
        LookbackPeriods = lookbackPeriods;
        Name = $"ROLLINGMAX({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RollingMaxList"/> class with initial reusable values.
    /// </summary>
    /// <param name="lookbackPeriods">Number of periods in the window, including the current one.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than 1.</exception>
    public RollingMaxList(int lookbackPeriods, IReadOnlyList<IReusable> values)
        : this(lookbackPeriods) => Add(values);

    /// <summary>
    /// Gets the number of periods in the window, including the current one.
    /// </summary>
    public int LookbackPeriods { get; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
        => AddInternal(new TimeValue(timestamp, _window.Add(value)));

    /// <summary>
    /// Adds a new reusable value to the list.
    /// </summary>
    /// <param name="value">Reusable value to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    public void Add(IReusable value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Add(value.Timestamp, value.Value);
    }

    /// <summary>
    /// Adds a list of reusable values to the list.
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
    /// Clears the list and resets the rolling window so the instance can be reused.
    /// </summary>
    public override void Clear()
    {
        base.Clear();
        _window.Clear();
    }
}

public static partial class ResultMath
{
    /// <summary>
    /// Creates a buffer list for the rolling highest value.
    /// </summary>
    /// <param name="source">Chainable results, time sorted.</param>
    /// <param name="lookbackPeriods">Number of periods in the window, including the current one.</param>
    /// <returns>A buffer list of rolling highest value values.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than 1.</exception>
    public static RollingMaxList ToRollingMaxList(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
        => new(lookbackPeriods) { source };
}
