namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Price Momentum Oscillator (PMO) from incremental reusable values.
/// </summary>
public class PmoList : BufferList<PmoResult>, IIncrementFromChain, IPmo
{
    private readonly double _smoothingConstant1;
    private readonly double _smoothingConstant2;
    private readonly double _smoothingConstant3;

    private double _prevPrice = double.NaN;
    private double _prevRocEma = double.NaN;
    private double _prevPmo = double.NaN;
    private double _prevSignal = double.NaN;

    private readonly List<double> _rocHistory;
    private readonly List<double> _rocEmaHistory;
    private readonly List<double> _pmoHistory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PmoList"/> class.
    /// </summary>
    /// <param name="timePeriods">Number of periods for the time span.</param>
    /// <param name="smoothPeriods">Number of periods for smoothing.</param>
    /// <param name="signalPeriods">Number of periods for the signal line.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="signalPeriods"/> is invalid.</exception>
    public PmoList(
        int timePeriods = 35,
        int smoothPeriods = 20,
        int signalPeriods = 10)
    {
        Pmo.Validate(timePeriods, smoothPeriods, signalPeriods);

        TimePeriods = timePeriods;
        SmoothPeriods = smoothPeriods;
        SignalPeriods = signalPeriods;

        _smoothingConstant1 = 2d / smoothPeriods;
        _smoothingConstant2 = 2d / timePeriods;
        _smoothingConstant3 = 2d / (signalPeriods + 1);

        _rocHistory = [];
        _rocEmaHistory = [];
        _pmoHistory = [];

        Name = $"PMO({timePeriods}, {smoothPeriods}, {signalPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PmoList"/> class with initial reusable values.
    /// </summary>
    /// <param name="timePeriods">Number of periods for the time span.</param>
    /// <param name="smoothPeriods">Number of periods for smoothing.</param>
    /// <param name="signalPeriods">Number of periods for the signal line.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    public PmoList(
        int timePeriods,
        int smoothPeriods,
        int signalPeriods,
        IReadOnlyList<IReusable> values)
        : this(timePeriods, smoothPeriods, signalPeriods) => Add(values);

    /// <inheritdoc />
    public int TimePeriods { get; }

    /// <inheritdoc />
    public int SmoothPeriods { get; }

    /// <inheritdoc />
    public int SignalPeriods { get; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // Calculate rate of change (ROC)
        double roc = _prevPrice == 0 ? double.NaN : 100 * ((value / _prevPrice) - 1);
        _prevPrice = value;
        _rocHistory.Add(roc);

        // Calculate ROC smoothed moving average
        double rocEma;
        if (double.IsNaN(_prevRocEma) && _rocHistory.Count >= TimePeriods)
        {
            double sum = 0;
            for (int p = _rocHistory.Count - TimePeriods; p < _rocHistory.Count; p++)
            {
                sum += _rocHistory[p];
            }

            rocEma = sum / TimePeriods;
        }
        else
        {
            rocEma = _prevRocEma + (_smoothingConstant2 * (roc - _prevRocEma));
        }

        double rocEmaScaled = rocEma * 10;
        _rocEmaHistory.Add(rocEmaScaled);
        _prevRocEma = rocEma;

        // Calculate price momentum oscillator
        double pmo;
        if (double.IsNaN(_prevPmo) && _rocEmaHistory.Count >= SmoothPeriods)
        {
            double sum = 0;
            for (int p = _rocEmaHistory.Count - SmoothPeriods; p < _rocEmaHistory.Count; p++)
            {
                sum += _rocEmaHistory[p];
            }

            pmo = sum / SmoothPeriods;
        }
        else
        {
            pmo = _prevPmo + (_smoothingConstant1 * (rocEmaScaled - _prevPmo));
        }

        _pmoHistory.Add(pmo);
        _prevPmo = pmo;

        // Calculate signal (EMA of PMO)
        double signal;
        if (double.IsNaN(_prevSignal) && _pmoHistory.Count >= SignalPeriods)
        {
            double sum = 0;
            for (int p = _pmoHistory.Count - SignalPeriods; p < _pmoHistory.Count; p++)
            {
                sum += _pmoHistory[p];
            }

            signal = sum / SignalPeriods;
        }
        else
        {
            signal = Ema.Increment(_smoothingConstant3, _prevSignal, pmo);
        }

        _prevSignal = signal;

        PmoResult result = new(
            Timestamp: timestamp,
            Pmo: pmo.NaN2Null(),
            Signal: signal.NaN2Null());

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
        _rocHistory.Clear();
        _rocEmaHistory.Clear();
        _pmoHistory.Clear();
        _prevPrice = double.NaN;
        _prevRocEma = double.NaN;
        _prevPmo = double.NaN;
        _prevSignal = double.NaN;
    }
}

public static partial class Pmo
{
    /// <summary>
    /// Creates a buffer list for Price Momentum Oscillator (PMO) calculations.
    /// </summary>
    /// <param name="source">Collection of input values, time sorted.</param>
    /// <param name="timePeriods">Number of time periods</param>
    /// <param name="smoothPeriods">Number of periods for smoothing</param>
    /// <param name="signalPeriods">Number of periods for the signal line</param>
    public static PmoList ToPmoList(
        this IReadOnlyList<IReusable> source,
        int timePeriods = 35,
        int smoothPeriods = 20,
        int signalPeriods = 10)
        => new(timePeriods, smoothPeriods, signalPeriods) { source };
}
