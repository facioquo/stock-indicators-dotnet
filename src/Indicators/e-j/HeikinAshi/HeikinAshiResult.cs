namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of a Heikin-Ashi calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="Open">Open price of the Heikin-Ashi candle.</param>
/// <param name="High">High price of the Heikin-Ashi candle.</param>
/// <param name="Low">Low price of the Heikin-Ashi candle.</param>
/// <param name="Close">Close price of the Heikin-Ashi candle.</param>
/// <param name="Volume">Volume of the Heikin-Ashi candle.</param>
[Serializable]
public record HeikinAshiResult(
    DateTime Timestamp,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume
) : Bar(Timestamp, Open, High, Low, Close, Volume);
