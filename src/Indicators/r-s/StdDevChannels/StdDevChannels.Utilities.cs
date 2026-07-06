namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods Standard Deviation Channels calculations.
/// </summary>
public static partial class StdDevChannels
{
    /// <summary>
    /// Removes empty (null) periods from the results.
    /// </summary>
    /// <param name="results">List of results to condense.</param>
    /// <returns>A condensed list of results.</returns>
    public static IReadOnlyList<StdDevChannelsResult> Condense(
        this IReadOnlyList<StdDevChannelsResult> results)
    {
        List<StdDevChannelsResult> resultsList = results
            .ToList();

        resultsList
            .RemoveAll(match: static x =>
               x.UpperChannel is null
            && x.LowerChannel is null
            && x.Centerline is null
            && !x.BreakPoint);

        return resultsList.ToSortedList();
    }

    /// <summary>
    /// Removes recommended warmup periods from the results.
    /// </summary>
    /// <param name="results">List of results to process.</param>
    /// <returns>A list of results with warmup periods removed.</returns>
    public static IReadOnlyList<StdDevChannelsResult> RemoveWarmupPeriods(
        this IReadOnlyList<StdDevChannelsResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        int removePeriods = results
            .FindIndex(static x => x.UpperChannel != null || x.LowerChannel != null);

        return results.Remove(removePeriods);
    }

    /// <summary>
    /// Validates the parameters for Standard Deviation Channels.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="stdDeviations">Number of standard deviations.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are out of range.</exception>
    internal static void Validate(
        int? lookbackPeriods,
        double stdDeviations)
    {
        // check parameter arguments
        if (lookbackPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 1 for Standard Deviation Channels.");
        }

        if (stdDeviations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stdDeviations), stdDeviations,
                "Standard Deviations must be greater than 0 for Standard Deviation Channels.");
        }
    }
}
