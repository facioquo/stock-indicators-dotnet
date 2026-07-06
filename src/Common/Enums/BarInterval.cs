namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Bar interval, the time period of aggregation represented by a single <see cref="IBar"/>.
/// </summary>
public enum BarInterval
{
    /// <summary>
    /// Monthly period.
    /// </summary>
    Month = 0,

    /// <summary>
    /// Weekly period.
    /// </summary>
    Week = 1,

    /// <summary>
    /// Daily period.
    /// </summary>
    Day = 2,

    /// <summary>
    /// Four-hour period.
    /// </summary>
    FourHours = 3,

    /// <summary>
    /// Two-hour period.
    /// </summary>
    TwoHours = 4,

    /// <summary>
    /// One-hour period.
    /// </summary>
    OneHour = 5,

    /// <summary>
    /// Thirty-minute period.
    /// </summary>
    ThirtyMinutes = 6,

    /// <summary>
    /// Fifteen-minute period.
    /// </summary>
    FifteenMinutes = 7,

    /// <summary>
    /// Five-minute period.
    /// </summary>
    FiveMinutes = 8,

    /// <summary>
    /// Three-minute period.
    /// </summary>
    ThreeMinutes = 9,

    /// <summary>
    /// Two-minute period.
    /// </summary>
    TwoMinutes = 10,

    /// <summary>
    /// One-minute period.
    /// </summary>
    OneMinute = 11
}
