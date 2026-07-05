namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a candlestick analysis.
/// </summary>
[Serializable]
public record CandleResult : IReusable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CandleResult"/> record.
    /// </summary>
    /// <param name="timestamp">Timestamp of record.</param>
    /// <param name="bar">Bar used for the analysis.</param>
    /// <param name="match">Match result of the analysis.</param>
    /// <param name="price">Price associated with the result.</param>
    public CandleResult(
        DateTime timestamp,
        IBar bar,
        Match match,
        decimal? price)
    {
        Timestamp = timestamp;
        Price = price;
        Match = match;
        Candle = bar.ToCandle();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CandleResult"/> record.
    /// </summary>
    /// <param name="timestamp">Timestamp of record.</param>
    /// <param name="candle">Candlestick properties.</param>
    /// <param name="match">Match result of the analysis.</param>
    /// <param name="price">Price associated with the result.</param>
    [JsonConstructor]
    public CandleResult(
        DateTime timestamp,
        CandleProperties candle,
        Match match,
        decimal? price)
    {
        Timestamp = timestamp;
        Price = price;
        Match = match;
        Candle = candle;
    }

    /// <summary>
    /// Gets the timestamp of the result.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets the price associated with the result.
    /// </summary>
    public decimal? Price { get; init; }

    /// <summary>
    /// Gets the match result of the analysis.
    /// </summary>
    public Match Match { get; init; }

    /// <summary>
    /// Gets the candlestick properties.
    /// </summary>
    public CandleProperties Candle { get; init; }

    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Price.Null2NaN();
}
