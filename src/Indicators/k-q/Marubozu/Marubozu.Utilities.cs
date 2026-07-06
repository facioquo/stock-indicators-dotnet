namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the Marubozu candlestick pattern.
/// </summary>
public static partial class Marubozu
{
    /// <summary>
    /// Validates the parameters for the Marubozu calculation.
    /// </summary>
    /// <param name="minBodyPercent">Minimum body percentage to qualify as a Marubozu.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the minimum body percentage is greater than 100 or less than 80.
    /// </exception>
    internal static void Validate(
        double minBodyPercent)
    {
        // check parameter arguments
        if (minBodyPercent > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minBodyPercent), minBodyPercent,
                "Minimum Body Percent must be less than 100 " +
                "for Marubozu (<=100%).");
        }

        if (minBodyPercent < 80)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minBodyPercent), minBodyPercent,
                "Minimum Body Percent must at least 80 (80%) " +
                "for Marubozu and is usually greater than 90 (90%).");
        }
    }
}
