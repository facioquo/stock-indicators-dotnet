namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Bar Part Hub.
/// </summary>
public interface IBarPart
{
    /// <summary>
    /// Gets the selected candle part.
    /// </summary>
    CandlePart CandlePartSelection { get; }
}
