namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Rate of Change with Bands (RocWb) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the RocWb result.</param>
/// <param name="Roc">Rate of change value of the RocWb result.</param>
/// <param name="RocEma">Exponential moving average of the rate of change.</param>
/// <param name="UpperBand">Upper band value of the RocWb result.</param>
/// <param name="LowerBand">Lower band value of the RocWb result.</param>
[Serializable]
public record RocWbResult
(
    DateTime Timestamp,
    double? Roc,
    double? RocEma,
    double? UpperBand,
    double? LowerBand
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Roc.Null2NaN();
}
