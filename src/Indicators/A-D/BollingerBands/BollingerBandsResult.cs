namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of Bollinger Bands calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="Sma">Simple moving average.</param>
/// <param name="UpperBand">Upper Bollinger Band.</param>
/// <param name="LowerBand">Lower Bollinger Band.</param>
/// <param name="PercentB">The %B value.</param>
/// <param name="ZScore">Z-score value.</param>
/// <param name="Width">Width of the Bollinger Bands.</param>
[Serializable]
public record BollingerBandsResult
(
    DateTime Timestamp,
    double? Sma = null,
    double? UpperBand = null,
    double? LowerBand = null,
    double? PercentB = null,
    double? ZScore = null,
    double? Width = null
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => PercentB.Null2NaN();
}
