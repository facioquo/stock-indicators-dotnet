using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace FacioQuo.Stock.Indicators;
#pragma warning disable RCS1141,RCS1142

/// <summary>
/// OBSOLETE IN v3.0.0
/// </summary>
[ExcludeFromCodeCoverage]
[Obsolete("The broad Indicator class has been replaced by specific indicator classes.", true)]
public static partial class Indicator
{
    // UTILITIES

    /// <summary>Obsolete. This method no longer supports IEnumerable&lt;IBar&gt; and tuple return types.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete("This method no longer supports IEnumerable<IBar> and tuple return types.", false)]
    public static IEnumerable<(DateTime Timestamp, double Value)> Use(
        this IEnumerable<IBar> bars,
            CandlePart candlePart = CandlePart.Close)
        => bars
            .ToSortedList()
            .ToReusable(candlePart)
            .Select(static x => (x.Timestamp, x.Value));

    [ExcludeFromCodeCoverage]
    [Obsolete("Use 'Collection<T> Foo = [..myEnumerable];' instead.")]
    internal static Collection<T> ToCollection<T>(this IEnumerable<T> source) => [.. source];

    /// <summary>Obsolete. Rename Use() to Use(CandlePart.Close) for an explicit conversion.</summary>
    [Obsolete("This method no longer defaults to Close.  Rename Use() to Use(CandlePart.Close) for an explicit conversion.", false)]
    public static IEnumerable<(DateTime Timestamp, double Value)> Use(
        this IReadOnlyList<IBar> bars)
        => bars.Select(static x => (x.Timestamp, x.Value));

    /// <summary>Obsolete. Refactor to use ToSortedList().</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete("Refactor to use `ToSortedList()`", false)]
    public static Collection<TSeries> ToSortedCollection<TSeries>(
    this IEnumerable<TSeries> series)
    where TSeries : ISeries
        => series
            .OrderBy(static x => x.Timestamp)
            .ToCollection();

    /// <summary>Obsolete. Refactor to use ToReusable().</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete("Refactor to use `ToReusable()`", false)]
    public static Collection<(DateTime Timestamp, double Value)> ToTupleChainable<TResult>(
        this IEnumerable<TResult> reusable)
        where TResult : IReusable
        => reusable
            .Select(static x => (x.Timestamp, x.Value))
            .OrderBy(static x => x.Timestamp)
            .ToCollection();

    /// <summary>Obsolete. Reference the Value property instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete("Reference the `Value` property. Conversion is obsolete.", false)]
    public static Collection<(DateTime Date, double Value)> ToTupleNaN(
        this IEnumerable<IReusable> reusable)
        => reusable
            .ToSortedList()
            .Select(static x => (x.Timestamp, x.Value))
            .ToCollection();

    /// <summary>Obsolete. Refactor to use ToReusable().</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete("Refactor to use `ToReusable()`", false)]
    public static Collection<(DateTime Timestamp, double Value)> ToTupleCollection(
        this IEnumerable<IBar> bars,
        CandlePart candlePart = CandlePart.Close)
        => bars
            .ToList()
            .ToReusable(candlePart)
            .Select(static x => (x.Timestamp, x.Value))
            .ToCollection();

    /// <summary>Obsolete. Refactor to use List&lt;TBar&gt; bars input.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete("Refactor to use `List<TBar> bars` input.", false)]
    public static IReadOnlyList<TBar> Validate<TBar>(
        this IEnumerable<TBar> bars)
        where TBar : IBar
        => bars.ToSortedList().Validate();

    /// <summary>Obsolete. Refactor to use List&lt;TBar&gt; bars input.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete("Refactor to use `List<TBar> bars` input.", false)]
    public static IEnumerable<Bar> Aggregate(
        this IEnumerable<IBar> bars, BarInterval newSize)
        => bars.ToSortedList().Aggregate(newSize);

    /// <summary>Obsolete. Rename PeriodSize to BarInterval.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete("Rename `PeriodSize` to `BarInterval`.", false)]
    public static IEnumerable<Bar> Aggregate(
        this IEnumerable<IBar> bars, PeriodSize newSize)
        => bars.ToSortedList().Aggregate((BarInterval)newSize);

    /// <summary>Obsolete. Rename PeriodSize to BarInterval.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete("Rename `PeriodSize` to `BarInterval`.", false)]
    public static TimeSpan ToTimeSpan(this PeriodSize periodSize)
        => ((BarInterval)periodSize).ToTimeSpan();

    /// <summary>Obsolete. Refactor to use List&lt;TBar&gt; bars input.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete("Refactor to use `List<TBar> bars` input.", false)]
    public static IEnumerable<CandleProperties> ToCandles(
        this IEnumerable<IBar> bars)
        => bars.ToSortedList().ToCandles();

    /// <summary>Obsolete. Refactor to use List.First(c => c.Timestamp == lookupDate).</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete("Refactor to use `List.First(c => c.Timestamp == lookupDate)`", false)]
    public static TSeries Find<TSeries>(this IEnumerable<TSeries> series, DateTime lookupDate)
        where TSeries : ISeries => series.First(x => x.Timestamp == lookupDate);

    /// <summary>Obsolete. Refactor to use List.FindIndex(c => c.Timestamp == lookupDate).</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete("Refactor to use `List.FindIndex(c => c.Timestamp == lookupDate)`", false)]
    public static int FindIndex<TSeries>(this List<TSeries> series, DateTime lookupDate)
        where TSeries : ISeries => series?.FindIndex(x => x.Timestamp == lookupDate) ?? -1;

    /// <summary>Obsolete. Use Math.Round() instead.</summary>
    [Obsolete("Rounding deprecated.  Use Math.Round() instead.")]
    [ExcludeFromCodeCoverage]
    public static decimal? Round(this decimal? value, int digits)
        => value.HasValue
        ? Math.Round(value.GetValueOrDefault(), digits)
        : null;

    /// <summary>Obsolete. Use Math.Round() instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete("Rounding deprecated.  Use Math.Round() instead.")]
    public static double? Round(this double? value, int digits)
        => value.HasValue
        ? Math.Round(value.GetValueOrDefault(), digits)
        : null;

    /// <summary>Obsolete. Use Math.Round() instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete("Rounding deprecated.  Use Math.Round() instead.")]
    public static double Round(this double value, int digits)
        => Math.Round(value, digits);

    /// <summary>Obsolete. Use Math.Round() instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete("Rounding deprecated.  Use Math.Round() instead.")]
    public static decimal Round(this decimal value, int digits)
        => Math.Round(value, digits);
}

// CLASSES AND INTERFACES

// NOTE: The IQuote/Quote/PeriodSize stubs below intentionally keep their
// pre-rename names (issue #1933) and are warning-level [Obsolete] aliases of
// the new types, so v2 consumers' existing type references keep compiling and
// running during a deprecation window while the warnings guide the rename.
// Do not let bulk Quote->Bar / PeriodSize->BarInterval passes rename them.

/// <summary>Obsolete. Rename IQuote to IBar.</summary>
[Obsolete("Rename `IQuote` to `IBar`", false)]
public interface IQuote : IBar;

/// <summary>Obsolete. Rename Quote to Bar.</summary>
[ExcludeFromCodeCoverage]
[Obsolete("Rename `Quote` to `Bar`", false)]
[Serializable]
public sealed record Quote(
    DateTime Timestamp,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume) : IBar
{
    /// <inheritdoc/>
    [JsonIgnore]
    public double Value => (double)Close;
}

/// <summary>Obsolete. Rename PeriodSize to BarInterval.</summary>
[Obsolete("Rename `PeriodSize` to `BarInterval`", false)]
public enum PeriodSize
{
    /// <summary>Obsolete. Monthly period.</summary>
    Month = 0,

    /// <summary>Obsolete. Weekly period.</summary>
    Week = 1,

    /// <summary>Obsolete. Daily period.</summary>
    Day = 2,

    /// <summary>Obsolete. Four-hour period.</summary>
    FourHours = 3,

    /// <summary>Obsolete. Two-hour period.</summary>
    TwoHours = 4,

    /// <summary>Obsolete. One-hour period.</summary>
    OneHour = 5,

    /// <summary>Obsolete. Thirty-minute period.</summary>
    ThirtyMinutes = 6,

    /// <summary>Obsolete. Fifteen-minute period.</summary>
    FifteenMinutes = 7,

    /// <summary>Obsolete. Five-minute period.</summary>
    FiveMinutes = 8,

    /// <summary>Obsolete. Three-minute period.</summary>
    ThreeMinutes = 9,

    /// <summary>Obsolete. Two-minute period.</summary>
    TwoMinutes = 10,

    /// <summary>Obsolete. One-minute period.</summary>
    OneMinute = 11
}

/// <summary>Obsolete. Rename IReusableResult to IReusable.</summary>
[Obsolete($"Rename `{nameof(IReusableResult)}` to `{nameof(IReusable)}`.", false)]
public interface IReusableResult : IReusable;

/// <summary>Obsolete. Rename BasicData to TimeValue.</summary>
[ExcludeFromCodeCoverage]
[Obsolete($"Rename `{nameof(BasicData)}` to `{nameof(TimeValue)}`.", false)]
public sealed class BasicData : IReusable
{
    /// <inheritdoc />
    public DateTime Timestamp { get; set; }

    /// <inheritdoc />
    [JsonIgnore]
    public double Value { get; set; }
}

/// <summary>Obsolete. Rename ChandelierType to Direction.</summary>
[Obsolete($"Rename `{nameof(ChandelierType)}` to `{nameof(Direction)}`.", false)]
[SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Not really an issue.")]
public enum ChandelierType
{
    /// <summary>Obsolete. Long direction.</summary>
    Long = 0,

    /// <summary>Obsolete. Short direction.</summary>
    Short = 1
}
