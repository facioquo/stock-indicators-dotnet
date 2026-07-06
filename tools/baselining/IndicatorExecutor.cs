using System.Reflection;
using FacioQuo.Stock.Indicators;
using TestData = Test.Data.Data;

namespace Test.DataGenerator;

/// <summary>
/// Executes indicators and captures their output for baseline generation.
/// </summary>
internal static class IndicatorExecutor
{
    private static readonly IReadOnlyList<Bar> Bars = TestData.GetDefault();
    private static readonly IReadOnlyList<Bar> OtherBars = TestData.GetCompare();

    /// <summary>
    /// Executes an indicator and returns its results.
    /// </summary>
    /// <param name="listing">Indicator listing to execute.</param>
    /// <returns>Indicator results as a list.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the indicator method cannot be found or executed.</exception>
    public static object Execute(IndicatorListing listing)
    {
        // Get the method name from the listing
        string? methodName = listing.MethodName;

        if (string.IsNullOrEmpty(methodName))
        {
            throw new InvalidOperationException($"Method name not specified for indicator '{listing.Uiid}'");
        }

        // Check if this indicator uses SeriesParameter (requires dual bar lists)
        bool isDualInput = listing.Parameters?.Any(p => p.DataType == "IReadOnlyList<T> where T : IReusable") == true;

        // Find all types in the indicators assembly
        Assembly indicatorsAssembly = typeof(Bar).Assembly;

        // Calculate expected parameter count for method lookup
        // Extension source (this) counts as parameter 1
        // For dual-input: first series param is extension, second series param is explicit
        int expectedParamCount = 1; // Extension source
        bool firstSeriesParamSeen = false;
        if (listing.Parameters?.Count > 0)
        {
            foreach (IndicatorParam param in listing.Parameters)
            {
                if (param.DataType == "IReadOnlyList<T> where T : IReusable")
                {
                    if (!firstSeriesParamSeen)
                    {
                        // First series param is the extension source, already counted
                        firstSeriesParamSeen = true;
                        continue;
                    }

                    // Second series param is an explicit parameter
                    expectedParamCount++;
                }
                else
                {
                    // Non-series parameters are explicit
                    expectedParamCount++;
                }
            }
        }

        // Look for extension methods across all types
        MethodInfo? method = null;
        foreach (Type type in indicatorsAssembly.GetTypes())
        {
            method = type
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == methodName && m.IsGenericMethod && m.GetParameters().Length == expectedParamCount);

            if (method != null)
            {
                break;
            }
        }

        // If no generic method found, try to find a non-generic method with correct parameter count
        if (method == null)
        {
            foreach (Type type in indicatorsAssembly.GetTypes())
            {
                method = type
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m => m.Name == methodName && !m.IsGenericMethod && m.GetParameters().Length == expectedParamCount);

                if (method != null)
                {
                    break;
                }
            }
        }

        // Fallback: try any matching method if exact match not found
        if (method == null)
        {
            foreach (Type type in indicatorsAssembly.GetTypes())
            {
                method = type
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m => m.Name == methodName);

                if (method != null)
                {
                    break;
                }
            }
        }

        if (method == null)
        {
            throw new InvalidOperationException($"Method '{methodName}' not found for indicator '{listing.Uiid}'");
        }

        // Make generic method with Bar type if it's generic, otherwise use method directly
        MethodInfo targetMethod = method.IsGenericMethod
            ? method.MakeGenericMethod(typeof(Bar))
            : method;

        // Prepare parameters
        object?[] parameters = PrepareParameters(listing, targetMethod, isDualInput);

        // Invoke the method
        try
        {
            object? result = targetMethod.Invoke(null, parameters);

            return result
                ?? throw new InvalidOperationException($"Indicator '{listing.Uiid}' returned null result");
        }
        catch (TargetInvocationException ex)
        {
            // Unwrap the inner exception for clearer error messages
            if (ex.InnerException != null)
            {
                throw ex.InnerException;
            }

            throw;
        }
    }

    /// <summary>
    /// Gets the baseline file path for an indicator.
    /// </summary>
    /// <param name="listing">Indicator listing.</param>
    /// <returns>Full path to the baseline file.</returns>
    public static string GetBaselinePath(IndicatorListing listing)
    {
        // Baselines are stored in TestData/results/ directory
        // Filename pattern: {uiid-lowercase}.standard.json
        string resultsDir = Path.Combine(
            Directory.GetCurrentDirectory(),
            "TestData",
            "results");

        // Create results directory if it doesn't exist
        if (!Directory.Exists(resultsDir))
        {
            Directory.CreateDirectory(resultsDir);
        }

        // Use lowercase UIID with hyphen format (e.g., "SMA" -> "sma.standard.json")
        string fileName = $"{listing.Uiid.ToLowerInvariant()}.standard.json";
        return Path.GetFullPath(Path.Combine(resultsDir, fileName));
    }

    private static object?[] PrepareParameters(IndicatorListing listing, MethodInfo method, bool isDualInput)
    {
        ParameterInfo[] methodParams = method.GetParameters();

        // For dual-input indicators, first parameter is OtherBars (extension source)
        // For single-input indicators, first parameter is Bars
        List<object?> parameters = isDualInput ? new List<object?>() { OtherBars } : new List<object?>() { Bars };

        // Add additional parameters from the listing with their default values
        if (listing.Parameters?.Count > 0)
        {
            bool firstSeriesParamSkipped = false;
            int paramIndex = 1; // Start at 1 because first param is bars (extension source)

            foreach (IndicatorParam param in listing.Parameters)
            {
                // For dual-input indicators, the first series parameter is the extension source (already added)
                // The second series parameter is the comparison source
                if (param.DataType == "IReadOnlyList<T> where T : IReusable")
                {
                    // The first series parameter corresponds to the extension source
                    // which we already added to `parameters` above. Always skip it.
                    if (!firstSeriesParamSkipped)
                    {
                        firstSeriesParamSkipped = true;
                        continue;
                    }

                    // This is the second series parameter. Only for dual-input
                    // indicators do we add an explicit second series argument.
                    if (!isDualInput)
                    {
                        // Single-input indicator: no explicit second series parameter
                        // in the method signature — skip without advancing paramIndex.
                        continue;
                    }

                    // Dual-input indicator: add the comparison source and advance index
                    parameters.Add(Bars);
                    paramIndex++;

                    continue;
                }

                object? value = param.DefaultValue;

                // Special handling for required parameters with no default
                if (param.IsRequired && value == null && paramIndex < methodParams.Length)
                {
                    ParameterInfo methodParam = methodParams[paramIndex];

                    // For DateTime parameters (like VWAP startDate), use the first bar date
                    if (methodParam.ParameterType == typeof(DateTime))
                    {
                        value = Bars[0].Timestamp;
                    }
                }

                // Handle type conversions if needed
                if (value != null && paramIndex < methodParams.Length)
                {
                    ParameterInfo methodParam = methodParams[paramIndex];
                    Type targetType = methodParam.ParameterType;

                    // Convert double to decimal if needed (for RENKO, ZIGZAG, etc.)
                    if (value is double doubleValue && targetType == typeof(decimal))
                    {
                        value = (decimal)doubleValue;
                    }
                }

                parameters.Add(value);
                paramIndex++;
            }
        }

        return parameters.ToArray();
    }
}
