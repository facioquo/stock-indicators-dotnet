using System.Diagnostics.CodeAnalysis;

namespace FacioQuo.Stock.Indicators;
#pragma warning disable RCS1141,RCS1142

// OBSOLETE IN v3.1.0
// Non-extension delegates for extension-holder classes renamed to `*Extensions`
// (issue #2146). Removal of these shims is tracked in issue #2139 (v4).

/// <summary>Obsolete. Use <see cref="ReusableExtensions"/> instead.</summary>
public static class Reusable
{
    /// <summary>Obsolete. Use <see cref="ReusableExtensions.ToReusable(IReadOnlyList{IBar}, CandlePart)"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(ReusableExtensions)}.{nameof(ReusableExtensions.ToReusable)}` instead.", false)]
    public static IReadOnlyList<IReusable> ToReusable(
        IReadOnlyList<IBar> bars,
        CandlePart candlePart)
        => bars.ToReusable(candlePart);

    /// <summary>Obsolete. Use <see cref="ReusableExtensions.Condense{T}(IReadOnlyList{T})"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(ReusableExtensions)}.{nameof(ReusableExtensions.Condense)}` instead.", false)]
    public static IReadOnlyList<T> Condense<T>(
        IReadOnlyList<T> results)
        where T : IReusable
        => results.Condense();
}

/// <summary>Obsolete. Use <see cref="PruningExtensions"/> instead.</summary>
public static class Pruning
{
    /// <summary>Obsolete. Use <see cref="PruningExtensions.RemoveWarmupPeriods{T}(IReadOnlyList{T}, int)"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(PruningExtensions)}.{nameof(PruningExtensions.RemoveWarmupPeriods)}` instead.", false)]
    public static IReadOnlyList<T> RemoveWarmupPeriods<T>(
        IReadOnlyList<T> series,
        int removePeriods)
        => series.RemoveWarmupPeriods(removePeriods);
}

/// <summary>Obsolete. Use <see cref="SeekingExtensions"/> instead.</summary>
public static class Seeking
{
    /// <summary>Obsolete. Use <see cref="SeekingExtensions.Find{T}(IReadOnlyList{T}, DateTime)"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(SeekingExtensions)}.{nameof(SeekingExtensions.Find)}` instead.", false)]
    public static T? Find<T>(
        IReadOnlyList<T> series,
        DateTime lookupDate)
        where T : ISeries
        => series.Find(lookupDate);
}

/// <summary>Obsolete. Use <see cref="SortingExtensions"/> instead.</summary>
public static class Sorting
{
    /// <summary>Obsolete. Use <see cref="SortingExtensions.ToSortedList{T}(IEnumerable{T})"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(SortingExtensions)}.{nameof(SortingExtensions.ToSortedList)}` instead.", false)]
    public static IReadOnlyList<T> ToSortedList<T>(
        IEnumerable<T> series)
        where T : ISeries
        => series.ToSortedList();
}

/// <summary>Obsolete. Use <see cref="StringOutExtensions"/> instead.</summary>
public static class StringOut
{
    /// <summary>Obsolete. Use <see cref="StringOutExtensions.ToConsole{T}(T)"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToConsole)}` instead.", false)]
    public static string ToConsole<T>(T obj) where T : ISeries
        => obj.ToConsole();

    /// <summary>Obsolete. Use <see cref="StringOutExtensions.ToConsole{T}(IReadOnlyList{T}, IDictionary{string, string})"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToConsole)}` instead.", false)]
    public static string ToConsole<T>(
        IReadOnlyList<T> source,
        IDictionary<string, string>? args = null)
        where T : ISeries
        => source.ToConsole(args);

    /// <summary>Obsolete. Use <see cref="StringOutExtensions.ToConsole{T}(IReadOnlyList{T}, ValueTuple{string, string}[])"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToConsole)}` instead.", false)]
    public static string ToConsole<T>(
        IReadOnlyList<T> source,
        params (string key, string value)[] args)
        where T : ISeries
        => source.ToConsole(args);

    /// <summary>Obsolete. Use <see cref="StringOutExtensions.ToConsole{T}(IEnumerable{T}, Func{T, bool}, IDictionary{string, string})"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToConsole)}` instead.", false)]
    public static string ToConsole<T>(
        IEnumerable<T> source,
        Func<T, bool> filter,
        IDictionary<string, string>? args = null)
        where T : ISeries
        => source.ToConsole(filter, args);

    /// <summary>Obsolete. Use <see cref="StringOutExtensions.ToConsole{T}(IEnumerable{T}, Func{T, bool}, ValueTuple{string, string}[])"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToConsole)}` instead.", false)]
    public static string ToConsole<T>(
        IEnumerable<T> source,
        Func<T, bool> filter,
        params (string key, string value)[] args)
        where T : ISeries
        => source.ToConsole(filter, args);

    /// <summary>Obsolete. Use <see cref="StringOutExtensions.ToConsole{T}(IEnumerable{T}, Func{T, bool}, int, IDictionary{string, string})"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToConsole)}` instead.", false)]
    public static string ToConsole<T>(
        IEnumerable<T> source,
        Func<T, bool> filter,
        int limitQty = int.MaxValue,
        IDictionary<string, string>? args = null)
        where T : ISeries
        => source.ToConsole(filter, limitQty, args);

    /// <summary>Obsolete. Use <see cref="StringOutExtensions.ToConsole{T}(IEnumerable{T}, Func{T, bool}, int, ValueTuple{string, string}[])"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToConsole)}` instead.", false)]
    public static string ToConsole<T>(
        IEnumerable<T> source,
        Func<T, bool> filter,
        int limitQty,
        params (string key, string value)[] args)
        where T : ISeries
        => source.ToConsole(filter, limitQty, args);

    /// <summary>Obsolete. Use <see cref="StringOutExtensions.ToStringOut{T}(T)"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToStringOut)}` instead.", false)]
    public static string ToStringOut<T>(T obj) where T : ISeries
        => obj.ToStringOut();

    /// <summary>Obsolete. Use <see cref="StringOutExtensions.ToStringOut{T}(IEnumerable{T}, IDictionary{string, string})"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToStringOut)}` instead.", false)]
    public static string ToStringOut<T>(
        IEnumerable<T> source, IDictionary<string, string>? args = null)
        where T : ISeries
        => source.ToStringOut(args);

    /// <summary>Obsolete. Use <see cref="StringOutExtensions.ToStringOut{T}(IEnumerable{T}, int, IDictionary{string, string})"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToStringOut)}` instead.", false)]
    public static string ToStringOut<T>(
        IEnumerable<T> source, int limitQty, IDictionary<string, string>? args = null)
        where T : ISeries
        => source.ToStringOut(limitQty, args);

    /// <summary>Obsolete. Use <see cref="StringOutExtensions.ToStringOut{T}(IEnumerable{T}, int, int, IDictionary{string, string})"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToStringOut)}` instead.", false)]
    public static string ToStringOut<T>(
        IEnumerable<T> source, int startIndex, int endIndex, IDictionary<string, string>? args = null)
        where T : ISeries
        => source.ToStringOut(startIndex, endIndex, args);

    /// <summary>Obsolete. Use <see cref="StringOutExtensions.ToStringOut{T}(IEnumerable{T}, Func{T, bool}, IDictionary{string, string})"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToStringOut)}` instead.", false)]
    public static string ToStringOut<T>(
        IEnumerable<T> source,
        Func<T, bool> filter,
        IDictionary<string, string>? args = null)
        where T : ISeries
        => source.ToStringOut(filter, args);

    /// <summary>Obsolete. Use <see cref="StringOutExtensions.ToStringOut{T}(IEnumerable{T}, Func{T, bool}, int, IDictionary{string, string})"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToStringOut)}` instead.", false)]
    public static string ToStringOut<T>(
        IEnumerable<T> source,
        Func<T, bool> filter,
        int limitQty,
        IDictionary<string, string>? args = null)
        where T : ISeries
        => source.ToStringOut(filter, limitQty, args);

    /// <summary>Obsolete. Use <see cref="StringOutExtensions.ToStringOut{T}(IReadOnlyList{T}, IDictionary{string, string})"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToStringOut)}` instead.", false)]
    public static string ToStringOut<T>(
        IReadOnlyList<T> source,
        IDictionary<string, string>? args = null)
        where T : ISeries
        => source.ToStringOut(args);

    /// <summary>Obsolete. Use <see cref="StringOutExtensions.ToStringOut{T}(IReadOnlyList{T}, int, IDictionary{string, string})"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToStringOut)}` instead.", false)]
    public static string ToStringOut<T>(
        IReadOnlyList<T> source,
        int limitQty,
        IDictionary<string, string>? args = null)
        where T : ISeries
        => source.ToStringOut(limitQty, args);

    /// <summary>Obsolete. Use <see cref="StringOutExtensions.ToStringOut{T}(IReadOnlyList{T}, int, int, IDictionary{string, string})"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToStringOut)}` instead.", false)]
    public static string ToStringOut<T>(
        IReadOnlyList<T> source,
        int startIndex,
        int endIndex,
        IDictionary<string, string>? args = null)
        where T : ISeries
        => source.ToStringOut(startIndex, endIndex, args);

    /// <summary>Obsolete. Use <see cref="StringOutExtensions.ColloquialTypeName(Type)"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ColloquialTypeName)}` instead.", false)]
    public static string ColloquialTypeName(Type? type)
        => StringOutExtensions.ColloquialTypeName(type);
}

/// <summary>Obsolete. Use <see cref="CandlesticksExtensions"/> instead.</summary>
public static class Candlesticks
{
    /// <summary>Obsolete. Use <see cref="CandlesticksExtensions.Condense(IReadOnlyList{CandleResult})"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(CandlesticksExtensions)}.{nameof(CandlesticksExtensions.Condense)}` instead.", false)]
    public static IReadOnlyList<CandleResult> Condense(
        IReadOnlyList<CandleResult> candleResults) => candleResults.Condense();

    /// <summary>Obsolete. Use <see cref="CandlesticksExtensions.ToCandle{TBar}(TBar)"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(CandlesticksExtensions)}.{nameof(CandlesticksExtensions.ToCandle)}` instead.", false)]
    public static CandleProperties ToCandle<TBar>(
        TBar bar)
        where TBar : IBar
        => bar.ToCandle();

    /// <summary>Obsolete. Use <see cref="CandlesticksExtensions.ToCandles(IReadOnlyList{IBar})"/> instead.</summary>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(CandlesticksExtensions)}.{nameof(CandlesticksExtensions.ToCandles)}` instead.", false)]
    public static IReadOnlyList<CandleProperties> ToCandles(
        IReadOnlyList<IBar> bars)
        => bars.ToCandles();
}

public static partial class IndicatorConfigExtensions
{
    /// <summary>
    /// Obsolete. Permanently shadowed by the <see cref="IndicatorConfig.ToBuilder"/> instance
    /// method, which always takes precedence over this same-named extension method. Removal
    /// tracked in issue #2139.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Obsolete("This extension method is permanently shadowed by the `IndicatorConfig.ToBuilder()` instance method and will be removed.", false)]
    public static ListingExecutionBuilder ToBuilder(this IndicatorConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        // not recursive: C# overload resolution binds this call to the
        // IndicatorConfig.ToBuilder() instance method, which always takes
        // precedence over this same-named extension method
        return config.ToBuilder();
    }
}
