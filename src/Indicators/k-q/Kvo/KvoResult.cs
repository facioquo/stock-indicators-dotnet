namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of the KVO (Klinger Volume Oscillator) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="Oscillator">Oscillator value of the KVO.</param>
/// <param name="Signal">Signal value of the KVO.</param>
[Serializable]
public record KvoResult
(
    DateTime Timestamp,
    double? Oscillator = null,
    double? Signal = null
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Oscillator.Null2NaN();
}
