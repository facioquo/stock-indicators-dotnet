namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the Connors RSI indicator.
/// </summary>
public static partial class ConnorsRsi
{
    /// <summary>
    /// Validates the parameters for the Connors RSI calculation.
    /// </summary>
    /// <param name="rsiPeriods">Number of periods for the RSI calculation on the close price.</param>
    /// <param name="streakPeriods">Number of periods for the RSI calculation on the streak.</param>
    /// <param name="rankPeriods">Number of periods for the percent rank calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the RSI periods for the close price are less than or equal to 1,
    /// the RSI periods for the streak are less than or equal to 1,
    /// or the percent rank periods are less than or equal to 1.
    /// </exception>
    internal static void Validate(
        int rsiPeriods,
        int streakPeriods,
        int rankPeriods)
    {
        // check parameter arguments
        if (rsiPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(rsiPeriods), rsiPeriods,
                "RSI period for Close price must be greater than 1 for ConnorsRsi.");
        }

        if (streakPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(streakPeriods), streakPeriods,
                "RSI period for Streak must be greater than 1 for ConnorsRsi.");
        }

        if (rankPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(rankPeriods), rankPeriods,
                "Percent Rank periods must be greater than 1 for ConnorsRsi.");
        }
    }
}
