namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Force Index calculations.
/// </summary>
public interface IForceIndex
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LookbackPeriods { get; }
}
