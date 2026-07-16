namespace FacioQuo.Stock.Indicators;

/// <summary>
/// String output format type.
/// </summary>
public enum OutType
{
    /// <summary>
    /// Fixed width format.
    /// </summary>
    FixedWidth = 0,

    /// <summary>
    /// Comma-separated values format.
    /// </summary>
    CSV = 1,

    /// <summary>
    /// JSON format.
    /// </summary>
    JSON = 2
}
