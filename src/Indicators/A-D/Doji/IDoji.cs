namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Doji candlestick pattern streaming and buffered list.
/// </summary>
public interface IDoji
{
    /// <summary>
    /// Gets the maximum price change percentage for identifying Doji patterns.
    /// </summary>
    double MaxPriceChangePercent { get; }
}
