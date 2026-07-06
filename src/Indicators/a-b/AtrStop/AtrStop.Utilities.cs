namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the ATR Trailing Stop indicator.
/// </summary>
public static partial class AtrStop
{
    /// <summary>
    /// Removes empty (null) periods from the ATR Trailing Stop results.
    /// </summary>
    /// <param name="results">List of ATR Trailing Stop results.</param>
    /// <returns>A list of ATR Trailing Stop results with empty periods removed.</returns>
    public static IReadOnlyList<AtrStopResult> Condense(
        this IReadOnlyList<AtrStopResult> results)
    {
        List<AtrStopResult> resultsList = results
            .ToList();

        resultsList.RemoveAll(match: static x => x.AtrStop is null);

        return resultsList.ToSortedList();
    }

    /// <summary>
    /// Removes the recommended warmup periods from the ATR Trailing Stop results.
    /// </summary>
    /// <param name="results">List of ATR Trailing Stop results.</param>
    /// <returns>A list of ATR Trailing Stop results with the warmup periods removed.</returns>
    public static IReadOnlyList<AtrStopResult> RemoveWarmupPeriods(
        this IReadOnlyList<AtrStopResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        int removePeriods = results
            .FindIndex(static x => x.AtrStop != null);

        return results.Remove(removePeriods);
    }

    /// <summary>
    /// Validates the parameters for the ATR Trailing Stop calculation.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="multiplier">Multiplier for the ATR calculation, must be greater than 0.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the lookback periods are less than or equal to 1,
    /// or the multiplier is less than or equal to 0.
    /// </exception>
    internal static void Validate(
        int lookbackPeriods,
        double multiplier)
    {
        // check parameter arguments
        if (lookbackPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 1 for ATR Trailing Stop.");
        }

        if (multiplier <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(multiplier), multiplier,
                "Multiplier must be greater than 0 for ATR Trailing Stop.");
        }
    }
}
