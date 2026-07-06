namespace FacioQuo.Stock.Indicators;

/// <summary>
/// ATR Trailing Stop from incremental bar values.
/// </summary>
public class AtrStopList : BufferList<AtrStopResult>, IIncrementFromBar, IAtrStop
{
    private readonly AtrList _atrList;
    private bool _isBullish;
    private double _upperBand;
    private double _lowerBand;
    private double _previousClose;
    private bool _isInitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="AtrStopList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="multiplier">Multiplier for the ATR.</param>
    /// <param name="endType">Candle threshold point to use for reversals.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="endType"/> is invalid.</exception>
    public AtrStopList(
        int lookbackPeriods = 21,
        double multiplier = 3,
        EndType endType = EndType.Close)
    {
        AtrStop.Validate(lookbackPeriods, multiplier);
        LookbackPeriods = lookbackPeriods;
        Multiplier = multiplier;
        EndType = endType;

        _atrList = new AtrList(lookbackPeriods);
        _isBullish = true;
        _upperBand = double.MaxValue;
        _lowerBand = double.MinValue;
        _isInitialized = false;

        Name = $"ATRSTOP({21}, {3}, {EndType.Close})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtrStopList"/> class with initial bars.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="multiplier">Multiplier for the ATR.</param>
    /// <param name="endType">Candle threshold point to use for reversals.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public AtrStopList(
        int lookbackPeriods,
        double multiplier,
        EndType endType,
        IReadOnlyList<IBar> bars)
        : this(lookbackPeriods, multiplier, endType) => Add(bars);

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc/>
    public double Multiplier { get; init; }

    /// <inheritdoc/>
    public EndType EndType { get; init; }

    /// <summary>
    /// Adds a new bar to the AtrStop list.
    /// </summary>
    /// <param name="bar">Bar to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the bar is null.</exception>
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        // Add to ATR list to get ATR calculation
        _atrList.Add(bar);
        AtrResult atrResult = _atrList[^1];

        // Handle warmup periods - need LookbackPeriods values for initial ATR
        if (_atrList.Count < LookbackPeriods + 1)
        {
            _previousClose = (double)bar.Close;
            AddInternal(new AtrStopResult(Timestamp: bar.Timestamp));
            return;
        }

        double close = (double)bar.Close;
        double high = (double)bar.High;
        double low = (double)bar.Low;

        // Initialize direction on first evaluation
        if (!_isInitialized)
        {
            _isBullish = close >= _previousClose;
            _isInitialized = true;
        }

        double atr = atrResult.Atr ?? double.NaN;

        // Calculate potential bands
        double upperEval;
        double lowerEval;

        if (EndType == EndType.Close)
        {
            upperEval = close + (Multiplier * atr);
            lowerEval = close - (Multiplier * atr);
        }
        else
        {
            upperEval = high + (Multiplier * atr);
            lowerEval = low - (Multiplier * atr);
        }

        // New upper band: can only go down, or reverse
        if (upperEval < _upperBand || _previousClose > _upperBand)
        {
            _upperBand = upperEval;
        }

        // New lower band: can only go up, or reverse
        if (lowerEval > _lowerBand || _previousClose < _lowerBand)
        {
            _lowerBand = lowerEval;
        }

        // Determine stop based on direction
        AtrStopResult result;

        // Upper band (short / buy-to-stop)
        if (close <= (_isBullish ? _lowerBand : _upperBand))
        {
            _isBullish = false;

            result = new AtrStopResult(
                Timestamp: bar.Timestamp,
                AtrStop: _upperBand,
                BuyStop: _upperBand,
                SellStop: null,
                Atr: atr);
        }
        // Lower band (long / sell-to-stop)
        else
        {
            _isBullish = true;

            result = new AtrStopResult(
                Timestamp: bar.Timestamp,
                AtrStop: _lowerBand,
                BuyStop: null,
                SellStop: _lowerBand,
                Atr: atr);
        }

        _previousClose = close;
        AddInternal(result);
    }

    /// <summary>
    /// Adds a list of bars to the AtrStop list.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <exception cref="ArgumentNullException">Thrown when the bars list is null.</exception>
    public void Add(IReadOnlyList<IBar> bars)
    {
        ArgumentNullException.ThrowIfNull(bars);

        for (int i = 0; i < bars.Count; i++)
        {
            Add(bars[i]);
        }
    }

    /// <summary>
    /// Clears the list and resets internal buffers so the instance can be reused.
    /// </summary>
    public override void Clear()
    {
        base.Clear();
        _atrList.Clear();
        _isBullish = true;
        _upperBand = double.MaxValue;
        _lowerBand = double.MinValue;
        _previousClose = 0;
        _isInitialized = false;
    }
    /// <summary>
    /// Removes oldest results from both the outer list and the nested ATR list
    /// when the list exceeds <see cref="BufferList{TResult}.MaxListSize"/>.
    /// This prevents unbounded growth of the auxiliary ATR cache.
    /// </summary>
    protected override void PruneList()
    {
        // Synchronize the nested ATR list's MaxListSize with the outer list
        // Keep enough ATR history to support calculations (LookbackPeriods + 1)
        // plus room for the current MaxListSize
        int minAtrSize = LookbackPeriods + 1;
        _atrList.MaxListSize = Math.Max(minAtrSize, MaxListSize + 1);

        // Call base implementation to prune the outer result list
        base.PruneList();
    }
}

/// <summary>
/// EXTENSION METHODS
/// </summary>
public static partial class AtrStop
{
    /// <summary>
    /// Creates a buffer list for ATR Trailing Stop calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="multiplier">Multiplier for the ATR.</param>
    /// <param name="endType">Candle threshold point to use for reversals.</param>
    /// <returns>An AtrStopList instance pre-populated with historical data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when bars is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are invalid.</exception>
    public static AtrStopList ToAtrStopList(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 21,
        double multiplier = 3,
        EndType endType = EndType.Close)
        => new(lookbackPeriods, multiplier, endType) { bars };
}
