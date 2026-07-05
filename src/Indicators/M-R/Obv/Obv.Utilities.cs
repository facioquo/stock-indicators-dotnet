namespace FacioQuo.Stock.Indicators;

public static partial class Obv
{
    /// <summary>
    /// Get the next incremental On-Balance Volume (OBV) value.
    /// </summary>
    /// <param name="timestamp">Timestamp of the current period.</param>
    /// <param name="close">Close price of the current period.</param>
    /// <param name="volume">Volume of the current period.</param>
    /// <param name="prevClose">Close price of the previous period.</param>
    /// <param name="prevObv">Last OBV value from the prior period.</param>
    /// <returns>New OBV result value.</returns>
    public static ObvResult Increment(
        DateTime timestamp,
        double close,
        double volume,
        double prevClose,
        double prevObv)
    {
        double obv;

        if (double.IsNaN(prevClose))
        {
            // First period starts at 0
            obv = 0;
        }
        else if (close > prevClose)
        {
            obv = prevObv + volume;
        }
        else if (close < prevClose)
        {
            obv = prevObv - volume;
        }
        else
        {
            // No change if prices are equal
            obv = prevObv;
        }

        return new ObvResult(
            Timestamp: timestamp,
            Obv: obv);
    }

    /// <summary>
    /// Get the next incremental On-Balance Volume (OBV) value.
    /// </summary>
    /// <param name="timestamp">Timestamp of the current period.</param>
    /// <param name="close">Close price of the current period.</param>
    /// <param name="volume">Volume of the current period.</param>
    /// <param name="prevClose">Close price of the previous period.</param>
    /// <param name="prevObv">Last OBV value from the prior period.</param>
    /// <returns>New OBV result value.</returns>
    internal static ObvResult Increment(
        DateTime timestamp,
        decimal close,
        decimal volume,
        decimal prevClose,
        double prevObv)
        => Increment(
            timestamp,
            (double)close,
            (double)volume,
            (double)prevClose,
            prevObv);
}
