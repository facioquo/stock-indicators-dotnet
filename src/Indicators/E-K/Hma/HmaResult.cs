namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Hull Moving Average (HMA) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="Hma">Value of the Hull Moving Average.</param>
[Serializable]
public record HmaResult
(
    DateTime Timestamp,
    double? Hma = null
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Hma.Null2NaN();
}
