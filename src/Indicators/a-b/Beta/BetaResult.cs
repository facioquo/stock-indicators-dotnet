namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of the Beta calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="Beta">Beta value.</param>
/// <param name="BetaUp">Beta+ upside value.</param>
/// <param name="BetaDown">Beta- downside value.</param>
/// <param name="Ratio">Ratio of BetaUp to BetaDown.</param>
/// <param name="Convexity">Convexity of the Beta.</param>
/// <param name="ReturnsEval">Returns of the evaluated asset.</param>
/// <param name="ReturnsMrkt">Returns of the market.</param>
[Serializable]
public record BetaResult(
    DateTime Timestamp,
    double? Beta = null,
    double? BetaUp = null,
    double? BetaDown = null,
    double? Ratio = null,
    double? Convexity = null,
    double? ReturnsEval = null,
    double? ReturnsMrkt = null
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Beta.Null2NaN();
}
