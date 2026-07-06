namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Endpoint Moving Average (EPMA) calculations.
/// </summary>
public interface IEpma
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LookbackPeriods { get; }
}
