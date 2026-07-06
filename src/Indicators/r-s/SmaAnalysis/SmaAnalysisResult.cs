namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Simple Moving Average (SMA) calculation
/// with extended analysis values.
/// </summary>
/// <param name="Timestamp">Timestamp of the data point.</param>
/// <param name="Sma">Simple Moving Average (SMA) at this point.</param>
/// <param name="Mad">Mean absolute deviation (MAD) at this point.</param>
/// <param name="Mse">Mean square error (MSE) at this point.</param>
/// <param name="Mape">Mean absolute percentage error (MAPE) at this point.</param>
[Serializable]
public record SmaAnalysisResult
(
    DateTime Timestamp,
    double? Sma = null,
    double? Mad = null,
    double? Mse = null,
    double? Mape = null
) : SmaResult(Timestamp, Sma);
