namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for Stochastic Momentum Index (SMI) calculations.
/// </summary>
public static partial class Smi
{
    /// <summary>
    /// Removes the recommended warmup periods from the results.
    /// </summary>
    /// <param name="results">List of SMI results.</param>
    /// <returns>A list of SMI results with the warmup periods removed.</returns>
    public static IReadOnlyList<SmiResult> RemoveWarmupPeriods(
        this IReadOnlyList<SmiResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        int removePeriods = results
            .FindIndex(static x => x.Smi != null);

        return results.Remove(removePeriods + 2 + 100);
    }

    /// <summary>
    /// Validates the parameters for the SMI calculation.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="firstSmoothPeriods">Number of first smoothing periods.</param>
    /// <param name="secondSmoothPeriods">Number of second smoothing periods.</param>
    /// <param name="signalPeriods">Number of signal periods.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when any of the parameters are less than or equal to 0.
    /// </exception>
    internal static void Validate(
        int lookbackPeriods,
        int firstSmoothPeriods,
        int secondSmoothPeriods,
        int signalPeriods)
    {
        // check parameter arguments
        if (lookbackPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for SMI.");
        }

        if (firstSmoothPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(firstSmoothPeriods), firstSmoothPeriods,
                "Smoothing periods must be greater than 0 for SMI.");
        }

        if (secondSmoothPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(secondSmoothPeriods), secondSmoothPeriods,
                "Smoothing periods must be greater than 0 for SMI.");
        }

        if (signalPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(signalPeriods), signalPeriods,
                "Signal periods must be greater than 0 for SMI.");
        }
    }
}
