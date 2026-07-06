namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Fractal Channel Band (FCB) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="UpperBand">Upper band value.</param>
/// <param name="LowerBand">Lower band value.</param>
[Serializable]
public record FcbResult
(
    DateTime Timestamp,
    decimal? UpperBand,
    decimal? LowerBand
) : ISeries;
