namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Weighted Moving Average (WMA) calculations.
/// </summary>
public interface IWma
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LookbackPeriods { get; }
}
