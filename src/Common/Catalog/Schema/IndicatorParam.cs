namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents the configuration for an indicator parameter.
/// </summary>
[Serializable]
public record IndicatorParam
{
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
    /// Gets or sets whether the parameter is required.
    /// </summary>
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
