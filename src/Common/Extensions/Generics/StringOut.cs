using System.Diagnostics.CodeAnalysis;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Obsolete. Use <see cref="StringOutExtensions"/> instead.
/// </summary>
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
