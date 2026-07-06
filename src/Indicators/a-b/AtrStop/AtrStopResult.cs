namespace FacioQuo.Stock.Indicators;

/// <summary>
/// ATR Trailing Stop result
/// </summary>
/// <param name="Timestamp">
/// Date corresponding to evaluated price data
/// </param>
/// <param name="AtrStop">
/// Trailing stop line (includes both buy and sell stops)
/// </param>
/// <param name="BuyStop">
/// Stop line (buy to close) short position
/// </param>
/// <param name="SellStop">
/// Stop line (sell to close) long position
/// </param>
/// <param name="Atr">
/// Average True Range (ATR)
/// </param>
[Serializable]
public record AtrStopResult(
    DateTime Timestamp,
    double? AtrStop = null,
    double? BuyStop = null,
    double? SellStop = null,
    double? Atr = null
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => AtrStop.Null2NaN();
}
