namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the MACD (Moving Average Convergence Divergence) oscillator.
/// </summary>
public static partial class Macd
{
    /// <summary>
    /// Removes the warmup periods and the further periods MACD needs for its values to converge.
    /// </summary>
    /// <param name="results">MACD results to evaluate.</param>
    /// <returns>MACD results with the warmup periods removed.</returns>
    public static IReadOnlyList<MacdResult> RemoveWarmupPeriods(
        this IReadOnlyList<MacdResult> results)
        => results.RemoveBeforeFirstValue(
            static x => x.Signal != null,
            static i => i + 252);

    /// <summary>
    /// Validates the parameters for the MACD calculation.
    /// </summary>
    /// <param name="fastPeriods">Number of periods for the fast EMA.</param>
    /// <param name="slowPeriods">Number of periods for the slow EMA.</param>
    /// <param name="signalPeriods">Number of periods for the signal line.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when any of the parameters are out of their valid range.
    /// </exception>
    internal static void Validate(
        int fastPeriods,
        int slowPeriods,
        int signalPeriods)
    {
        // check parameter arguments
        if (fastPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fastPeriods), fastPeriods,
                "Fast periods must be greater than 0 for MACD.");
        }

        if (signalPeriods < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(signalPeriods), signalPeriods,
                "Signal periods must be greater than or equal to 0 for MACD.");
        }

        if (slowPeriods <= fastPeriods)
        {
            throw new ArgumentOutOfRangeException(nameof(slowPeriods), slowPeriods,
                "Slow periods must be greater than the fast period for MACD.");
        }
    }
}
