using System.Diagnostics.CodeAnalysis;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Obsolete. Use <see cref="SeekingExtensions"/> instead.
/// </summary>
public static class Seeking
{
    /// <summary>
    /// Obsolete. Use <see cref="SeekingExtensions.Find{T}(IReadOnlyList{T}, DateTime)"/> instead.
    /// </summary>
    /// <typeparam name="T">Type of the elements in the series, which must implement <see cref="ISeries"/>.</typeparam>
    /// <param name="series">Series of elements to search.</param>
    /// <param name="lookupDate">Timestamp to look for.</param>
    /// <returns>Element with the matching timestamp, or the default value if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the series is null.</exception>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(SeekingExtensions)}.{nameof(SeekingExtensions.Find)}` instead.", false)]
    public static T? Find<T>(
        IReadOnlyList<T> series,
        DateTime lookupDate)
        where T : ISeries
        => series.Find(lookupDate);
}
