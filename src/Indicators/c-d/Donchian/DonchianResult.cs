namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Donchian Channel calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="UpperBand">Upper band value of the Donchian Channel.</param>
/// <param name="Centerline">Centerline value of the Donchian Channel.</param>
/// <param name="LowerBand">Lower band value of the Donchian Channel.</param>
/// <param name="Width">Width of the Donchian Channel.</param>
[Serializable]
public record DonchianResult
(
    DateTime Timestamp,
    double? UpperBand = null,
    double? Centerline = null,
    double? LowerBand = null,
    double? Width = null
) : ISeries;
