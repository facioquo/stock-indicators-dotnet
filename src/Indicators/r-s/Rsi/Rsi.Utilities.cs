namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for Relative Strength Index (RSI) calculations.
/// </summary>
public static partial class Rsi
{
    /// <summary>
    /// Removes the warmup periods and the further periods RSI needs for its values to converge.
    /// </summary>
    /// <param name="results">RSI results to evaluate.</param>
    /// <returns>RSI results with the warmup periods removed.</returns>
    public static IReadOnlyList<RsiResult> RemoveWarmupPeriods(
        this IReadOnlyList<RsiResult> results)
        => results.RemoveBeforeFirstValue(
            static x => x.Rsi != null,
            static i => 10 * i);

    /// <summary>
    /// Validates the parameters for RSI calculations.
    /// </summary>
    /// <param name="lookbackPeriods">Number of periods to look back for the RSI calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than 1.</exception>
    internal static void Validate(
        int lookbackPeriods)
    {
        // check parameter arguments
        if (lookbackPeriods < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for RSI.");
        }
    }
}
