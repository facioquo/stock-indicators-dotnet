namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents a serializable indicator configuration that can be stored as JSON
/// and later used to build <see cref="ListingExecutionBuilder"/> instances.
/// </summary>
public class IndicatorConfig
{
    /// <summary>
    /// The indicator ID (e.g., "EMA", "RSI", "MACD").
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The indicator style (Series, Stream, or Buffer).
    /// </summary>
    public Style Style { get; set; } = Style.Series;

    /// <summary>
    /// Parameter overrides for the indicator.
    /// </summary>
    public Dictionary<string, object> Parameters { get; init; } = [];

    /// <summary>
    /// Names of parameters to leave out of the call entirely.
    /// </summary>
    /// <remarks>
    /// These take no argument at all, selecting a shorter overload rather than the
    /// listing's declared default — see <see cref="ListingExecutionBuilder.WithoutParam(string)"/>.
    /// Omitting is not the same as leaving a name out of <see cref="Parameters"/>, which
    /// falls back to the declared default, so a configuration must carry both to
    /// round-trip faithfully.
    /// </remarks>
    public IList<string> OmittedParameters { get; init; } = [];

    /// <summary>
    /// Optional display name for this configuration.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Optional description for this configuration.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Builds a <see cref="ListingExecutionBuilder"/> from this configuration.
    /// </summary>
    /// <returns>A <see cref="ListingExecutionBuilder"/> configured according to this config.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the indicator cannot be found.</exception>
    public ListingExecutionBuilder ToBuilder()
    {
        IndicatorListing? listing = Catalog.Get(Id, Style);
        return listing == null
            ? throw new InvalidOperationException($"Indicator '{Id}' with style '{Style}' not found in catalog")
            : new ListingExecutionBuilder(
                listing,
                Parameters,
                new HashSet<string>(OmittedParameters, StringComparer.Ordinal));
    }

    /// <summary>
    /// Creates an <see cref="IndicatorConfig"/> from a <see cref="ListingExecutionBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="ListingExecutionBuilder"/> to convert.</param>
    /// <returns>An <see cref="IndicatorConfig"/> representing the builder's configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
    public static IndicatorConfig FromBuilder(ListingExecutionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return new() {
            Id = builder.BaseListing.Uiid,
            Style = builder.BaseListing.Style,
            Parameters = new Dictionary<string, object>(builder.ParameterOverrides),
            OmittedParameters = [.. builder.OmittedParameters],
            DisplayName = builder.BaseListing.Name
        };
    }
}
