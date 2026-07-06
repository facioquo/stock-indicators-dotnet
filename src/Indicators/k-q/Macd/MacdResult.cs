namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a MACD (Moving Average Convergence Divergence) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the MACD result.</param>
/// <param name="Macd">MACD value.</param>
/// <param name="Signal">Signal line value.</param>
/// <param name="Histogram">Histogram value.</param>
/// <param name="FastEma">Fast EMA value.</param>
/// <param name="SlowEma">Slow EMA value.</param>
[Serializable]
public record MacdResult
(
    DateTime Timestamp,
    double? Macd,
    double? Signal,
    double? Histogram,

    // extra/interim data
    double? FastEma,
    double? SlowEma

) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Macd.Null2NaN();
}
