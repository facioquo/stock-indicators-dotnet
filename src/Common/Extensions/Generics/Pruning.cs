using System.Diagnostics.CodeAnalysis;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Obsolete. Use <see cref="PruningExtensions"/> instead.
/// </summary>
public static class Pruning
{
    /// <summary>
    /// Obsolete. Use <see cref="PruningExtensions.RemoveWarmupPeriods{T}(IReadOnlyList{T}, int)"/> instead.
    /// </summary>
    /// <typeparam name="T">Type of elements in the series.</typeparam>
    /// <param name="series">Series from which to remove warmup periods.</param>
    /// <param name="removePeriods">Number of periods to remove.</param>
    /// <returns>A new series with the specified number of warmup periods removed.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when removePeriods is less than 0.</exception>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(PruningExtensions)}.{nameof(PruningExtensions.RemoveWarmupPeriods)}` instead.", false)]
    public static IReadOnlyList<T> RemoveWarmupPeriods<T>(
        IReadOnlyList<T> series,
        int removePeriods)
        => series.RemoveWarmupPeriods(removePeriods);
}
