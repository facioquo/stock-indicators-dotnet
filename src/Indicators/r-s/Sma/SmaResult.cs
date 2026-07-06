namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Simple Moving Average (SMA) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the data point.</param>
/// <param name="Sma">Value of the SMA at this point.</param>
[Serializable]
public record SmaResult(
    DateTime Timestamp,
    double? Sma
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Sma.Null2NaN();
}
