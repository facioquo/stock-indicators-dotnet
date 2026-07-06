namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Dynamic calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="Dynamic">Dynamic value.</param>
[Serializable]
public record DynamicResult
(
    DateTime Timestamp,
    double? Dynamic
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Dynamic.Null2NaN();
}
