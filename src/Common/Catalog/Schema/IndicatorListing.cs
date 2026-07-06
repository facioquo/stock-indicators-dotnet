using System.ComponentModel.DataAnnotations;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents an indicator listing with its configuration and parameters.
/// </summary>
[Serializable]
public record IndicatorListing
{
    /// <summary>
    /// Gets or sets the unique identifier of the indicator.
    /// </summary>
    [MinLength(2), UrlSafe]
    public required string Uiid { get; init; }

    /// <summary>
    /// Gets or sets the name of the indicator.
    /// </summary>
    [MinLength(5)]
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the style of the indicator (Series, Buffer, Stream).
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Style Style { get; init; } = Style.Series;

    /// <summary>
    /// Gets or sets the category of the indicator.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required Category Category { get; init; }

    /// <summary>
    /// Gets or sets the collection of parameters for the indicator.
    /// </summary>
    public IReadOnlyList<IndicatorParam>? Parameters { get; init; }

    /// <summary>
    /// Gets or sets the collection of result configurations for the indicator.
    /// </summary>
    public required IReadOnlyList<IndicatorResult> Results { get; init; }

    /// <summary>
    /// Gets or sets the return type name for the indicator method.
    /// </summary>
    public string? ReturnType { get; init; }

    /// <summary>
    /// Gets or sets the method name for automation use cases.
    /// </summary>
    public string? MethodName { get; init; }

    /// <summary>
    /// Gets or sets the legend template for the indicator.
    /// </summary>
    public required string LegendTemplate { get; init; }
}
