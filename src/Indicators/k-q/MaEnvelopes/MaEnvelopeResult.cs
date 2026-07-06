namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Moving Average Envelope calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="Centerline">Value of the centerline (moving average).</param>
/// <param name="UpperEnvelope">Value of the upper envelope.</param>
/// <param name="LowerEnvelope">Value of the lower envelope.</param>
[Serializable]
public record MaEnvelopeResult
(
    DateTime Timestamp,
    double? Centerline,
    double? UpperEnvelope,
    double? LowerEnvelope
) : ISeries;
