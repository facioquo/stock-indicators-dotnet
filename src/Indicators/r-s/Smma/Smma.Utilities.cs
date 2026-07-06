namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for Smoothed Moving Average (SMMA) calculations.
/// </summary>
public static partial class Smma
{
    /// <summary>
    /// Removes the recommended warmup periods from the results.
    /// </summary>
    /// <param name="results">List of SMMA results.</param>
    /// <returns>A list of SMMA results with the warmup periods removed.</returns>
    public static IReadOnlyList<SmmaResult> RemoveWarmupPeriods(
        this IReadOnlyList<SmmaResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        int n = results
            .FindIndex(static x => x.Smma != null) + 1;

        return results.Remove(n + 100);
    }

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
