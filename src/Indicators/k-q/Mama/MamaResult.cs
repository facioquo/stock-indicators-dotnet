namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a MAMA (MESA Adaptive Moving Average) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="Mama">Value of the MAMA.</param>
/// <param name="Fama">Value of the FAMA.</param>
[Serializable]
public record MamaResult
(
    DateTime Timestamp,
    double? Mama = null,
    double? Fama = null
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Mama.Null2NaN();
}
