namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the Keltner Channel indicator.
/// </summary>
public static partial class Keltner
{
    /// <summary>
    /// Removes empty (null) periods from the Keltner Channel results.
    /// </summary>
    /// <param name="results">List of Keltner Channel results to condense.</param>
    /// <returns>A condensed list of Keltner Channel results without null periods.</returns>
    public static IReadOnlyList<KeltnerResult> Condense(
        this IReadOnlyList<KeltnerResult> results)
    {
        List<KeltnerResult> resultsList = results
            .ToList();

        resultsList
            .RemoveAll(match:
                static x => x.UpperBand is null && x.LowerBand is null && x.Centerline is null);

        return resultsList.ToSortedList();
    }

    /// <summary>
    /// Removes the recommended warmup periods from the Keltner Channel results.
    /// </summary>
    /// <param name="results">List of Keltner Channel results to process.</param>
    /// <returns>A list of Keltner Channel results with the warmup periods removed.</returns>
    public static IReadOnlyList<KeltnerResult> RemoveWarmupPeriods(
        this IReadOnlyList<KeltnerResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        int n = results
            .FindIndex(static x => x.Width != null) + 1;

        return results.Remove(Math.Max(2 * n, n + 100));
    }

    /// <summary>
    /// Validates the parameters for the Keltner Channel calculation.
    /// </summary>
    /// <param name="emaPeriods">Number of periods for the EMA.</param>
    /// <param name="multiplier">Multiplier for the ATR.</param>
    /// <param name="atrPeriods">Number of periods for the ATR.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when any of the parameters are out of their valid range.
    /// </exception>
    internal static void Validate(
        int emaPeriods,
        double multiplier,
        int atrPeriods)
    {
        // check parameter arguments
        if (emaPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(emaPeriods), emaPeriods,
                "EMA periods must be greater than 1 for Keltner Channel.");
        }

        if (atrPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(atrPeriods), atrPeriods,
                "ATR periods must be greater than 1 for Keltner Channel.");
        }

        if (multiplier <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(multiplier), multiplier,
                "Multiplier must be greater than 0 for Keltner Channel.");
        }
    }
}
