namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of the Chande Momentum Oscillator (CMO) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="Cmo">Chande Momentum Oscillator value.</param>
[Serializable]
public record CmoResult
(
    DateTime Timestamp,
    double? Cmo = null
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Cmo.Null2NaN();
}
