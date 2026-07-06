namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Utility for candlestick data.
/// </summary>
public static class Candlesticks
{
    /// <summary>
    /// Condenses the list of candle results by filtering out those with no match.
    /// </summary>
    /// <param name="candleResults">List of candle results to condense.</param>
    /// <returns>A condensed list of candle results.</returns>
    public static IReadOnlyList<CandleResult> Condense(
        this IReadOnlyList<CandleResult> candleResults) => candleResults
            .Where(static candle => candle.Match != Match.None)
            .ToList();

    /// <summary>
    /// Converts a bar to candle properties.
    /// </summary>
    /// <typeparam name="TBar">Type of bar record</typeparam>
    /// <param name="bar">Bar to convert.</param>
    /// <returns>Candle properties.</returns>
    public static CandleProperties ToCandle<TBar>(
        this TBar bar)
        where TBar : IBar
        => new(
            Timestamp: bar.Timestamp,
            Open: bar.Open,
            High: bar.High,
            Low: bar.Low,
            Close: bar.Close,
            Volume: bar.Volume);

    /// <summary>
    /// Converts and sorts a list of bars into a list of candle properties.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <returns>A sorted list of candle properties.</returns>
    public static IReadOnlyList<CandleProperties> ToCandles(
        this IReadOnlyList<IBar> bars)
        => bars
            .Select(static x => x.ToCandle())
            .OrderBy(static x => x.Timestamp)
            .ToList();
}
