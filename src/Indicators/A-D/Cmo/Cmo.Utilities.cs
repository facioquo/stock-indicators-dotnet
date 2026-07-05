namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the CMO (Chande Momentum Oscillator) indicator.
/// </summary>
public static partial class Cmo
{
    /// <summary>
    /// Validates the parameters for the CMO calculation.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than or equal to 0.</exception>
    internal static void Validate(
        int lookbackPeriods)
    {
        // check parameter arguments
        if (lookbackPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for CMO.");
        }
    }

    /// <summary>
    /// Calculates CMO from a collection of ticks.
    /// </summary>
    /// <param name="ticks">Collection of ticks with direction and value.</param>
    /// <returns>Calculated CMO value, or null if not enough valid data.</returns>
    internal static double? PeriodCalculation(IEnumerable<(bool? isUp, double value)> ticks)
    {
        double sH = 0;
        double sL = 0;

        foreach ((bool? isUp, double pDiff) in ticks)
        {
            if (double.IsNaN(pDiff))
            {
                return null;
            }

            // up
            if (isUp is true)
            {
                sH += pDiff;
            }
            // down
            else
            {
                sL += pDiff;
            }
        }

        return sH + sL != 0
            ? (100 * (sH - sL) / (sH + sL)).NaN2Null()
            : 0d;
    }
}
