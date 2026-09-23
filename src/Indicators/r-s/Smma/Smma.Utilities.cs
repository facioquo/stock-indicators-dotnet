namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for Smoothed Moving Average (SMMA) calculations.
/// </summary>
public static partial class Smma
{
    /// <summary>
    /// Removes the warmup periods and the further periods SMMA needs for its values to converge.
    /// </summary>
    /// <param name="results">SMMA results to evaluate.</param>
    /// <returns>SMMA results with the warmup periods removed.</returns>
    public static IReadOnlyList<SmmaResult> RemoveWarmupPeriods(
        this IReadOnlyList<SmmaResult> results)
        => results.RemoveBeforeFirstValue(
            static x => x.Smma != null,
            static i => i + 101);

    /// <summary>
    /// Validates the lookback periods parameter.
    /// </summary>
    /// <param name="lookbackPeriods">Number of lookback periods to validate.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than or equal to 0.</exception>
    internal static void Validate(
        int lookbackPeriods)
    {
        // check parameter arguments
        if (lookbackPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for SMMA.");
        }
    }
}
