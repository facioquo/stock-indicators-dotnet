namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of the Chaikin Money Flow (CMF) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="MoneyFlowMultiplier">Money flow multiplier value.</param>
/// <param name="MoneyFlowVolume">Money flow volume value.</param>
/// <param name="Cmf">Chaikin Money Flow value.</param>
[Serializable]
public record CmfResult
(
    DateTime Timestamp,
    double? MoneyFlowMultiplier,
    double? MoneyFlowVolume,
    double? Cmf
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Cmf.Null2NaN();
}
