namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Triple Exponential Moving Average (TEMA) calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the data point.</param>
/// <param name="Tema">Value of the TEMA at this point.</param>
[Serializable]
public record TemaResult
(
    DateTime Timestamp,
    double? Tema = null
) : IReusable
{
    /// <summary>
    /// internal state (not exposed publicly) to support robust stream recalculations
    /// </summary>
    [JsonIgnore] internal double Ema1 { get; init; }
    [JsonIgnore] internal double Ema2 { get; init; }
    [JsonIgnore] internal double Ema3 { get; init; }

    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => Tema.Null2NaN();
}
