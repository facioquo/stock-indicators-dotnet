namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the properties of a candlestick.
/// </summary>
/// <param name="Timestamp">Timestamp of the candlestick.</param>
/// <param name="Open">Opening price of the candlestick.</param>
/// <param name="High">Highest price of the candlestick.</param>
/// <param name="Low">Lowest price of the candlestick.</param>
/// <param name="Close">Closing price of the candlestick.</param>
/// <param name="Volume">Volume of the candlestick.</param>
[Serializable]
public record CandleProperties
(
    DateTime Timestamp,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume
) : Bar(Timestamp, Open, High, Low, Close, Volume)
{
    /// <summary>
    /// Gets the size of the candlestick.
    /// </summary>
    public decimal? Size => High - Low;

    /// <summary>
    /// Gets the body size of the candlestick.
    /// </summary>
    public decimal? Body => Open > Close ? Open - Close : Close - Open;

    /// <summary>
    /// Gets the size of the upper wick of the candlestick.
    /// </summary>
    public decimal? UpperWick => High - (Open > Close ? Open : Close);

    /// <summary>
    /// Gets the size of the lower wick of the candlestick.
    /// </summary>
    public decimal? LowerWick => (Open > Close ? Close : Open) - Low;

    /// <summary>
    /// Gets the body size as a percentage of the total size.
    /// </summary>
    public double? BodyPct => Size != 0 ? (double?)(Body / Size) : 1;

    /// <summary>
    /// Gets the upper wick size as a percentage of the total size.
    /// </summary>
    public double? UpperWickPct => Size != 0 ? (double?)(UpperWick / Size) : 1;

    /// <summary>
    /// Gets the lower wick size as a percentage of the total size.
    /// </summary>
    public double? LowerWickPct => Size != 0 ? (double?)(LowerWick / Size) : 1;

    /// <summary>
    /// Gets a value indicating whether the candlestick is bullish.
    /// </summary>
    public bool IsBullish => Close > Open;

    /// <summary>
    /// Gets a value indicating whether the candlestick is bearish.
    /// </summary>
    public bool IsBearish => Close < Open;
}
