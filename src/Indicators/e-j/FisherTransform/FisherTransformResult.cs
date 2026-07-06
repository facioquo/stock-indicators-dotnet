namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Fisher Transform calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="Fisher">Fisher Transform value.</param>
/// <param name="Trigger">Trigger value.</param>
[Serializable]
public record FisherTransformResult
(
    DateTime Timestamp,
    double Fisher,
    double? Trigger
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Fisher;
}
