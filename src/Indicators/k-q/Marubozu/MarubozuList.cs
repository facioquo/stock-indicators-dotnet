namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Marubozu candlestick pattern from incremental bars.
/// </summary>
public class MarubozuList : BufferList<CandleResult>, IIncrementFromBar, IMarubozu
{
    private readonly double minBodyPercent;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarubozuList"/> class.
    /// </summary>
    /// <param name="minBodyPercent">Minimum body percentage to qualify as a Marubozu. Default is 95.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="minBodyPercent"/> is invalid.</exception>
    public MarubozuList(double minBodyPercent = 95)
    {
        Marubozu.Validate(minBodyPercent);
        MinBodyPercent = minBodyPercent;
        this.minBodyPercent = minBodyPercent / 100;

        Name = $"MARUBOZU({minBodyPercent})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MarubozuList"/> class with initial bars.
    /// </summary>
    /// <param name="minBodyPercent">Minimum body percentage to qualify as a Marubozu.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public MarubozuList(double minBodyPercent, IReadOnlyList<IBar> bars)
        : this(minBodyPercent) => Add(bars);

    /// <inheritdoc />
    public double MinBodyPercent { get; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        DateTime timestamp = bar.Timestamp;
        CandleProperties candle = bar.ToCandle();

        decimal? matchPrice = null;
        Match matchType = Match.None;

        // check for current signal
        if (candle.BodyPct >= minBodyPercent)
        {
            matchPrice = bar.Close;
            matchType = candle.IsBullish ? Match.BullSignal : Match.BearSignal;
        }

        CandleResult result = new(
            timestamp: timestamp,
            candle: candle,
            match: matchType,
            price: matchPrice);

        AddInternal(result);
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
    public override void Clear() => base.Clear();
}

public static partial class Marubozu
{
    /// <summary>
    /// Creates a buffer list for Marubozu candlestick pattern calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="minBodyPercent">Minimum body percent threshold</param>
    public static MarubozuList ToMarubozuList(
        this IReadOnlyList<IBar> bars,
        double minBodyPercent = 95)
        => new(minBodyPercent) { bars };
}
