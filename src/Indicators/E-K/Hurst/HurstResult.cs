namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Hurst Exponent calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="HurstExponent">Value of the Hurst Exponent.</param>
/// <param name="HurstExponentAL">Value of the Anis-Lloyd corrected Hurst Exponent.</param>
[Serializable]
public record HurstResult
(
    DateTime Timestamp,
    double? HurstExponent,
    double? HurstExponentAL
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => HurstExponent.Null2NaN();
}
