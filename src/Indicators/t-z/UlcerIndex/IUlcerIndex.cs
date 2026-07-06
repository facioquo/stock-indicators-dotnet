namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Ulcer Index calculations.
/// </summary>
public interface IUlcerIndex
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LookbackPeriods { get; }
}
