namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Detrended Price Oscillator (DPO) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="Dpo">DPO value.</param>
/// <param name="Sma">Simple Moving Average (SMA) value.</param>
[Serializable]
public record DpoResult
(
    DateTime Timestamp,
    double? Dpo = null,
    double? Sma = null
    ) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Dpo.Null2NaN();
}
