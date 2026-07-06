namespace FacioQuo.Stock.Indicators;

/// <summary>
/// True Strength Index (TSI) from incremental reusable values.
/// </summary>
public class TsiList : BufferList<TsiResult>, IIncrementFromChain, ITsi
{
    private readonly double _mult1;
    private readonly double _mult2;
    private readonly double _multS;

    private double _prevValue = double.NaN;
    private double _prevCs1 = double.NaN;
    private double _prevAs1 = double.NaN;
    private double _prevCs2 = double.NaN;
    private double _prevAs2 = double.NaN;
    private double _prevSignal = double.NaN;

    private readonly List<double> _changeHistory;
    private readonly List<double> _absChangeHistory;
    private readonly List<double> _cs1History;
    private readonly List<double> _as1History;
    private readonly List<double> _tsiHistory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TsiList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Number of periods for the lookback calculation.</param>
    /// <param name="smoothPeriods">Number of periods for the smoothing calculation.</param>
    /// <param name="signalPeriods">Number of periods for the signal calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="signalPeriods"/> is invalid.</exception>
    public TsiList(
        int lookbackPeriods = 25,
        int smoothPeriods = 13,
        int signalPeriods = 7)
    {
        Tsi.Validate(lookbackPeriods, smoothPeriods, signalPeriods);

        LookbackPeriods = lookbackPeriods;
        SmoothPeriods = smoothPeriods;
        SignalPeriods = signalPeriods;

        _mult1 = 2d / (lookbackPeriods + 1);
        _mult2 = 2d / (smoothPeriods + 1);
        _multS = 2d / (signalPeriods + 1);

        _changeHistory = [];
        _absChangeHistory = [];
        _cs1History = [];
        _as1History = [];
        _tsiHistory = [];

        Name = $"TSI({lookbackPeriods}, {smoothPeriods}, {signalPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TsiList"/> class with initial reusable values.
    /// </summary>
    /// <param name="lookbackPeriods">Number of periods for the lookback calculation.</param>
    /// <param name="smoothPeriods">Number of periods for the smoothing calculation.</param>
    /// <param name="signalPeriods">Number of periods for the signal calculation.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    public TsiList(
        int lookbackPeriods,
        int smoothPeriods,
        int signalPeriods,
        IReadOnlyList<IReusable> values)
        : this(lookbackPeriods, smoothPeriods, signalPeriods) => Add(values);

    /// <inheritdoc />
    public int LookbackPeriods { get; }

    /// <inheritdoc />
    public int SmoothPeriods { get; }

    /// <inheritdoc />
    public int SignalPeriods { get; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // Skip first period
        if (double.IsNaN(_prevValue))
        {
            _prevValue = value;
            _changeHistory.Add(double.NaN);
            _absChangeHistory.Add(double.NaN);
            AddInternal(new TsiResult(timestamp));
            return;
        }

        // Calculate price change
        double c = value - _prevValue;
        double a = Math.Abs(c);
        _prevValue = value;

        _changeHistory.Add(c);
        _absChangeHistory.Add(a);

        // Calculate first smoothing (EMA)
        double cs1 = double.NaN;
        double as1 = double.NaN;

        if (double.IsNaN(_prevCs1) && _changeHistory.Count >= LookbackPeriods)
        {
            // Initialize first smoothing with SMA
            double sumC = 0;
            double sumA = 0;

            for (int p = _changeHistory.Count - LookbackPeriods; p < _changeHistory.Count; p++)
            {
                sumC += _changeHistory[p];
                sumA += _absChangeHistory[p];
            }

            cs1 = sumC / LookbackPeriods;
            as1 = sumA / LookbackPeriods;
            _prevCs1 = cs1;
            _prevAs1 = as1;
        }
        else if (!double.IsNaN(_prevCs1))
        {
            // Continue with EMA
            cs1 = ((c - _prevCs1) * _mult1) + _prevCs1;
            as1 = ((a - _prevAs1) * _mult1) + _prevAs1;
            _prevCs1 = cs1;
            _prevAs1 = as1;
        }

        _cs1History.Add(cs1);
        _as1History.Add(as1);

        // Calculate second smoothing (EMA of first EMA)
        double cs2 = double.NaN;
        double as2 = double.NaN;

        if (double.IsNaN(_prevCs2) && !double.IsNaN(cs1) && _cs1History.Count >= SmoothPeriods)
        {
            // Initialize second smoothing with SMA
            double sumCs = 0;
            double sumAs = 0;

            for (int p = _cs1History.Count - SmoothPeriods; p < _cs1History.Count; p++)
            {
                sumCs += _cs1History[p];
                sumAs += _as1History[p];
            }

            cs2 = sumCs / SmoothPeriods;
            as2 = sumAs / SmoothPeriods;
            _prevCs2 = cs2;
            _prevAs2 = as2;
        }
        else if (!double.IsNaN(_prevCs2) && !double.IsNaN(cs1))
        {
            // Continue with EMA
            cs2 = ((cs1 - _prevCs2) * _mult2) + _prevCs2;
            as2 = ((as1 - _prevAs2) * _mult2) + _prevAs2;
            _prevCs2 = cs2;
            _prevAs2 = as2;
        }

        // Calculate TSI
        double tsi = as2 != 0
            ? 100d * (cs2 / as2)
            : double.NaN;

        _tsiHistory.Add(tsi);

        // Calculate signal line (EMA of TSI)
        double signal;

        if (SignalPeriods > 1)
        {
            if (double.IsNaN(_prevSignal) && Count > SignalPeriods && !double.IsNaN(tsi))
            {
                // Check if we have enough non-NaN TSI values (last signalPeriods values including current)
                bool hasEnoughValues = _tsiHistory.Count >= SignalPeriods;

                if (hasEnoughValues)
                {
                    for (int p = _tsiHistory.Count - SignalPeriods; p < _tsiHistory.Count; p++)
                    {
                        if (double.IsNaN(_tsiHistory[p]))
                        {
                            hasEnoughValues = false;
                            break;
                        }
                    }
                }

                if (hasEnoughValues)
                {
                    // Initialize signal with SMA matching Series implementation order
                    // Add current TSI first, then previous values
                    double sum = tsi;
                    for (int p = _tsiHistory.Count - SignalPeriods; p < _tsiHistory.Count - 1; p++)
                    {
                        sum += _tsiHistory[p];
                    }

                    signal = sum / SignalPeriods;
                }
                else
                {
                    signal = double.NaN;
                }
            }
            else if (!double.IsNaN(_prevSignal) && !double.IsNaN(tsi))
            {
                // Continue with EMA
                signal = ((tsi - _prevSignal) * _multS) + _prevSignal;
            }
            else
            {
                signal = double.NaN;
            }
        }
        else
        {
            signal = SignalPeriods == 1
                ? tsi
                : double.NaN;
        }

        _prevSignal = signal;

        TsiResult result = new(
            Timestamp: timestamp,
            Tsi: tsi.NaN2Null(),
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
        _changeHistory.Clear();
        _absChangeHistory.Clear();
        _cs1History.Clear();
        _as1History.Clear();
        _tsiHistory.Clear();
        _prevValue = double.NaN;
        _prevCs1 = double.NaN;
        _prevAs1 = double.NaN;
        _prevCs2 = double.NaN;
        _prevAs2 = double.NaN;
        _prevSignal = double.NaN;
    }
    /// <inheritdoc />
    protected override void PruneList()
    {
        int overflow = Count - MaxListSize;

        if (overflow > 0)
        {
            // Maintain minimum buffer size for calculations
            int minBufferSize = LookbackPeriods + SmoothPeriods;
            int removable = Math.Min(overflow, Math.Max(0, _changeHistory.Count - minBufferSize));

            if (removable > 0)
            {
                RemoveStateRange(removable);
            }
        }

        base.PruneList();
    }

    private void RemoveStateRange(int count)
    {
        _changeHistory.RemoveRange(0, count);
        _absChangeHistory.RemoveRange(0, count);
        _cs1History.RemoveRange(0, count);
        _as1History.RemoveRange(0, count);
        _tsiHistory.RemoveRange(0, count);
    }
}

public static partial class Tsi
{
    /// <summary>
    /// Creates a buffer list for True Strength Index (TSI) calculations.
    /// </summary>
    /// <param name="source">Collection of input values, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="smoothPeriods">Number of periods for smoothing</param>
    /// <param name="signalPeriods">Number of periods for the signal line</param>
    public static TsiList ToTsiList(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods = 25,
        int smoothPeriods = 13,
        int signalPeriods = 7)
        => new(lookbackPeriods, smoothPeriods, signalPeriods) { source };
}
