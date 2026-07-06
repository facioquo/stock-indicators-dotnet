using System.Globalization;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for pivot points calculations.
/// </summary>
public static partial class PivotPoints
{
    private static readonly CultureInfo invariantCulture
        = CultureInfo.InvariantCulture;

    private static readonly Calendar calendar
        = invariantCulture.Calendar;

    private static readonly CalendarWeekRule calendarWeekRule
        = invariantCulture.DateTimeFormat.CalendarWeekRule;

    private static readonly DayOfWeek firstDayOfWeek
        = invariantCulture.DateTimeFormat.FirstDayOfWeek;

    /// <summary>
    /// Removes the warmup periods from the pivot points results.
    /// </summary>
    /// <param name="results">List of pivot points results.</param>
    /// <returns>A list of pivot points results without the warmup periods.</returns>
    public static IReadOnlyList<PivotPointsResult> RemoveWarmupPeriods(
        this IReadOnlyList<PivotPointsResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        int removePeriods = results
            .FindIndex(static x => x.PP != null);

        return results.Remove(removePeriods);
    }
}
