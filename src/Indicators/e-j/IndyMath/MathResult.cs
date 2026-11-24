namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of an IndyMath operation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="Result">Operation result; <see langword="null"/> when incalculable.</param>
[Serializable]
public record MathResult
(
    DateTime Timestamp,
    double? Result = null
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Result.Null2NaN();
}
