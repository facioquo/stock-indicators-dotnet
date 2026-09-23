namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the Triple EMA Oscillator (TRIX) indicator.
/// </summary>
public static partial class Trix
{
    /// <summary>
    /// Removes the warmup periods and the further periods TRIX needs for its values to converge.
    /// </summary>
    /// <param name="results">TRIX results to evaluate.</param>
    /// <returns>TRIX results with the warmup periods removed.</returns>
    public static IReadOnlyList<TrixResult> RemoveWarmupPeriods(
        this IReadOnlyList<TrixResult> results)
        => results.RemoveBeforeFirstValue(
            static x => x.Trix != null,
            static i => (3 * i) + 100);

    /// <summary>
    /// Validates the parameters for the TRIX calculation.
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
                "Lookback periods must be greater than 0 for TRIX.");
        }
    }
}
