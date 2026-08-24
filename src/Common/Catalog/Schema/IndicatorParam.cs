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
    /// Tracks what a caller must pass to get the behavior this listing describes.
    /// <c>false</c> means leaving the argument out is both legal and equivalent to
    /// <see cref="DefaultValue"/>: either the parameter carries that value as a C#
    /// default, or the listing declares no default at all — VWAP's <c>startDate</c>
    /// is the latter, omittable through <c>ToVwap(bars)</c> with nothing promised.
    /// <para>
    /// <c>true</c> means the caller must pass a value, which includes the case where a
    /// shorter overload exists but does something other than the declared default.
    /// <c>ToPrs(sourceEval, sourceBase)</c> computes no <c>PrsPercent</c>, so PRS's
    /// <c>lookbackPeriods</c> is required despite being droppable; reach that overload
    /// deliberately with <see cref="ListingExecutionBuilder.WithoutParam(string)"/>.
    /// </para>
    /// Catalog-driven code generation reads this to decide whether it may skip the
    /// argument, so an understated value yields either source that does not compile or
    /// an indicator that quietly differs from the one described.
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
