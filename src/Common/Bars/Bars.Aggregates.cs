namespace FacioQuo.Stock.Indicators;

// BAR UTILITIES

public static partial class Bars
{
    /// <summary>
    /// Aggregates the bars to a specified period size.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="newSize">New period size to aggregate to.</param>
    /// <returns>A list of aggregated bars.</returns>
    public static IReadOnlyList<Bar> Aggregate(
        this IReadOnlyList<IBar> bars,
        BarInterval newSize)
    {
        if (newSize == BarInterval.Month)
        {
            return bars
                .OrderBy(static x => x.Timestamp)
                .GroupBy(static x => new DateTime(x.Timestamp.Year, x.Timestamp.Month, 1))
                .Select(static x => new Bar(
                    Timestamp: x.Key,
                    Open: x.First().Open,
                    High: x.Max(static t => t.High),
                    Low: x.Min(static t => t.Low),
                    Close: x.Last().Close,
                    Volume: x.Sum(static t => t.Volume)))
                .ToList();
        }

        // parameter conversion
        TimeSpan newTimeSpan = newSize.ToTimeSpan();

        // convert
        return bars.Aggregate(newTimeSpan);

        // month
    }

    /// <summary>
    /// Aggregates the bars to a specified time span.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="timeSpan">Time span to aggregate to.</param>
    /// <returns>A list of aggregated bars.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the time span is less than or equal to zero.</exception>
    public static IReadOnlyList<Bar> Aggregate(
        this IReadOnlyList<IBar> bars,
        TimeSpan timeSpan)
    {
        if (timeSpan <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeSpan), timeSpan,
                "Bars Aggregation must use a usable new size value (see documentation for options).");
        }

        // return aggregation
        return bars
            .OrderBy(x => x.Timestamp)
            .GroupBy(x => x.Timestamp.RoundDown(timeSpan))
            .Select(x => new Bar(
                Timestamp: x.Key,
                Open: x.First().Open,
                High: x.Max(t => t.High),
                Low: x.Min(t => t.Low),
                Close: x.Last().Close,
                Volume: x.Sum(t => t.Volume)))
            .ToList();
    }
}
