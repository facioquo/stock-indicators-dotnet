namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for calculating the True Strength Index (TSI).
/// </summary>
public static partial class Tsi
{
    // remove recommended periods
    /// <summary>
    /// Removes the warmup periods and the further periods TSI needs for its values to converge.
    /// </summary>
    /// <param name="results">TSI results to evaluate.</param>
    /// <returns>TSI results with the warmup periods removed.</returns>
    public static IReadOnlyList<TsiResult> RemoveWarmupPeriods(
        this IReadOnlyList<TsiResult> results)
        => results.RemoveBeforeFirstValue(
            static x => x.Tsi != null,
            static i => i + 251);

    // parameter validation
    /// <summary>
    /// Validates the parameters for the TSI calculation.
    /// </summary>
    /// <param name="lookbackPeriods">Lookback periods.</param>
    /// <param name="smoothPeriods">Smoothing periods.</param>
    /// <param name="signalPeriods">Signal periods.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any of the parameters are out of range.</exception>
    internal static void Validate(
        int lookbackPeriods,
        int smoothPeriods,
        int signalPeriods)
    {
        // check parameter arguments
        if (lookbackPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for TSI.");
        }

        if (smoothPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(smoothPeriods), smoothPeriods,
                "Smoothing periods must be greater than 0 for TSI.");
        }

        if (signalPeriods < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(signalPeriods), signalPeriods,
                "Signal periods must be greater than or equal to 0 for TSI.");
        }
    }
}
