namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the Percentage Volume Oscillator (PVO).
/// </summary>
public static partial class Pvo
{
    /// <summary>
    /// Removes the warmup periods and the further periods PVO needs for its values to converge.
    /// </summary>
    /// <param name="results">PVO results to evaluate.</param>
    /// <returns>PVO results with the warmup periods removed.</returns>
    public static IReadOnlyList<PvoResult> RemoveWarmupPeriods(
        this IReadOnlyList<PvoResult> results)
        => results.RemoveBeforeFirstValue(
            static x => x.Signal != null,
            static i => i + 252);

    /// <summary>
    /// Validates the parameters for PVO calculations.
    /// </summary>
    /// <param name="fastPeriods">Number of periods for the fast EMA.</param>
    /// <param name="slowPeriods">Number of periods for the slow EMA.</param>
    /// <param name="signalPeriods">Number of periods for the signal line.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any parameter is out of range.</exception>
    internal static void Validate(
        int fastPeriods,
        int slowPeriods,
        int signalPeriods)
    {
        // check parameter arguments
        if (fastPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fastPeriods), fastPeriods,
                "Fast periods must be greater than 0 for PVO.");
        }

        if (signalPeriods < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(signalPeriods), signalPeriods,
                "Signal periods must be greater than or equal to 0 for PVO.");
        }

        if (slowPeriods <= fastPeriods)
        {
            throw new ArgumentOutOfRangeException(nameof(slowPeriods), slowPeriods,
                "Slow periods must be greater than the fast period for PVO.");
        }
    }
}
