namespace FacioQuo.Stock.Indicators;

// BARPART TYPE UTILITIES

public static partial class BarParts
{
    /// <summary>
    /// convert TBar element to a basic <see cref="TimeValue"/> class
    /// </summary>
    /// <param name="q">Bar to convert</param>
    /// <param name="candlePart">The <see cref="CandlePart" /> element.</param>
    /// <returns>Date and value pair</returns>
    internal static TimeValue ToBarPart(this IBar q, CandlePart candlePart)
        => new(q.Timestamp, q.ToBarPartValue(candlePart));

    /// <summary>
    /// convert IBar to value based on CandlePart
    /// </summary>
    /// <param name="q">Bar to convert</param>
    /// <param name="candlePart">The <see cref="CandlePart" /> element.</param>
    /// <returns>Value of bar part</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a parameter is out of the valid range</exception>
    internal static double ToBarPartValue(this IBar q, CandlePart candlePart)

        => candlePart switch {

            CandlePart.Open => (double)q.Open,
            CandlePart.High => (double)q.High,
            CandlePart.Low => (double)q.Low,
            CandlePart.Close => (double)q.Close,
            CandlePart.Volume => (double)q.Volume,
            CandlePart.HL2 => (double)(q.High + q.Low) / 2,
            CandlePart.HLC3 => (double)(q.High + q.Low + q.Close) / 3,
            CandlePart.OC2 => (double)(q.Open + q.Close) / 2,
            CandlePart.OHL3 => (double)(q.Open + q.High + q.Low) / 3,
            CandlePart.OHLC4 => (double)(q.Open + q.High + q.Low + q.Close) / 4,

            _ => throw new ArgumentOutOfRangeException(
                nameof(candlePart), candlePart, "Invalid candlePart provided.")
        };

    /// <summary>
    /// conditional HL2 value if IBar type
    /// </summary>
    /// <typeparam name="T">Type of record</typeparam>
    /// <param name="item">Item to process</param>
    /// <returns>HL2 value if bar type or reusable value</returns>
    internal static double Hl2OrValue<T>(
        this T item)
        where T : IReusable
        => item.BarPartOrValue(CandlePart.HL2);

    /// <summary>
    /// conditional CandlePart value if IBar type
    /// </summary>
    /// <typeparam name="T">Type of record</typeparam>
    /// <param name="item">Item to process</param>
    /// <param name="candlePart">The <see cref="CandlePart" /> element.</param>
    /// <returns>Converted reusable value</returns>
    internal static double BarPartOrValue<T>(
        this T item, CandlePart candlePart)
        where T : IReusable
        => item is IBar q
            ? q.ToBarPartValue(candlePart)
            : item.Value;

    /// <summary>
    /// Uses CandlePart to sort and convert a list from an
    /// <see cref="IBar"/> to and <see cref="IReusable"/> type.
    /// </summary>
    /// <remarks>
    /// Use this conversion if your source list needs to
    /// conditionally use something other than Close price
    /// as the IReusable value.
    ///<para>
    /// If you provide a list of
    /// IReusable items that are not IBar, it will simply
    /// cast itself from <typeparamref name="T"/>.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">Type of record</typeparam>
    /// <param name="items">List of IBar or IReusable items</param>
    /// <param name="candlePart">The <see cref="CandlePart" /> element.</param>
    /// <returns>List of IReusable items</returns>
    internal static IReadOnlyList<IReusable> ToPreferredList<T>(
        this IReadOnlyList<T> items, CandlePart candlePart)
        where T : IReusable
    {
        ArgumentNullException.ThrowIfNull(items);

        return items is IReadOnlyList<IBar> bars

            ? bars.ToBarPart(candlePart)

            : items is IReadOnlyList<IReusable> reusable // fast-path: already the right type
                ? reusable

                : items
                    .Cast<IReusable>()
                    .ToList();
    }
}
