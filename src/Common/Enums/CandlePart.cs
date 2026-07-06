namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Component or composite value of an OHLCV bar aggregate (e.g., Close, High, HL2, and others).
/// </summary>
public enum CandlePart
{
    /// <summary>
    /// Opening price.
    /// </summary>
    Open = 0,

    /// <summary>
    /// Highest price.
    /// </summary>
    High = 1,

    /// <summary>
    /// Lowest price.
    /// </summary>
    Low = 2,

    /// <summary>
    /// Closing price.
    /// </summary>
    Close = 3,

    /// <summary>
    /// Volume of trades.
    /// </summary>
    Volume = 4,

    /// <summary>
    /// Average of high and low prices.
    /// </summary>
    HL2 = 5,

    /// <summary>
    /// Average of high, low, and close prices.
    /// </summary>
    HLC3 = 6,

    /// <summary>
    /// Average of open and close prices.
    /// </summary>
    OC2 = 7,

    /// <summary>
    /// Average of open, high, and low prices.
    /// </summary>
    OHL3 = 8,

    /// <summary>
    /// Average of open, high, low, and close prices.
    /// </summary>
    OHLC4 = 9
}
