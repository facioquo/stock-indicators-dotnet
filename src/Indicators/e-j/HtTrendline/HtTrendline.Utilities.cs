namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for Hilbert Transform Instantaneous Trendline (HTL) calculations.
/// </summary>
public static partial class HtTrendline
{
    /// <summary>
    /// Removes the warmup periods and the further periods HT Trendline needs for its values to converge.
    /// </summary>
    /// <param name="results">HT Trendline results to evaluate.</param>
    /// <returns>HT Trendline results with the warmup periods removed.</returns>
    public static IReadOnlyList<HtlResult> RemoveWarmupPeriods(
        this IReadOnlyList<HtlResult> results)
            => results.Remove(100);
}
