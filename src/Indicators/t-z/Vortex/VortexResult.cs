namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Vortex indicator calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the data point.</param>
/// <param name="Pvi">Positive vortex indicator value at this point.</param>
/// <param name="Nvi">Negative vortex indicator value at this point.</param>
[Serializable]
public record VortexResult
(
    DateTime Timestamp,
    double? Pvi = null,
    double? Nvi = null
) : ISeries;
