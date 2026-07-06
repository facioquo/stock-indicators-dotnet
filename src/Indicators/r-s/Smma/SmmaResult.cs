namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Smoothed Moving Average (SMMA) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the data point.</param>
/// <param name="Smma">Value of the SMMA at this point.</param>
[Serializable]
public record SmmaResult
(
    DateTime Timestamp,
    double? Smma = null
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Smma.Null2NaN();
}
