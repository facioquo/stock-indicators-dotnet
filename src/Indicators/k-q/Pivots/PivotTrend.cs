namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the trend direction of a pivot point.
/// </summary>
public enum PivotTrend
{
    /// <summary>
    /// Higher high trend.
    /// </summary>
    Hh = 0,

    /// <summary>
    /// Lower high trend.
    /// </summary>
    Lh = 1,

    /// <summary>
    /// Higher low trend.
    /// </summary>
    Hl = 2,

    /// <summary>
    /// Lower low trend.
    /// </summary>
    Ll = 3
}
