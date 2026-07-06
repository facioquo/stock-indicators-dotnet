namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the Ultimate Oscillator indicator.
/// </summary>
public static partial class Ultimate
{
    // parameter validation
    /// <summary>
    /// Validates the parameters for the Ultimate Oscillator calculation.
    /// </summary>
    /// <param name="shortPeriods">Number of short lookback periods.</param>
    /// <param name="middleAverage">Number of middle lookback periods.</param>
    /// <param name="longPeriods">Number of long lookback periods.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when any of the periods are less than or equal to 0, or when the periods are not in increasing order.
    /// </exception>
    internal static void Validate(
        int shortPeriods,
        int middleAverage,
        int longPeriods)
    {
        // check parameter arguments
        if (shortPeriods <= 0 || middleAverage <= 0 || longPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(longPeriods), longPeriods,
                "Average periods must be greater than 0 for Ultimate Oscillator.");
        }

        if (shortPeriods >= middleAverage || middleAverage >= longPeriods)
        {
            throw new ArgumentOutOfRangeException(nameof(middleAverage), middleAverage,
                "Average periods must be increasingly larger than each other for Ultimate Oscillator.");
        }
    }
}
