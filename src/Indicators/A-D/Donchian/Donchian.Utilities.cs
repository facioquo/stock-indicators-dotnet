namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for Donchian Channel calculations.
/// </summary>
public static partial class Donchian
{
    /// <summary>
    /// Removes empty (null) periods from the Donchian Channel results.
    /// </summary>
    /// <inheritdoc cref="Reusable.Condense{T}(IReadOnlyList{T})"/>
    public static IReadOnlyList<DonchianResult> Condense(
        this IReadOnlyList<DonchianResult> results)
    {
        List<DonchianResult> resultsList = results
            .ToList();

        resultsList
            .RemoveAll(match:
                static x => x.UpperBand is null && x.LowerBand is null && x.Centerline is null);

        return resultsList.ToSortedList();
    }

    /// <summary>
    /// Removes the recommended warmup periods from the Donchian Channel results.
    /// </summary>
    /// <inheritdoc cref="Reusable.RemoveWarmupPeriods{T}(IReadOnlyList{T})"/>
    public static IReadOnlyList<DonchianResult> RemoveWarmupPeriods(
        this IReadOnlyList<DonchianResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        int removePeriods = results
          .FindIndex(static x => x.Width != null);

        return results.Remove(removePeriods);
    }

    /// <summary>
    /// Validates the lookback periods for Donchian Channel calculations.
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
                "Lookback periods must be greater than 0 for Donchian Channel.");
        }
    }
}
