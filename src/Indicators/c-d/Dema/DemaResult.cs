namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a DEMA (Double Exponential Moving Average) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the DEMA result.</param>
/// <param name="Dema">DEMA value.</param>
[Serializable]
public record DemaResult
(
    DateTime Timestamp,
    double? Dema = null
) : IReusable
{
    /// <summary>
    /// internal state (not exposed publicly) to support robust stream recalculations
    /// </summary>
    [JsonIgnore] internal double Ema1 { get; init; }
    [JsonIgnore] internal double Ema2 { get; init; }

    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Dema.Null2NaN();
}
