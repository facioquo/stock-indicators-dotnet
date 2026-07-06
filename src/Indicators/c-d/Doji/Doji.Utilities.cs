namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for Doji candlestick patterns.
/// </summary>
public static partial class Doji
{
    /// <summary>
    /// Validates the maximum price change percentage for Doji candlestick patterns.
    /// </summary>
    /// <param name="maxPriceChangePercent">Maximum price change percentage to validate.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the max price change percent is less than 0 or greater than 0.5.
    /// </exception>
    internal static void Validate(
        double maxPriceChangePercent)
    {
        // check parameter arguments
        if (maxPriceChangePercent is < 0 or > 0.5)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPriceChangePercent), maxPriceChangePercent,
                "Maximum Percent Change must be between 0 and 0.5 for Doji (0% to 0.5%).");
        }
    }
}
