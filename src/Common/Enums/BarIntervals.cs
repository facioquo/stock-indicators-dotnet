namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Maps <see cref="BarInterval"/> values to and from their short string codes
/// (e.g. <c>"5m"</c>, <c>"1h"</c>, <c>"1d"</c>), as commonly used by market-data
/// feed APIs and the streaming test server.
/// </summary>
public static class BarIntervals
{
    // canonical code per interval - the single source of truth
    private static readonly Dictionary<BarInterval, string> CanonicalCodes = new() {
        { BarInterval.OneMinute, "1m" },
        { BarInterval.TwoMinutes, "2m" },
        { BarInterval.ThreeMinutes, "3m" },
        { BarInterval.FiveMinutes, "5m" },
        { BarInterval.FifteenMinutes, "15m" },
        { BarInterval.ThirtyMinutes, "30m" },
        { BarInterval.OneHour, "1h" },
        { BarInterval.TwoHours, "2h" },
        { BarInterval.FourHours, "4h" },
        { BarInterval.Day, "1d" },
        { BarInterval.Week, "1w" },
        { BarInterval.Month, "1mo" }
    };

    // accepted aliases (canonical codes are added below), all lower-case
    private static readonly Dictionary<string, BarInterval> CodeLookup = new(StringComparer.OrdinalIgnoreCase) {
        { "1min", BarInterval.OneMinute },
        { "1minute", BarInterval.OneMinute },
        { "2min", BarInterval.TwoMinutes },
        { "2minutes", BarInterval.TwoMinutes },
        { "3min", BarInterval.ThreeMinutes },
        { "3minutes", BarInterval.ThreeMinutes },
        { "5min", BarInterval.FiveMinutes },
        { "5minutes", BarInterval.FiveMinutes },
        { "15min", BarInterval.FifteenMinutes },
        { "15minutes", BarInterval.FifteenMinutes },
        { "30min", BarInterval.ThirtyMinutes },
        { "30minutes", BarInterval.ThirtyMinutes },
        { "1hr", BarInterval.OneHour },
        { "1hour", BarInterval.OneHour },
        { "2hr", BarInterval.TwoHours },
        { "2hours", BarInterval.TwoHours },
        { "4hr", BarInterval.FourHours },
        { "4hours", BarInterval.FourHours },
        { "1day", BarInterval.Day },
        { "day", BarInterval.Day },
        { "1week", BarInterval.Week },
        { "week", BarInterval.Week },
        { "1month", BarInterval.Month },
        { "month", BarInterval.Month }
    };

    static BarIntervals()
    {
        // canonical codes are themselves valid inputs
        foreach (KeyValuePair<BarInterval, string> pair in CanonicalCodes)
        {
            CodeLookup[pair.Value] = pair.Key;
        }
    }

    /// <summary>
    /// Gets the canonical short string code for a <see cref="BarInterval"/>
    /// (e.g. <see cref="BarInterval.FiveMinutes"/> returns <c>"5m"</c>).
    /// </summary>
    /// <param name="interval">The bar interval.</param>
    /// <returns>The canonical short code.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="interval"/> is not a defined value.
    /// </exception>
    public static string ToCode(this BarInterval interval)
        => CanonicalCodes.TryGetValue(interval, out string? code)
            ? code
            : throw new ArgumentOutOfRangeException(
                nameof(interval), interval, "Unknown bar interval.");

    /// <summary>
    /// Parses a short string code (e.g. <c>"5m"</c>, <c>"1h"</c>, <c>"1d"</c>)
    /// into a <see cref="BarInterval"/>. Case-insensitive; common aliases such
    /// as <c>"5min"</c> and <c>"1day"</c> are accepted.
    /// </summary>
    /// <param name="code">The interval code.</param>
    /// <returns>The matching <see cref="BarInterval"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="code"/> is null, empty, or unrecognized.
    /// </exception>
    public static BarInterval ToBarInterval(this string code)
        => TryToBarInterval(code, out BarInterval interval)
            ? interval
            : throw new ArgumentException(
                $"Unsupported interval '{code}'. Valid codes: "
                + "1m, 2m, 3m, 5m, 15m, 30m, 1h, 2h, 4h, 1d, 1w, 1mo.",
                nameof(code));

    /// <summary>
    /// Attempts to parse a short string code into a <see cref="BarInterval"/>.
    /// </summary>
    /// <param name="code">The interval code.</param>
    /// <param name="interval">The parsed interval, when successful.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise <c>false</c>.</returns>
    public static bool TryToBarInterval(string? code, out BarInterval interval)
    {
        if (!string.IsNullOrWhiteSpace(code)
            && CodeLookup.TryGetValue(code.Trim(), out interval))
        {
            return true;
        }

        interval = default;
        return false;
    }
}
