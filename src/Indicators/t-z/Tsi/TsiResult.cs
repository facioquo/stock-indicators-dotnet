namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a True Strength Index (TSI) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the data point.</param>
/// <param name="Tsi">Value of the TSI at this point.</param>
/// <param name="Signal">Signal line value at this point.</param>
[Serializable]
public record TsiResult
(
    DateTime Timestamp,
    double? Tsi = null,
    double? Signal = null
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Tsi.Null2NaN();
}
