namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Williams %R calculations.
/// </summary>
public interface IWilliamsR
{
    /// <summary>
    /// Gets the number of periods to look back for the Williams %R calculation.
    /// </summary>
    int LookbackPeriods { get; }
}
