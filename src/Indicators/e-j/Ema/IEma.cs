namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Exponential Moving Average (EMA) calculations.
/// </summary>
public interface IEma
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
