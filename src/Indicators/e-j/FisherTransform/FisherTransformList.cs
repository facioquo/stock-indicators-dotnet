namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Fisher Transform from incremental reusable values.
/// </summary>
public class FisherTransformList : BufferList<FisherTransformResult>, IIncrementFromChain, IFisherTransform
{
    private readonly Queue<double> _priceBuffer;
    private double _previousXv;

    /// <summary>
    /// Initializes a new instance of the <see cref="FisherTransformList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public FisherTransformList(
        int lookbackPeriods = 10
    )
    {
        FisherTransform.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;

        _priceBuffer = new Queue<double>(lookbackPeriods);
        _previousXv = 0;

        Name = $"FISHERTRANSFORM({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FisherTransformList"/> class with initial reusable values.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    public FisherTransformList(
        int lookbackPeriods,
        IReadOnlyList<IReusable> values
    )
        : this(lookbackPeriods) => Add(values);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // add current value to buffer first
        _priceBuffer.Update(LookbackPeriods, value);

        // calculate min/max in lookback period (all values in buffer)
        double minPrice = value;
        double maxPrice = value;

        foreach (double price in _priceBuffer)
        {
            if (price < minPrice)
            {
                minPrice = price;
            }

            if (price > maxPrice)
            {
                maxPrice = price;
            }
        }

        double fisher;
        double? trigger = null;

        if (Count > 0)
        {
            // calculate value transform
            double xv = maxPrice - minPrice != 0
                ? (0.33 * 2 * (((value - minPrice) / (maxPrice - minPrice)) - 0.5))
                      + (0.67 * _previousXv)
                : 0d;

            // limit xv to prevent log issues
            xv = xv > 0.99 ? 0.999 : xv;
            xv = xv < -0.99 ? -0.999 : xv;

            // calculate Fisher Transform
            fisher = DeMath.Atanh(xv) + (0.5d * this[^1].Fisher);

            trigger = this[^1].Fisher;
            _previousXv = xv;
        }
        else
        {
            _previousXv = 0;
            fisher = 0;
        }

        AddInternal(new(
            Timestamp: timestamp,
            Fisher: fisher,
            Trigger: trigger));
    }

    /// <inheritdoc />
    public void Add(IReusable value)
    {
        ArgumentNullException.ThrowIfNull(value);
        // prefer HL2 when source is an IBar (Fisher Transform specification)
        Add(value.Timestamp, value.Hl2OrValue());
    }

    /// <inheritdoc />
    public void Add(IReadOnlyList<IReusable> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        for (int i = 0; i < values.Count; i++)
        {
            IReusable v = values[i];
            // prefer HL2 when source is an IBar (Fisher Transform specification)
            Add(v.Timestamp, v.Hl2OrValue());
        }
    }

    /// <inheritdoc />
    public override void Clear()
    {
        base.Clear();
        _priceBuffer.Clear();
        _previousXv = 0;
    }
}

public static partial class FisherTransform
{
    /// <summary>
    /// Creates a buffer list for Fisher Transform calculations.
    /// </summary>
    /// <param name="source">Collection of input values, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    public static FisherTransformList ToFisherTransformList(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods = 10)
        => new(lookbackPeriods) { source };
}
