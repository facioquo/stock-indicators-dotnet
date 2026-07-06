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

        // Get the assembly containing the indicators
        Assembly indicatorsAssembly = typeof(Ema).Assembly;

        // Find all static classes in the assembly
        Type[] types = indicatorsAssembly.GetTypes()
            .Where(t => t.IsClass && t.IsAbstract && t.IsSealed) // static classes
            .ToArray();

        List<MethodInfo> methods = [];

        // Search for the method across all static classes
        foreach (Type type in types)
        {
            MethodInfo[] typeMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == methodName)
                .ToArray();
            methods.AddRange(typeMethods);
        }

        if (methods.Count == 0)
        {
            throw new InvalidOperationException($"Method {methodName} not found");
        }

        // Build parameter array using catalog metadata and user overrides
        List<object> parameterList = [bars];

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

        // Find the method that matches our parameter count
        MethodInfo? targetMethod = methods.FirstOrDefault(m => m.GetParameters().Length == parameterList.Count) ?? throw new InvalidOperationException($"No {methodName} method found with {parameterList.Count} parameters");

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
