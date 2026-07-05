namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the CHOP (Choppiness Index) indicator.
/// </summary>
public static partial class Chop
{
    /// <summary>
    /// Validates the parameters for the CHOP calculation.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than or equal to 1.</exception>
    internal static void Validate(
        int lookbackPeriods)
    {
        // check parameter arguments
        if (lookbackPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 1 for CHOP.");
        }
    }
}
