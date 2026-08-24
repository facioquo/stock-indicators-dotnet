namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents a customized indicator configuration that can be configured, sourced, and executed.
/// Supports fluent API for building indicator execution pipelines.
/// </summary>
public class ListingExecutionBuilder
{
    private readonly Dictionary<string, object> _parameterOverrides;
    private readonly HashSet<string> _omittedParameters;
    private IEnumerable<IBar>? _bars;

    internal ListingExecutionBuilder(
        IndicatorListing baseListing,
        Dictionary<string, object>? parameterOverrides = null,
        HashSet<string>? omittedParameters = null)
    {
        BaseListing = baseListing ?? throw new ArgumentNullException(nameof(baseListing));
        _parameterOverrides = parameterOverrides ?? [];
        _omittedParameters = omittedParameters ?? new HashSet<string>(StringComparer.Ordinal);
    }

    /// <summary>
    /// Sets or overrides a parameter value for the indicator.
    /// </summary>
    /// <param name="parameterName">Name of the parameter to override.</param>
    /// <param name="value">Value to set for the parameter.</param>
    /// <returns>A new <see cref="ListingExecutionBuilder"/> with the parameter override applied.</returns>
    /// <exception cref="ArgumentException">Thrown when an argument is invalid</exception>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
    public ListingExecutionBuilder WithParamValue(string parameterName, object value)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
        {
            throw new ArgumentException("Parameter name cannot be null or empty", nameof(parameterName));
        }

        // A null argument cannot bind to the value types these parameters use, so it
        // previously failed deep inside reflection with no indication of the cause.
        // Leaving a parameter out is a separate, explicit request.
        if (value is null)
        {
            throw new ArgumentNullException(
                nameof(value),
                $"Parameter '{parameterName}' cannot be set to null. "
              + $"To leave it out of the call entirely, use WithoutParam(\"{parameterName}\").");
        }

        ValidateParameterType(parameterName, value);

        Dictionary<string, object> newOverrides = new(_parameterOverrides) {
            [parameterName] = value
        };

        // supplying a value reverses an earlier omission, so the last call wins
        HashSet<string> newOmitted = new(_omittedParameters, StringComparer.Ordinal);
        newOmitted.Remove(parameterName);

        return new ListingExecutionBuilder(BaseListing, newOverrides, newOmitted) {
            _bars = _bars
        };
    }

    /// <summary>
    /// Leaves a parameter out of the call entirely, so overload resolution runs as if
    /// the caller had never supplied it.
    /// </summary>
    /// <remarks>
    /// Some overloads mean "this argument was not given" rather than "this argument
    /// took its default". <c>ToPrs(sourceEval, sourceBase)</c> computes no
    /// <c>PrsPercent</c> at all, which no value inside the listing's declared
    /// <see cref="IndicatorParam.Minimum"/>/<see cref="IndicatorParam.Maximum"/> range
    /// produces — it is reachable only by passing the <c>int.MinValue</c> sentinel that
    /// overload forwards, which catalog metadata gives a caller no way to discover.
    /// Omitting the argument expresses the same intent without the magic constant.
    /// <para>
    /// A shorter overload is selected where one exists. Where none does, the method's
    /// own default for that parameter applies instead, so omitting is a request about
    /// the call rather than a guarantee about which overload runs.
    /// </para>
    /// <para>
    /// Arguments bind positionally, so only a trailing run of parameters can be
    /// dropped. Omitting one while a later one is still supplied is rejected when
    /// <see cref="Execute{TResult}"/> runs, rather than silently shifting arguments
    /// into the wrong slots — it cannot be rejected here, because a later call may
    /// still drop the parameters that follow.
    /// </para>
    /// </remarks>
    /// <param name="parameterName">Name of the parameter to leave out.</param>
    /// <returns>A new <see cref="ListingExecutionBuilder"/> with the omission applied.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the name is empty, names no parameter of this indicator, or names a
    /// series parameter, which supplies data rather than a setting.
    /// </exception>
    public ListingExecutionBuilder WithoutParam(string parameterName)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
        {
            throw new ArgumentException("Parameter name cannot be null or empty", nameof(parameterName));
        }

        IndicatorParam param
            = BaseListing.Parameters?.FirstOrDefault(p => p.ParameterName == parameterName)
            ?? throw new ArgumentException(
                $"Parameter '{parameterName}' not found in indicator '{BaseListing.Uiid}'",
                nameof(parameterName));

        if (param.DataType == IndicatorParam.SeriesDataType)
        {
            throw new ArgumentException(
                $"Series parameter '{parameterName}' cannot be omitted for indicator '{BaseListing.Uiid}'; "
              + "it supplies the data the indicator reads, not a setting with a shorter form.",
                nameof(parameterName));
        }

        // an omission reverses an earlier value, so the last call wins
        Dictionary<string, object> newOverrides = new(_parameterOverrides);
        newOverrides.Remove(parameterName);

        HashSet<string> newOmitted = new(_omittedParameters, StringComparer.Ordinal) {
            parameterName
        };

        return new ListingExecutionBuilder(BaseListing, newOverrides, newOmitted) {
            _bars = _bars
        };
    }

    /// <summary>
    /// Sets multiple parameter values for the indicator.
    /// </summary>
    /// <param name="parameters">Dictionary of parameter names and values.</param>
    /// <returns>A new <see cref="ListingExecutionBuilder"/> with the parameter overrides applied.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="parameters"/> is <c>null</c>, or any value in it is <c>null</c>.
    /// </exception>
    public ListingExecutionBuilder WithParams(Dictionary<string, object> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        Dictionary<string, object> newOverrides = new(_parameterOverrides);
        HashSet<string> newOmitted = new(_omittedParameters, StringComparer.Ordinal);

        foreach (KeyValuePair<string, object> kvp in parameters)
        {
            if (kvp.Value is null)
            {
                throw new ArgumentNullException(
                    nameof(parameters),
                    $"Parameter '{kvp.Key}' cannot be set to null. "
                  + $"To leave it out of the call entirely, use WithoutParam(\"{kvp.Key}\").");
            }

            ValidateParameterType(kvp.Key, kvp.Value);

            newOverrides[kvp.Key] = kvp.Value;
            newOmitted.Remove(kvp.Key);
        }

        return new ListingExecutionBuilder(BaseListing, newOverrides, newOmitted) {
            _bars = _bars
        };
    }

    /// <summary>
    /// Specifies the source bars for the indicator calculation.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <returns>A new <see cref="ListingExecutionBuilder"/> with the bars set.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="bars"/> is <c>null</c>.</exception>
    public ListingExecutionBuilder FromSource(IEnumerable<IBar> bars)
    {
        ArgumentNullException.ThrowIfNull(bars);

        return new(BaseListing, _parameterOverrides, _omittedParameters) {
            _bars = bars
        };
    }

    /// <summary>
    /// Specifies a series source for the indicator calculation.
    /// This is equivalent to calling WithParamValue for a series parameter,
    /// but provides a more intuitive fluent API for chaining indicators.
    /// </summary>
    /// <typeparam name="T">Type of elements in the series, which must implement IReusable.</typeparam>
    /// <param name="series">Series data to process.</param>
    /// <param name="parameterName">Name of the series parameter. If null, attempts to find the first series parameter.</param>
    /// <returns>A new <see cref="ListingExecutionBuilder"/> with the series parameter set.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="series"/> is <c>null</c>.</exception>
    public ListingExecutionBuilder FromSource<T>(IReadOnlyList<T> series, string? parameterName = null)
        where T : IReusable
    {
        ArgumentNullException.ThrowIfNull(series);

        // Find the appropriate series parameter
        string targetParam = parameterName ?? FindFirstSeriesParameter();

        return WithParamValue(targetParam, series);
    }

    /// <summary>
    /// Executes the configured indicator and returns the results.
    /// </summary>
    /// <typeparam name="TResult">Expected result type.</typeparam>
    /// <returns>Indicator results.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when bars have not been set via From(); when an omission requested by
    /// <see cref="WithoutParam(string)"/> is not a trailing run, or names a parameter
    /// the method has no shorter form for; or when the assembled arguments match no
    /// overload.
    /// </exception>
    public IReadOnlyList<TResult> Execute<TResult>()
        where TResult : class => _bars == null
            ? throw new InvalidOperationException("Bars must be set using From() before calling Execute()")
            : ListingExecutor.Execute<TResult>(_bars, BaseListing, _parameterOverrides, _omittedParameters);

    /// <summary>
    /// Validates that a parameter value is compatible with the expected parameter type.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="value">Value to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the parameter type is incompatible.</exception>
    private void ValidateParameterType(string parameterName, object value)
    {
        IndicatorParam? param = (BaseListing.Parameters?.FirstOrDefault(p => p.ParameterName == parameterName))
            ?? throw new ArgumentException($"Parameter '{parameterName}' not found in indicator '{BaseListing.Uiid}'", nameof(parameterName));

        // Validate series parameters
        if (param.DataType == IndicatorParam.SeriesDataType)
        {
            if (value is not System.Collections.IEnumerable)
            {
                throw new ArgumentException(
                    $"Parameter '{parameterName}' expects a series (IReadOnlyList<T> where T : IReusable), but received {value.GetType().Name}", nameof(value));
            }

            // Check if the series elements implement IReusable
            Type valueType = value.GetType();

            if ((valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(List<>))
              || valueType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)))
            {
                Type? elementType = valueType.GetGenericArguments().FirstOrDefault()
                    ?? valueType.GetInterfaces()
                        .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))?.GetGenericArguments().FirstOrDefault();

                if (elementType != null && !typeof(IReusable).IsAssignableFrom(elementType))
                {
                    throw new ArgumentException(
                        $"Parameter '{parameterName}' expects series elements to implement IReusable, but received {elementType.Name}", nameof(value));
                }
            }
        }
        // Validate basic parameter types
        else if (param.DataType == "Int32" && value is not int)
        {
            throw new ArgumentException(
                $"Parameter '{parameterName}' expects an integer value, but received {value.GetType().Name}", nameof(value));
        }
        else if ((param.DataType == "Double" || param.DataType == "Nullable`1") && value is not double && value is not int) // Allow int to double conversion
        {
            throw new ArgumentException(
                $"Parameter '{parameterName}' expects a double value, but received {value.GetType().Name}", nameof(value));
        }
        else if (param.DataType == "Decimal" && value is not decimal)
        {
            throw new ArgumentException(
                $"Parameter '{parameterName}' expects a decimal value, but received {value.GetType().Name}. "
              + "Use a decimal literal (for example 2.5m); a double cannot be bound to a decimal parameter.", nameof(value));
        }
        else if (param.DataType == "Boolean" && value is not bool)
        {
            throw new ArgumentException(
                $"Parameter '{parameterName}' expects a boolean value, but received {value.GetType().Name}", nameof(value));
        }
        // Additional type validations can be added here for other parameter types as needed
    }

    /// <summary>
    /// Finds the first series parameter in the indicator's parameter list.
    /// </summary>
    /// <returns>Name of the first series parameter.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no series parameter is found.</exception>
    private string FindFirstSeriesParameter()
    {
        List<IndicatorParam> seriesParams
            = BaseListing.Parameters?
                .Where(static p => p.DataType == IndicatorParam.SeriesDataType)
                .ToList() ?? [];

        // With one series parameter the intent is unambiguous. With more than one,
        // guessing would bind the caller's series to the first slot and demote the
        // bars source to the second — silently inverting an asymmetric calculation
        // such as PRS or BETA. Require the name instead.
        return seriesParams.Count switch {
            0 => throw new InvalidOperationException(
                $"No series parameter found in indicator '{BaseListing.Uiid}'"),
            1 => seriesParams[0].ParameterName,
            _ => throw new InvalidOperationException(
                $"Indicator '{BaseListing.Uiid}' has {seriesParams.Count} series parameters "
              + $"({string.Join(", ", seriesParams.Select(static p => p.ParameterName))}); "
              + "name the one to bind: FromSource(series, \"parameterName\")")
        };
    }

    /// <summary>
    /// Gets the base indicator listing.
    /// </summary>
    public IndicatorListing BaseListing { get; }

    /// <summary>
    /// Gets the parameter overrides.
    /// </summary>
    public IReadOnlyDictionary<string, object> ParameterOverrides => _parameterOverrides;

    /// <summary>
    /// Gets the names of parameters that will be left out of the call entirely.
    /// </summary>
    /// <remarks>
    /// These take no argument at all, selecting a shorter overload rather than the
    /// listing's declared default. See <see cref="WithoutParam(string)"/>.
    /// <para>
    /// A copy, so that mutating the returned set through a downcast cannot reach this
    /// builder or the ones derived from it — instances share the backing collection
    /// because every configuring call copies before it changes anything.
    /// </para>
    /// </remarks>
    public IReadOnlySet<string> OmittedParameters
        => new HashSet<string>(_omittedParameters, StringComparer.Ordinal);

    /// <summary>
    /// Gets whether bars have been set.
    /// </summary>
    public bool HasBars => _bars != null;
}
