namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Slope calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the data point.</param>
/// <param name="Slope">Value of the slope at this point.</param>
/// <param name="Intercept">Intercept value at this point.</param>
/// <param name="StdDev">Standard deviation value at this point.</param>
/// <param name="RSquared">R-squared value at this point.</param>
/// <param name="Line">Value of the last line segment only.</param>
[Serializable]
public record SlopeResult
(
    DateTime Timestamp,
    double? Slope = null,
    double? Intercept = null,
    double? StdDev = null,
    double? RSquared = null,
    decimal? Line = null // last line segment only
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Slope.Null2NaN();
}
