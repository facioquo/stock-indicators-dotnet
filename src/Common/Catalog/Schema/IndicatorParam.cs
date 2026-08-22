namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the configuration for an indicator parameter.
/// </summary>
[Serializable]
public record IndicatorParam
{
    /// <summary>
    /// The <see cref="DataType"/> value identifying a series-input parameter.
    /// </summary>
    internal const string SeriesDataType = "IReadOnlyList<T> where T : IReusable";


    /// <summary>
    /// Gets or sets the display name of the parameter.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets or sets the name of the parameter.
    /// </summary>
    public required string ParameterName { get; init; }

    /// <summary>
    /// Gets or sets the data type of the parameter.
    /// </summary>
    public required string DataType { get; init; }

    /// <summary>
    /// Gets or sets the description of the parameter.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets whether the caller must supply this parameter.
    /// </summary>
    /// <remarks>
    /// Tracks C# callability. <c>false</c> means some public form of
    /// <see cref="IndicatorListing.MethodName"/> lets a caller leave the argument out —
    /// either because the parameter carries a default value, or because a shorter
    /// overload omits it entirely, as <c>ToVwap(bars)</c> does for <c>startDate</c>.
    /// Catalog-driven code generation reads this to decide whether it may skip the
    /// argument, so an understated value yields source that does not compile.
    /// This is not a display hint: <see cref="DefaultValue"/> seeds an input field, and
    /// a required parameter may carry one too.
    /// </remarks>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Gets or sets the default value for the parameter.
    /// </summary>
    public object? DefaultValue { get; init; }

    /// <summary>
    /// Gets or sets the minimum value for the parameter.
    /// Can be null if no minimum is specified.
    /// </summary>
    public double? Minimum { get; init; }

    /// <summary>
    /// Gets or sets the maximum value for the parameter.
    /// Can be null if no maximum is specified.
    /// </summary>
    public double? Maximum { get; init; }

    /// <summary>
    /// Gets or sets the enum values dictionary for enum parameters.
    /// Maps enum integer values to their field names.
    /// This will be null for non-enum parameters.
    /// </summary>
    /// <remarks>Selection options for parameter arguments.</remarks>
    public Dictionary<int, string>? EnumOptions { get; init; }
}
