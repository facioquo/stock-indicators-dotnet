namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of the
/// ALMA (Arnaud Legoux Moving Average) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="Alma">ALMA value.</param>
[Serializable]
public record AlmaResult
(
    DateTime Timestamp,
    double? Alma
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Alma.Null2NaN();
}
