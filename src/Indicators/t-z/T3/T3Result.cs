namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a T3 indicator calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the data point.</param>
/// <param name="T3">Value of the T3 indicator at this point.</param>
[Serializable]
public record T3Result
(
    DateTime Timestamp,
    double? T3
) : IReusable
{
    /// <summary>
    /// internal state (not exposed publicly) to support robust stream recalculations
    /// </summary>
    [JsonIgnore] internal double Ema1 { get; init; }
    [JsonIgnore] internal double Ema2 { get; init; }
    [JsonIgnore] internal double Ema3 { get; init; }
    [JsonIgnore] internal double Ema4 { get; init; }
    [JsonIgnore] internal double Ema5 { get; init; }
    [JsonIgnore] internal double Ema6 { get; init; }

    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => T3.Null2NaN();
}
