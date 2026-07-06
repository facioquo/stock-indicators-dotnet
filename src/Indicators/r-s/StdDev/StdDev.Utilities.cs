namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for Standard deviation (StdDev) calculations.
/// </summary>
public static partial class StdDev
{
    /// <summary>
    /// Validates the lookback periods parameter.
    /// </summary>
    /// <param name="lookbackPeriods">Number of lookback periods to validate.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than or equal to 1.</exception>
    internal static void Validate(
        int lookbackPeriods)
    {
        // check parameter arguments
        if (lookbackPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 1 for Standard deviation.");
        }
    }
}
