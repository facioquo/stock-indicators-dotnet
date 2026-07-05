namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the result of the Connors RSI calculation.
/// </summary>
/// <param name="Timestamp">Timestamp of the result.</param>
/// <param name="Streak">Streak value.</param>
/// <param name="Rsi">RSI value.</param>
/// <param name="RsiStreak">RSI streak value.</param>
/// <param name="PercentRank">Percent rank value.</param>
/// <param name="ConnorsRsi">Connors RSI value.</param>
[Serializable]
public record ConnorsRsiResult
(
    DateTime Timestamp,
    double Streak,
    double? Rsi = null,
    double? RsiStreak = null,
    double? PercentRank = null,
    double? ConnorsRsi = null
) : IReusable
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => ConnorsRsi.Null2NaN();
}
