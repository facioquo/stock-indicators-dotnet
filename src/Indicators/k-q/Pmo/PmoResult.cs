namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Price Momentum Oscillator (PMO) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the PMO result.</param>
/// <param name="Pmo">PMO value.</param>
/// <param name="Signal">Signal line value.</param>
[Serializable]
public record PmoResult
(
    DateTime Timestamp,
    double? Pmo,
    double? Signal
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Pmo.Null2NaN();
}
