namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Doji candlestick pattern from incremental bars.
/// </summary>
public class DojiList : BufferList<CandleResult>, IIncrementFromBar, IDoji
{
    private readonly double maxPriceChangeFraction;

    /// <summary>
    /// Initializes a new instance of the <see cref="DojiList"/> class.
    /// </summary>
    /// <param name="maxPriceChangePercent">Maximum absolute percent difference in open and close price.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxPriceChangePercent"/> is invalid.</exception>
    public DojiList(double maxPriceChangePercent = 0.1)
    {
        Doji.Validate(maxPriceChangePercent);
        MaxPriceChangePercent = maxPriceChangePercent;
        maxPriceChangeFraction = maxPriceChangePercent / 100;

        Name = $"DOJI({maxPriceChangePercent})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DojiList"/> class with initial bars.
    /// </summary>
    /// <param name="maxPriceChangePercent">Maximum absolute percent difference in open and close price.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public DojiList(double maxPriceChangePercent, IReadOnlyList<IBar> bars)
        : this(maxPriceChangePercent) => Add(bars);

    /// <inheritdoc />
    public double MaxPriceChangePercent { get; init; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        DateTime timestamp = bar.Timestamp;
        decimal? matchPrice = null;
        Match matchType = Match.None;

        // Check for Doji pattern
        if (bar.Open != 0
            && Math.Abs((double)(bar.Close / bar.Open) - 1d) <= maxPriceChangeFraction)
        {
            matchPrice = bar.Close;
            matchType = Match.Neutral;
        }

        AddInternal(new CandleResult(
            timestamp: timestamp,
            bar: bar,
            match: matchType,
            price: matchPrice));
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

    /// <summary>
    /// Clears the list and resets internal state so the instance can be reused.
    /// </summary>
    public override void Clear() => base.Clear();
}

public static partial class Doji
{
    /// <summary>
    /// Creates a buffer list for Doji candlestick pattern detection.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="maxPriceChangePercent">Maximum price change percent threshold</param>
    public static DojiList ToDojiList(
        this IReadOnlyList<IBar> bars,
        double maxPriceChangePercent = 0.1)
        => new(maxPriceChangePercent) { bars };
}
