namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Vortex indicator from incremental bars.
/// </summary>
public class VortexList : BufferList<VortexResult>, IIncrementFromBar
{
    private readonly Queue<(double Tr, double Pvm, double Nvm)> _buffer;
    private double _prevHigh;
    private double _prevLow;
    private double _prevClose;
    private bool _isInitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="VortexList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public VortexList(int lookbackPeriods = 14)
    {
        Vortex.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        _buffer = new Queue<(double, double, double)>(lookbackPeriods);
        _isInitialized = false;

        Name = $"VORTEX({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VortexList"/> class with initial bars.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public VortexList(int lookbackPeriods, IReadOnlyList<IBar> bars)
        : this(lookbackPeriods) => Add(bars);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        double high = (double)bar.High;
        double low = (double)bar.Low;
        double close = (double)bar.Close;

        // Handle first period - no calculations possible
        if (!_isInitialized)
        {
            _prevHigh = high;
            _prevLow = low;
            _prevClose = close;
            _isInitialized = true;

            AddInternal(new VortexResult(bar.Timestamp));
            return;
        }

        // Calculate trend information
        double highMinusPrevClose = Math.Abs(high - _prevClose);
        double lowMinusPrevClose = Math.Abs(low - _prevClose);

        double tr = Math.Max(high - low, Math.Max(highMinusPrevClose, lowMinusPrevClose));
        double pvm = Math.Abs(high - _prevLow);
        double nvm = Math.Abs(low - _prevHigh);

        // Update buffer
        _buffer.Update(LookbackPeriods, (tr, pvm, nvm));

        double? pvi = null;
        double? nvi = null;

        // Calculate vortex indicator when we have enough data
        if (_buffer.Count == LookbackPeriods)
        {
            double sumTr = 0;
            double sumPvm = 0;
            double sumNvm = 0;

            foreach ((double Tr, double Pvm, double Nvm) item in _buffer)
            {
                sumTr += item.Tr;
                sumPvm += item.Pvm;
                sumNvm += item.Nvm;
            }

            if (sumTr is not 0)
            {
                pvi = sumPvm / sumTr;
                nvi = sumNvm / sumTr;
            }
        }

        AddInternal(new VortexResult(
            Timestamp: bar.Timestamp,
            Pvi: pvi,
            Nvi: nvi));

        _prevHigh = high;
        _prevLow = low;
        _prevClose = close;
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
        _buffer.Clear();
        _prevHigh = 0;
        _prevLow = 0;
        _prevClose = 0;
        _isInitialized = false;
    }
}

public static partial class Vortex
{
    /// <summary>
    /// Creates a buffer list for Vortex calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    public static VortexList ToVortexList(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14)
        => new(lookbackPeriods) { bars };
}
