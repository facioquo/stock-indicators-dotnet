using System.Reflection;

namespace Catalogging;

/// <summary>
/// Resolves the compiled members that a catalog listing claims to describe.
/// </summary>
/// <remarks>
/// A listing carries only names: <see cref="IndicatorListing.MethodName"/>,
/// <see cref="IndicatorResult.DataName"/>, and <see cref="IndicatorParam.ParameterName"/>.
/// Nothing in the compiler binds those strings to the members they name, so this
/// helper resolves them by reflection and lets the binding tests assert the
/// catalog and the library still agree.
/// </remarks>
internal static class CatalogReflection
{
    private static readonly Assembly IndicatorsAssembly = typeof(Catalog).Assembly;

    /// <summary>
    /// Static classes that host the public indicator extension methods.
    /// </summary>
    private static readonly Type[] StaticHosts = IndicatorsAssembly
        .GetTypes()
        .Where(static t => t.IsClass && t.IsAbstract && t.IsSealed)
        .ToArray();

    /// <summary>
    /// Gets every public static overload with the given method name.
    /// </summary>
    /// <param name="methodName">Method name from a catalog listing.</param>
    /// <returns>All matching overloads; empty when the name resolves to nothing.</returns>
    internal static IReadOnlyList<MethodInfo> GetOverloads(string methodName)
        => StaticHosts
            .SelectMany(static t => t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .Where(m => m.Name == methodName)
            .ToList();

    /// <summary>
    /// Gets the result record type produced by an indicator method, for any style.
    /// </summary>
    /// <param name="method">Indicator method bound to a catalog listing.</param>
    /// <returns>
    /// Result record type — <c>EmaResult</c> for all three of
    /// <c>IReadOnlyList&lt;EmaResult&gt;</c> (Series), <c>EmaList</c> (Buffer),
    /// and <c>EmaHub</c> (Stream) — or <c>null</c> when it cannot be determined.
    /// </returns>
    internal static Type GetResultType(MethodInfo method)
    {
        Type returnType = method.ReturnType;

        // Series: IReadOnlyList<TResult>
        if (returnType.IsGenericType
         && returnType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
        {
            return returnType.GetGenericArguments()[0];
        }

        // Buffer: a BufferList implementing IReadOnlyList<TResult>
        Type listInterface = returnType
            .GetInterfaces()
            .FirstOrDefault(static i => i.IsGenericType
                                     && i.GetGenericTypeDefinition() == typeof(IReadOnlyList<>));

        if (listInterface != null)
        {
            return listInterface.GetGenericArguments()[0];
        }

        // Stream: a hub deriving from StreamHub<TIn, TOut>
        for (Type baseType = returnType; baseType != null; baseType = baseType.BaseType)
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
    /// Gets the indicator style implied by a method's return shape.
    /// </summary>
    /// <remarks>
    /// The three styles return three distinct shapes — <c>IReadOnlyList&lt;TResult&gt;</c>,
    /// a <c>BufferList</c>, and a <c>StreamHub</c>. Comparing the implied style against
    /// the listing's declared <see cref="IndicatorListing.Style"/> catches a listing bound
    /// to a real method of the wrong style, which the result record alone cannot reveal
    /// because all three styles share it.
    /// </remarks>
    /// <param name="method">Indicator method bound to a catalog listing.</param>
    /// <returns>The implied style, or <c>null</c> when the shape is unrecognized.</returns>
    internal static Style? GetImpliedStyle(MethodInfo method)
    {
        Type returnType = method.ReturnType;

        if (returnType.IsGenericType
         && returnType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
        {
            return Style.Series;
        }

        for (Type baseType = returnType; baseType != null; baseType = baseType.BaseType)
        {
            if (baseType.IsGenericType
             && baseType.GetGenericTypeDefinition() == typeof(StreamHub<,>))
            {
                return Style.Stream;
            }
        }

        return returnType.GetInterfaces().Any(static i => i.IsGenericType
                                                       && i.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
            ? Style.Buffer
            : null;
    }

    /// <summary>
    /// Gets the public instance property names on a result record.
    /// </summary>
    /// <param name="resultType">Result record type.</param>
    /// <returns>Property names available to a catalog-driven consumer.</returns>
    internal static ISet<string> GetPropertyNames(Type resultType)
        => resultType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(static p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

    /// <summary>
    /// Determines whether the catalog parameter names appear as an unbroken run,
    /// in order, within a method's parameter list.
    /// </summary>
    /// <remarks>
    /// The run must be contiguous, not merely ordered. <c>ListingExecutor</c> builds one
    /// argument per catalog parameter, in listing order, and selects an overload by
    /// argument count — so a catalog that skips a parameter in the middle still binds,
    /// and every later argument lands one slot early. Allowing a gapped match would let
    /// that silent misbinding pass. The run may start at any offset, because some
    /// listings declare the source parameter and most do not.
    /// </remarks>
    /// <param name="catalogNames">Catalog parameter names, in listing order.</param>
    /// <param name="methodNames">Method parameter names, in signature order.</param>
    /// <returns><c>true</c> when the catalog names form a contiguous run.</returns>
    internal static bool IsContiguousRun(string[] catalogNames, string[] methodNames)
        => methodNames.AsSpan().IndexOf(catalogNames.AsSpan()) >= 0;

    /// <summary>
    /// Gets a short identity for a listing, for use in failure messages.
    /// </summary>
    /// <param name="listing">Indicator listing.</param>
    /// <returns>Identity in the form <c>UIID/Style</c>.</returns>
    internal static string Describe(IndicatorListing listing)
        => $"{listing.Uiid}/{listing.Style}";
}
