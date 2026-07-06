namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Price Relative Strength (PRS) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the PRS result.</param>
/// <param name="Prs">PRS value.</param>
/// <param name="PrsPercent">PRS percentage value.</param>
[Serializable]
public record PrsResult
(
    DateTime Timestamp,
    double? Prs,
    double? PrsPercent
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Prs.Null2NaN();
}
