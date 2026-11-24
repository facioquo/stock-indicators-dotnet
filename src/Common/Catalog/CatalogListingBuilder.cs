using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Builder for creating IndicatorListing instances using a fluent interface.
/// </summary>
internal class CatalogListingBuilder
{
    private string _name = string.Empty;
    private string _id = string.Empty;
    private Category _category = Category.Undefined;
    private string? _methodName;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
    private Type? _methodDeclaringType;

    private readonly string? _inheritedResultRecordType;
    private readonly List<IndicatorParam> _parameters = [];
    private readonly List<IndicatorResult> _results = [];
    private readonly List<IndicatorPane> _panes = [];

    /// <summary>
    /// Gets the current style setting for the builder.
    /// </summary>
    protected Style CurrentStyle { get; private set; } = Style.Series;

    /// <summary>
    /// Initializes a new instance of the CatalogListingBuilder class.
    /// </summary>
    internal CatalogListingBuilder()
    {
    }

    /// <summary>
    /// Initializes a new instance of the CatalogListingBuilder class from an existing listing.
    /// This enables the "inherit and override" pattern for creating multiple styles.
    /// </summary>
    /// <param name="baseListing">Base listing to inherit properties from.</param>
    internal CatalogListingBuilder(IndicatorListing baseListing)
    {
        ArgumentNullException.ThrowIfNull(baseListing);

        _name = baseListing.Name;
        _id = baseListing.Uiid;
        _category = baseListing.Category;
        _methodName = baseListing.MethodName;
        _inheritedResultRecordType = baseListing.ResultRecordType;
        CurrentStyle = baseListing.Style;

        // `with` copies every property, so one added later is never silently dropped;
        // EnumOptions is a mutable dictionary, so each copy gets its own.
        if (baseListing.Parameters != null)
        {
            foreach (IndicatorParam param in baseListing.Parameters)
            {
                _parameters.Add(param with {
                    EnumOptions = param.EnumOptions is null ? null : new(param.EnumOptions)
                });
            }
        }

        _results.AddRange(baseListing.Results);

        if (baseListing.Panes != null)
        {
            _panes.AddRange(baseListing.Panes);
        }
    }

    /// <summary>
    /// Sets the name of the indicator.
    /// </summary>
    /// <param name="name">Indicator name.</param>
    /// <returns>Builder instance for method chaining.</returns>
    internal CatalogListingBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the ID of the indicator.
    /// </summary>
    /// <param name="id">Indicator ID.</param>
    /// <returns>Builder instance for method chaining.</returns>
    internal CatalogListingBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the style of the indicator.
    /// </summary>
    /// <param name="style">Indicator style.</param>
    /// <returns>Builder instance for method chaining.</returns>
    internal CatalogListingBuilder WithStyle(Style style)
    {
        CurrentStyle = style;
        return this;
    }

    /// <summary>
    /// Sets the category of the indicator.
    /// </summary>
    /// <param name="category">Indicator category.</param>
    /// <returns>Builder instance for method chaining.</returns>
    internal CatalogListingBuilder WithCategory(Category category)
    {
        _category = category;
        return this;
    }

    /// <summary>
    /// Sets the indicator method that executes this listing.
    /// </summary>
    /// <param name="declaringType">Static class declaring the method.</param>
    /// <param name="methodName">Method name.</param>
    /// <returns>Builder instance for method chaining.</returns>
    internal CatalogListingBuilder WithMethodName(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type declaringType,
        string methodName)
    {
        _methodDeclaringType = declaringType;
        _methodName = methodName;
        return this;
    }

    /// <summary>
    /// Adds a parameter to the indicator.
    /// </summary>
    /// <typeparam name="T">Type of the parameter.</typeparam>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="displayName">Display name of the parameter.</param>
    /// <param name="description">Optional description of the parameter.</param>
    /// <param name="isRequired">Whether the parameter is required.</param>
    /// <param name="defaultValue">Optional default value for the parameter.</param>
    /// <param name="minimum">Optional minimum value for the parameter.</param>
    /// <param name="maximum">Optional maximum value for the parameter.</param>
    /// <returns>Builder instance for method chaining.</returns>
    internal CatalogListingBuilder AddParameter<T>(
        string parameterName,
        string displayName,
        string? description = null,
        bool isRequired = false,
        object? defaultValue = null,
        double? minimum = null,
        double? maximum = null)
    {
        _parameters.Add(new IndicatorParam {
            ParameterName = parameterName,
            DisplayName = displayName,
            Description = description,
            DataType = typeof(T).Name,
            IsRequired = isRequired,
            DefaultValue = defaultValue,
            Minimum = minimum,
            Maximum = maximum
        });

        return this;
    }

    /// <summary>
    /// Adds an enum parameter to the indicator.
    /// </summary>
    /// <typeparam name="T">Enum type of the parameter.</typeparam>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="displayName">Display name of the parameter.</param>
    /// <param name="description">Optional description of the parameter.</param>
    /// <param name="isRequired">Whether the parameter is required.</param>
    /// <param name="defaultValue">Optional default value for the parameter.</param>
    /// <returns>Builder instance for method chaining.</returns>
    internal CatalogListingBuilder AddEnumParameter<T>(
        string parameterName,
        string displayName,
        string? description = null,
        bool isRequired = false,
        T? defaultValue = default)
        where T : struct, Enum
    {
        // Create enum options dictionary
        Dictionary<int, string> enumOptions = [];
        foreach (T enumValue in Enum.GetValues<T>())
        {
            enumOptions.Add(Convert.ToInt32(enumValue, CultureInfo.InvariantCulture), enumValue.ToString());
        }

        _parameters.Add(new IndicatorParam {
            ParameterName = parameterName,
            DisplayName = displayName,
            Description = description,
            DataType = "enum",
            IsRequired = isRequired,
            DefaultValue = defaultValue.HasValue ? Convert.ToInt32(defaultValue.Value, CultureInfo.InvariantCulture) : null,
            Minimum = null,
            Maximum = null,
            EnumOptions = enumOptions
        });

        return this;
    }

    /// <summary>
    /// Adds a date parameter to the indicator.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="displayName">Display name of the parameter.</param>
    /// <param name="description">Optional description of the parameter.</param>
    /// <param name="isRequired">Whether the parameter is required.</param>
    /// <param name="defaultValue">Optional default value for the parameter.</param>
    /// <returns>Builder instance for method chaining.</returns>
    internal CatalogListingBuilder AddDateParameter(
        string parameterName,
        string displayName,
        string? description = null,
        bool isRequired = false,
        DateTime? defaultValue = null)
    {
        _parameters.Add(new IndicatorParam {
            ParameterName = parameterName,
            DisplayName = displayName,
            Description = description,
            DataType = "DateTime",
            IsRequired = isRequired,
            DefaultValue = defaultValue?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Minimum = null,
            Maximum = null
        });

        return this;
    }

    /// <summary>
    /// Adds a series parameter for IReadOnlyList&lt;T&gt; where T : IReusable type parameters.
    /// </summary>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <param name="displayName">Display name of the parameter.</param>
    /// <param name="description">Optional description of the parameter.</param>
    /// <param name="isRequired">Whether the parameter is required.</param>
    /// <returns>Builder instance for method chaining.</returns>
    internal CatalogListingBuilder AddSeriesParameter(
        string parameterName,
        string displayName,
        string? description = null,
        bool isRequired = true)
    {
        _parameters.Add(new IndicatorParam {
            ParameterName = parameterName,
            DisplayName = displayName,
            Description = description,
            DataType = IndicatorParam.SeriesDataType,
            IsRequired = isRequired,
            DefaultValue = null,
            Minimum = null,
            Maximum = null
        });

        return this;
    }

    /// <summary>
    /// Adds a result to the indicator.
    /// </summary>
    /// <param name="dataName">
    /// Name of the data property. Pass <c>nameof(TResult.Property)</c> rather than a
    /// literal so a renamed result property fails the build.
    /// </param>
    /// <param name="displayName">Display name of the result.</param>
    /// <param name="chartPane">Chart pane: <see cref="IndicatorResult.PricePane"/>, a named pane, or <c>null</c> when not charted.</param>
    /// <param name="dataType">Type of the result.</param>
    /// <param name="isReusable">Whether this is the reusable result.</param>
    /// <returns>Builder instance for method chaining.</returns>
    internal CatalogListingBuilder AddResult(
        string dataName,
        string displayName,
        string? chartPane,
        ResultType dataType = ResultType.Default,
        bool isReusable = false)
    {
        _results.Add(new IndicatorResult {
            DataName = dataName,
            DisplayName = displayName,
            ChartPane = chartPane,
            DataType = dataType,
            IsReusable = isReusable
        });

        return this;
    }

    /// <summary>
    /// Sets the fixed axis range and reference lines of a chart pane.
    /// </summary>
    /// <param name="name">Pane name; must match the <c>chartPane</c> of a result on this listing.</param>
    /// <param name="minimum">Fixed lower axis bound, or <c>null</c> to autoscale.</param>
    /// <param name="maximum">Fixed upper axis bound, or <c>null</c> to autoscale.</param>
    /// <param name="thresholds">Reference line values, in drawing order.</param>
    /// <returns>Builder instance for method chaining.</returns>
    internal CatalogListingBuilder WithPane(
        string name,
        double? minimum,
        double? maximum,
        params double[] thresholds)
    {
        _panes.Add(new IndicatorPane {
            Name = name,
            Minimum = minimum,
            Maximum = maximum,
            Thresholds = thresholds.Length > 0 ? [.. thresholds] : null
        });

        return this;
    }

    /// <summary>
    /// Adds common price High, Low, Close results to the indicator.
    /// </summary>
    /// <returns>Builder instance for method chaining.</returns>
    internal CatalogListingBuilder AddPriceHlcResult()
    {
        AddResult("High", "High", IndicatorResult.PricePane, ResultType.Default, false);
        AddResult("Low", "Low", IndicatorResult.PricePane, ResultType.Default, false);
        AddResult("Close", "Close", IndicatorResult.PricePane, ResultType.Default, false);
        return this;
    }

    /// <summary>
    /// Builds the IndicatorListing instance.
    /// </summary>
    /// <returns>A new IndicatorListing instance.</returns>
    internal IndicatorListing Build()
    {
        ValidateBeforeBuild();
        return new IndicatorListing {
            Name = _name,
            Uiid = _id,
            Style = CurrentStyle,
            Category = _category,
            Parameters = _parameters.Count > 0 ? _parameters : null,
            Results = _results,
            Panes = _panes.Count > 0 ? _panes : null,
            MethodName = _methodName,
            // a listing inheriting its method keeps the result record already derived for it
            ResultRecordType = _methodDeclaringType is null
                ? _inheritedResultRecordType
                : CatalogMethodResolver.Register(_methodDeclaringType, _methodName),
            LegendTemplate = GenerateLegendTemplate()
        };
    }

    private string GenerateLegendTemplate() =>
        // A simple legend template based on the indicator name
        $"{_name} ({_id})";

    /// <summary>
    /// Rejects a pane that draws no result or describes an impossible axis, so pane
    /// data cannot drift from the results it applies to.
    /// </summary>
    /// <exception cref="InvalidOperationException">A pane is unmatched, duplicated, empty, non-finite, or out of range.</exception>
    private void ValidatePanes()
    {
        HashSet<string> seen = [with(StringComparer.Ordinal)];

        foreach (IndicatorPane pane in _panes)
        {
            if (pane.Name == IndicatorResult.PricePane
                || !_results.Exists(r => r.ChartPane == pane.Name))
            {
                throw new InvalidOperationException(
                    $"Pane '{pane.Name}' must match a non-price chart pane of this listing's results.");
            }

            if (!seen.Add(pane.Name))
            {
                throw new InvalidOperationException($"Duplicate pane '{pane.Name}' is not allowed.");
            }

            if (pane.Minimum is null && pane.Maximum is null && pane.Thresholds is null)
            {
                throw new InvalidOperationException(
                    $"Pane '{pane.Name}' sets no range or thresholds; leave it out to autoscale.");
            }

            if (!double.IsFinite(pane.Minimum ?? 0)
                || !double.IsFinite(pane.Maximum ?? 0)
                || pane.Thresholds?.Any(static t => !double.IsFinite(t)) == true)
            {
                throw new InvalidOperationException(
                    $"Pane '{pane.Name}' range and thresholds must be finite.");
            }

            if (pane.Minimum >= pane.Maximum)
            {
                throw new InvalidOperationException(
                    $"Pane '{pane.Name}' minimum must be less than its maximum.");
            }

            if (pane.Thresholds?.Any(t => t < pane.Minimum || t > pane.Maximum) == true)
            {
                throw new InvalidOperationException(
                    $"Pane '{pane.Name}' thresholds must lie within its range.");
            }
        }
    }

    private void ValidateBeforeBuild()
    {
        if (string.IsNullOrWhiteSpace(_name))
        {
            throw new InvalidOperationException("Indicator name must be specified.");
        }

        if (string.IsNullOrWhiteSpace(_id))
        {
            throw new InvalidOperationException("Indicator ID must be specified.");
        }

        if (_results.Count == 0)
        {
            throw new InvalidOperationException("At least one result must be specified.");
        }

        // Check for duplicate parameter names
        List<string> parameterNames = _parameters.ConvertAll(static p => p.ParameterName);
        if (parameterNames.Count != parameterNames.Distinct().Count())
        {
            throw new InvalidOperationException("Duplicate parameter names are not allowed.");
        }

        // Check for duplicate result names
        List<string> resultNames = _results.ConvertAll(static r => r.DataName);
        if (resultNames.Count != resultNames.Distinct().Count())
        {
            throw new InvalidOperationException("Duplicate result names are not allowed.");
        }

        ValidatePanes();

        // Validate reusable result rules:
        // - For IReusable models: exactly one result must be marked as reusable
        // - For ISeries models: no results should be marked as reusable
        List<IndicatorResult> reusableResults = _results.Where(static r => r.IsReusable).ToList();

        if (reusableResults.Count > 1)
        {
            throw new InvalidOperationException("Only one result can be marked as reusable.");
        }

        // Note: We allow both scenarios:
        // 1. Multiple results with exactly one reusable (IReusable models)
        // 2. Multiple results with zero reusable (ISeries models)
        // The catalog instructions specify which to use based on the model type.
    }
}
