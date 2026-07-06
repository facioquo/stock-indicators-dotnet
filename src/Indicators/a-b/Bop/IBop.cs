namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Balance of Power (BOP) calculations.
/// </summary>
public interface IBop
{
    /// <summary>
    /// Gets the number of smoothing periods.
    /// </summary>
    int SmoothPeriods { get; }
}
