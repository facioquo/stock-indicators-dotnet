namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Renko chart calculation.
/// </summary>
/// <inheritdoc cref="Bar"/>
[Serializable]
public record RenkoResult
(
    DateTime Timestamp,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume,
    bool IsUp
) : Bar(Timestamp, Open, High, Low, Close, Volume);
