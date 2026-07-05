namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Marubozu candlestick pattern streaming and buffered list.
/// </summary>
public interface IMarubozu
{
    /// <summary>
    /// Gets the minimum body percentage to qualify as a Marubozu.
    /// </summary>
    double MinBodyPercent { get; }
}
