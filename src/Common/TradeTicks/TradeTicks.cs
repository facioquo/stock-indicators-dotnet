namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides extension methods for aggregating tick data streams into OHLCV price bars using a <see cref="TradeTickAggregatorHub"/>.
/// </summary>
/// <remarks>
/// The TradeTicks class offers static methods to facilitate the transformation of tick data into aggregated
/// price bars, supporting both fixed period sizes and custom time spans. These methods enable seamless integration with
/// tick data providers and allow optional gap filling to maintain continuity in price data. All members are static and
/// intended for use as extension methods on <see cref="IStreamObservable{ITradeTick}"/> instances.
/// </remarks>
public static class TradeTicks
{
    /// <summary>
    /// Creates a TradeTickAggregatorHub that aggregates ticks from the provider into OHLCV price bars.
    /// </summary>
    /// <param name="tickProvider">The tick provider to aggregate.</param>
    /// <param name="barInterval">The period size to aggregate to.</param>
    /// <param name="fillGaps">Whether to fill gaps by carrying forward the last known price.</param>
    /// <returns>A new instance of TradeTickAggregatorHub.</returns>
    public static TradeTickAggregatorHub ToTradeTickAggregatorHub(
        this IStreamObservable<ITradeTick> tickProvider,
        BarInterval barInterval,
        bool fillGaps = false)
        => new(tickProvider, barInterval, fillGaps);

    /// <summary>
    /// Creates a TradeTickAggregatorHub that aggregates ticks from the provider into OHLCV price bars.
    /// </summary>
    /// <param name="tickProvider">The tick provider to aggregate.</param>
    /// <param name="timeSpan">The time span to aggregate to.</param>
    /// <param name="fillGaps">Whether to fill gaps by carrying forward the last known price.</param>
    /// <returns>A new instance of TradeTickAggregatorHub.</returns>
    public static TradeTickAggregatorHub ToTradeTickAggregatorHub(
        this IStreamObservable<ITradeTick> tickProvider,
        TimeSpan timeSpan,
        bool fillGaps = false)
        => new(tickProvider, timeSpan, fillGaps);
}
