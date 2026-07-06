namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods for calculating the Klinger Volume Oscillator (KVO) indicator.
/// </summary>
public static partial class Kvo
{
    /// <summary>
    /// Creates a KVO streaming hub from a bar provider.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="fastPeriods">Number of periods for the fast EMA.</param>
    /// <param name="slowPeriods">Number of periods for the slow EMA.</param>
    /// <param name="signalPeriods">Number of periods for the signal line.</param>
    /// <returns>A KVO hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bar provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any of the parameters are invalid.</exception>
    public static KvoHub ToKvoHub(
        this IBarProvider<IBar> barProvider,
        int fastPeriods = 34,
        int slowPeriods = 55,
        int signalPeriods = 13)
        => new(barProvider, fastPeriods, slowPeriods, signalPeriods);
}

/// <summary>
/// Streaming hub for Klinger Volume Oscillator (KVO) calculations.
/// </summary>
public class KvoHub
    : ChainHub<IBar, KvoResult>, IKvo
{
    private readonly int _fastPeriods;
    private readonly int _slowPeriods;
    private readonly int _signalPeriods;
    private readonly double _kFast;
    private readonly double _kSlow;
    private readonly double _kSignal;

    /// <summary>
    /// State variables
    /// </summary>
    private double _prevHlc;
    private double _prevTrend;
    private double _prevDm;
    private double _prevCm;
    private double _prevVfFastEma;
    private double _prevVfSlowEma;
    private double _sumVf;

    // recent state snapshots for O(1) near-tail rollback
    private readonly RollbackRing<KvoState> _rollback = new();

    internal KvoHub(
        IBarProvider<IBar> provider,
        int fastPeriods,
        int slowPeriods,
        int signalPeriods) : base(provider)
    {
        Kvo.Validate(fastPeriods, slowPeriods, signalPeriods);

        _fastPeriods = fastPeriods;
        _slowPeriods = slowPeriods;
        _signalPeriods = signalPeriods;
        _kFast = 2d / (fastPeriods + 1);
        _kSlow = 2d / (slowPeriods + 1);
        _kSignal = 2d / (signalPeriods + 1);

        FastPeriods = fastPeriods;
        SlowPeriods = slowPeriods;
        SignalPeriods = signalPeriods;

        Name = $"KVO({fastPeriods},{slowPeriods},{signalPeriods})";

        // Validate cache size for warmup requirements
        // KVO needs the longer of fast/slow periods plus signal period
        int requiredWarmup = Math.Max(fastPeriods, slowPeriods) + signalPeriods + 1;
        ValidateCacheSize(requiredWarmup, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int FastPeriods { get; init; }

    /// <inheritdoc/>
    public int SlowPeriods { get; init; }

    /// <inheritdoc/>
    public int SignalPeriods { get; init; }
    /// <inheritdoc/>
    protected override (KvoResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        double high = (double)item.High;
        double low = (double)item.Low;
        double close = (double)item.Close;
        double volume = (double)item.Volume;

        double? kvo = null;
        double? sig = null;

        // trend basis
        double hlc = high + low + close;

        // daily measurement
        double dm = high - low;

        if (i <= 0)
        {
            _prevHlc = hlc;
            _rollback.Add(item.Timestamp, CaptureState());
            KvoResult r0 = new(item.Timestamp);
            return (r0, i);
        }

        // trend direction
        double trend = hlc > _prevHlc ? 1 : -1;

        if (i <= 1)
        {
            _prevHlc = hlc;
            _prevTrend = trend;
            _prevDm = dm;
            _prevCm = 0;
            _rollback.Add(item.Timestamp, CaptureState());
            KvoResult r1 = new(item.Timestamp);
            return (r1, i);
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
        if (i > 1)
        {
            _sumVf += vf;
        }

        // fast-period EMA of VF
        double vfFastEma = 0;
        if (i > _fastPeriods + 1)
        {
            vfFastEma = (vf * _kFast) + (_prevVfFastEma * (1 - _kFast));
        }
        else if (i == _fastPeriods + 1)
        {
            vfFastEma = _sumVf / _fastPeriods;
        }

        // slow-period EMA of VF
        double vfSlowEma = 0;
        if (i > _slowPeriods + 1)
        {
            vfSlowEma = (vf * _kSlow) + (_prevVfSlowEma * (1 - _kSlow));
        }
        else if (i == _slowPeriods + 1)
        {
            vfSlowEma = _sumVf / _slowPeriods;
        }

        // Klinger Oscillator
        if (i >= _slowPeriods + 1)
        {
            kvo = vfFastEma - vfSlowEma;

            // Signal
            if (i > _slowPeriods + _signalPeriods)
            {
                double prevSignal = Cache[i - 1].Signal ?? 0;
                sig = (kvo * _kSignal) + (prevSignal * (1 - _kSignal));
            }
            else if (i == _slowPeriods + _signalPeriods)
            {
                // Sum of KVO values for signal initialization
                double sum = 0;
                for (int p = _slowPeriods + 1; p < i; p++)
                {
                    sum += Cache[p].Oscillator ?? 0;
                }

                sum += kvo ?? 0;
                sig = sum / _signalPeriods;
            }
        }

        KvoResult r = new(
            Timestamp: item.Timestamp,
            Oscillator: kvo,
            Signal: sig);

        // Save state for next iteration
        _prevHlc = hlc;
        _prevTrend = trend;
        _prevDm = dm;
        _prevCm = cm;
        _prevVfFastEma = vfFastEma;
        _prevVfSlowEma = vfSlowEma;

        // snapshot state for O(1) near-tail rollback
        _rollback.Add(item.Timestamp, CaptureState());

        return (r, i);
    }

    /// <summary>
    /// Restore rolling state up to the specified index for accurate rebuilds.
    /// </summary>
    /// <param name="restoreIndex">Last ProviderCache index to preserve, or -1 to reset.</param>
    protected override void RollbackState(int restoreIndex)
    {
        // Reset all state
        _prevHlc = 0;
        _prevTrend = 0;
        _prevDm = 0;
        _prevCm = 0;
        _prevVfFastEma = 0;
        _prevVfSlowEma = 0;
        _sumVf = 0;

        if (restoreIndex < 0)
        {
            return;
        }

        // O(1) fast path: restore the exact state snapshot recorded when the
        // restore-point item was processed (near-tail rollbacks, the common
        // case for live corrections and forming-bar updates)
        if (_rollback.TryGet(
            ProviderCache[restoreIndex].Timestamp,
            out KvoState snapshot))
        {
            _prevHlc = snapshot.PrevHlc;
            _prevTrend = snapshot.PrevTrend;
            _prevDm = snapshot.PrevDm;
            _prevCm = snapshot.PrevCm;
            _prevVfFastEma = snapshot.PrevVfFastEma;
            _prevVfSlowEma = snapshot.PrevVfSlowEma;
            _sumVf = snapshot.SumVf;
            return;
        }

        // Rebuild state by recalculating all values up to restoreIndex
        for (int i = 0; i <= restoreIndex; i++)
        {
            IBar item = ProviderCache[i];
            double high = (double)item.High;
            double low = (double)item.Low;
            double close = (double)item.Close;
            double volume = (double)item.Volume;

            // trend basis
            double hlc = high + low + close;

            // daily measurement
            double dm = high - low;

            if (i == 0)
            {
                _prevHlc = hlc;
                continue;
            }

            // trend direction
            double trend = hlc > _prevHlc ? 1 : -1;

            if (i == 1)
            {
                _prevHlc = hlc;
                _prevTrend = trend;
                _prevDm = dm;
                _prevCm = 0;
                continue;
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
            if (i > 1)
            {
                _sumVf += vf;
            }

            // fast-period EMA of VF
            double vfFastEma = 0;
            if (i > _fastPeriods + 1)
            {
                vfFastEma = (vf * _kFast) + (_prevVfFastEma * (1 - _kFast));
            }
            else if (i == _fastPeriods + 1)
            {
                vfFastEma = _sumVf / _fastPeriods;
            }

            // slow-period EMA of VF
            double vfSlowEma = 0;
            if (i > _slowPeriods + 1)
            {
                vfSlowEma = (vf * _kSlow) + (_prevVfSlowEma * (1 - _kSlow));
            }
            else if (i == _slowPeriods + 1)
            {
                vfSlowEma = _sumVf / _slowPeriods;
            }

            // Save state for next iteration
            _prevHlc = hlc;
            _prevTrend = trend;
            _prevDm = dm;
            _prevCm = cm;
            _prevVfFastEma = vfFastEma;
            _prevVfSlowEma = vfSlowEma;
        }
    }

    // Captures the current scalar state for rollback snapshots.
    private KvoState CaptureState()
        => new(
            _prevHlc,
            _prevTrend,
            _prevDm,
            _prevCm,
            _prevVfFastEma,
            _prevVfSlowEma,
            _sumVf);

    // Scalar state snapshot for O(1) near-tail rollback.
    private readonly record struct KvoState(
        double PrevHlc,
        double PrevTrend,
        double PrevDm,
        double PrevCm,
        double PrevVfFastEma,
        double PrevVfSlowEma,
        double SumVf);
}
