namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the category of an indicator.
/// </summary>
public enum Category
{
    /// <summary>
    /// Undefined category.
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// Indicators related to candlestick patterns.
    /// </summary>
    CandlestickPattern = 1,

    /// <summary>
    /// Indicators related to moving averages.
    /// </summary>
    MovingAverage = 2,

    /// <summary>
    /// Indicators that are oscillators.
    /// </summary>
    Oscillator = 3,

    /// <summary>
    /// Indicators related to price channels.
    /// </summary>
    PriceChannel = 4,

    /// <summary>
    /// Indicators that describe price characteristics.
    /// </summary>
    PriceCharacteristic = 5,

    /// <summary>
    /// Indicators related to price patterns.
    /// </summary>
    PricePattern = 6,

    /// <summary>
    /// Indicators that transform price data.
    /// </summary>
    PriceTransform = 7,

    /// <summary>
    /// Indicators that analyze price trends.
    /// </summary>
    PriceTrend = 8,

    /// <summary>
    /// Indicators related to stop and reverse strategies.
    /// </summary>
    StopAndReverse = 9,

    /// <summary>
    /// Indicators based on volume data.
    /// </summary>
    VolumeBased = 10,
}
