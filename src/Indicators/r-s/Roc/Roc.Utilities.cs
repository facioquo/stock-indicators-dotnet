namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for Rate of Change (ROC) calculations.
/// </summary>
public static partial class Roc
{
    /// <summary>
    /// Validates the parameters for ROC calculations.
    /// </summary>
    /// <param name="lookbackPeriods">Number of periods to look back for the ROC calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than or equal to 0.</exception>
    internal static void Validate(
        int lookbackPeriods)
    {
        // check parameter arguments
        if (lookbackPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for ROC.");
        }
    }
}
