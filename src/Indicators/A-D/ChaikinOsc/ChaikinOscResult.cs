namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of the Chaikin Oscillator calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="MoneyFlowMultiplier">Money flow multiplier value.</param>
/// <param name="MoneyFlowVolume">Money flow volume value.</param>
/// <param name="Adl">Accumulation/Distribution Line (ADL) value.</param>
/// <param name="Oscillator">Chaikin Oscillator value.</param>
/// <param name="FastEma">Fast EMA value.</param>
/// <param name="SlowEma">Slow EMA value.</param>
[Serializable]
public record ChaikinOscResult
(
    DateTime Timestamp,
    double? MoneyFlowMultiplier,
    double? MoneyFlowVolume,
    double? Adl,
    double? Oscillator,

    // extra/interim data
    double? FastEma = null,
    double? SlowEma = null
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Oscillator.Null2NaN();
}
