namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for ConnorsRsi calculations.
/// </summary>
public interface IConnorsRsi
{
    /// <summary>
    /// Gets the number of periods for the RSI calculation on the close price.
    /// </summary>
    int RsiPeriods { get; }

    /// <summary>
    /// Gets the number of periods for the RSI calculation on the streak.
    /// </summary>
    int StreakPeriods { get; }

    /// <summary>
    /// Gets the number of periods for the percent rank calculation.
    /// </summary>
    int RankPeriods { get; }
}
