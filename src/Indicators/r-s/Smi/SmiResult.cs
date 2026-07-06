namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Stochastic Momentum Index (SMI) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the data point.</param>
/// <param name="Smi">Value of the SMI at this point.</param>
/// <param name="Signal">Signal line value at this point.</param>
[Serializable]
public record SmiResult
(
    DateTime Timestamp,
    double? Smi,
    double? Signal
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Smi.Null2NaN();
}
