namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for Kaufman's Adaptive Moving Average (KAMA) calculations.
/// </summary>
public static partial class Kama
{
    /// <summary>
    /// Removes the recommended warmup periods from the KAMA results.
    /// </summary>
    /// <param name="results">List of KAMA results to process.</param>
    /// <returns>A list of KAMA results with the warmup periods removed.</returns>
    public static IReadOnlyList<KamaResult> RemoveWarmupPeriods(
        this IReadOnlyList<KamaResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        int erPeriods = results
            .FindIndex(static x => x.Er != null);

        return results.Remove(Math.Max(erPeriods + 100, 10 * erPeriods));
    }

    /// <summary>
    /// Validates the parameters for the KAMA calculation.
    /// </summary>
    /// <param name="erPeriods">Number of periods for the Efficiency Ratio (ER).</param>
    /// <param name="fastPeriods">Number of periods for the fast EMA.</param>
    /// <param name="slowPeriods">Number of periods for the slow EMA.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when any of the parameters are out of their valid range.
    /// </exception>
    internal static void Validate(
        int erPeriods,
        int fastPeriods,
        int slowPeriods)
    {
        // check parameter arguments
        if (erPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(erPeriods), erPeriods,
                "Efficiency Ratio periods must be greater than 0 for KAMA.");
        }

        if (fastPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fastPeriods), fastPeriods,
                "Fast EMA periods must be greater than 0 for KAMA.");
        }

        if (slowPeriods <= fastPeriods)
        {
            throw new ArgumentOutOfRangeException(nameof(slowPeriods), slowPeriods,
                "Slow EMA periods must be greater than Fast EMA period for KAMA.");
        }
    }
}
