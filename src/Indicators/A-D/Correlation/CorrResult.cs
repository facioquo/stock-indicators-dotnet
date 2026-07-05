namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of the correlation calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="VarianceA">Variance of series A.</param>
/// <param name="VarianceB">Variance of series B.</param>
/// <param name="Covariance">Covariance between series A and B.</param>
/// <param name="Correlation">Correlation coefficient between series A and B.</param>
/// <param name="RSquared">R-squared value of the correlation.</param>
[Serializable]
public record CorrResult
(
    DateTime Timestamp,
    double? VarianceA = null,
    double? VarianceB = null,
    double? Covariance = null,
    double? Correlation = null,
    double? RSquared = null
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Correlation.Null2NaN();
}
