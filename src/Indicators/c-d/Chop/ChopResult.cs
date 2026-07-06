namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of the Choppiness Index (CHOP) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="Chop">Choppiness Index value.</param>
[Serializable]
public record ChopResult
(
    DateTime Timestamp,
    double? Chop
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Chop.Null2NaN();
}
