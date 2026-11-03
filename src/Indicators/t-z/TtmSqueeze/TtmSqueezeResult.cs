namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of the TTM Squeeze calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="Momentum">Momentum histogram value.</param>
/// <param name="IsSqueezeOn">Whether the Bollinger Bands are inside the Keltner Channels.</param>
[Serializable]
public record TtmSqueezeResult
(
    DateTime Timestamp,
    double? Momentum = null,
    bool? IsSqueezeOn = null
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Momentum.Null2NaN();

    /// <summary>
    /// Gets the momentum before smoothing (interim data for internal streaming calculations).
    /// </summary>
    internal double? Delta { get; init; }
}
