namespace FacioQuo.Stock.Indicators;

/// <summary>
/// T3 Moving Average from incremental reusable values.
/// </summary>
public class T3List : BufferList<T3Result>, IIncrementFromChain, IT3
{
    /// <summary>
    /// State for six-layer EMA calculations
    /// </summary>
    private double _lastEma1 = double.NaN;
    private double _lastEma2 = double.NaN;
    private double _lastEma3 = double.NaN;
    private double _lastEma4 = double.NaN;
    private double _lastEma5 = double.NaN;
    private double _lastEma6 = double.NaN;

    /// <summary>
    /// Initializes a new instance of the <see cref="T3List"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="volumeFactor">Volume factor for the calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="volumeFactor"/> is invalid.</exception>
    public T3List(
        int lookbackPeriods = 5,
        double volumeFactor = 0.7
    )
    {
        T3.Validate(lookbackPeriods, volumeFactor);
        LookbackPeriods = lookbackPeriods;
        VolumeFactor = volumeFactor;
        K = 2d / (lookbackPeriods + 1);

        double a = volumeFactor;
        C1 = -a * a * a;
        C2 = (3 * a * a) + (3 * a * a * a);
        C3 = (-6 * a * a) - (3 * a) - (3 * a * a * a);
        C4 = 1 + (3 * a) + (a * a * a) + (3 * a * a);

        Name = $"T3({5}, {0.7})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T3List"/> class with initial reusable values.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="volumeFactor">Volume factor for the calculation.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    public T3List(
        int lookbackPeriods,
        double volumeFactor,
        IReadOnlyList<IReusable> values
    )
        : this(lookbackPeriods, volumeFactor) => Add(values);

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc/>
    public double VolumeFactor { get; init; }

    /// <inheritdoc/>
    public double K { get; private init; }

    /// <inheritdoc/>
    public double C1 { get; private init; }

    /// <inheritdoc/>
    public double C2 { get; private init; }

    /// <inheritdoc/>
    public double C3 { get; private init; }

    /// <inheritdoc/>
    public double C4 { get; private init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        double ema1;
        double ema2;
        double ema3;
        double ema4;
        double ema5;
        double ema6;

        // re/seed values on first data point
        if (double.IsNaN(_lastEma6))
        {
            ema1 = ema2 = ema3 = ema4 = ema5 = ema6 = value;
        }
        // normal T3 calculation with six layers
        else
        {
            ema1 = _lastEma1 + (K * (value - _lastEma1));
            ema2 = _lastEma2 + (K * (ema1 - _lastEma2));
            ema3 = _lastEma3 + (K * (ema2 - _lastEma3));
            ema4 = _lastEma4 + (K * (ema3 - _lastEma4));
            ema5 = _lastEma5 + (K * (ema4 - _lastEma5));
            ema6 = _lastEma6 + (K * (ema5 - _lastEma6));
        }

        // calculate T3 with coefficients
        double t3 = (C1 * ema6) + (C2 * ema5) + (C3 * ema4) + (C4 * ema3);

        AddInternal(new T3Result(
            timestamp,
            t3.NaN2Null()) {
            Ema1 = ema1,
            Ema2 = ema2,
            Ema3 = ema3,
            Ema4 = ema4,
            Ema5 = ema5,
            Ema6 = ema6
        });

        // store state for next calculation
        _lastEma1 = ema1;
        _lastEma2 = ema2;
        _lastEma3 = ema3;
        _lastEma4 = ema4;
        _lastEma5 = ema5;
        _lastEma6 = ema6;
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
        _lastEma1 = double.NaN;
        _lastEma2 = double.NaN;
        _lastEma3 = double.NaN;
        _lastEma4 = double.NaN;
        _lastEma5 = double.NaN;
        _lastEma6 = double.NaN;
    }
}

public static partial class T3
{
    /// <summary>
    /// Creates a buffer list for T3 calculations.
    /// </summary>
    /// <param name="source">Source list of reusable values.</param>
    /// <param name="lookbackPeriods">Number of lookback periods.</param>
    /// <param name="volumeFactor">Volume smoothing factor.</param>
    /// <returns>A buffer list for T3 calculations.</returns>
    public static T3List ToT3List(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods = 5,
        double volumeFactor = 0.7)
        => new(lookbackPeriods, volumeFactor) { source };
}
