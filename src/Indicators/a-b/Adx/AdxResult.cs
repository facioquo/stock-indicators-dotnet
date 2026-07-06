namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of the ADX (Average Directional Index) calculation.
/// </summary>
/// <param name="Timestamp">Gets the timestamp of the result.</param>
/// <param name="Pdi">Gets the Positive Directional Indicator (PDI) value.</param>
/// <param name="Mdi">Gets the Negative Directional Indicator (MDI) value.</param>
/// <param name="Dx">Gets the Directional Index (DX) value.</param>
/// <param name="Adx">Gets the Average Directional Index (ADX) value.</param>
/// <param name="Adxr">Gets the Average Directional Movement Rating (ADXR) value.</param>
[Serializable]
public record AdxResult
(
    DateTime Timestamp,
    double? Pdi = null,
    double? Mdi = null,
    double? Dx = null,
    double? Adx = null,
    double? Adxr = null
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Adx.Null2NaN();
}
