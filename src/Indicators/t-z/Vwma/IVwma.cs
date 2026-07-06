namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Volume Weighted Moving Average (VWMA) calculations.
/// </summary>
public interface IVwma
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LookbackPeriods { get; }
}
