namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Average Directional Index (ADX) streaming and buffered list.
/// </summary>
public interface IAdx
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LookbackPeriods { get; }
}
