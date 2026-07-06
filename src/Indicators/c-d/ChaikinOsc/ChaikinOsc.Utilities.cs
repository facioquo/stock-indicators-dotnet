namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the Chaikin Oscillator indicator.
/// </summary>
public static partial class ChaikinOsc
{
    /// <summary>
    /// Removes the recommended warmup periods from the Chaikin Oscillator results.
    /// </summary>
    /// <param name="results">List of Chaikin Oscillator results.</param>
    /// <returns>A list of Chaikin Oscillator results with the warmup periods removed.</returns>
    public static IReadOnlyList<ChaikinOscResult> RemoveWarmupPeriods(
        this IReadOnlyList<ChaikinOscResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        int s = results
            .FindIndex(static x => x.Oscillator != null) + 1;

        return results.Remove(s + 100);
    }

    /// <summary>
    /// Validates the parameters for the Chaikin Oscillator calculation.
    /// </summary>
    /// <param name="fastPeriods">Number of fast lookback periods for the calculation.</param>
    /// <param name="slowPeriods">Number of slow lookback periods for the calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the fast lookback periods are less than or equal to 0,
    /// or the slow lookback periods are less than or equal to the fast lookback periods.
    /// </exception>
    internal static void Validate(
        int fastPeriods,
        int slowPeriods)
    {
        // check parameter arguments
        if (fastPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fastPeriods), fastPeriods,
                "Fast lookback periods must be greater than 0 for Chaikin Oscillator.");
        }

        if (slowPeriods <= fastPeriods)
        {
            throw new ArgumentOutOfRangeException(nameof(slowPeriods), slowPeriods,
                "Slow lookback periods must be greater than Fast lookback period for Chaikin Oscillator.");
        }
    }
}
