namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods for manipulating and handling bar data.
/// </summary>
public static partial class Bars
{
    /* LISTS */

    /// <summary>
    /// Convert IBar list to built-in Bar type list (public API only).
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <returns>A list of converted bars.</returns>
    public static IReadOnlyList<Bar> ToBarList(
        this IReadOnlyList<IBar> bars)

        => bars
            .OrderBy(static x => x.Timestamp)
            .Select(static x => x.ToBar())
            .ToList();

    /// <summary>
    /// Convert IBar list to BarD type list with inline casting.
    /// Uses direct loop instead of LINQ for better performance.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <returns>A list of converted bars in double precision.</returns>
    /// <exception cref="ArgumentNullException">Thrown when bars is null.</exception>
    internal static List<BarD> ToBarDList(
        this IReadOnlyList<IBar> bars)
    {
        ArgumentNullException.ThrowIfNull(bars);

        int length = bars.Count;
        List<BarD> result = new(length);

        for (int i = 0; i < length; i++)
        {
            IBar q = bars[i];
            result.Add(new BarD(
                Timestamp: q.Timestamp,
                Open: (double)q.Open,
                High: (double)q.High,
                Low: (double)q.Low,
                Close: (double)q.Close,
                Volume: (double)q.Volume));
        }

        return result;
    }

    /* TYPES */

    /// <summary>
    /// Convert any IBar type to native Bar type (public API only).
    /// </summary>
    /// <typeparam name="TBar">Type of bar record</typeparam>
    /// <param name="bar">Bar to convert.</param>
    /// <returns>A converted bar.</returns>
    public static Bar ToBar<TBar>(this TBar bar)
        where TBar : IBar

        => new(
            Timestamp: bar.Timestamp,
            Open: bar.Open,
            High: bar.High,
            Low: bar.Low,
            Close: bar.Close,
            Volume: bar.Volume);

    /// <summary>
    /// Convert to bar in double precision.
    /// </summary>
    /// <param name="bar">Bar to convert.</param>
    /// <returns>A converted bar in double precision.</returns>
    internal static BarD ToBarD(this IBar bar)

        => new(
            Timestamp: bar.Timestamp,
            Open: (double)bar.Open,
            High: (double)bar.High,
            Low: (double)bar.Low,
            Close: (double)bar.Close,
            Volume: (double)bar.Volume);
}
