namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of the Aroon indicator calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="AroonUp">Aroon Up value.</param>
/// <param name="AroonDown">Aroon Down value.</param>
/// <param name="Oscillator">Aroon Oscillator value.</param>
[Serializable]
public record AroonResult
(
    DateTime Timestamp,
    double? AroonUp,
    double? AroonDown,
    double? Oscillator
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Oscillator.Null2NaN();
}
