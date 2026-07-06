namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for McGinley Dynamic calculations.
/// </summary>
public interface IDynamic
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LookbackPeriods { get; }

    /// <summary>
    /// Gets the smoothing factor for the calculation.
    /// </summary>
    double KFactor { get; }
}
