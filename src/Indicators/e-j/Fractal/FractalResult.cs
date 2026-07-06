namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Fractal calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="FractalBear">Fractal Bear value.</param>
/// <param name="FractalBull">Fractal Bull value.</param>
[Serializable]
public record FractalResult
(
    DateTime Timestamp,
    decimal? FractalBear,
    decimal? FractalBull
) : ISeries;
