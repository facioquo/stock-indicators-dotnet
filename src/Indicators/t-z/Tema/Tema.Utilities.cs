namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the Triple Exponential Moving Average (TEMA) indicator.
/// </summary>
public static partial class Tema
{
    /// <summary>
    /// Removes the warmup periods and the further periods TEMA needs for its values to converge.
    /// </summary>
    /// <param name="results">TEMA results to evaluate.</param>
    /// <returns>TEMA results with the warmup periods removed.</returns>
    public static IReadOnlyList<TemaResult> RemoveWarmupPeriods(
        this IReadOnlyList<TemaResult> results)
        => results.RemoveBeforeFirstValue(
            static x => x.Tema != null,
            static i => 3 * i + 103);

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
