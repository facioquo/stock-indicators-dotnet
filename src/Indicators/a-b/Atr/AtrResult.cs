namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of the Average True Range (ATR) indicator calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="Tr">True Range (TR) value.</param>
/// <param name="Atr">Average True Range (ATR) value.</param>
/// <param name="Atrp">ATR percentage value.</param>
[Serializable]
public record AtrResult
(
    DateTime Timestamp,
    double? Tr = null,
    double? Atr = null,
    double? Atrp = null
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Atrp.Null2NaN();
}
