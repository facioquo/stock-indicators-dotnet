namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Klinger Volume Oscillator (KVO) from incremental bar values.
/// </summary>
public class KvoList : BufferList<KvoResult>, IIncrementFromBar, IKvo
{
    private readonly int _fastPeriods;
    private readonly int _slowPeriods;
    private readonly double _kFast;
    private readonly double _kSlow;
    private readonly double _kSignal;

    private double _prevHlc;
    private double _prevTrend;
    private double _prevDm;
    private double _prevCm;
    private double _prevVfFastEma;
    private double _prevVfSlowEma;
    private double _sumVf;

    // oscillators that seed the signal, kept here because MaxListSize can prune them from the list
    private readonly List<double> _signalSeed = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="KvoList"/> class.
    /// </summary>
    /// <param name="fastPeriods">Number of periods for the fast EMA.</param>
    /// <param name="slowPeriods">Number of periods for the slow EMA.</param>
    /// <param name="signalPeriods">Number of periods for the signal line.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="signalPeriods"/> is invalid.</exception>
    public KvoList(
        int fastPeriods = 34,
        int slowPeriods = 55,
        int signalPeriods = 13)
    {
        Kvo.Validate(fastPeriods, slowPeriods, signalPeriods);
        FastPeriods = fastPeriods;
        SlowPeriods = slowPeriods;
        SignalPeriods = signalPeriods;

        _fastPeriods = fastPeriods;
        _slowPeriods = slowPeriods;
        _kFast = 2d / (fastPeriods + 1);
        _kSlow = 2d / (slowPeriods + 1);
        _kSignal = 2d / (signalPeriods + 1);

        _prevHlc = 0;
        _prevTrend = 0;
        _prevDm = 0;
        _prevCm = 0;
        _prevVfFastEma = 0;
        _prevVfSlowEma = 0;
        _sumVf = 0;

        Name = $"KVO({fastPeriods}, {slowPeriods}, {signalPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KvoList"/> class with initial bars.
    /// </summary>
    /// <param name="fastPeriods">Number of periods for the fast EMA.</param>
    /// <param name="slowPeriods">Number of periods for the slow EMA.</param>
    /// <param name="signalPeriods">Number of periods for the signal line.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public KvoList(
        int fastPeriods,
        int slowPeriods,
        int signalPeriods,
        IReadOnlyList<IBar> bars)
        : this(fastPeriods, slowPeriods, signalPeriods) => Add(bars);

    /// <inheritdoc />
    public int FastPeriods { get; init; }

    /// <inheritdoc />
    public int SlowPeriods { get; init; }

    /// <inheritdoc />
    public int SignalPeriods { get; init; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        double high = (double)bar.High;
        double low = (double)bar.Low;
        double close = (double)bar.Close;
        double volume = (double)bar.Volume;

        double? kvo = null;
        double? sig = null;

        // trend basis
        double hlc = high + low + close;

        // daily measurement
        double dm = high - low;

        if (TotalCount <= 0)
        {
            _prevHlc = hlc;
            AddInternal(new KvoResult(bar.Timestamp));
            return;
        }

        // trend direction
        double trend = hlc > _prevHlc ? 1 : -1;

        if (TotalCount <= 1)
        {
            _prevHlc = hlc;
            _prevTrend = trend;
            _prevDm = dm;
            _prevCm = 0;
            AddInternal(new KvoResult(bar.Timestamp));
            return;
        }

        // cumulative measurement
        double cm = trend == _prevTrend ?
                _prevCm + dm : _prevDm + dm;

        // volume force (VF)
        double vf = dm == cm || volume == 0 ? 0
            : dm == 0 ? volume * 2d * trend * 100d
            : cm != 0 ? volume * Math.Abs(2d * ((dm / cm) - 1)) * trend * 100d
            : 0;

        // accumulate VF for EMA initialization (starting from period 2)
        if (TotalCount > 1)
        {
            _sumVf += vf;
        }

        // fast-period EMA of VF
        double vfFastEma = 0;
        if (TotalCount > _fastPeriods + 1)
        {
            vfFastEma = (vf * _kFast) + (_prevVfFastEma * (1 - _kFast));
        }
        else if (TotalCount == _fastPeriods + 1)
        {
            vfFastEma = _sumVf / _fastPeriods;
        }

        // slow-period EMA of VF
        double vfSlowEma = 0;
        if (TotalCount > _slowPeriods + 1)
        {
            vfSlowEma = (vf * _kSlow) + (_prevVfSlowEma * (1 - _kSlow));
        }
        else if (TotalCount == _slowPeriods + 1)
        {
            vfSlowEma = _sumVf / _slowPeriods;
        }

        // Klinger Oscillator
        if (TotalCount >= _slowPeriods + 1)
        {
            kvo = vfFastEma - vfSlowEma;

            // Signal
            if (TotalCount > _slowPeriods + SignalPeriods)
            {
                double prevSignal = this[^1].Signal ?? 0;
                sig = (kvo * _kSignal) + (prevSignal * (1 - _kSignal));
            }
            else if (TotalCount == _slowPeriods + SignalPeriods)
            {
                // current value first, then the earlier ones: Series' order, so the sum rounds the same
                double sum = kvo ?? 0;
                foreach (double oscillator in _signalSeed)
                {
                    sum += oscillator;
                }

                sig = sum / SignalPeriods;
                _signalSeed.Clear();
            }
            else
            {
                _signalSeed.Add(kvo ?? 0);
            }
        }

        AddInternal(new KvoResult(
            Timestamp: bar.Timestamp,
            Oscillator: kvo,
            Signal: sig));

        // Save state for next iteration
        _prevHlc = hlc;
        _prevTrend = trend;
        _prevDm = dm;
        _prevCm = cm;
        _prevVfFastEma = vfFastEma;
        _prevVfSlowEma = vfSlowEma;
    }

    /// <inheritdoc />
    public void Add(IReadOnlyList<IBar> bars)
    {
        ArgumentNullException.ThrowIfNull(bars);

        for (int i = 0; i < bars.Count; i++)
        {
            Add(bars[i]);
        }
    }

    /// <inheritdoc />
    public override void Clear()
    {
        base.Clear();
        _prevHlc = 0;
        _prevTrend = 0;
        _prevDm = 0;
        _prevCm = 0;
        _prevVfFastEma = 0;
        _prevVfSlowEma = 0;
        _sumVf = 0;
        _signalSeed.Clear();
    }
}

public static partial class Kvo
{
    /// <summary>
    /// Creates a buffer list for Klinger Volume Oscillator (KVO) calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="fastPeriods">Number of periods for the fast moving average</param>
    /// <param name="slowPeriods">Number of periods for the slow moving average</param>
    /// <param name="signalPeriods">Number of periods for the signal line</param>
    public static KvoList ToKvoList(
        this IReadOnlyList<IBar> bars,
        int fastPeriods = 34,
        int slowPeriods = 55,
        int signalPeriods = 13)
        => new(fastPeriods, slowPeriods, signalPeriods) { bars };
}
