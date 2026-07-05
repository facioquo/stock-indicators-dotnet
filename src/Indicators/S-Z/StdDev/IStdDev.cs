namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Standard deviation (StdDev) calculations.
/// </summary>
public interface IStdDev
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LookbackPeriods { get; }
}
