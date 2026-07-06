namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the Chandelier Exit indicator.
/// </summary>
public static partial class Chandelier
{
    /// <summary>
    /// Validates the parameters for the Chandelier Exit calculation.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="multiplier">Multiplier for the ATR calculation, must be greater than 0.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the lookback periods are less than or equal to 0,
    /// or the multiplier is less than or equal to 0.
    /// </exception>
    internal static void Validate(
        int lookbackPeriods,
        double multiplier)
    {
        // check parameter arguments
        if (lookbackPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for Chandelier Exit.");
        }

        if (multiplier <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(multiplier), multiplier,
                "Multiplier must be greater than 0 for Chandelier Exit.");
        }
    }
}
