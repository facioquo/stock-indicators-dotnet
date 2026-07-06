namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Moving average type.
/// </summary>
public enum MaType
{
    /// <summary>
    /// Arnaud Legoux Moving Average.
    /// </summary>
    ALMA = 0,

    /// <summary>
    /// Double Exponential Moving Average.
    /// </summary>
    DEMA = 1,

    /// <summary>
    /// Exponential Percentage Moving Average.
    /// </summary>
    EPMA = 2,

    /// <summary>
    /// Exponential Moving Average.
    /// </summary>
    EMA = 3,

    /// <summary>
    /// Hull Moving Average.
    /// </summary>
    HMA = 4,

    /// <summary>
    /// Kaufman Adaptive Moving Average.
    /// </summary>
    KAMA = 5,

    /// <summary>
    /// MESA Adaptive Moving Average.
    /// </summary>
    MAMA = 6,

    /// <summary>
    /// Simple Moving Average.
    /// </summary>
    SMA = 7,

    /// <summary>
    /// Smoothed Moving Average.
    /// </summary>
    SMMA = 8,

    /// <summary>
    /// Triple Exponential Moving Average.
    /// </summary>
    TEMA = 9,

    /// <summary>
    /// Weighted Moving Average.
    /// </summary>
    WMA = 10
}
