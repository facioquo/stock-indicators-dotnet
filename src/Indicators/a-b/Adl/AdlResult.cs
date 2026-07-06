namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of the Accumulation/Distribution Line (ADL) indicator.
/// </summary>
/// <param name="Timestamp">Gets the timestamp of the result.</param>
/// <param name="Adl">Gets the ADL value.</param>
/// <param name="MoneyFlowMultiplier">Gets the Money Flow Multiplier.</param>
/// <param name="MoneyFlowVolume">Gets the Money Flow Volume.</param>
[Serializable]
public record AdlResult
(
    DateTime Timestamp,
    double Adl,
    double? MoneyFlowMultiplier = null,
    double? MoneyFlowVolume = null
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Adl;
}
