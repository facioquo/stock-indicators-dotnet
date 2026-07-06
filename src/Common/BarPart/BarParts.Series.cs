namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides extension methods for converting bars to bar parts.
/// </summary>
public static partial class BarParts
{
    /// <summary>
    /// Converts list of bars to a list of <see cref="TimeValue"/>.
    /// <para>
    /// Use this converter if an indicator needs to use something other than the default Close price.
    /// </para>
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="candlePart">The <see cref="CandlePart" /> element.</param>
    /// <returns>List of <see cref="TimeValue"/> records.</returns>
    /// <remarks>This is an alias of <see cref="Use(IReadOnlyList{IBar}, CandlePart)"/></remarks>
    public static IReadOnlyList<TimeValue> ToBarPart(
        this IReadOnlyList<IBar> bars,
        CandlePart candlePart)
    {
        ArgumentNullException.ThrowIfNull(bars);
        int length = bars.Count;
        List<TimeValue> result = new(length);

        for (int i = 0; i < length; i++)
        {
            result.Add(bars[i].ToBarPart(candlePart));
        }

        return result;
    }

    /// <inheritdoc cref="ToBarPart(IReadOnlyList{IBar}, CandlePart)"/>
    /// <remarks>This is an alias of <see cref="ToBarPartList(IReadOnlyList{IBar}, CandlePart)"/></remarks>
    public static IReadOnlyList<TimeValue> Use(
        this IReadOnlyList<IBar> bars,
        CandlePart candlePart)
        => bars.ToBarPart(candlePart);

    // TODO: should we deprecate Use in favor of "ToBarPart"?
    // Probably not, this is a fairly simple alias.
}
