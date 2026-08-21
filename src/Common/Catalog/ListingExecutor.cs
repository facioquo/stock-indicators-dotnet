using System.Reflection;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for dynamic indicator execution based on catalog metadata.
/// </summary>
internal static class ListingExecutor
{
    /// <summary>
    /// Executes an indicator method dynamically using catalog metadata.
    /// </summary>
    /// <typeparam name="TResult">Expected result type.</typeparam>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="listing">Indicator listing containing metadata.</param>
    /// <param name="parameters">
    /// Optional parameter value overrides. This dictionary provides user-specified values
    /// that override the default values defined in <paramref name="listing"/>.Parameters.
    /// The listing.Parameters metadata defines the schema (names, types, defaults),
    /// while this dictionary provides runtime override values.
    /// </param>
    /// <returns>Indicator results.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the indicator cannot be executed.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="bars"/> is <c>null</c>.</exception>
    internal static IReadOnlyList<TResult> Execute<TResult>(
        IEnumerable<IBar> bars,
        IndicatorListing listing,
        Dictionary<string, object>? parameters = null)
        where TResult : class
    {
        // Validate inputs
        ArgumentNullException.ThrowIfNull(bars);
        ArgumentNullException.ThrowIfNull(listing);

        string methodName = listing.MethodName
            ?? throw new InvalidOperationException("MethodName is required for dynamic execution");

        // Find the method's overloads across the library's static classes
        List<MethodInfo> methods = CatalogMethodResolver.GetOverloads(methodName).ToList();

        if (methods.Count == 0)
        {
            throw new InvalidOperationException($"Method '{methodName}' not found");
        }

        // Reject overrides that name no catalog parameter. Ignoring them would fall
        // through to the default below, so the caller would receive a value it did not
        // ask for, with no indication that its argument was discarded.
        if (parameters is not null)
        {
            foreach (string providedName in parameters.Keys)
            {
                if (listing.Parameters?.Any(p => p.ParameterName == providedName) != true)
                {
                    string expected = listing.Parameters is { Count: > 0 }
                        ? string.Join(", ", listing.Parameters.Select(static p => p.ParameterName))
                        : "(none - this indicator takes no parameters)";

                    throw new InvalidOperationException(
                        $"Parameter '{providedName}' is not defined for indicator '{listing.Uiid}'. "
                      + $"Expected one of: {expected}");
                }
            }
        }

        // Build parameter array using catalog metadata and user overrides
        List<object?> parameterList = [bars];

        // Add parameters based on catalog metadata
        if (listing.Parameters != null)
        {
            foreach (IndicatorParam param in listing.Parameters)
            {

                // Check if user provided an override
                if (parameters?.TryGetValue(param.ParameterName, out object? value) == true)
                {
                    parameterList.Add(value);
                }
                else if (param.IsRequired)
                {
                    // Use default value for required parameters
                    if (param.DefaultValue == null)
                    {
                        throw new InvalidOperationException(
                            $"Required parameter {param.ParameterName} has no default value and was not provided");
                    }

                    parameterList.Add(param.DefaultValue);
                }
                else
                {
                    // For optional parameters, use default value if available
                    if (param.DefaultValue != null)
                    {
                        parameterList.Add(param.DefaultValue);
                    }
                }
            }
        }

        // Find the method that matches our parameter count. Failing an exact match,
        // accept an overload whose extra trailing parameters are all optional and
        // supply their declared defaults: a catalog need not enumerate every optional
        // parameter a method offers, and refusing to bind would make such a listing
        // permanently unexecutable. Prefer the fewest extra parameters so the choice
        // does not depend on reflection order.
        MethodInfo? targetMethod = SelectByArity(methods, parameterList);

        if (targetMethod is null)
        {
            targetMethod = methods
                .Where(m => m.GetParameters().Length > parameterList.Count
                         && m.GetParameters().Skip(parameterList.Count).All(static p => p.IsOptional))
                .OrderBy(static m => m.GetParameters().Length)
                .FirstOrDefault()
                ?? throw new InvalidOperationException($"No '{methodName}' method found with {parameterList.Count} parameters");

            foreach (ParameterInfo optional in targetMethod.GetParameters().Skip(parameterList.Count))
            {
                parameterList.Add(optional.DefaultValue);
            }
        }

        // If the method is generic, make it specific for the IBar interface type.
        // Indicator methods that are generic use IBar as the constraint.
        if (targetMethod.IsGenericMethodDefinition)
        {
            Type[] genericArguments = targetMethod.GetGenericArguments();
            if (genericArguments.Length == 1)
            {
                targetMethod = targetMethod.MakeGenericMethod(typeof(IBar));
            }
        }

        // Execute the method via reflection
        object? result = targetMethod.Invoke(null, parameterList.ToArray())
            ?? throw new InvalidOperationException("Method execution returned null");

        // Cast to expected type
        return result is IReadOnlyList<TResult> typedResult
            ? typedResult
            : throw new InvalidOperationException($"Result is not of expected type {typeof(IReadOnlyList<TResult>).Name}");
    }

    /// <summary>
    /// Selects the overload whose parameter count matches the assembled arguments.
    /// </summary>
    /// <remarks>
    /// Two overloads can share an argument count — <c>ToGator</c> has one taking
    /// <c>IReadOnlyList&lt;IReusable&gt;</c> and one taking
    /// <c>IReadOnlyList&lt;AlligatorResult&gt;</c>, both arity one. Picking the first
    /// match would let <c>Type.GetMethods</c> decide, and that order is documented as
    /// unspecified, so the same call could bind differently across runtimes or builds.
    /// Prefer the candidate whose parameter types actually accept the arguments; when
    /// that still leaves a tie, order the candidates so the choice is at least
    /// reproducible rather than incidental.
    /// </remarks>
    /// <param name="methods">Overloads sharing the listing's method name.</param>
    /// <param name="arguments">Arguments assembled from the catalog metadata.</param>
    /// <returns>The chosen overload, or <c>null</c> when none has a matching count.</returns>
    private static MethodInfo? SelectByArity(
        IReadOnlyList<MethodInfo> methods,
        List<object?> arguments)
    {
        List<MethodInfo> candidates = methods
            .Where(m => m.GetParameters().Length == arguments.Count)
            .ToList();

        if (candidates.Count <= 1)
        {
            return candidates.FirstOrDefault();
        }

        List<MethodInfo> accepting = candidates
            .Where(m => Accepts(m, arguments))
            .ToList();

        return (accepting.Count == 1 ? accepting : Reproducible(accepting.Count > 0 ? accepting : candidates))
            .First();
    }

    /// <summary>
    /// Determines whether a method's parameters accept the assembled arguments.
    /// </summary>
    /// <param name="method">Candidate overload.</param>
    /// <param name="arguments">Arguments assembled from the catalog metadata.</param>
    /// <returns><c>true</c> when every argument is assignable to its parameter.</returns>
    private static bool Accepts(MethodInfo method, List<object?> arguments)
    {
        // a generic definition's parameter types are still open, so they cannot be
        // tested until the type argument is substituted further below
        if (method.IsGenericMethodDefinition)
        {
            return true;
        }

        ParameterInfo[] parameters = method.GetParameters();

        for (int i = 0; i < parameters.Length; i++)
        {
            Type parameterType = parameters[i].ParameterType;

            if (arguments[i] is null)
            {
                if (parameterType.IsValueType && Nullable.GetUnderlyingType(parameterType) is null)
                {
                    return false;
                }
            }
            else if (!parameterType.IsInstanceOfType(arguments[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Orders overloads so an unavoidable tie resolves the same way every time.
    /// </summary>
    /// <param name="methods">Tied candidates.</param>
    /// <returns>The candidates in a stable, declaration-independent order.</returns>
    private static IEnumerable<MethodInfo> Reproducible(IEnumerable<MethodInfo> methods)
        => methods
            .OrderBy(static m => m.DeclaringType?.FullName, StringComparer.Ordinal)
            .ThenBy(static m => string.Join(
                ",", m.GetParameters().Select(static p => p.ParameterType.FullName)), StringComparer.Ordinal);

    /// <summary>
    /// Executes an indicator method dynamically using catalog metadata with parameter values.
    /// This is a convenience method that creates the parameter dictionary automatically.
    /// </summary>
    /// <typeparam name="TResult">Expected result type.</typeparam>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="listing">Indicator listing containing metadata.</param>
    /// <param name="parameterValues">Parameter values in the order they appear in the listing.</param>
    /// <returns>Indicator results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="listing"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when an argument is invalid</exception>
    internal static IReadOnlyList<TResult> Execute<TResult>(
        IEnumerable<IBar> bars,
        IndicatorListing listing,
        params object[] parameterValues)
        where TResult : class
    {
        // Validate inputs
        ArgumentNullException.ThrowIfNull(listing);
        ArgumentNullException.ThrowIfNull(parameterValues);

        Dictionary<string, object>? parameters = null;

        if (parameterValues.Length > 0 && listing.Parameters != null)
        {
            if (parameterValues.Length > listing.Parameters.Count)
            {
                throw new ArgumentException($"Too many parameter values provided. Expected {listing.Parameters.Count}, got {parameterValues.Length}");
            }

            parameters = [];
            for (int i = 0; i < parameterValues.Length; i++)
            {
                parameters[listing.Parameters[i].ParameterName] = parameterValues[i];
            }
        }

        return Execute<TResult>(bars, listing, parameters);
    }
}
