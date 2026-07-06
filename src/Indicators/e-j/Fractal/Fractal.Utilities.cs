namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for Williams Fractal calculations.
/// </summary>
public static partial class Fractal
{
    /// <summary>
    /// Removes empty (null) periods from the Fractal results.
    /// </summary>
    /// <inheritdoc cref="Reusable.Condense{T}(IReadOnlyList{T})"/>
    public static IReadOnlyList<FractalResult> Condense(
        this IReadOnlyList<FractalResult> results)
    {
        List<FractalResult> resultsList = results
            .ToList();

        resultsList
            .RemoveAll(match:
                static x => x.FractalBull is null && x.FractalBear is null);

        return resultsList.ToSortedList();
    }

    /// <summary>
    /// Validates the window span for Fractal calculations.
    /// </summary>
    /// <param name="windowSpan">Number of periods to look back and forward for the calculation.</param>
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
                "Window span must be at least 2 for Fractal.");
        }
    }
}
