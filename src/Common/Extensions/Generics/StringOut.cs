using System.Diagnostics.CodeAnalysis;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Obsolete. Use <see cref="StringOutExtensions"/> instead.
/// </summary>
public static class StringOut
{
    /// <summary>
    /// Obsolete. Use <see cref="StringOutExtensions.ToConsole{T}(T)"/> instead.
    /// </summary>
    /// <typeparam name="T">Type of the ISeries instance.</typeparam>
    /// <param name="obj">ISeries instance to write to the console.</param>
    /// <returns>String representation of the ISeries instance.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToConsole)}` instead.", false)]
    public static string ToConsole<T>(T obj) where T : ISeries
        => obj.ToConsole();

    /// <summary>
    /// Obsolete. Use <see cref="StringOutExtensions.ToConsole{T}(IReadOnlyList{T}, IDictionary{string, string})"/> instead.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="args">Optional overrides for `ToString()` formatter. Key values can be type or property name.</param>
    /// <returns>Fixed-width formatted string representation of the list.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToConsole)}` instead.", false)]
    public static string ToConsole<T>(
        IReadOnlyList<T> source,
        IDictionary<string, string>? args = null)
        where T : ISeries
        => source.ToConsole(args);

    /// <summary>
    /// Obsolete. Use <see cref="StringOutExtensions.ToConsole{T}(IReadOnlyList{T}, ValueTuple{string, string}[])"/> instead.
    /// </summary>
    /// <typeparam name="T">Type of elements in the series. Must implement <see cref="ISeries"/>.</typeparam>
    /// <param name="source">Read-only list of series elements to be written to the console.</param>
    /// <param name="args">Optional key-value pairs that provide additional formatting or output options.</param>
    /// <returns>A string containing the console output generated from the series.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToConsole)}` instead.", false)]
    public static string ToConsole<T>(
        IReadOnlyList<T> source,
        params (string key, string value)[] args)
        where T : ISeries
        => source.ToConsole(args);

    /// <summary>
    /// Obsolete. Use <see cref="StringOutExtensions.ToConsole{T}(IEnumerable{T}, Func{T, bool}, IDictionary{string, string})"/> instead.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="filter">A predicate to filter the elements.</param>
    /// <param name="args">Optional overrides for `ToString()` formatter. Key values can be type or property name.</param>
    /// <returns>Fixed-width formatted string representation of the filtered list.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToConsole)}` instead.", false)]
    public static string ToConsole<T>(
        IEnumerable<T> source,
        Func<T, bool> filter,
        IDictionary<string, string>? args = null)
        where T : ISeries
        => source.ToConsole(filter, args);

    /// <summary>
    /// Obsolete. Use <see cref="StringOutExtensions.ToConsole{T}(IEnumerable{T}, Func{T, bool}, ValueTuple{string, string}[])"/> instead.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="filter">A predicate to filter the elements.</param>
    /// <param name="args">Optional formatting arguments as key-value pairs.</param>
    /// <returns>Fixed-width formatted string representation of the filtered list.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToConsole)}` instead.", false)]
    public static string ToConsole<T>(
        IEnumerable<T> source,
        Func<T, bool> filter,
        params (string key, string value)[] args)
        where T : ISeries
        => source.ToConsole(filter, args);

    /// <summary>
    /// Obsolete. Use <see cref="StringOutExtensions.ToConsole{T}(IEnumerable{T}, Func{T, bool}, int, IDictionary{string, string})"/> instead.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="filter">A predicate to filter the elements.</param>
    /// <param name="limitQty">Maximum number of elements to include in the output.</param>
    /// <param name="args">Optional formatting arguments as key-value pairs.</param>
    /// <returns>Fixed-width formatted string representation of the filtered list.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToConsole)}` instead.", false)]
    public static string ToConsole<T>(
        IEnumerable<T> source,
        Func<T, bool> filter,
        int limitQty = int.MaxValue,
        IDictionary<string, string>? args = null)
        where T : ISeries
        => source.ToConsole(filter, limitQty, args);

    /// <summary>
    /// Obsolete. Use <see cref="StringOutExtensions.ToConsole{T}(IEnumerable{T}, Func{T, bool}, int, ValueTuple{string, string}[])"/> instead.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="filter">A predicate to filter the elements.</param>
    /// <param name="limitQty">Maximum number of elements to include in the output.</param>
    /// <param name="args">Optional formatting arguments as key-value pairs.</param>
    /// <returns>Fixed-width formatted string representation of the filtered list.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToConsole)}` instead.", false)]
    public static string ToConsole<T>(
        IEnumerable<T> source,
        Func<T, bool> filter,
        int limitQty,
        params (string key, string value)[] args)
        where T : ISeries
        => source.ToConsole(filter, limitQty, args);

    /// <summary>
    /// Obsolete. Use <see cref="StringOutExtensions.ToStringOut{T}(T)"/> instead.
    /// </summary>
    /// <typeparam name="T">Type of the ISeries instance.</typeparam>
    /// <param name="obj">ISeries instance to convert.</param>
    /// <returns>A formatted string representation of the ISeries instance.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToStringOut)}` instead.", false)]
    public static string ToStringOut<T>(T obj) where T : ISeries
        => obj.ToStringOut();

    /// <summary>
    /// Obsolete. Use <see cref="StringOutExtensions.ToStringOut{T}(IEnumerable{T}, IDictionary{string, string})"/> instead.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="args">Optional overrides for `ToString()` formatter. Key values can be type or property name.</param>
    /// <returns>A fixed-width formatted string representation of the list.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToStringOut)}` instead.", false)]
    public static string ToStringOut<T>(
        IEnumerable<T> source, IDictionary<string, string>? args = null)
        where T : ISeries
        => source.ToStringOut(args);

    /// <summary>
    /// Obsolete. Use <see cref="StringOutExtensions.ToStringOut{T}(IEnumerable{T}, int, IDictionary{string, string})"/> instead.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="limitQty">Maximum number of elements to include in the output.</param>
    /// <param name="args">Optional overrides for `ToString()` formatter. Key values can be type or property name.</param>
    /// <returns>A fixed-width formatted string representation of the list.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToStringOut)}` instead.", false)]
    public static string ToStringOut<T>(
        IEnumerable<T> source, int limitQty, IDictionary<string, string>? args = null)
        where T : ISeries
        => source.ToStringOut(limitQty, args);

    /// <summary>
    /// Obsolete. Use <see cref="StringOutExtensions.ToStringOut{T}(IEnumerable{T}, int, int, IDictionary{string, string})"/> instead.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="startIndex">Starting index of the elements to include in the output.</param>
    /// <param name="endIndex">Ending index of the elements to include in the output.</param>
    /// <param name="args">Optional overrides for `ToString()` formatter. Key values can be type or property name.</param>
    /// <returns>A fixed-width formatted string representation of the list.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToStringOut)}` instead.", false)]
    public static string ToStringOut<T>(
        IEnumerable<T> source, int startIndex, int endIndex, IDictionary<string, string>? args = null)
        where T : ISeries
        => source.ToStringOut(startIndex, endIndex, args);

    /// <summary>
    /// Obsolete. Use <see cref="StringOutExtensions.ToStringOut{T}(IEnumerable{T}, Func{T, bool}, IDictionary{string, string})"/> instead.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="filter">A predicate to filter the elements.</param>
    /// <param name="args">Optional overrides for `ToString()` formatter. Key values can be type or property name.</param>
    /// <returns>A fixed-width formatted string representation of the filtered list.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToStringOut)}` instead.", false)]
    public static string ToStringOut<T>(
        IEnumerable<T> source,
        Func<T, bool> filter,
        IDictionary<string, string>? args = null)
        where T : ISeries
        => source.ToStringOut(filter, args);

    /// <summary>
    /// Obsolete. Use <see cref="StringOutExtensions.ToStringOut{T}(IEnumerable{T}, Func{T, bool}, int, IDictionary{string, string})"/> instead.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="filter">A predicate to filter the elements.</param>
    /// <param name="limitQty">Maximum number of elements to include in the output.</param>
    /// <param name="args">Optional overrides for `ToString()` formatter. Key values can be type or property name.</param>
    /// <returns>A fixed-width formatted string representation of the filtered list.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToStringOut)}` instead.", false)]
    public static string ToStringOut<T>(
        IEnumerable<T> source,
        Func<T, bool> filter,
        int limitQty,
        IDictionary<string, string>? args = null)
        where T : ISeries
        => source.ToStringOut(filter, limitQty, args);

    /// <summary>
    /// Obsolete. Use <see cref="StringOutExtensions.ToStringOut{T}(IReadOnlyList{T}, IDictionary{string, string})"/> instead.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="args">Optional overrides for `ToString()` formatter. Key values can be type or property name.</param>
    /// <returns>A fixed-width formatted string representation of the list.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToStringOut)}` instead.", false)]
    public static string ToStringOut<T>(
        IReadOnlyList<T> source,
        IDictionary<string, string>? args = null)
        where T : ISeries
        => source.ToStringOut(args);

    /// <summary>
    /// Obsolete. Use <see cref="StringOutExtensions.ToStringOut{T}(IReadOnlyList{T}, int, IDictionary{string, string})"/> instead.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="limitQty">Maximum number of elements to include in the output.</param>
    /// <param name="args">Optional overrides for `ToString()` formatter. Key values can be type or property name.</param>
    /// <returns>A fixed-width formatted string representation of the list.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToStringOut)}` instead.", false)]
    public static string ToStringOut<T>(
        IReadOnlyList<T> source,
        int limitQty,
        IDictionary<string, string>? args = null)
        where T : ISeries
        => source.ToStringOut(limitQty, args);

    /// <summary>
    /// Obsolete. Use <see cref="StringOutExtensions.ToStringOut{T}(IReadOnlyList{T}, int, int, IDictionary{string, string})"/> instead.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="startIndex">Starting index of the elements to include in the output.</param>
    /// <param name="endIndex">Ending index of the elements to include in the output.</param>
    /// <param name="args">Optional overrides for `ToString()` formatter. Key values can be type or property name.</param>
    /// <returns>A fixed-width formatted string representation of the list.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ToStringOut)}` instead.", false)]
    public static string ToStringOut<T>(
        IReadOnlyList<T> source,
        int startIndex,
        int endIndex,
        IDictionary<string, string>? args = null)
        where T : ISeries
        => source.ToStringOut(startIndex, endIndex, args);

    /// <summary>
    /// Obsolete. Use <see cref="StringOutExtensions.ColloquialTypeName(Type)"/> instead.
    /// </summary>
    /// <param name="type">Type to get the colloquial name for.</param>
    /// <returns>Colloquial type name.</returns>
    [ExcludeFromCodeCoverage]
    [Obsolete($"Use `{nameof(StringOutExtensions)}.{nameof(StringOutExtensions.ColloquialTypeName)}` instead.", false)]
    public static string ColloquialTypeName(Type? type)
        => StringOutExtensions.ColloquialTypeName(type);
}
