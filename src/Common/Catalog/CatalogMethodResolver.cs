using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Resolves indicator methods and their result record types from a catalog method name.
/// </summary>
/// <remarks>
/// A catalog listing identifies its indicator by method name only. This resolver turns
/// that name back into the compiled members it refers to, so metadata derived from the
/// method — such as <see cref="IndicatorListing.ResultRecordType"/> — cannot drift away from
/// the method it describes. Lookups are cached; the assembly is scanned once.
/// </remarks>
internal static class CatalogMethodResolver
{
    private static readonly Lazy<ILookup<string, MethodInfo>> MethodsByName
        = new(BuildMethodLookup, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// Gets every public static overload with the given method name.
    /// </summary>
    /// <param name="methodName">Method name from a catalog listing.</param>
    /// <returns>All matching overloads; empty when the name resolves to nothing.</returns>
    [RequiresUnreferencedCode("Enumerates assembly types to resolve indicator methods by name.")]
    internal static IEnumerable<MethodInfo> GetOverloads(string methodName)
        => MethodsByName.Value[methodName];

    /// <summary>
    /// Gets the name of the result record type produced by a named indicator method.
    /// </summary>
    /// <param name="methodName">Method name from a catalog listing.</param>
    /// <returns>
    /// Result record type name, or <c>null</c> when the method does not exist or its
    /// result record cannot be determined.
    /// </returns>
    [RequiresUnreferencedCode("Enumerates assembly types to resolve indicator methods by name.")]
    internal static string? GetResultTypeName(string? methodName)
        => string.IsNullOrWhiteSpace(methodName)
            ? null
            : MethodsByName.Value[methodName]
                .Select(GetResultType)
                .FirstOrDefault(static t => t is not null)
                ?.Name;

    /// <summary>
    /// Gets the result record type produced by an indicator method, for any style.
    /// </summary>
    /// <param name="method">Indicator method bound to a catalog listing.</param>
    /// <returns>
    /// Result record type — <c>EmaResult</c> for all three of
    /// <c>IReadOnlyList&lt;EmaResult&gt;</c> (Series), <c>EmaList</c> (Buffer),
    /// and <c>EmaHub</c> (Stream) — or <c>null</c> when it cannot be determined.
    /// </returns>
    internal static Type? GetResultType(MethodInfo method)
    {
        Type returnType = method.ReturnType;

        // Series: IReadOnlyList<TResult>
        if (returnType.IsGenericType
         && returnType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
        {
            return returnType.GetGenericArguments()[0];
        }

        // Buffer: a BufferList implementing IReadOnlyList<TResult>
        Type? listInterface = returnType
            .GetInterfaces()
            .FirstOrDefault(static i => i.IsGenericType
                                     && i.GetGenericTypeDefinition() == typeof(IReadOnlyList<>));

        if (listInterface is not null)
        {
            return listInterface.GetGenericArguments()[0];
        }

        // Stream: a hub deriving from StreamHub<TIn, TOut>
        for (Type? baseType = returnType; baseType is not null; baseType = baseType.BaseType)
        {
            if (baseType.IsGenericType
             && baseType.GetGenericTypeDefinition() == typeof(StreamHub<,>))
            {
                return baseType.GetGenericArguments()[1]; // TOut
            }
        }

        return null;
    }

    /// <summary>
    /// Indexes every public static method on the library's static classes by name.
    /// </summary>
    /// <returns>Lookup of overloads, keyed by method name.</returns>
    private static ILookup<string, MethodInfo> BuildMethodLookup()
    {
        Type?[] types;

        // Build() runs inside the static initializer of each indicator class, so an
        // unhandled failure here would leave those types permanently unusable — taking
        // the calculations down with the metadata. Use whatever loaded instead.
        try
        {
            types = typeof(CatalogMethodResolver).Assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types;
        }

        return types
            .Where(static t => t is { IsClass: true, IsAbstract: true, IsSealed: true }) // static classes
            .SelectMany(static t => GetStaticMethods(t!))
            .ToLookup(static m => m.Name, StringComparer.Ordinal);
    }

    /// <summary>
    /// Gets a type's own public static methods, tolerating a type-load failure.
    /// </summary>
    /// <remarks>
    /// <c>GetMethods</c> throws for the same reason <c>GetTypes</c> does — a signature
    /// referencing a type that failed to load — and the result of the surrounding scan
    /// is cached in a <see cref="Lazy{T}"/>, which caches a thrown exception for the
    /// life of the process. Skipping the offending type keeps one bad signature from
    /// disabling catalog metadata for every indicator. <c>DeclaredOnly</c> keeps
    /// inherited <see cref="object"/> statics out of the lookup, so a listing can never
    /// resolve its method name to <c>Equals</c> or <c>ReferenceEquals</c>.
    /// </remarks>
    /// <param name="type">Static class to enumerate.</param>
    /// <returns>The type's own public static methods, or empty when it cannot be read.</returns>
    private static MethodInfo[] GetStaticMethods(Type type)
    {
        try
        {
            return type.GetMethods(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        }
        catch (ReflectionTypeLoadException)
        {
            return [];
        }
    }
}
