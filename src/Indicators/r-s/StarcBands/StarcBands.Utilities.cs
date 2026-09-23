namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for STARC Bands calculations.
/// </summary>
public static partial class StarcBands
{
    /// <summary>
    /// Removes empty (null) periods from the results.
    /// </summary>
    /// <param name="results">List of STARC Bands results.</param>
    /// <returns>A condensed list of STARC Bands results.</returns>
    public static IReadOnlyList<StarcBandsResult> Condense(
        this IReadOnlyList<StarcBandsResult> results)
    {
        List<StarcBandsResult> resultsList = results
            .ToList();

        resultsList
            .RemoveAll(match:
                static x => x.UpperBand is null && x.LowerBand is null && x.Centerline is null);

        return resultsList.ToSortedList();
    }

    /// <summary>
    /// Removes the warmup periods and the further periods STARC Bands needs for its values to converge.
    /// </summary>
    /// <param name="results">STARC Bands results to evaluate.</param>
    /// <returns>STARC Bands results with the warmup periods removed.</returns>
    public static IReadOnlyList<StarcBandsResult> RemoveWarmupPeriods(
        this IReadOnlyList<StarcBandsResult> results)
        => results.RemoveBeforeFirstValue(
            static x => x.UpperBand != null || x.LowerBand != null,
            static i => i + 151);

    /// <summary>
    /// Validates the parameters for STARC Bands calculation.
    /// </summary>
    /// <param name="smaPeriods">Number of periods for the simple moving average.</param>
    /// <param name="multiplier">Multiplier for the ATR.</param>
    /// <param name="atrPeriods">Number of periods for the average true range.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a parameter is out of range.</exception>
    internal static void Validate(
        int smaPeriods,
        double multiplier,
        int atrPeriods)
    {
        // check parameter arguments
        if (smaPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(smaPeriods), smaPeriods,
                "SMA periods must be greater than 1 for STARC Bands.");
        }

        if (atrPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(atrPeriods), atrPeriods,
                "ATR periods must be greater than 1 for STARC Bands.");
        }

        if (multiplier <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(multiplier), multiplier,
                "Multiplier must be greater than 0 for STARC Bands.");
        }
    }
}
