namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a SuperTrend indicator calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the data point.</param>
/// <param name="SuperTrend">Value of the SuperTrend at this point.</param>
/// <param name="UpperBand">Upper band value at this point.</param>
/// <param name="LowerBand">Lower band value at this point.</param>
[Serializable]
public record SuperTrendResult
(
    DateTime Timestamp,
    decimal? SuperTrend,
    decimal? UpperBand,
    decimal? LowerBand
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => SuperTrend.Null2NaN();
}
