namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Standard deviation calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the data point.</param>
/// <param name="StdDev">Value of the standard deviation at this point.</param>
/// <param name="Mean">Mean value at this point.</param>
/// <param name="ZScore">Z-score value at this point.</param>
[Serializable]
public record StdDevResult
(
    DateTime Timestamp,
    double? StdDev,
    double? Mean,
    double? ZScore
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => StdDev.Null2NaN();
}
