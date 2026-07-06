namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Kaufman's Adaptive Moving Average (KAMA) from incremental reusable values.
/// </summary>
public class KamaList : BufferList<KamaResult>, IIncrementFromChain, IKama
{
    private readonly Queue<double> _buffer;
    private readonly double[] _bufferArray;
    private readonly int _erPeriods;
    private readonly double _scFast;
    private readonly double _scSlow;
    private double _prevKama = double.NaN;

    /// <summary>
    /// Initializes a new instance of the <see cref="KamaList"/> class.
    /// </summary>
    /// <param name="erPeriods">Number of periods for the Efficiency Ratio (ER).</param>
    /// <param name="fastPeriods">Number of periods for the fast EMA.</param>
    /// <param name="slowPeriods">Number of periods for the slow EMA.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="slowPeriods"/> is invalid.</exception>
    public KamaList(
        int erPeriods = 10,
        int fastPeriods = 2,
        int slowPeriods = 30
    )
    {
        Kama.Validate(erPeriods, fastPeriods, slowPeriods);

        _erPeriods = erPeriods;
        ErPeriods = erPeriods;
        FastPeriods = fastPeriods;
        SlowPeriods = slowPeriods;

        _scFast = 2d / (fastPeriods + 1);
        _scSlow = 2d / (slowPeriods + 1);

        _buffer = new Queue<double>(erPeriods + 1);
        _bufferArray = new double[erPeriods + 1];

        Name = $"KAMA({erPeriods}, {fastPeriods}, {slowPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KamaList"/> class with initial reusable values.
    /// </summary>
    /// <param name="erPeriods">Number of periods for the Efficiency Ratio (ER).</param>
    /// <param name="fastPeriods">Number of periods for the fast EMA.</param>
    /// <param name="slowPeriods">Number of periods for the slow EMA.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    public KamaList(
        int erPeriods,
        int fastPeriods,
        int slowPeriods,
        IReadOnlyList<IReusable> values
    )
        : this(erPeriods, fastPeriods, slowPeriods) => Add(values);

    /// <inheritdoc />
    public int ErPeriods { get; init; }

    /// <inheritdoc />
    public int FastPeriods { get; init; }

    /// <inheritdoc />
    public int SlowPeriods { get; init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // Use universal buffer extension method for consistent buffer management
        _buffer.Update(_erPeriods + 1, value);

        // add nulls for incalculable periods
        if (_buffer.Count < _erPeriods)
        {
            AddInternal(new KamaResult(timestamp));
            return;
        }

        double er;
        double kama;

        // Calculate if we have enough data
        if (_buffer.Count == _erPeriods + 1)
        {
            // copy buffer into reusable array (avoids per-Add allocation)
            double[] bufferArray = _bufferArray;
            _buffer.CopyTo(bufferArray, 0);
            double newVal = bufferArray[^1]; // Current value

            // Check if we have a previous KAMA value
            if (!double.IsNaN(_prevKama))
            {
                // ER period change
                double change = Math.Abs(newVal - bufferArray[0]); // First value in buffer

                // volatility - sum of absolute differences
                double sumPv = 0;
                for (int i = 1; i < bufferArray.Length; i++)
                {
                    sumPv += Math.Abs(bufferArray[i] - bufferArray[i - 1]);
                }

                if (sumPv != 0)
                {
                    // efficiency ratio
                    er = change / sumPv;

                    // smoothing constant
                    double sc = (er * (_scFast - _scSlow)) + _scSlow;  // squared later

                    // kama calculation
                    kama = _prevKama + (sc * sc * (newVal - _prevKama));
                }
                else
                {
                    // handle flatline case
                    er = 0;
                    kama = newVal;
                }
            }
            else
            {
                // re/initialize
                er = double.NaN;
                kama = newVal;
            }
        }
        else
        {
            // Not enough data yet
            er = double.NaN;
            kama = value;
        }

        AddInternal(new KamaResult(
            Timestamp: timestamp,
            Er: er.NaN2Null(),
            Kama: kama.NaN2Null()));

        _prevKama = kama;
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
        _prevKama = double.NaN;
    }
}

public static partial class Kama
{
    /// <summary>
    /// Creates a buffer list for Kaufman's Adaptive Moving Average (KAMA) calculations.
    /// </summary>
    /// <param name="source">Collection of input values, time sorted.</param>
    /// <param name="erPeriods">Number of periods for efficiency ratio</param>
    /// <param name="fastPeriods">Number of periods for the fast moving average</param>
    /// <param name="slowPeriods">Number of periods for the slow moving average</param>
    public static KamaList ToKamaList(
        this IReadOnlyList<IReusable> source,
        int erPeriods = 10,
        int fastPeriods = 2,
        int slowPeriods = 30)
        => new(erPeriods, fastPeriods, slowPeriods) { source };
}
