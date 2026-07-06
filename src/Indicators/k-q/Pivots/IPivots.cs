namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Pivot Points calculations.
/// </summary>
public interface IPivots
{
    /// <summary>
    /// Gets the number of periods to the left for pivot identification.
    /// </summary>
    int LeftSpan { get; }

    /// <summary>
    /// Gets the number of periods to the right for pivot identification.
    /// </summary>
    int RightSpan { get; }

    /// <summary>
    /// Gets the maximum number of periods to track trend.
    /// </summary>
    int MaxTrendPeriods { get; }

    /// <summary>
    /// Gets the end type for price calculations.
    /// </summary>
    EndType EndType { get; }
}
