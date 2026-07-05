namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods for identifying Marubozu candlestick patterns in a series of bars.
/// </summary>
public static partial class Marubozu
{
    /// <summary>
    /// Converts a list of bars to Marubozu candlestick pattern results.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="minBodyPercent">Minimum body percentage to qualify as a Marubozu. Default is 95.</param>
    /// <returns>A list of <see cref="CandleResult"/> indicating the presence of Marubozu patterns.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bars list is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the minimum body percentage is out of range.</exception>
    public static IReadOnlyList<CandleResult> ToMarubozu(
        this IReadOnlyList<IBar> bars,
        double minBodyPercent = 95)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(bars);
        Validate(minBodyPercent);

        // initialize
        int length = bars.Count;
        List<CandleResult> results = new(length);

        minBodyPercent /= 100;

        // roll through candles
        for (int i = 0; i < length; i++)
        {
            IBar q = bars[i];
            decimal? matchPrice = null;
            Match matchType = Match.None;
            CandleProperties candle = q.ToCandle();

            // check for current signal
            if (candle.BodyPct >= minBodyPercent)
            {
                matchPrice = q.Close;
                matchType = candle.IsBullish ? Match.BullSignal : Match.BearSignal;
            }

            results.Add(new(
                timestamp: q.Timestamp,
                candle: candle,
                match: matchType,
                price: matchPrice));
        }

        return results;
    }
}
