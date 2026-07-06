namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Hurst Exponent from incremental reusable values.
/// </summary>
public class HurstList : BufferList<HurstResult>, IIncrementFromChain, IHurst
{
    private readonly Queue<double> _buffer;
    private readonly double[] _bufferArray;
    private readonly double[] _values;
    private readonly double[] _alCorrections;

    /// <summary>
    /// Initializes a new instance of the <see cref="HurstList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Number of periods to look back for the calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public HurstList(int lookbackPeriods)
    {
        Hurst.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        _buffer = new Queue<double>(lookbackPeriods + 1);
        _bufferArray = new double[lookbackPeriods + 1];
        _values = new double[lookbackPeriods];
        _alCorrections = Hurst.PrecomputeAlCorrections(lookbackPeriods);

        Name = $"HURST({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HurstList"/> class with initial reusable values.
    /// </summary>
    /// <param name="lookbackPeriods">Number of periods to look back for the calculation.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public HurstList(int lookbackPeriods, IReadOnlyList<IReusable> values)
        : this(lookbackPeriods) => Add(values);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        _buffer.Update(LookbackPeriods + 1, value);

        double? h = null;
        double? hAl = null;

        // need enough periods to calculate Hurst (lookbackPeriods + 1 values to get lookbackPeriods returns)
        if (_buffer.Count == LookbackPeriods + 1)
        {
            // get evaluation batch - calculate returns from buffer values
            // using reusable arrays (avoids per-Add allocation)
            double[] values = _values;
            double[] bufferArray = _bufferArray;
            _buffer.CopyTo(bufferArray, 0);

            int x = 0;
            double l = bufferArray[0];

            // skip first value (used as initial l) and calculate returns for the rest
            for (int p = 1; p < bufferArray.Length; p++)
            {
                double ps = bufferArray[p];

                // log returns require strictly positive prices on both ends
                values[x] = (l > 0 && ps > 0) ? DeMath.Log(ps / l) : double.NaN;

                l = ps;
                x++;
            }

            // calculate hurst exponent
            (double rawH, double correctedH) = Hurst.CalcHurstWindow(values, _alCorrections);
            h = rawH.NaN2Null();
            hAl = correctedH.NaN2Null();
        }

        AddInternal(new HurstResult(timestamp, h, hAl));
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

public static partial class Hurst
{
    /// <summary>
    /// Creates a buffer list for Hurst Exponent calculations.
    /// </summary>
    /// <param name="source">Historical reusable values.</param>
    /// <param name="lookbackPeriods">Number of periods for Hurst calculation.</param>
    /// <returns>An initialized <see cref="HurstList" />.</returns>
    public static HurstList ToHurstList(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods = 100)
        => new(lookbackPeriods) { source };
}
