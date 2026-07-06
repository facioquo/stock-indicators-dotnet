namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Percentage Volume Oscillator (PVO) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the PVO result.</param>
/// <param name="Pvo">PVO value.</param>
/// <param name="Signal">Signal line value.</param>
/// <param name="Histogram">Histogram value.</param>
[Serializable]
public record PvoResult
(
    DateTime Timestamp,
    double? Pvo,
    double? Signal,
    double? Histogram
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Pvo.Null2NaN();
}
