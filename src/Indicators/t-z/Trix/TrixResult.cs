namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a TRIX indicator calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the data point.</param>
/// <param name="Ema3">Value of the triple-smoothed EMA at this point.</param>
/// <param name="Trix">Value of the TRIX indicator at this point.</param>
[Serializable]
public record TrixResult
(
    DateTime Timestamp,
    double? Ema3 = null,
    double? Trix = null
) : IReusable
{
    /// <summary>
    /// internal state (not exposed publicly) to support robust stream recalculations
    /// </summary>
    [JsonIgnore] internal double Ema1 { get; init; }
    [JsonIgnore] internal double Ema2 { get; init; }

    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Trix.Null2NaN();
}
