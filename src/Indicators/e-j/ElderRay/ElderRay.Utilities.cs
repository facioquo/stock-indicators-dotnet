namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for Elder Ray calculations.
/// </summary>
public static partial class ElderRay
{
    /// <summary>
    /// Removes the warmup periods and the further periods Elder-ray needs for its values to converge.
    /// </summary>
    /// <param name="results">Elder-ray results to evaluate.</param>
    /// <returns>Elder-ray results with the warmup periods removed.</returns>
    public static IReadOnlyList<ElderRayResult> RemoveWarmupPeriods(
        this IReadOnlyList<ElderRayResult> results)
        => results.RemoveBeforeFirstValue(
            static x => x.BullPower != null,
            static i => i + 101);

    /// <summary>
    /// Validates the lookback periods for Elder Ray calculations.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the lookback periods are less than or equal to 0.
    /// </exception>
    internal static void Validate(
        int lookbackPeriods)
    {
        // check parameter arguments
        if (lookbackPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for Elder-ray Index.");
        }
    }
}
