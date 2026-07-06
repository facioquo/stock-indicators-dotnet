namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of the Commodity Channel Index (CCI) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="Cci">Commodity Channel Index value.</param>
[Serializable]
public record CciResult
(
    DateTime Timestamp,
    double? Cci
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Cci.Null2NaN();
}
