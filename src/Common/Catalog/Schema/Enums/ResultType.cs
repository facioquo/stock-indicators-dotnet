namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the format type of result produced by a specific operation or process.
/// </summary>
/// <remarks>
/// This enumeration defines display types for indicator result types.
/// </remarks>
public enum ResultType
{
    /// <summary>
    /// Standard result type, typically used for most indicators.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Used to identify the central price channel
    /// </summary>
    Centerline = 1,

    /// <summary>
    /// Identifies one or more outlier channel boundaries
    /// </summary>
    Channel = 2,

    /// <summary>
    /// Result should be represented as a standalone bar chart format.
    /// </summary>
    Bar = 3,

    /// <summary>
    /// Represents a stacked bar chart visualization where data series are displayed as stacked bars.
    /// </summary>
    /// <remarks>
    /// When selected, it is expected that there are multiple results with this marker.
    /// </remarks>
    BarStacked = 4,

    /// <summary>
    /// Result should be represented as individual data points.
    /// </summary>
    Point = 5
}
