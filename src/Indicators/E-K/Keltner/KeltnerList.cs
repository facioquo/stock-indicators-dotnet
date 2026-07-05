namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Keltner Channels from incremental bar values.
/// </summary>
public class KeltnerList : BufferList<KeltnerResult>, IIncrementFromBar
{
    private readonly EmaList _emaList;
    private readonly AtrList _atrList;
    private readonly int _lookbackPeriods;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeltnerList"/> class.
    /// </summary>
    /// <param name="emaPeriods">Number of periods for the EMA.</param>
    /// <param name="multiplier">Multiplier for the ATR.</param>
    /// <param name="atrPeriods">Number of periods for the ATR.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="atrPeriods"/> is invalid.</exception>
    public KeltnerList(
        int emaPeriods = 20,
        double multiplier = 2,
        int atrPeriods = 10)
    {
        Keltner.Validate(emaPeriods, multiplier, atrPeriods);
        EmaPeriods = emaPeriods;
        Multiplier = multiplier;
        AtrPeriods = atrPeriods;

        _emaList = new EmaList(emaPeriods);
        _atrList = new AtrList(atrPeriods);
        _lookbackPeriods = Math.Max(emaPeriods, atrPeriods);

        Name = $"KELTNER({emaPeriods}, {multiplier}, {atrPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeltnerList"/> class with initial bars.
    /// </summary>
    /// <param name="emaPeriods">Number of periods for the EMA.</param>
    /// <param name="multiplier">Multiplier for the ATR.</param>
    /// <param name="atrPeriods">Number of periods for the ATR.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public KeltnerList(
        int emaPeriods,
        double multiplier,
        int atrPeriods,
        IReadOnlyList<IBar> bars)
        : this(emaPeriods, multiplier, atrPeriods) => Add(bars);

    /// <inheritdoc />
    public int EmaPeriods { get; init; }

    /// <inheritdoc />
    public double Multiplier { get; init; }

    /// <inheritdoc />
    public int AtrPeriods { get; init; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        _emaList.Add(bar);
        _atrList.Add(bar);

        if (Count >= _lookbackPeriods - 1)
        {
            EmaResult ema = _emaList[^1];
            AtrResult atr = _atrList[^1];
            double? atrSpan = atr.Atr * Multiplier;

            AddInternal(new KeltnerResult(
                Timestamp: bar.Timestamp,
                UpperBand: ema.Ema + atrSpan,
                Centerline: ema.Ema,
                LowerBand: ema.Ema - atrSpan,
                Width: ema.Ema == 0 ? null : 2 * atrSpan / ema.Ema) {
                Atr = atr.Atr
            });
        }
        else
        {
            AddInternal(new KeltnerResult(bar.Timestamp));
        }
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
        _emaList.Clear();
        _atrList.Clear();
    }
}

public static partial class Keltner
{
    /// <summary>
    /// Creates a buffer list for Keltner Channels calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="emaPeriods">Number of periods for the exponential moving average</param>
    /// <param name="multiplier">Multiplier for calculation</param>
    /// <param name="atrPeriods">Number of periods for ATR calculation</param>
    public static KeltnerList ToKeltnerList(
        this IReadOnlyList<IBar> bars,
        int emaPeriods = 20,
        double multiplier = 2,
        int atrPeriods = 10)
        => new(emaPeriods, multiplier, atrPeriods) { bars };
}
