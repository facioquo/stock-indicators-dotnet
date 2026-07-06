namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Percentage Volume Oscillator (PVO) from incremental bars.
/// </summary>
public class PvoList : BufferList<PvoResult>, IIncrementFromBar, IPvo
{
    private readonly Queue<double> _fastBuffer;
    private readonly Queue<double> _slowBuffer;
    private double _fastBufferSum;
    private double _slowBufferSum;

    private double? _lastFastEma;
    private double? _lastSlowEma;
    private double _lastSignalEma = double.NaN;

    /// <summary>
    /// Initializes a new instance of the <see cref="PvoList"/> class.
    /// </summary>
    /// <param name="fastPeriods">Number of periods for the fast EMA.</param>
    /// <param name="slowPeriods">Number of periods for the slow EMA.</param>
    /// <param name="signalPeriods">Number of periods for the signal line.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="signalPeriods"/> is invalid.</exception>
    public PvoList(
        int fastPeriods = 12,
        int slowPeriods = 26,
        int signalPeriods = 9)
    {
        Pvo.Validate(fastPeriods, slowPeriods, signalPeriods);
        FastPeriods = fastPeriods;
        SlowPeriods = slowPeriods;
        SignalPeriods = signalPeriods;

        FastK = 2d / (fastPeriods + 1);
        SlowK = 2d / (slowPeriods + 1);
        SignalK = 2d / (signalPeriods + 1);

        _fastBuffer = new Queue<double>(fastPeriods);
        _slowBuffer = new Queue<double>(slowPeriods);

        _fastBufferSum = 0;
        _slowBufferSum = 0;

        Name = $"PVO({fastPeriods}, {slowPeriods}, {signalPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PvoList"/> class with initial bars.
    /// </summary>
    /// <param name="fastPeriods">Number of periods for the fast EMA.</param>
    /// <param name="slowPeriods">Number of periods for the slow EMA.</param>
    /// <param name="signalPeriods">Number of periods for the signal line.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public PvoList(
        int fastPeriods,
        int slowPeriods,
        int signalPeriods,
        IReadOnlyList<IBar> bars)
        : this(fastPeriods, slowPeriods, signalPeriods) => Add(bars);

    /// <inheritdoc/>
    public int FastPeriods { get; init; }

    /// <inheritdoc/>
    public int SlowPeriods { get; init; }

    /// <inheritdoc/>
    public int SignalPeriods { get; init; }

    /// <inheritdoc />
    private double FastK { get; init; }

    /// <inheritdoc />
    private double SlowK { get; init; }

    /// <inheritdoc />
    private double SignalK { get; init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // Calculate fast EMA
        double? fastEma;
        if (_lastFastEma == null && _fastBuffer.Count == FastPeriods - 1)
        {
            // Initialize fast EMA with SMA
            _fastBufferSum += value;
            _fastBuffer.Update(FastPeriods, value);
            fastEma = _fastBufferSum / FastPeriods;
        }
        else if (_lastFastEma == null)
        {
            // Not enough data yet
            _fastBufferSum += value;
            _fastBuffer.Update(FastPeriods, value);
            fastEma = null;
        }
        else
        {
            // Calculate fast EMA
            fastEma = Ema.Increment(FastK, _lastFastEma.Value, value);
        }

        _lastFastEma = fastEma;

        // Calculate slow EMA
        double? slowEma;
        if (_lastSlowEma == null && _slowBuffer.Count == SlowPeriods - 1)
        {
            // Initialize slow EMA with SMA
            _slowBufferSum += value;
            _slowBuffer.Update(SlowPeriods, value);
            slowEma = _slowBufferSum / SlowPeriods;
        }
        else if (_lastSlowEma == null)
        {
            // Not enough data yet
            _slowBufferSum += value;
            _slowBuffer.Update(SlowPeriods, value);
            slowEma = null;
        }
        else
        {
            // Calculate slow EMA
            slowEma = Ema.Increment(SlowK, _lastSlowEma.Value, value);
        }

        _lastSlowEma = slowEma;

        // Calculate PVO
        double? pvo = null;
        if (fastEma.HasValue && slowEma.HasValue && slowEma.Value != 0)
        {
            pvo = 100 * ((fastEma.Value - slowEma.Value) / slowEma.Value);
        }

        // Calculate signal line (EMA of PVO)
        double signal;
        if (pvo.HasValue)
        {
            if (double.IsNaN(_lastSignalEma) && Count >= SignalPeriods + SlowPeriods - 2)
            {
                // Initialize signal EMA with SMA of PVO values from results
                double sum = pvo.Value;
                for (int p = Count - SignalPeriods + 1; p < Count; p++)
                {
                    sum += this[p].Value;
                }

                signal = sum / SignalPeriods;
            }
            else
            {
                // Calculate signal EMA
                signal = Ema.Increment(SignalK, _lastSignalEma, pvo.Value);
            }

            _lastSignalEma = signal;
        }
        else
        {
            signal = double.NaN;
        }

        // Calculate histogram
        double? histogram = (pvo - signal).NaN2Null();

        PvoResult result = new(
            Timestamp: timestamp,
            Pvo: pvo,
            Signal: signal.NaN2Null(),
            Histogram: histogram);

        AddInternal(result);
    }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);
        Add(bar.Timestamp, (double)bar.Volume);
    }

    /// <inheritdoc />
    public void Add(IReadOnlyList<IBar> bars)
    {
        ArgumentNullException.ThrowIfNull(bars);

        for (int i = 0; i < bars.Count; i++)
        {
            Add(bars[i].Timestamp, (double)bars[i].Volume);
        }
    }

    /// <inheritdoc />
    public override void Clear()
    {
        base.Clear();
        _fastBuffer.Clear();
        _slowBuffer.Clear();
        _fastBufferSum = 0;
        _slowBufferSum = 0;
        _lastFastEma = null;
        _lastSlowEma = null;
        _lastSignalEma = double.NaN;
    }
}

public static partial class Pvo
{
    /// <summary>
    /// Creates a buffer list for Percentage Volume Oscillator (PVO) calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="fastPeriods">Number of periods for the fast moving average</param>
    /// <param name="slowPeriods">Number of periods for the slow moving average</param>
    /// <param name="signalPeriods">Number of periods for the signal line</param>
    public static PvoList ToPvoList(
        this IReadOnlyList<IBar> bars,
        int fastPeriods = 12,
        int slowPeriods = 26,
        int signalPeriods = 9)
        => new(fastPeriods, slowPeriods, signalPeriods) { bars };
}
