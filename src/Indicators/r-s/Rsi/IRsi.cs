namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Relative Strength Index (RSI) calculations.
/// </summary>
public interface IRsi
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LookbackPeriods { get; }
}
