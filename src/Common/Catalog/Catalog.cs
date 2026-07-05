using System.Globalization;
using System.Text;
using System.Text.Json;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Registry for indicator listings.
/// </summary>
/// <remarks>
/// The indicator registry provides a central point for registering and retrieving
/// indicator listings. It supports various query operations based on style, category, etc.
/// All listings must be explicitly defined and registered.
/// </remarks>
public static partial class Catalog
{
    private static readonly JsonSerializerOptions InboundJsonOptions = new() {
        PropertyNameCaseInsensitive = true,
        Converters = { new StyleJsonConverter() }
    };

    private static readonly JsonSerializerOptions OutboundJsonOptions = new() {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Gets all indicator catalog listings
    /// </summary>
    /// <returns>All indicator listings.</returns>
    public static IReadOnlyList<IndicatorListing> Get() => Listings;

    /// <summary>
    /// Gets an indicator by its ID and style.
    /// </summary>
    /// <param name="id">Unique ID of the indicator.</param>
    /// <param name="style">Style of the indicator.</param>
    /// <returns>Indicator listing with the specified ID and style, or null if not found.</returns>
    public static IndicatorListing? Get(string id, Style style)
        => string.IsNullOrWhiteSpace(id)
            ? null
            : Listings.FirstOrDefault(x => string.Equals(x.Uiid, id, StringComparison.OrdinalIgnoreCase) && x.Style == style);

    /// <summary>
    /// Gets all indicators with the specified ID.
    /// </summary>
    /// <param name="id">Unique ID of the indicator.</param>
    /// <returns>All indicator listings with the specified ID.</returns>
    public static IReadOnlyList<IndicatorListing> Get(string id)
        => string.IsNullOrWhiteSpace(id)
            ? Array.Empty<IndicatorListing>()
            : Listings.Where(x => string.Equals(x.Uiid, id, StringComparison.OrdinalIgnoreCase)).ToList();

    /// <summary>
    /// Gets all indicators with the specified style.
    /// </summary>
    /// <param name="style">Style of indicators to retrieve.</param>
    /// <returns>All indicator listings with the specified style.</returns>
    public static IReadOnlyList<IndicatorListing> Get(Style style)
        => Listings
            .Where(x => x.Style == style)
            .ToList();

    /// <summary>
    /// Gets all indicators in the specified category.
    /// </summary>
    /// <param name="category">Category of indicators to retrieve.</param>
    /// <returns>All indicator listings in the specified category.</returns>
    public static IReadOnlyList<IndicatorListing> Get(Category category)
        => Listings
            .Where(x => x.Category == category)
            .ToList();

    /// <summary>
    /// Searches for indicators by name or ID, using a partial match.
    /// </summary>
    /// <param name="query">Search query.</param>
    /// <returns>All indicator listings that match the search query.</returns>
    public static IReadOnlyList<IndicatorListing> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Listings;
        }

        string normalizedQuery = query.Trim();

        return Listings
            .Where(x => x.Uiid.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                     || x.Name.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Searches for indicators by name, with an optional style filter.
    /// </summary>
    /// <param name="query">Search query.</param>
    /// <param name="style">Optional style filter.</param>
    /// <returns>All indicator listings that match the search query and style filter.</returns>
    internal static IReadOnlyList<IndicatorListing> Search(string query, Style style)
    {
        IReadOnlyList<IndicatorListing> allResults = Search(query);

        return allResults
            .Where(x => x.Style == style)
            .ToList();
    }

    /// <summary>
    /// Searches for indicators by name, with an optional category filter.
    /// </summary>
    /// <param name="query">Search query.</param>
    /// <param name="category">Optional category filter.</param>
    /// <returns>All indicator listings that match the search query and category filter.</returns>
    internal static IReadOnlyList<IndicatorListing> Search(string query, Category category)
        => Search(query)
            .Where(x => x.Category == category)
            .ToList();

    /// <summary>
    /// Converts the catalog to a JSON string representation.
    /// </summary>
    /// <param name="catalog">Catalog of indicator listings.</param>
    /// <param name="filePath">Optional file path to save the JSON output. If provided, the content will be written to this file.</param>
    /// <returns>A JSON string containing all catalog properties according to the defined schema.</returns>
    public static string ToJson(
        this IReadOnlyList<IndicatorListing> catalog,
        string? filePath = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        string json = JsonSerializer.Serialize(catalog, OutboundJsonOptions);

        // Save to file if path is provided
        if (!string.IsNullOrWhiteSpace(filePath))
        {
            File.WriteAllText(filePath, json);
        }

        return json;
    }

    /// <summary>
    /// Converts the catalog to a Markdown checklist format.
    /// </summary>
    /// <param name="catalog">Catalog of indicator listings.</param>
    /// <param name="filePath">
    /// Optional file path to save the Markdown output.
    /// If provided, the content will be written to this file.
    /// </param>
    /// <returns>A Markdown checklist string with format: - [ ] {id}: {name} ({available types})</returns>
    public static string ToMarkdownChecklist(
        this IReadOnlyList<IndicatorListing> catalog,
        string? filePath = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        StringBuilder sb = new();

        // Group by ID to show all styles for each indicator
        IOrderedEnumerable<IGrouping<string, IndicatorListing>> groupedIndicators = catalog
            .GroupBy(static listing => listing.Uiid)
            .OrderBy(static group => group.Key);

        foreach (IGrouping<string, IndicatorListing> group in groupedIndicators)
        {
            IndicatorListing firstListing = group.First();
            string availableTypes = string.Join(", ", group.OrderBy(static s => (int)s.Style).Select(static l => l.Style.ToString()));

            sb.AppendLine(CultureInfo.InvariantCulture, $"- [ ] {group.Key}: {firstListing.Name} ({availableTypes})");
        }

        string output = sb.ToString().TrimEnd();

        // Save to file if path is provided
        if (!string.IsNullOrWhiteSpace(filePath))
        {
            File.WriteAllText(filePath, output);
        }

        return output;
    }

    /// <summary>
    /// Converts the catalog to a Markdown table format.
    /// </summary>
    /// <param name="catalog">Catalog of indicator listings.</param>
    /// <param name="filePath">
    /// Optional file path to save the Markdown output.
    /// If provided, the content will be written to this file.
    /// </param>
    /// <returns>A Markdown table with columns for ID, Name, Series, Buffer, Stream.</returns>
    public static string ToMarkdownTable(
        this IReadOnlyList<IndicatorListing> catalog,
        string? filePath = null)
    {
        StringBuilder sb = new();

        // Group by ID to consolidate multiple styles into single rows
        IOrderedEnumerable<IGrouping<string, IndicatorListing>> groupedIndicators = catalog
            .GroupBy(static listing => listing.Uiid)
            .OrderBy(static group => group.Key);

        // Table header
        sb.AppendLine("| ID | Name | Series | Buffer | Stream |")
            .AppendLine("|---|---|:---:|:---:|:---:|");

        // Table rows
        foreach (IGrouping<string, IndicatorListing>? group in groupedIndicators)
        {
            IndicatorListing firstListing = group.First();
            List<IndicatorListing> styles = group.ToList();

            // Check which styles are available
            string hasSeries = styles.Any(static s => s.Style == Style.Series) ? "✓" : "";
            string hasBuffer = styles.Any(static s => s.Style == Style.Buffer) ? "✓" : "";
            string hasStream = styles.Any(static s => s.Style == Style.Stream) ? "✓" : "";

            sb.AppendLine(CultureInfo.InvariantCulture, $"| {group.Key} | {firstListing.Name} | {hasSeries} | {hasBuffer} | {hasStream} |");
        }

        string output = sb.ToString().TrimEnd();

        // Save to file if path is provided
        if (!string.IsNullOrWhiteSpace(filePath))
        {
            File.WriteAllText(filePath, output);
        }

        return output;
    }

    /// <summary>
    /// Executes an indicator using only its ID and style with a typed result.
    /// </summary>
    /// <typeparam name="TResult">Expected indicator result type.</typeparam>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="id">Indicator ID (e.g., "EMA", "RSI", "MACD").</param>
    /// <param name="style">Indicator style (Series, Stream, or Buffer).</param>
    /// <param name="parameters">Optional parameter overrides.</param>
    /// <returns>Indicator results as a typed list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when bars is null.</exception>
    /// <exception cref="ArgumentException">Thrown when id is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the indicator cannot be found or executed.</exception>
    internal static IReadOnlyList<TResult> ExecuteById<TResult>(
        this IEnumerable<IBar> bars,
        string id,
        Style style,
        Dictionary<string, object>? parameters = null)
        where TResult : class
    {
        // Validate inputs
        ArgumentNullException.ThrowIfNull(bars);

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Indicator ID cannot be null or empty.", nameof(id));
        }

        // Find the indicator listing
        IndicatorListing? listing = Get(id, style)
            ?? throw new InvalidOperationException($"Indicator '{id}' with style '{style}' not found in catalog.");

        // Execute using typed executor
        return ListingExecutor.Execute<TResult>(bars, listing, parameters);
    }

    /// <summary>
    /// Executes an indicator from a JSON configuration string with a typed result.
    /// </summary>
    /// <typeparam name="TResult">Expected indicator result type.</typeparam>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="json">JSON configuration string containing indicator settings.</param>
    /// <returns>Indicator results as a typed list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when json or bars is null.</exception>
    /// <exception cref="ArgumentException">Thrown when json is empty or invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the indicator cannot be found or executed.</exception>
    internal static IReadOnlyList<TResult> ExecuteFromJson<TResult>(
        this IEnumerable<IBar> bars,
        string json)
        where TResult : class
    {
        // Validate inputs
        ArgumentNullException.ThrowIfNull(bars);
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("JSON configuration cannot be null or empty.", nameof(json));
        }

        // Parse the JSON configuration
        IndicatorConfig config;
        try
        {
            config = JsonSerializer.Deserialize<IndicatorConfig>(json, InboundJsonOptions)
                ?? throw new ArgumentException("Failed to parse JSON configuration - result was null.", nameof(json));
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON configuration: {ex.Message}", nameof(json), ex);
        }

        // Convert JsonElement values to proper types in Parameters
        Dictionary<string, object>? convertedParameters = ConvertJsonElementsInParameters(config.Parameters);

        // Execute using the parsed configuration
        return bars.ExecuteById<TResult>(config.Id, config.Style, convertedParameters);
    }

    /// <summary>
    /// Converts JsonElement values in parameters dictionary to their appropriate types.
    /// </summary>
    /// <param name="parameters">Parameters dictionary that may contain JsonElement values.</param>
    /// <returns>A new dictionary with converted values.</returns>
    private static Dictionary<string, object>? ConvertJsonElementsInParameters(Dictionary<string, object>? parameters)
    {
        if (parameters == null || parameters.Count == 0)
        {
            return parameters;
        }

        Dictionary<string, object> converted = new();
        foreach (KeyValuePair<string, object> kvp in parameters)
        {
            if (kvp.Value is JsonElement jsonElement)
            {
                object? convertedValue = ConvertJsonElement(jsonElement);
                // Skip null values to maintain Dictionary<string, object> compatibility
                if (convertedValue != null)
                {
                    converted[kvp.Key] = convertedValue;
                }
            }
            else
            {
                converted[kvp.Key] = kvp.Value;
            }
        }

        return converted;
    }

    /// <summary>
    /// Converts a JsonElement to its appropriate .NET type.
    /// </summary>
    /// <param name="element">JsonElement to convert.</param>
    /// <returns>Converted value, or null for null JsonElements.</returns>
    /// <exception cref="NotImplementedException">Thrown when the feature is not yet implemented</exception>
    private static object? ConvertJsonElement(JsonElement element)

        // TODO: find alternative without using NotImplementedException
        // due to it triggering IDE3000 code analysis findings
        => element.ValueKind switch {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number when element.TryGetInt32(out int intValue) => intValue,
            JsonValueKind.Number when element.TryGetDecimal(out decimal decimalValue) => decimalValue,
            JsonValueKind.Number when element.TryGetDouble(out double doubleValue) => doubleValue,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Undefined => throw new NotImplementedException(),
            JsonValueKind.Object => element.ToString(),
            JsonValueKind.Array => element.ToString(),
            _ => element.ToString()
        };

    /// <summary>
    /// Custom JSON converter for Style enum that accepts either integer values or string names (case-insensitive).
    /// </summary>
    private sealed class StyleJsonConverter : JsonConverter<Style>
    {
#pragma warning disable IDE0060,S1172 // Remove unused parameter - signature required by JsonConverter base class
        public override Style Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
#pragma warning restore IDE0060,S1172
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                int numeric = reader.GetInt32();
                return Enum.IsDefined(typeof(Style), numeric)
                    ? (Style)numeric
                    : throw new JsonException($"Invalid Style value: {numeric}");
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                string? str = reader.GetString();
                return !string.IsNullOrWhiteSpace(str) && Enum.TryParse(str, ignoreCase: true, out Style parsed)
                    ? parsed
                    : throw new JsonException($"Invalid Style value: '{str}'");
            }

            throw new JsonException($"Unexpected token parsing Style: {reader.TokenType}");
        }

        /// <summary>
        /// Writes the string representation of a <see cref="Style"/> value to the
        /// specified <see cref="Utf8JsonWriter"/>.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="Utf8JsonWriter"/> to which the value will be written.
        /// Cannot be <see langword="null"/>.
        /// </param>
        /// <param name="value">
        /// The <see cref="Style"/> value to write. Cannot be <see langword="null"/>.
        /// </param>
        /// <param name="options">
        /// The serialization options to use. This parameter is not used in this implementation
        /// but cannot be <see langword="null"/>.
        /// </param>
#pragma warning disable IDE0060 // Remove unused parameter - signature required by JsonConverter base class
#pragma warning disable S1172 // Unused method parameters should be removed - signature required by JsonConverter base class
        public override void Write(
            Utf8JsonWriter writer,
            Style value,
            JsonSerializerOptions options)
#pragma warning restore S1172, IDE0060
                => writer.WriteStringValue(value.ToString());
    }
}
