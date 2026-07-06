namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of an Ultimate Oscillator calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the data point.</param>
/// <param name="Ultimate">Value of the Ultimate Oscillator at this point.</param>
[Serializable]
public record UltimateResult
(
    DateTime Timestamp,
    double? Ultimate
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Ultimate.Null2NaN();
}
