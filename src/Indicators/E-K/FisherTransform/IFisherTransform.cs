namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Fisher Transform calculations.
/// </summary>
public interface IFisherTransform
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LookbackPeriods { get; }
}
