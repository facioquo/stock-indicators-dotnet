namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a STARC Bands calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the data point.</param>
/// <param name="UpperBand">Value of the upper band at this point.</param>
/// <param name="Centerline">Value of the centerline at this point.</param>
/// <param name="LowerBand">Value of the lower band at this point.</param>
[Serializable]
public record StarcBandsResult
(
    DateTime Timestamp,
    double? UpperBand,
    double? Centerline,
    double? LowerBand
) : ISeries;
