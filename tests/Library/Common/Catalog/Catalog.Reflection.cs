#nullable enable
using System.Globalization;
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
    /// Gets every public static overload with the given method name.
    /// </summary>
    /// <remarks>
    /// Delegates to the library's own <see cref="CatalogMethodResolver"/> so the tests
    /// exercise the same resolution the catalog build uses. Re-implementing it here
    /// would make the derivation tests compare a value against a copy of the function
    /// that produced it.
    /// </remarks>
    /// <param name="methodName">Method name from a catalog listing.</param>
    /// <returns>All matching overloads; empty when the name resolves to nothing.</returns>
    internal static IReadOnlyList<MethodInfo> GetOverloads(string methodName)
        => CatalogMethodResolver.GetOverloads(methodName).ToList();

    /// <summary>
    /// Gets the result record type produced by an indicator method, for any style.
    /// </summary>
    /// <param name="method">Indicator method bound to a catalog listing.</param>
    /// <returns>Result record type, or <c>null</c> when it cannot be determined.</returns>
    internal static Type? GetResultType(MethodInfo method)
        => CatalogMethodResolver.GetResultType(method);

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

        for (Type? baseType = returnType; baseType is not null; baseType = baseType.BaseType)
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
        => IndexOfRun(catalogNames, methodNames) >= 0;

    /// <summary>
    /// Finds the offset at which the catalog parameter names begin within a method's
    /// parameter list, as an unbroken in-order run.
    /// </summary>
    /// <param name="catalogNames">Catalog parameter names, in listing order.</param>
    /// <param name="methodNames">Method parameter names, in signature order.</param>
    /// <returns>The starting offset, or <c>-1</c> when there is no such run.</returns>
    internal static int IndexOfRun(string[] catalogNames, string[] methodNames)
        => methodNames.AsSpan().IndexOf(catalogNames.AsSpan());

    /// <summary>
    /// Determines whether an overload carrying the whole catalog run gives the
    /// parameter at <paramref name="index"/> a C# default value.
    /// </summary>
    /// <remarks>
    /// This is the only way omitting the argument provably yields a known value, so it
    /// is what lets a listing advertise a <see cref="IndicatorParam.DefaultValue"/> that
    /// a caller can rely on by leaving the argument out.
    /// </remarks>
    /// <param name="overloads">All overloads of the listing's method.</param>
    /// <param name="catalogNames">Catalog parameter names, in listing order.</param>
    /// <param name="index">Index of the parameter being tested.</param>
    /// <returns><c>true</c> when the parameter carries a C# default.</returns>
    internal static bool HasCSharpDefault(
        IReadOnlyList<MethodInfo> overloads,
        string[] catalogNames,
        int index)
        => TryGetCSharpDefault(overloads, catalogNames, index, out _);

    /// <summary>
    /// Gets the C# default value for the catalog parameter at <paramref name="index"/>.
    /// </summary>
    /// <param name="overloads">All overloads of the listing's method.</param>
    /// <param name="catalogNames">Catalog parameter names, in listing order.</param>
    /// <param name="index">Index of the parameter being tested.</param>
    /// <param name="value">The compiler-recorded default, when there is one.</param>
    /// <returns><c>true</c> when the parameter carries a C# default.</returns>
    internal static bool TryGetCSharpDefault(
        IReadOnlyList<MethodInfo> overloads,
        string[] catalogNames,
        int index,
        out object? value)
    {
        foreach (MethodInfo method in overloads)
        {
            int offset = IndexOfRun(catalogNames, GetParameterNames(method));

            if (offset >= 0 && method.GetParameters()[offset + index] is { HasDefaultValue: true } parameter)
            {
                value = parameter.DefaultValue;
                return true;
            }
        }

        value = null;
        return false;
    }

    /// <summary>
    /// Determines whether a listing's declared default is the same value the compiler
    /// applies when the argument is left out.
    /// </summary>
    /// <remarks>
    /// The two arrive as differently typed boxes for the same number — a
    /// <c>decimal</c> parameter default against a <c>double</c> in the listing, or an
    /// enum member against the <c>int</c> the builder stores — so they are compared
    /// numerically rather than by boxed identity or rendered text.
    /// </remarks>
    /// <param name="csharpDefault">Compiler-recorded default.</param>
    /// <param name="declaredDefault">Value the listing advertises.</param>
    /// <returns><c>true</c> when the two agree.</returns>
    internal static bool DefaultsAgree(object? csharpDefault, object? declaredDefault)
    {
        if (csharpDefault is null || declaredDefault is null)
        {
            return csharpDefault is null && declaredDefault is null;
        }

        return IsNumeric(csharpDefault) && IsNumeric(declaredDefault)
            ? Convert.ToDecimal(csharpDefault, CultureInfo.InvariantCulture)
           == Convert.ToDecimal(declaredDefault, CultureInfo.InvariantCulture)
            : string.Equals(
                Convert.ToString(csharpDefault, CultureInfo.InvariantCulture),
                Convert.ToString(declaredDefault, CultureInfo.InvariantCulture),
                StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines whether a boxed value is a number, counting enums as their backing
    /// integer because that is how a listing stores an enum default.
    /// </summary>
    /// <param name="value">Boxed value.</param>
    /// <returns><c>true</c> when it converts to a number.</returns>
    private static bool IsNumeric(object value)
        => value is byte or sbyte or short or ushort or int or uint
                 or long or ulong or float or double or decimal
        || value.GetType().IsEnum;

    /// <summary>
    /// Determines whether a shorter overload drops the parameter at
    /// <paramref name="index"/> while still accepting every catalog parameter before it.
    /// </summary>
    /// <remarks>
    /// This makes the argument omittable but says nothing about what the shorter
    /// overload then does, and the compiler records no default to compare against.
    /// <c>ToPrs(sourceEval, sourceBase)</c> computes no <c>PrsPercent</c>, reachable
    /// otherwise only through an <c>int.MinValue</c> sentinel outside the parameter's
    /// declared range; <c>ToVwap(bars)</c> starts from the first bar's timestamp, a
    /// value that depends on the data rather than a constant. Neither is a default a
    /// listing could honestly advertise, so this kind of omittability can back only
    /// the absence of one.
    /// </remarks>
    /// <param name="overloads">All overloads of the listing's method.</param>
    /// <param name="catalogNames">Catalog parameter names, in listing order.</param>
    /// <param name="index">Index of the parameter being tested.</param>
    /// <returns><c>true</c> when a shorter overload omits it.</returns>
    internal static bool HasShorterOverload(
        IReadOnlyList<MethodInfo> overloads,
        string[] catalogNames,
        int index)
    {
        foreach (MethodInfo method in overloads)
        {
            string[] methodNames = GetParameterNames(method);

            if (!methodNames.Contains(catalogNames[index], StringComparer.Ordinal)
             && IndexOfRun(catalogNames[..index], methodNames) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets a method's parameter names in signature order.
    /// </summary>
    /// <param name="method">Indicator method bound to a catalog listing.</param>
    /// <returns>Parameter names, with <c>null</c> names normalized to empty.</returns>
    internal static string[] GetParameterNames(MethodInfo method)
        => method.GetParameters().Select(static p => p.Name ?? string.Empty).ToArray();

    /// <summary>
    /// Finds a public library type by its simple name.
    /// </summary>
    /// <remarks>
    /// Mirrors what a catalog-driven consumer does with a type name it reads from a
    /// listing: look the type up in the library and reflect over its properties.
    /// </remarks>
    /// <param name="typeName">Simple type name, such as <c>EmaResult</c>.</param>
    /// <returns>The matching public type, or <c>null</c> when there is none.</returns>
    internal static Type? FindPublicType(string typeName)
        => string.IsNullOrWhiteSpace(typeName)
            ? null
            : IndicatorsAssembly
                .GetTypes()
                .FirstOrDefault(t => t.IsPublic && string.Equals(t.Name, typeName, StringComparison.Ordinal));

    /// <summary>
    /// Gets a short identity for a listing, for use in failure messages.
    /// </summary>
    /// <param name="listing">Indicator listing.</param>
    /// <returns>Identity in the form <c>UIID/Style</c>.</returns>
    internal static string Describe(IndicatorListing listing)
        => $"{listing.Uiid}/{listing.Style}";
}
