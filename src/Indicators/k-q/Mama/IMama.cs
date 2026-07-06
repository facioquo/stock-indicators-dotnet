namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for MESA Adaptive Moving Average (MAMA) calculations.
/// </summary>
public interface IMama
{
    /// <summary>
    /// Gets the fast limit for the MAMA calculation.
    /// </summary>
    double FastLimit { get; }

    /// <summary>
    /// Gets the slow limit for the MAMA calculation.
    /// </summary>
    double SlowLimit { get; }
}
