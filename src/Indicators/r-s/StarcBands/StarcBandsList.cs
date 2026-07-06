namespace FacioQuo.Stock.Indicators;

/// <summary>
/// STARC Bands from incremental bars.
/// </summary>
public class StarcBandsList : BufferList<StarcBandsResult>, IIncrementFromBar, IStarcBands
{
    private readonly SmaList _smaList;
    private readonly AtrList _atrList;

    /// <summary>
    /// Initializes a new instance of the <see cref="StarcBandsList"/> class.
    /// </summary>
    /// <param name="smaPeriods">Number of periods for the Simple Moving Average (SMA).</param>
    /// <param name="multiplier">Multiplier for the Average True Range (ATR).</param>
    /// <param name="atrPeriods">Number of periods for the ATR calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="atrPeriods"/> is invalid.</exception>
    public StarcBandsList(
        int smaPeriods = 5,
        double multiplier = 2,
        int atrPeriods = 10)
    {
        StarcBands.Validate(smaPeriods, multiplier, atrPeriods);
        SmaPeriods = smaPeriods;
        Multiplier = multiplier;
        AtrPeriods = atrPeriods;

        _smaList = new SmaList(smaPeriods);
        _atrList = new AtrList(atrPeriods);

        Name = $"STARCBANDS({smaPeriods}, {multiplier}, {atrPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StarcBandsList"/> class with initial bars.
    /// </summary>
    /// <param name="smaPeriods">Number of periods for the Simple Moving Average (SMA).</param>
    /// <param name="multiplier">Multiplier for the Average True Range (ATR).</param>
    /// <param name="atrPeriods">Number of periods for the ATR calculation.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public StarcBandsList(
        int smaPeriods,
        double multiplier,
        int atrPeriods,
        IReadOnlyList<IBar> bars)
        : this(smaPeriods, multiplier, atrPeriods) => Add(bars);

    /// <inheritdoc />
    public int SmaPeriods { get; init; }

    /// <inheritdoc />
    public double Multiplier { get; init; }

    /// <inheritdoc />
    public int AtrPeriods { get; init; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        DateTime timestamp = bar.Timestamp;

        // Add bar to both component lists
        _smaList.Add(bar);
        _atrList.Add(bar);

        // Get the latest results from each component
        SmaResult smaResult = _smaList[^1];
        AtrResult atrResult = _atrList[^1];

        // Calculate STARC Bands
        double? upperBand = null;
        double? centerline = smaResult.Sma;
        double? lowerBand = null;

        if (smaResult.Sma.HasValue && atrResult.Atr.HasValue)
        {
            upperBand = smaResult.Sma.Value + (Multiplier * atrResult.Atr.Value);
            lowerBand = smaResult.Sma.Value - (Multiplier * atrResult.Atr.Value);
        }

        AddInternal(new StarcBandsResult(
            Timestamp: timestamp,
            UpperBand: upperBand,
            Centerline: centerline,
            LowerBand: lowerBand));
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
        _smaList.Clear();
        _atrList.Clear();
    }
}

public static partial class StarcBands
{
    /// <summary>
    /// Creates a buffer list for STARC Bands calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="smaPeriods">Number of periods for the simple moving average</param>
    /// <param name="multiplier">Multiplier for calculation</param>
    /// <param name="atrPeriods">Number of periods for ATR calculation</param>
    public static StarcBandsList ToStarcBandsList(
        this IReadOnlyList<IBar> bars,
        int smaPeriods = 5,
        double multiplier = 2,
        int atrPeriods = 10)
        => new(smaPeriods, multiplier, atrPeriods) { bars };
}
