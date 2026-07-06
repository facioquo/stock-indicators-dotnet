namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Price candle end types used to select which aspect of the candle
/// to use in indicator threshold calculations.
/// </summary>
public enum EndType
{
    /// <summary>
    /// Closing price.
    /// </summary>
    Close = 0,

    /// <summary>
    /// High and low prices.
    /// </summary>
    HighLow = 1
}
