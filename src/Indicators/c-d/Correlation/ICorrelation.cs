namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Correlation streaming and buffered list.
/// </summary>
public interface ICorrelation
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LookbackPeriods { get; }
}
