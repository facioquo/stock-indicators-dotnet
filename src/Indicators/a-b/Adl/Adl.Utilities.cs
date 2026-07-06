namespace FacioQuo.Stock.Indicators;

public static partial class Adl
{
    /// <summary>
    /// Get the next incremental Accumulation/Distribution Line (ADL) value.
    /// </summary>
    /// <param name="timestamp">Timestamp of the current period.</param>
    /// <param name="high">High price of the current period.</param>
    /// <param name="low">Low price of the current period.</param>
    /// <param name="close">Close price of the current period.</param>
    /// <param name="volume">Volume of the current period.</param>
    /// <param name="prevAdl">Last ADL value from the prior period.</param>
    /// <returns>New ADL result value.</returns>
    public static AdlResult Increment(
        DateTime timestamp,
        double high,
        double low,
        double close,
        double volume,
        double prevAdl)
    {
        double mfm = high == low
            ? 0
            : (close - low - (high - close))
            / (high - low);

        double mfv = mfm * volume;
        double adl = mfv + prevAdl;

        return new AdlResult(
            Timestamp: timestamp,
            Adl: adl,
            MoneyFlowMultiplier: mfm,
            MoneyFlowVolume: mfv);
    }

    /// <summary>
    /// Get the next incremental Accumulation/Distribution Line (ADL) value.
    /// </summary>
    /// <param name="timestamp">Timestamp of the current period.</param>
    /// <param name="high">High price of the current period.</param>
    /// <param name="low">Low price of the current period.</param>
    /// <param name="close">Close price of the current period.</param>
    /// <param name="volume">Volume of the current period.</param>
    /// <param name="prevAdl">Last ADL value from the prior period.</param>
    /// <returns>New ADL result value.</returns>
    internal static AdlResult Increment(
        DateTime timestamp,
        decimal high,
        decimal low,
        decimal close,
        decimal volume,
        double prevAdl)
        => Increment(
            timestamp,
            (double)high,
            (double)low,
            (double)close,
            (double)volume,
            prevAdl);
}
