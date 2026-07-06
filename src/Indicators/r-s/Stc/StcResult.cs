namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Schaff Trend Cycle (STC) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the data point.</param>
/// <param name="Stc">Value of the STC at this point.</param>
[Serializable]
public record StcResult
(
    DateTime Timestamp,
    double? Stc
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Stc.Null2NaN();
}
