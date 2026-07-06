namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Commodity Channel Index (CCI) calculations.
/// </summary>
public interface ICci
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LookbackPeriods { get; }
}
