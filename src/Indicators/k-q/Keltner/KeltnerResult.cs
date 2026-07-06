namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of the Keltner Channel calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="UpperBand">Upper band value of the Keltner Channel.</param>
/// <param name="Centerline">Centerline value of the Keltner Channel.</param>
/// <param name="LowerBand">Lower band value of the Keltner Channel.</param>
/// <param name="Width">Width of the Keltner Channel.</param>
[Serializable]
public record KeltnerResult
(
    DateTime Timestamp,
    double? UpperBand = null,
    double? Centerline = null,
    double? LowerBand = null,
    double? Width = null
) : ISeries
{
    /// <summary>
    /// Gets the Average True Range value (interim data for internal streaming calculations).
    /// </summary>
    internal double? Atr { get; init; }
}
