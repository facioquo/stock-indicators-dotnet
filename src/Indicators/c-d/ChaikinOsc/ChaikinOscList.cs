namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Chaikin Oscillator from incremental bars.
/// </summary>
public class ChaikinOscList : BufferList<ChaikinOscResult>, IIncrementFromBar, IChaikinOsc
{
    private readonly AdlList _adlList;
    private readonly EmaList _fastEmaList;
    private readonly EmaList _slowEmaList;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChaikinOscList"/> class.
    /// </summary>
    /// <param name="fastPeriods">Number of periods to use for the fast EMA.</param>
    /// <param name="slowPeriods">Number of periods to use for the slow EMA.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="slowPeriods"/> is invalid.</exception>
    public ChaikinOscList(int fastPeriods = 3, int slowPeriods = 10)
    {
        ChaikinOsc.Validate(fastPeriods, slowPeriods);
        FastPeriods = fastPeriods;
        SlowPeriods = slowPeriods;

        _adlList = [];
        _fastEmaList = new EmaList(fastPeriods);
        _slowEmaList = new EmaList(slowPeriods);

        Name = $"CHAIKINOSC({fastPeriods}, {slowPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChaikinOscList"/> class with initial bars.
    /// </summary>
    /// <param name="fastPeriods">Number of periods to use for the fast EMA.</param>
    /// <param name="slowPeriods">Number of periods to use for the slow EMA.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public ChaikinOscList(int fastPeriods, int slowPeriods, IReadOnlyList<IBar> bars)
        : this(fastPeriods, slowPeriods) => Add(bars);

    /// <inheritdoc />
    public int FastPeriods { get; init; }

    /// <inheritdoc />
    public int SlowPeriods { get; init; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        DateTime timestamp = bar.Timestamp;

        // Calculate ADL
        _adlList.Add(bar);
        AdlResult adlResult = _adlList[^1];

        // Calculate fast and slow EMAs of ADL
        _fastEmaList.Add(adlResult);
        _slowEmaList.Add(adlResult);

        EmaResult fastEma = _fastEmaList[^1];
        EmaResult slowEma = _slowEmaList[^1];

        // Calculate oscillator
        double? oscillator = fastEma.Ema - slowEma.Ema;

        AddInternal(new ChaikinOscResult(
            timestamp,
            adlResult.MoneyFlowMultiplier,
            adlResult.MoneyFlowVolume,
            adlResult.Adl,
            oscillator,
            fastEma.Ema,
            slowEma.Ema));
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
        _adlList.Clear();
        _fastEmaList.Clear();
        _slowEmaList.Clear();
    }
}

public static partial class ChaikinOsc
{
    /// <summary>
    /// Creates a buffer list for Chaikin Oscillator calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="fastPeriods">Number of periods for the fast moving average</param>
    /// <param name="slowPeriods">Number of periods for the slow moving average</param>
    public static ChaikinOscList ToChaikinOscList(
        this IReadOnlyList<IBar> bars,
        int fastPeriods = 3,
        int slowPeriods = 10)
        => new(fastPeriods, slowPeriods) { bars };
}
