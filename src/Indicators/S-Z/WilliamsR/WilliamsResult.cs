namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Williams %R calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the data point.</param>
/// <param name="WilliamsR">Value of the Williams %R at this point.</param>
[Serializable]
public record WilliamsResult
(
    DateTime Timestamp,
    double? WilliamsR
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => WilliamsR.Null2NaN();
}
