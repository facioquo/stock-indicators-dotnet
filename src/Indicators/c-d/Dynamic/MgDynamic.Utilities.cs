namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for McGinley Dynamic calculations.
/// </summary>
public static partial class MgDynamic
{
    /// <summary>
    /// Calculates the increment for the McGinley Dynamic.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="kFactor">Smoothing factor for the calculation.</param>
    /// <param name="newVal">New value.</param>
    /// <param name="prevDyn">Previous dynamic value.</param>
    /// <returns>Calculated increment.</returns>
    public static double Increment(
        int lookbackPeriods,
        double kFactor,
        double newVal,
        double prevDyn)
        => prevDyn + (
            (newVal - prevDyn)
          / (kFactor * lookbackPeriods * Math.Pow(newVal / prevDyn, 4)));

    /// <summary>
    /// Validates the parameters for McGinley Dynamic calculations.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="kFactor">Smoothing factor for the calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the lookback periods or kFactor are less than or equal to 0.
    /// </exception>
    internal static void Validate(
        int lookbackPeriods,
        double kFactor)
    {
        // check parameter arguments
        if (lookbackPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for DYNAMIC.");
        }

        if (kFactor <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(kFactor), kFactor,
                "K-Factor range adjustment must be greater than 0 for DYNAMIC.");
        }
    }
}
