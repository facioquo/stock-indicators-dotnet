namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for incrementing with OHLCV <see cref="IBar"/> values.
/// </summary>
/// <remarks>
/// This patterns supports indicators that can only be calculated from OHLCV bars.
/// Examples: TR, RSI, moving averages, and others.
/// </remarks>
internal interface IIncrementFromBar
{
    /// <summary>
    /// Apply new bar to increment indicator list values.
    /// </summary>
    /// <remarks>
    /// Bars must be supplied in chronological order. This method assumes
    /// <paramref name="bar"/> is newer than every previously added value;
    /// it does not detect or correct out-of-order, duplicate, or revised
    /// timestamps. Use a <c>StreamHub</c> when input can arrive out of order.
    /// </remarks>
    /// <param name="bar">Next bar value, newer than the last added.</param>
    void Add(IBar bar);

    /// <summary>
    /// Apply batch of bars increment many indicator list values.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, in chronological order.</param>
    void Add(IReadOnlyList<IBar> bars);
}
