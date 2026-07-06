namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Stochastic RSI calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the data point.</param>
/// <param name="StochRsi">Value of the Stochastic RSI at this point.</param>
/// <param name="Signal">Signal line value at this point.</param>
[Serializable]
public record StochRsiResult
(
    DateTime Timestamp,
    double? StochRsi = null,
    double? Signal = null
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => StochRsi.Null2NaN();
}
