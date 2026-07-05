namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of an Exponential Moving Average (EMA) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="Ema">EMA value.</param>
[Serializable]
public record EmaResult
(
    DateTime Timestamp,
    double? Ema = null
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Ema.Null2NaN();
}
