namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of the Awesome Oscillator calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="Oscillator">Value of the Awesome Oscillator.</param>
/// <param name="Normalized">Normalized value of the Awesome Oscillator.</param>
[Serializable]
public record AwesomeResult
(
    DateTime Timestamp,
    double? Oscillator,
    double? Normalized
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Oscillator.Null2NaN();
}
