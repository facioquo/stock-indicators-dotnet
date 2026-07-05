namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides extension methods for sorting a series of elements.
/// </summary>
public static class Sorting
{
    /// <summary>
    /// Sorts the series by their timestamps in ascending order.
    /// </summary>
    /// <typeparam name="T">Type of record</typeparam>
    /// <param name="series">Series of elements to sort.</param>
    /// <returns>A read-only list of the sorted elements.</returns>
    public static IReadOnlyList<T> ToSortedList<T>(
        this IEnumerable<T> series)
        where T : ISeries
        => series
            .OrderBy(static x => x.Timestamp)
            .ToList();

    /// <summary>
    /// Sorts bars by their timestamps in ascending order and returns as IReusable list.
    /// </summary>
    /// <remarks>
    /// This method is needed for backward compatibility with obsolete methods
    /// that need to convert IBar collections to IReusable for refactored indicators.
    /// </remarks>
    /// <typeparam name="TBar">Type of the bar elements, which must implement <see cref="IBar"/>.</typeparam>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <returns>A read-only list of IReusable elements.</returns>
    internal static IReadOnlyList<IReusable> ToSortedReusableList<TBar>(
        this IEnumerable<TBar> bars)
        where TBar : IBar
        => bars
            .OrderBy(static x => x.Timestamp)
            .Cast<IReusable>()
            .ToList();
}
