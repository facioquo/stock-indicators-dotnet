namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the ZigZag indicator.
/// </summary>
public static partial class ZigZag
{
    /// <summary>
    /// Removes empty (null) periods from the ZigZag results.
    /// </summary>
    /// <inheritdoc cref="Reusable.Condense{T}(IReadOnlyList{T})"/>
    public static IReadOnlyList<ZigZagResult> Condense(
        this IReadOnlyList<ZigZagResult> results)
    {
        List<ZigZagResult> resultsList = results
            .ToList();

        resultsList
            .RemoveAll(match:
                static x => x.PointType is null);

        return resultsList.ToSortedList();
    }

    /// <summary>
    /// Validates the parameters for the ZigZag indicator.
    /// </summary>
    /// <param name="percentChange">Percentage change threshold for ZigZag points.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the percent change is less than or equal to 0.</exception>
    internal static void Validate(
        decimal percentChange)
    {
        // check parameter arguments
        if (percentChange <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(percentChange), percentChange,
                "Percent change must be greater than 0 for ZIGZAG.");
        }
    }
}
