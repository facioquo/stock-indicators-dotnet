namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the Williams %R indicator.
/// </summary>
public static partial class WilliamsR
{
    // parameter validation
    /// <summary>
    /// Validates the parameters for the Williams %R calculation.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the lookback periods are less than or equal to 0.
    /// </exception>
    internal static void Validate(
        int lookbackPeriods)
    {
        // check parameter arguments
        if (lookbackPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for William %R.");
        }
    }
}
