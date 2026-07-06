namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Force Index calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="ForceIndex">Force Index value.</param>
[Serializable]
public record ForceIndexResult
(
    DateTime Timestamp,
    double? ForceIndex = null
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => ForceIndex.Null2NaN();
}
