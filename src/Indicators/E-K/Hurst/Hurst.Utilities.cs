namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for Hurst Exponent calculations.
/// </summary>
public static partial class Hurst
{
    /// <summary>
    /// Validates the lookback periods for Hurst Exponent calculations.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the lookback periods are less than 20.
    /// </exception>
    internal static void Validate(
        int lookbackPeriods)
    {
        // check parameter arguments
        if (lookbackPeriods < 20)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be at least 20 for Hurst Exponent.");
        }
    }
}
