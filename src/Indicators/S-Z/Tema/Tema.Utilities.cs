namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the Triple Exponential Moving Average (TEMA) indicator.
/// </summary>
public static partial class Tema
{
    /// <summary>
    /// Removes the recommended warmup periods from the results.
    /// </summary>
    /// <param name="results">List of TEMA results.</param>
    /// <returns>A list of TEMA results with warmup periods removed.</returns>
    public static IReadOnlyList<TemaResult> RemoveWarmupPeriods(
        this IReadOnlyList<TemaResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        int n = results
          .FindIndex(static x => x.Tema != null) + 1;

        return results.Remove((3 * n) + 100);
    }

    /// <summary>
    /// Validates the parameters for TEMA calculation.
    /// </summary>
    /// <param name="lookbackPeriods">Number of periods for the lookback.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a parameter is out of range.</exception>
    internal static void Validate(
        int lookbackPeriods)
    {
        // check parameter arguments
        if (lookbackPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for TEMA.");
        }
    }
}
