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
    /// Gets the name of the result record type returned by the indicator method.
    /// </summary>
    /// <remarks>
    /// Derived from <see cref="MethodName"/> when the listing is built, so it cannot
    /// drift from the method it describes. This is the record type, not the method's
    /// literal return type: it is <c>EmaResult</c> for <c>ToEma</c>, <c>ToEmaList</c>,
    /// and <c>ToEmaHub</c> alike, even though those return
    /// <c>IReadOnlyList&lt;EmaResult&gt;</c>, <c>EmaList</c>, and <c>EmaHub</c>
    /// respectively. It is the record whose properties
    /// <see cref="IndicatorResult.DataName"/> names. It is <c>null</c> when
    /// <see cref="MethodName"/> is unset, and can also be <c>null</c> if the method
    /// cannot be resolved — which trimming or NativeAOT publishing may cause, since
    /// the derivation reflects over the assembly. Check for <c>null</c> before use.
    /// </remarks>
    public string? ResultRecordType { get; init; }

    /// <summary>
    /// Gets the name of the result record type returned by the indicator method.
    /// </summary>
    /// <remarks>
    /// Renamed to <see cref="ResultRecordType"/>, which says what the value actually
    /// is: the result record, not the method's literal return type. This alias keeps
    /// code written against 3.0.0 compiling and is removed in the next major version.
    /// It is excluded from serialization so the JSON carries the value once, under
    /// the new name.
    /// </remarks>
    [JsonIgnore]
    [Obsolete("Renamed to ResultRecordType. This alias is removed in the next major version.")]
    public string? ReturnType
    {
        get => ResultRecordType;
        init => ResultRecordType = value;
    }

    /// <summary>
    /// Gets or sets the method name for automation use cases.
    /// </summary>
    public string? MethodName { get; init; }

    /// <summary>
    /// Gets or sets the legend template for the indicator.
    /// </summary>
    public required string LegendTemplate { get; init; }
}
