namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Standard Deviation Channels calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the data point.</param>
/// <param name="Centerline">Value of the centerline at this point.</param>
/// <param name="UpperChannel">Value of the upper channel at this point.</param>
/// <param name="LowerChannel">Value of the lower channel at this point.</param>
/// <param name="BreakPoint">Indicates if the current point is a breakpoint.</param>
[Serializable]
public record StdDevChannelsResult
(
    DateTime Timestamp,
    double? Centerline = null,
    double? UpperChannel = null,
    double? LowerChannel = null,
    bool BreakPoint = false
) : ISeries;
