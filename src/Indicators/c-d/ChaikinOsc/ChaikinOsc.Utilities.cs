namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the Chaikin Oscillator indicator.
/// </summary>
public static partial class ChaikinOsc
{
    /// <summary>
    /// Removes the warmup periods and the further periods Chaikin Oscillator needs for its values to converge.
    /// </summary>
    /// <param name="results">Chaikin Oscillator results to evaluate.</param>
    /// <returns>Chaikin Oscillator results with the warmup periods removed.</returns>
    public static IReadOnlyList<ChaikinOscResult> RemoveWarmupPeriods(
        this IReadOnlyList<ChaikinOscResult> results)
        => results.RemoveBeforeFirstValue(
            static x => x.Oscillator != null,
            static i => i + 101);

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
