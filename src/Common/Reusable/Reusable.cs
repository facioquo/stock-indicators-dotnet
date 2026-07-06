using System.Diagnostics.CodeAnalysis;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Obsolete. Use <see cref="ReusableExtensions"/> instead.
/// </summary>
public static class Reusable
{
    /// <summary>
    /// Obsolete. Use <see cref="ReusableExtensions.ToReusable(IReadOnlyList{IBar}, CandlePart)"/> instead.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="candlePart">The <see cref="CandlePart" /> element.</param>
    /// <returns>A list of reusable types.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(ReusableExtensions)}.{nameof(ReusableExtensions.ToReusable)}` instead.", false)]
    public static IReadOnlyList<IReusable> ToReusable(
        IReadOnlyList<IBar> bars,
        CandlePart candlePart)
        => bars.ToReusable(candlePart);

    /// <summary>
    /// Obsolete. Use <see cref="ReusableExtensions.Condense{T}(IReadOnlyList{T})"/> instead.
    /// </summary>
    /// <typeparam name="T">Any reusable result type.</typeparam>
    /// <param name="results">Indicator results to evaluate.</param>
    /// <returns>Time series of indicator results, condensed.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(ReusableExtensions)}.{nameof(ReusableExtensions.Condense)}` instead.", false)]
    public static IReadOnlyList<T> Condense<T>(
        IReadOnlyList<T> results)
        where T : IReusable
        => results.Condense();
}
