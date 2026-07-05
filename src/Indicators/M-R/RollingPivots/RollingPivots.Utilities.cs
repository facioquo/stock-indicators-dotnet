namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for Rolling Pivot Points calculations.
/// </summary>
public static partial class RollingPivots
{
    /// <summary>
    /// Removes the recommended warmup periods from the Rolling Pivots results.
    /// </summary>
    /// <inheritdoc cref="Reusable.RemoveWarmupPeriods{T}(IReadOnlyList{T})"/>
    public static IReadOnlyList<RollingPivotsResult> RemoveWarmupPeriods(
        this IReadOnlyList<RollingPivotsResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        int removePeriods = results
            .FindIndex(static x => x.PP != null);

        return results.Remove(removePeriods);
    }

    /// <summary>
    /// Returns the minimum number of source items required to produce a valid Rolling Pivot result
    /// (equivalent to windowPeriods + offsetPeriods, the item count before the first non-null result).
    /// </summary>
    /// <param name="windowPeriods">Number of periods in the rolling window.</param>
    /// <param name="offsetPeriods">Number of periods to offset the window.</param>
    /// <returns>Minimum warmup period count.</returns>
    public static int WarmupPeriod(
        int windowPeriods,
        int offsetPeriods)
        => windowPeriods + offsetPeriods;

    /// <summary>
    /// Validates the parameters for Rolling Pivots calculations.
    /// </summary>
    /// <param name="windowPeriods">Number of periods in the rolling window.</param>
    /// <param name="offsetPeriods">Number of periods to offset the window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any of the parameters are out of range.</exception>
    internal static void Validate(
        int windowPeriods,
        int offsetPeriods)
    {
        // check parameter arguments
        if (windowPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(windowPeriods), windowPeriods,
                "Window periods must be greater than 0 for Rolling Pivot Points.");
        }

        if (offsetPeriods < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offsetPeriods), offsetPeriods,
                "Offset periods must be greater than or equal to 0 for Rolling Pivot Points.");
        }
    }
}
