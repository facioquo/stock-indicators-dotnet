namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Kaufman's Adaptive Moving Average (KAMA) calculations.
/// </summary>
public interface IKama
{
    /// <summary>
    /// Gets the number of periods for the Efficiency Ratio (ER).
    /// </summary>
    int ErPeriods { get; }

    /// <summary>
    /// Gets the number of periods for the fast EMA.
    /// </summary>
    int FastPeriods { get; }

    /// <summary>
    /// Gets the number of periods for the slow EMA.
    /// </summary>
    int SlowPeriods { get; }
}
