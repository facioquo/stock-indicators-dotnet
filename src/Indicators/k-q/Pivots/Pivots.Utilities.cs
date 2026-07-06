namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the Pivot Points indicator.
/// </summary>
public static partial class Pivots
{
    /// <summary>
    /// Removes empty (null) periods from the pivot points results.
    /// </summary>
    /// <inheritdoc cref="Reusable.Condense{T}(IReadOnlyList{T})"/>
    public static IReadOnlyList<PivotsResult> Condense(
        this IReadOnlyList<PivotsResult> results)
    {
        List<PivotsResult> resultsList = results
            .ToList();

        resultsList
            .RemoveAll(match:
                static x => x.HighPoint is null && x.LowPoint is null);

        return resultsList.ToSortedList();
    }

    /// <summary>
    /// Validates the parameters for pivot points calculations.
    /// </summary>
    /// <param name="leftSpan">Number of periods to the left of the pivot point.</param>
    /// <param name="rightSpan">Number of periods to the right of the pivot point.</param>
    /// <param name="maxTrendPeriods">Maximum number of periods for trend calculation.</param>
    /// <param name="caller">Name of the calling method.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any parameter is out of range.</exception>
    internal static void Validate(
        int leftSpan,
        int rightSpan,
        int maxTrendPeriods,
        string caller = "Pivots")
    {
        // check parameter arguments
        if (rightSpan < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(rightSpan), rightSpan,
                $"Right span must be at least 2 for {caller}.");
        }

        if (leftSpan < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(leftSpan), leftSpan,
                $"Left span must be at least 2 for {caller}.");
        }

        if (maxTrendPeriods <= leftSpan)
        {
            throw new ArgumentOutOfRangeException(nameof(leftSpan), leftSpan,
                $"Lookback periods must be greater than the Left window span for {caller}.");
        }
    }
}
