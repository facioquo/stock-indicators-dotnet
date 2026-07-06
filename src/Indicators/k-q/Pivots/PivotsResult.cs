namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a pivot points calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the pivot point.</param>
/// <param name="HighPoint">High point value.</param>
/// <param name="LowPoint">Low point value.</param>
/// <param name="HighLine">High line value.</param>
/// <param name="LowLine">Low line value.</param>
/// <param name="HighTrend">High trend direction.</param>
/// <param name="LowTrend">Low trend direction.</param>
[Serializable]
public record PivotsResult
(
   DateTime Timestamp,
   decimal? HighPoint,
   decimal? LowPoint,
   decimal? HighLine,
   decimal? LowLine,
   PivotTrend? HighTrend,
   PivotTrend? LowTrend
) : ISeries;
