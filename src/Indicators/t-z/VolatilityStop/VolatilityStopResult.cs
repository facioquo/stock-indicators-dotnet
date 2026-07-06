namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Volatility Stop indicator calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the data point.</param>
/// <param name="Sar">Stop and reverse (SAR) value at this point.</param>
/// <param name="IsStop">Indicates if the current point is a stop.</param>
/// <param name="UpperBand">Upper band value at this point.</param>
/// <param name="LowerBand">Lower band value at this point.</param>
[Serializable]
public record VolatilityStopResult
(
    DateTime Timestamp,
    double? Sar = null,
    bool? IsStop = null,

    // SAR values as long/short stop bands
    double? UpperBand = null,
    double? LowerBand = null

) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Sar.Null2NaN();
}
