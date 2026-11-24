using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Resolves indicator methods and their result record types from a catalog method name.
/// </summary>
/// <remarks>
/// Each catalog listing registers its method when it is built, naming the static class
/// that declares it. Reflection reaches only that class, through a
/// <see cref="DynamicallyAccessedMembersAttribute"/> annotation the trimmer and NativeAOT
/// compiler honor, so resolution survives a trimmed publish. Later lookups are by name.
/// </remarks>
internal static class CatalogMethodResolver
{
    private const DynamicallyAccessedMemberTypes MethodMembers
        = DynamicallyAccessedMemberTypes.PublicMethods;

    private static readonly ConcurrentDictionary<string, MethodInfo[]> OverloadsByName
        = new(StringComparer.Ordinal);

    /// <summary>
    /// Registers a catalog method and gets the name of the result record type it produces.
    /// </summary>
    /// <param name="declaringType">Static class declaring the method.</param>
    /// <param name="methodName">Method name from a catalog listing.</param>
    /// <returns>
    /// Result record type name, or <c>null</c> when the method does not exist or its
    /// result record cannot be determined.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Another class already registered a method by this name.
    /// </exception>
    internal static string? Register(
        [DynamicallyAccessedMembers(MethodMembers)] Type? declaringType,
        string? methodName)
    {
        if (declaringType is null || string.IsNullOrWhiteSpace(methodName))
        {
            return null;
        }

        MethodInfo[] overloads = GetStaticMethods(declaringType)
            .Where(m => string.Equals(m.Name, methodName, StringComparison.Ordinal))
            .ToArray();

        // only names that resolve are kept, so a mistyped name stays unresolvable
        if (overloads.Length > 0)
        {
            MethodInfo[] registered = OverloadsByName.GetOrAdd(methodName, overloads);

            // lookups are by name alone, so one name must map to one declaring class; this runs in
            // static initializers, so a duplicate breaks the second class for good, and the binding
            // tests exist to catch one before release
            if (registered[0].DeclaringType != overloads[0].DeclaringType)
            {
                throw new InvalidOperationException(
                    $"Catalog method '{methodName}' is declared by both {registered[0].DeclaringType?.Name} and {declaringType.Name}.");
            }
        }

        return overloads
            .Select(GetResultType)
            .FirstOrDefault(static t => t is not null)
            ?.Name;
    }

    /// <summary>
    /// Gets every public static overload of a catalog method.
    /// </summary>
    /// <param name="methodName">Method name from a listing.</param>
    /// <returns>
    /// All overloads; empty when no catalog listing names the method.
    /// </returns>
    internal static MethodInfo[] GetOverloads(string? methodName)
    {
        if (string.IsNullOrWhiteSpace(methodName))
        {
            return [];
        }

        if (OverloadsByName.TryGetValue(methodName, out MethodInfo[]? overloads))
        {
            return overloads;
        }

        // building the catalog registers every catalog method
        _ = Catalog.Listings;

        return OverloadsByName.TryGetValue(methodName, out overloads) ? overloads : [];
    }

    /// <summary>
    /// Gets the overloads of a catalog method that produce one indicator style.
    /// </summary>
    /// <remarks>
    /// One name can serve two styles: IndyMath's operators, such as <c>Minus</c>, name
    /// both a Series method and a stream hub method, told apart by their receiver.
    /// </remarks>
    /// <param name="methodName">Method name from a listing.</param>
    /// <param name="style">Style of the listing.</param>
    /// <returns>
    /// The overloads that return the given style; every overload when none does.
    /// </returns>
    internal static MethodInfo[] GetOverloads(string? methodName, Style style)
    {
        MethodInfo[] overloads = GetOverloads(methodName);
        MethodInfo[] ofStyle = Array.FindAll(overloads, m => GetStyle(m) == style);

        return ofStyle.Length > 0 ? ofStyle : overloads;
    }

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
        => Classify(method)?.ResultType;

    /// <summary>
    /// Gets the indicator style an indicator method returns.
    /// </summary>
    /// <param name="method">Indicator method bound to a catalog listing.</param>
    /// <returns>The style, or <c>null</c> when the return shape is unrecognized.</returns>
    private static Style? GetStyle(MethodInfo method)
        => Classify(method)?.Style;

    /// <summary>
    /// Matches an indicator method's return type to one of the three style shapes.
    /// </summary>
    /// <remarks>
    /// Walks base types rather than interfaces: a trimmed app keeps every base type of a
    /// type it keeps, but not necessarily the interface list reflection would enumerate.
    /// </remarks>
    /// <param name="method">Indicator method bound to a catalog listing.</param>
    /// <returns>
    /// The style and its result record type: <c>IReadOnlyList&lt;TResult&gt;</c> for
    /// Series, <c>BufferList&lt;TResult&gt;</c> for Buffer, and <c>StreamHub&lt;TIn, TOut&gt;</c>
    /// for Stream; <c>null</c> when the return type matches none of them.
    /// </returns>
    private static (Style Style, Type ResultType)? Classify(MethodInfo method)
    {
        Type returnType = method.ReturnType;

        if (returnType.IsGenericType
         && returnType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
        {
            return (Style.Series, returnType.GetGenericArguments()[0]);
        }

        for (Type? baseType = returnType; baseType is not null; baseType = baseType.BaseType)
        {
            if (!baseType.IsGenericType)
            {
                continue;
            }

            Type definition = baseType.GetGenericTypeDefinition();

            if (definition == typeof(BufferList<>))
            {
                return (Style.Buffer, baseType.GetGenericArguments()[0]);
            }

            if (definition == typeof(StreamHub<,>))
            {
                return (Style.Stream, baseType.GetGenericArguments()[1]);
            }
        }

        return null;
    }

    /// <summary>
    /// Gets a type's own public static methods, tolerating a type-load failure.
    /// </summary>
    /// <remarks>
    /// Listings are built inside each indicator class's static initializer, so a throw
    /// here would leave that type permanently unusable — taking the calculations down
    /// with the metadata. <c>DeclaredOnly</c> keeps inherited <see cref="object"/>
    /// statics such as <c>ReferenceEquals</c> out of reach of a listing's method name.
    /// </remarks>
    /// <param name="type">Static class to enumerate.</param>
    /// <returns>The type's own public static methods, or empty when it cannot be read.</returns>
    private static MethodInfo[] GetStaticMethods(
        [DynamicallyAccessedMembers(MethodMembers)] Type type)
    {
        try
        {
            return type.GetMethods(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        }
        catch (Exception ex) when (ex is TypeLoadException or ReflectionTypeLoadException)
        {
            return [];
        }
    }
}
