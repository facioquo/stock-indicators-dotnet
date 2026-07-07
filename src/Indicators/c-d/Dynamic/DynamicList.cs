namespace FacioQuo.Stock.Indicators;

/// <summary>
/// McGinley Dynamic from incremental reusable values.
/// </summary>
public class DynamicList : BufferList<DynamicResult>, IIncrementFromChain, IDynamic
{
    private double? _previousDynamic;
    private double _previousValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="kFactor">Smoothing factor for the calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="kFactor"/> is invalid.</exception>
    public DynamicList(
        int lookbackPeriods,
        double kFactor = 0.6
    )
    {
        MgDynamic.Validate(lookbackPeriods, kFactor);
        LookbackPeriods = lookbackPeriods;
        KFactor = kFactor;

        _previousDynamic = null;
        _previousValue = 0;

        Name = $"DYNAMIC({lookbackPeriods}, {0.6})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicList"/> class with initial reusable values.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="kFactor">Smoothing factor for the calculation.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    public DynamicList(
        int lookbackPeriods,
        double kFactor,
        IReadOnlyList<IReusable> values
    )
        : this(lookbackPeriods, kFactor) => Add(values);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public double KFactor { get; init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // skip first period
        if (Count == 0)
        {
            _previousDynamic = null;
            _previousValue = value;
            AddInternal(new DynamicResult(timestamp, null));
            return;
        }

        // calculate dynamic
        double prevDyn = _previousDynamic ?? _previousValue;
        double? dyn = MgDynamic.Increment(
            LookbackPeriods,
            KFactor,
            newVal: value,
            prevDyn: prevDyn
        ).NaN2Null();

        _previousDynamic = dyn;
        _previousValue = value;
        AddInternal(new DynamicResult(timestamp, dyn));
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
        _previousDynamic = null;
        _previousValue = 0;
    }
}

public static partial class MgDynamic
{
    /// <summary>
    /// Creates a buffer list for McGinley Dynamic calculations.
    /// </summary>
    /// <param name="source">Collection of input values, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="kFactor">K-factor for calculations</param>
    public static DynamicList ToDynamicList(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods,
        double kFactor = 0.6)
        => new(lookbackPeriods, kFactor) { source };
}
