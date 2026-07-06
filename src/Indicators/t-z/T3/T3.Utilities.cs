namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the T3 moving average indicator.
/// </summary>
public static partial class T3
{
    /// <summary>
    /// Validates the parameters for the T3 calculation.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="volumeFactor">Volume factor.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods or volume factor are less than or equal to 0.</exception>
    internal static void Validate(
        int lookbackPeriods,
        double volumeFactor)
    {
        // check parameter arguments
        if (lookbackPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for T3.");
        }

        if (volumeFactor <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(volumeFactor), volumeFactor,
                "Volume Factor must be greater than 0 for T3.");
        }
    }
}
