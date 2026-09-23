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
}
