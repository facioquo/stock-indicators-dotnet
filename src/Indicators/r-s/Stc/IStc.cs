namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Schaff Trend Cycle (STC) calculations.
/// </summary>
public interface IStc
{
    /// <summary>
    /// Gets the number of periods for the cycle calculation.
    /// </summary>
    int CyclePeriods { get; }

    /// <summary>
    /// Gets the number of periods for the fast MA.
    /// </summary>
    int FastPeriods { get; }

    /// <summary>
    /// Gets the number of periods for the slow MA.
    /// </summary>
    int SlowPeriods { get; }
}
