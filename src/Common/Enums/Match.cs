namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Candlestick pattern matching type.
/// </summary>
public enum Match
{
    /// <summary>
    /// Strong bearish confirmation.
    /// </summary>
    BearConfirmed = -200,

    /// <summary>
    /// Bearish signal.
    /// </summary>
    BearSignal = -100,

    /// <summary>
    /// Bearish basis.
    /// </summary>
    BearBasis = -10,

    /// <summary>
    /// No pattern.
    /// </summary>
    None = 0,

    /// <summary>
    /// Neutral pattern.
    /// </summary>
    Neutral = 1,

    /// <summary>
    /// Bullish basis.
    /// </summary>
    BullBasis = 10,

    /// <summary>
    /// Bullish signal.
    /// </summary>
    BullSignal = 100,
    /// <summary>
    /// Strong bullish confirmation.
    /// </summary>
    BullConfirmed = 200
}
