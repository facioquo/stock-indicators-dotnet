namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Parabolic SAR calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="Sar">Value of the Parabolic SAR.</param>
/// <param name="IsReversal">Indicates whether a reversal has occurred.</param>
[Serializable]
public record ParabolicSarResult
(
    DateTime Timestamp,
    double? Sar = null,
    bool? IsReversal = null
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Sar.Null2NaN();
}
