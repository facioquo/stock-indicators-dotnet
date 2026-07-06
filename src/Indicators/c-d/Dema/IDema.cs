namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Double Exponential Moving Average (DEMA) calculations.
/// </summary>
public interface IDema
{
    /// <summary>
    /// Gets the number of periods to look back for the calculation.
    /// </summary>
    int LookbackPeriods { get; }

    /// <summary>
    /// Gets the smoothing factor for the calculation.
    /// </summary>
    double K { get; }
}
