namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the Triple EMA Oscillator (TRIX) indicator.
/// </summary>
public static partial class Trix
{
    /// <summary>
    /// Removes the recommended warmup periods from the TRIX results.
    /// </summary>
    /// <param name="results">List of TRIX results.</param>
    /// <returns>A list of TRIX results with the warmup periods removed.</returns>
    public static IReadOnlyList<TrixResult> RemoveWarmupPeriods(
        this IReadOnlyList<TrixResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        int n = results
            .FindIndex(static x => x.Trix != null);

        return results.Remove((3 * n) + 100);
    }

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
