namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Elder Ray calculations.
/// </summary>
public interface IElderRay
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LookbackPeriods { get; }
}
