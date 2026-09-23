namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for Fractal Chaos Bands (FCB) calculations.
/// </summary>
public static partial class Fcb
{
    /// <summary>
    /// Removes empty (null) periods from the FCB results.
    /// </summary>
    /// <inheritdoc cref="ReusableExtensions.Condense{T}(IReadOnlyList{T})"/>
    public static IReadOnlyList<FcbResult> Condense(
        this IReadOnlyList<FcbResult> results)
    {
        List<FcbResult> resultsList = results
            .ToList();

        resultsList
            .RemoveAll(match:
                static x => x.UpperBand is null && x.LowerBand is null);

        return resultsList.ToSortedList();
    }

    /// <summary>
    /// Removes the leading Fractal Chaos Bands results that have no calculated value.
    /// </summary>
    /// <param name="results">Fractal Chaos Bands results to evaluate.</param>
    /// <returns>Fractal Chaos Bands results with the warmup periods removed.</returns>
    public static IReadOnlyList<FcbResult> RemoveWarmupPeriods(
        this IReadOnlyList<FcbResult> results)
        => results.RemoveBeforeFirstValue(static x => x.UpperBand != null || x.LowerBand != null);

    /// <summary>
    /// Validates the window span for FCB calculations.
    /// </summary>
    /// <param name="windowSpan">Window span for the calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the window span is less than 2.
    /// </exception>
    internal static void Validate(
        int windowSpan)
    {
        // check parameter arguments
        if (windowSpan < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(windowSpan), windowSpan,
                "Window span must be at least 2 for FCB.");
        }
    }
}
