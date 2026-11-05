namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Choppiness Index (CHOP) from incremental bars.
/// </summary>
public class ChopList : BufferList<ChopResult>, IIncrementFromBar, IChop
{
    // ring of the lookback window, so it can be read newest first as the Series does
    private readonly (double TrueHigh, double TrueLow, double TrueRange)[] _window;
    private int _head;
    private int _fill;
    private double _previousClose;
    private bool _isInitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChopList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public ChopList(int lookbackPeriods = 14)
    {
        Chop.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;

        _window = new (double, double, double)[lookbackPeriods];
        _isInitialized = false;

        Name = $"CHOP({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChopList"/> class with initial bars.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public ChopList(int lookbackPeriods, IReadOnlyList<IBar> bars)
        : this(lookbackPeriods) => Add(bars);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        DateTime timestamp = bar.Timestamp;
        double high = (double)bar.High;
        double low = (double)bar.Low;
        double close = (double)bar.Close;

        double? chop = null;

        if (!_isInitialized)
        {
            _previousClose = close;
            _isInitialized = true;
        }
        else
        {
            // Calculate true high, true low, and true range
            double trueHigh = Math.Max(high, _previousClose);
            double trueLow = Math.Min(low, _previousClose);
            double trueRange = trueHigh - trueLow;

            _window[_head] = (trueHigh, trueLow, trueRange);
            if (++_head == LookbackPeriods)
            {
                _head = 0;
            }

            if (_fill < LookbackPeriods)
            {
                _fill++;
            }

            // Calculate CHOP when we have enough data
            if (_fill == LookbackPeriods)
            {
                // read newest first, as the Series does, so the sum rounds the same
                double sumTrueRange = trueRange;
                double maxTrueHigh = trueHigh;
                double minTrueLow = trueLow;
                int index = _head == 0 ? LookbackPeriods - 1 : _head - 1;

                for (int n = 1; n < LookbackPeriods; n++)
                {
                    index = (index == 0 ? LookbackPeriods : index) - 1;
                    (double TrueHigh, double TrueLow, double TrueRange) = _window[index];

                    sumTrueRange += TrueRange;
                    if (TrueHigh > maxTrueHigh)
                    {
                        maxTrueHigh = TrueHigh;
                    }

                    if (TrueLow < minTrueLow)
                    {
                        minTrueLow = TrueLow;
                    }
                }

                double range = maxTrueHigh - minTrueLow;

                if (range != 0)
                {
                    chop = 100 * (Math.Log(sumTrueRange / range) / Math.Log(LookbackPeriods));
                }
            }

            _previousClose = close;
        }

        AddInternal(new ChopResult(timestamp, chop));
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
        _head = 0;
        _fill = 0;
        _previousClose = 0;
        _isInitialized = false;
    }
}

public static partial class Chop
{
    /// <summary>
    /// Creates a buffer list for Choppiness Index (CHOP) calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    public static ChopList ToChopList(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14)
        => new(lookbackPeriods) { bars };
}
