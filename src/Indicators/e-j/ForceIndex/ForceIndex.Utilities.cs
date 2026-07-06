namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for Force Index calculations.
/// </summary>
public static partial class ForceIndex
{
    /// <summary>
    /// Removes the recommended warmup periods from the Force Index results.
    /// </summary>
    /// <inheritdoc cref="Reusable.RemoveWarmupPeriods{T}(IReadOnlyList{T})"/>
    public static IReadOnlyList<ForceIndexResult> RemoveWarmupPeriods(
        this IReadOnlyList<ForceIndexResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        int n = results
            .FindIndex(static x => x.ForceIndex != null);

        return results.Remove(n + 100);
    }

    /// <summary>
    /// Validates the lookback periods for Force Index calculations.
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
                "Lookback periods must be greater than 0 for Force Index.");
        }
    }
}
