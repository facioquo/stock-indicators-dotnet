namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Hurst Exponent calculations.
/// </summary>
public interface IHurst
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LookbackPeriods { get; }
}
