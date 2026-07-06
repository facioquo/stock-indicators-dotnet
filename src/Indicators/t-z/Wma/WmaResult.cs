namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Weighted Moving Average (WMA) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the data point.</param>
/// <param name="Wma">Value of the WMA at this point.</param>
[Serializable]
public record WmaResult
(
    DateTime Timestamp,
    double? Wma
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Wma.Null2NaN();
}
