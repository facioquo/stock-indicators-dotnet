namespace FacioQuo.Stock.Indicators;

/// <summary>
/// SuperTrend indicator from incremental bars.
/// </summary>
public class SuperTrendList : BufferList<SuperTrendResult>, IIncrementFromBar
{
    private readonly AtrList _atrList;
    private readonly double _multiplier;
    private bool _isBullish = true;
    private double? _upperBand;
    private double? _lowerBand;
    private double? _prevClose;
    private bool _isInitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="SuperTrendList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="multiplier">Multiplier for the ATR.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="multiplier"/> is invalid.</exception>
    public SuperTrendList(int lookbackPeriods = 10, double multiplier = 3)
    {
        SuperTrend.Validate(lookbackPeriods, multiplier);
        LookbackPeriods = lookbackPeriods;
        Multiplier = multiplier;

        _multiplier = multiplier;
        _atrList = new AtrList(lookbackPeriods);

        Name = $"SUPERTREND({lookbackPeriods}, {multiplier})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SuperTrendList"/> class with initial bars.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="multiplier">Multiplier for the ATR.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public SuperTrendList(int lookbackPeriods, double multiplier, IReadOnlyList<IBar> bars)
        : this(lookbackPeriods, multiplier) => Add(bars);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public double Multiplier { get; init; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        // Add to ATR list
        _atrList.Add(bar);

        double? superTrend;
        double? upperOnly;
        double? lowerOnly;

        // Calculate when we have enough data
        if (Count >= LookbackPeriods)
        {
            double? mid = ((double)bar.High + (double)bar.Low) / 2;
            double? atr = _atrList[^1].Atr;

            // potential bands
            double? upperEval = mid + (_multiplier * atr);
            double? lowerEval = mid - (_multiplier * atr);

            // initial values
            if (!_isInitialized)
            {
                _isBullish = (double)bar.Close >= mid;
                _upperBand = upperEval;
                _lowerBand = lowerEval;
                _isInitialized = true;
            }
            else
            {
                // new upper band
                if (upperEval < _upperBand || _prevClose > _upperBand)
                {
                    _upperBand = upperEval;
                }

                // new lower band
                if (lowerEval > _lowerBand || _prevClose < _lowerBand)
                {
                    _lowerBand = lowerEval;
                }
            }

            // supertrend
            if ((double)bar.Close <= (_isBullish ? _lowerBand : _upperBand))
            {
                superTrend = _upperBand;
                upperOnly = _upperBand;
                lowerOnly = null;
                _isBullish = false;
            }
            else
            {
                superTrend = _lowerBand;
                lowerOnly = _lowerBand;
                upperOnly = null;
                _isBullish = true;
            }

            _prevClose = (double)bar.Close;
        }
        else
        {
            superTrend = null;
            upperOnly = null;
            lowerOnly = null;
        }

        AddInternal(new SuperTrendResult(
            Timestamp: bar.Timestamp,
            SuperTrend: (decimal?)superTrend,
            UpperBand: (decimal?)upperOnly,
            LowerBand: (decimal?)lowerOnly));
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
        _atrList.Clear();
        _isBullish = true;
        _upperBand = null;
        _lowerBand = null;
        _prevClose = null;
        _isInitialized = false;
    }
}

public static partial class SuperTrend
{
    /// <summary>
    /// Creates a buffer list for SuperTrend calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="multiplier">Multiplier for calculation</param>
    public static SuperTrendList ToSuperTrendList(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 10,
        double multiplier = 3)
        => new(lookbackPeriods, multiplier, bars);
}
