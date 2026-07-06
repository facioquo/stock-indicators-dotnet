namespace FacioQuo.Stock.Indicators;

/// <summary>
/// MACD (Moving Average Convergence Divergence) from incremental reusable values.
/// </summary>
public class MacdList : BufferList<MacdResult>, IIncrementFromChain, IMacd
{
    private readonly Queue<double> _fastBuffer;
    private readonly Queue<double> _slowBuffer;
    private readonly Queue<double> _macdBuffer;
    private double _fastBufferSum;
    private double _slowBufferSum;
    private double _macdBufferSum;

    private double? _lastFastEma;
    private double? _lastSlowEma;
    private double? _lastSignalEma;

    /// <summary>
    /// Initializes a new instance of the <see cref="MacdList"/> class.
    /// </summary>
    /// <param name="fastPeriods">Number of periods for the fast EMA.</param>
    /// <param name="slowPeriods">Number of periods for the slow EMA.</param>
    /// <param name="signalPeriods">Number of periods for the signal line.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="signalPeriods"/> is invalid.</exception>
    public MacdList(
        int fastPeriods = 12,
        int slowPeriods = 26,
        int signalPeriods = 9)
    {
        Macd.Validate(fastPeriods, slowPeriods, signalPeriods);
        FastPeriods = fastPeriods;
        SlowPeriods = slowPeriods;
        SignalPeriods = signalPeriods;

        FastK = 2d / (fastPeriods + 1);
        SlowK = 2d / (slowPeriods + 1);
        SignalK = 2d / (signalPeriods + 1);

        _fastBuffer = new Queue<double>(fastPeriods);
        _slowBuffer = new Queue<double>(slowPeriods);
        _macdBuffer = new Queue<double>(signalPeriods);

        _fastBufferSum = 0;
        _slowBufferSum = 0;
        _macdBufferSum = 0;

        Name = $"MACD({fastPeriods}, {slowPeriods}, {signalPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MacdList"/> class with initial reusable values.
    /// </summary>
    /// <param name="fastPeriods">Number of periods for the fast EMA.</param>
    /// <param name="slowPeriods">Number of periods for the slow EMA.</param>
    /// <param name="signalPeriods">Number of periods for the signal line.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    public MacdList(
        int fastPeriods,
        int slowPeriods,
        int signalPeriods,
        IReadOnlyList<IReusable> values)
        : this(fastPeriods, slowPeriods, signalPeriods) => Add(values);

    /// <inheritdoc/>
    public int FastPeriods { get; init; }

    /// <inheritdoc/>
    public int SlowPeriods { get; init; }

    /// <inheritdoc/>
    public int SignalPeriods { get; init; }

    /// <inheritdoc />
    public double FastK { get; private init; }

    /// <inheritdoc />
    public double SlowK { get; private init; }

    /// <inheritdoc />
    public double SignalK { get; private init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // Update fast EMA buffer using BufferListUtilities
        double? dequeuedFast = _fastBuffer.UpdateWithDequeue(FastPeriods, value);
        if (dequeuedFast.HasValue)
        {
            _fastBufferSum = _fastBufferSum - dequeuedFast.Value + value;
        }
        else
        {
            _fastBufferSum += value;
        }

        // Update slow EMA buffer using BufferListUtilities
        double? dequeuedSlow = _slowBuffer.UpdateWithDequeue(SlowPeriods, value);
        if (dequeuedSlow.HasValue)
        {
            _slowBufferSum = _slowBufferSum - dequeuedSlow.Value + value;
        }
        else
        {
            _slowBufferSum += value;
        }

        // Calculate Fast EMA
        double? fastEma = null;
        if (Count >= FastPeriods - 1)
        {
            if (_lastFastEma is null)
            {
                // Initialize as SMA
                fastEma = _fastBufferSum / FastPeriods;
            }
            else
            {
                // Calculate EMA normally
                fastEma = Ema.Increment(FastK, _lastFastEma.Value, value);
            }

            _lastFastEma = fastEma;
        }

        // Calculate Slow EMA
        double? slowEma = null;
        if (Count >= SlowPeriods - 1)
        {
            if (_lastSlowEma is null)
            {
                // Initialize as SMA
                slowEma = _slowBufferSum / SlowPeriods;
            }
            else
            {
                // Calculate EMA normally
                slowEma = Ema.Increment(SlowK, _lastSlowEma.Value, value);
            }

            _lastSlowEma = slowEma;
        }

        // Calculate MACD
        double? macd = null;
        if (fastEma.HasValue && slowEma.HasValue)
        {
            macd = fastEma.Value - slowEma.Value;
        }

        // Calculate Signal
        double? signal = null;
        if (macd.HasValue)
        {
            // Update MACD buffer for signal calculation using BufferListUtilities
            double? dequeuedMacd = _macdBuffer.UpdateWithDequeue(SignalPeriods, macd.Value);
            if (dequeuedMacd.HasValue)
            {
                _macdBufferSum = _macdBufferSum - dequeuedMacd.Value + macd.Value;
            }
            else
            {
                _macdBufferSum += macd.Value;
            }

            // Calculate signal line
            if (Count >= SlowPeriods + SignalPeriods - 2)
            {
                if (_lastSignalEma is null)
                {
                    // Initialize signal as SMA of MACD values
                    signal = _macdBufferSum / SignalPeriods;
                }
                else
                {
                    // Calculate signal EMA normally
                    signal = Ema.Increment(SignalK, _lastSignalEma.Value, macd.Value);
                }

                _lastSignalEma = signal;
            }
        }

        // Calculate Histogram
        double? histogram = null;
        if (macd.HasValue && signal.HasValue)
        {
            histogram = macd.Value - signal.Value;
        }

        // Add result
        MacdResult result = new(
            Timestamp: timestamp,
            Macd: macd,
            Signal: signal,
            Histogram: histogram,
            FastEma: fastEma,
            SlowEma: slowEma);

        AddInternal(result);
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
        _fastBuffer.Clear();
        _slowBuffer.Clear();
        _macdBuffer.Clear();
        _fastBufferSum = 0;
        _slowBufferSum = 0;
        _macdBufferSum = 0;
        _lastFastEma = null;
        _lastSlowEma = null;
        _lastSignalEma = null;
    }
}

public static partial class Macd
{
    /// <summary>
    /// Creates a MACD buffer list from reusable values.
    /// </summary>
    /// <param name="source">List of source data.</param>
    /// <param name="fastPeriods">Number of periods for the fast EMA. Default is 12.</param>
    /// <param name="slowPeriods">Number of periods for the slow EMA. Default is 26.</param>
    /// <param name="signalPeriods">Number of periods for the signal line. Default is 9.</param>
    /// <returns>A MACD buffer list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the source list is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any of the parameters are invalid.</exception>
    public static MacdList ToMacdList(
        this IReadOnlyList<IReusable> source,
        int fastPeriods = 12,
        int slowPeriods = 26,
        int signalPeriods = 9)
        => new(fastPeriods, slowPeriods, signalPeriods) { source };
}
