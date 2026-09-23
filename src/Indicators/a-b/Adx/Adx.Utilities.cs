namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the ADX (Average Directional Index) indicator.
/// </summary>
public static partial class Adx
{
    /// <summary>
    /// Removes the warmup periods and the further periods ADX needs for its values to converge.
    /// </summary>
    /// <param name="results">ADX results to evaluate.</param>
    /// <returns>ADX results with the warmup periods removed.</returns>
    public static IReadOnlyList<AdxResult> RemoveWarmupPeriods(
        this IReadOnlyList<AdxResult> results)
        => results.RemoveBeforeFirstValue(
            static x => x.Pdi != null,
            static i => (2 * i) + 100);

    /// <summary>
    /// Validates the parameters for the ADX calculation.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than or equal to 1.</exception>
    internal static void Validate(
        int lookbackPeriods)
    {
        // check parameter arguments
        if (lookbackPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 1 for ADX.");
        }
    }
}
