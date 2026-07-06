using System.Diagnostics.CodeAnalysis;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Obsolete. Use <see cref="SortingExtensions"/> instead.
/// </summary>
public static class Sorting
{
    /// <summary>
    /// Obsolete. Use <see cref="SortingExtensions.ToSortedList{T}(IEnumerable{T})"/> instead.
    /// </summary>
    /// <typeparam name="T">Type of record</typeparam>
    /// <param name="series">Series of elements to sort.</param>
    /// <returns>A read-only list of the sorted elements.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(SortingExtensions)}.{nameof(SortingExtensions.ToSortedList)}` instead.", false)]
    public static IReadOnlyList<T> ToSortedList<T>(
        IEnumerable<T> series)
        where T : ISeries
        => series.ToSortedList();
}
