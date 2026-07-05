namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Awesome Oscillator from incremental reusable values.
/// </summary>
public class AwesomeList : BufferList<AwesomeResult>, IIncrementFromChain, IAwesome
{
    private readonly Queue<double> _buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="AwesomeList"/> class.
    /// </summary>
    /// <param name="fastPeriods">Number of periods for the fast moving average.</param>
    /// <param name="slowPeriods">Number of periods for the slow moving average.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="slowPeriods"/> is invalid.</exception>
    public AwesomeList(int fastPeriods = 5, int slowPeriods = 34)
    {
        Awesome.Validate(fastPeriods, slowPeriods);
        FastPeriods = fastPeriods;
        SlowPeriods = slowPeriods;

        _buffer = new Queue<double>(slowPeriods);

        Name = $"AWESOME({fastPeriods}, {slowPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AwesomeList"/> class with initial reusable values.
    /// </summary>
    /// <param name="fastPeriods">Number of periods for the fast moving average.</param>
    /// <param name="slowPeriods">Number of periods for the slow moving average.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    public AwesomeList(int fastPeriods, int slowPeriods, IReadOnlyList<IReusable> values)
        : this(fastPeriods, slowPeriods) => Add(values);

    /// <inheritdoc />
    public int FastPeriods { get; init; }

    /// <inheritdoc />
    public int SlowPeriods { get; init; }

    /// <summary>
    /// Adds a new value to the Awesome list.
    /// </summary>
    /// <param name="timestamp">Timestamp of the value.</param>
    /// <param name="value">Value to add.</param>
    public void Add(DateTime timestamp, double value)
    {
        // Add to buffer
        _buffer.Update(SlowPeriods, value);

        double? oscillator = null;
        double? normalized = null;

        // Calculate when we have enough data
        if (_buffer.Count == SlowPeriods)
        {
            // Calculate both SMAs from the same buffer
            double sumSlow = 0;
            double sumFast = 0;
            int index = 0;

            foreach (double val in _buffer)
            {
                sumSlow += val;

                // Fast SMA uses only the last FastPeriods values
                if (index >= SlowPeriods - FastPeriods)
                {
                    sumFast += val;
                }

                index++;
            }

            double fastSma = sumFast / FastPeriods;
            double slowSma = sumSlow / SlowPeriods;
            oscillator = fastSma - slowSma;
            normalized = value != 0 ? 100 * oscillator / value : null;
        }

        AddInternal(new AwesomeResult(
            Timestamp: timestamp,
            Oscillator: oscillator,
            Normalized: normalized
        ));
    }

    /// <inheritdoc />
    public void Add(IReusable value)
    {
        ArgumentNullException.ThrowIfNull(value);
        // Prefer HL2 when source is IBar (Awesome Oscillator specification)
        Add(value.Timestamp, value.Hl2OrValue());
    }

    /// <inheritdoc />
    public void Add(IReadOnlyList<IReusable> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        for (int i = 0; i < values.Count; i++)
        {
            IReusable v = values[i];
            // Prefer HL2 when source is IBar (Awesome Oscillator specification)
            Add(v.Timestamp, v.Hl2OrValue());
        }
    }

    /// <inheritdoc />
    public override void Clear()
    {
        base.Clear();
        _buffer.Clear();
    }
}

/// <summary>
/// EXTENSION METHODS
/// </summary>
public static partial class Awesome
{
    /// <summary>
    /// Creates a buffer list for Awesome Oscillator calculations.
    /// </summary>
    /// <param name="source">Time-series values to transform.</param>
    /// <param name="fastPeriods">Number of periods for the fast moving average. Default is 5.</param>
    /// <param name="slowPeriods">Number of periods for the slow moving average. Default is 34.</param>
    /// <returns>An AwesomeList instance pre-populated with historical data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are invalid.</exception>
    public static AwesomeList ToAwesomeList(
        this IReadOnlyList<IReusable> source,
        int fastPeriods = 5,
        int slowPeriods = 34)
        => new(fastPeriods, slowPeriods) { source };
}
