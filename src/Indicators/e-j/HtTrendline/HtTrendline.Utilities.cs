namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for Hilbert Transform Instantaneous Trendline (HTL) calculations.
/// </summary>
public static partial class HtTrendline
{
    /// <summary>
    /// Removes the recommended warmup periods from the HTL results.
    /// </summary>
    /// <inheritdoc cref="Reusable.RemoveWarmupPeriods{T}(IReadOnlyList{T})"/>
    public static IReadOnlyList<HtlResult> RemoveWarmupPeriods(
        this IReadOnlyList<HtlResult> results)
            => results.Remove(100);
}
