namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of the KAMA (Kaufman's Adaptive Moving Average) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="Er">Efficiency Ratio (ER) value.</param>
/// <param name="Kama">KAMA value.</param>
[Serializable]
public record KamaResult
(
    DateTime Timestamp,
    double? Er = null,
    double? Kama = null
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Kama.Null2NaN();
}
