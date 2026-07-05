using System.Reflection;
using System.Text;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides extension methods for converting ISeries lists to formatted strings.
/// </summary>
public static partial class StringOut
{
    // =======================================
    // CONSTANTS
    // =======================================

    private static readonly string[] IndexHeaderName = ["i"];

    /// <summary>
    /// Default formats for numeric and date properties.
    /// 'Key' value is either property type or property name.
    /// 'Value' is the format string for the property's ToString().
    /// <remarks>
    /// The last matching key is used, so when users provide
    /// an 'args' dictionary, it will override these defaults.
    /// </remarks>
    /// </summary>
    private static readonly Dictionary<string, string> defaultArgs = new()
    {
        { "Decimal" , "auto" },
        { "Double"  , "F6" },
        { "Single"  , "F6" },
        { "Int16"   , "F0" },
        { "Int32"   , "F0" },
        { "Int64"   , "F0" },
        { "DateOnly", "yyyy-MM-dd" },
        { "DateTime", "yyyy-MM-dd HH:mm:ss" },
        { "DateTimeOffset", "yyyy-MM-dd HH:mm:ss" },
        { "Timestamp", "auto" }
    };

    // =======================================
    // CONSOLE OUTPUT
    // =======================================

    /// <summary>
    /// Converts a list of ISeries to a fixed-width formatted string and writes it to the console.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="args">Optional overrides for `ToString()` formatter. Key values can be type or property name.</param>
    /// <returns>Fixed-width formatted string representation of the list.</returns>
    public static string ToConsole<T>(
        this IReadOnlyList<T> source,
        IDictionary<string, string>? args = null)
        where T : ISeries => source.ToConsole(filter: static _ => true, args: args);

    /// <summary>
    /// Writes the contents of the series to the console and returns the output as a string.
    /// </summary>
    /// <typeparam name="T">Type of elements in the series. Must implement <see cref="ISeries"/>.</typeparam>
    /// <param name="source">Read-only list of series elements to be written to the console.</param>
    /// <param name="args">Optional key-value pairs that provide additional formatting or output options.</param>
    /// <returns>A string containing the console output generated from the series.</returns>
    public static string ToConsole<T>(
        this IReadOnlyList<T> source,
        params (string key, string value)[] args)
    where T : ISeries => source.ToConsole(filter: static _ => true, args: args);

    /// <summary>
    /// Converts a list of ISeries to a fixed-width formatted string and writes it to the console.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="filter">A predicate to filter the elements.</param>
    /// <param name="args">Optional overrides for `ToString()` formatter. Key values can be type or property name.</param>
    /// <returns>Fixed-width formatted string representation of the filtered list.</returns>
    public static string ToConsole<T>(
        this IEnumerable<T> source,
        Func<T, bool> filter,
        IDictionary<string, string>? args = null)
        where T : ISeries
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(filter);

        string? output = source.ToStringOut(filter, args);
        Console.WriteLine(output);
        return output ?? string.Empty;
    }

    /// <summary>
    /// Converts a list of ISeries to a fixed-width formatted string and writes it to the console.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="filter">A predicate to filter the elements.</param>
    /// <param name="args">Optional formatting arguments as key-value pairs.</param>
    /// <returns>Fixed-width formatted string representation of the filtered list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="filter"/> is <c>null</c>.</exception>
    public static string ToConsole<T>(
        this IEnumerable<T> source,
        Func<T, bool> filter,
        params (string key, string value)[] args)
        where T : ISeries
    {
        Dictionary<string, string>? argsDict = args?.Length > 0
            ? args
                .GroupBy(static x => x.key)
                .ToDictionary(static g => g.Key, static g => g.Last().value)
            : null;

        return source.ToConsole(filter, argsDict);
    }

    /// <summary>
    /// Converts a list of ISeries to a fixed-width formatted string and writes it to the console.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="filter">A predicate to filter the elements.</param>
    /// <param name="limitQty">Maximum number of elements to include in the output.</param>
    /// <param name="args">Optional formatting arguments as key-value pairs.</param>
    /// <returns>Fixed-width formatted string representation of the filtered list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="filter"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="limitQty"/> is less than or equal to zero.</exception>
    public static string ToConsole<T>(
        this IEnumerable<T> source,
        Func<T, bool> filter,
        int limitQty = int.MaxValue,
        IDictionary<string, string>? args = null)
        where T : ISeries
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(filter);

        if (limitQty <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limitQty), limitQty,
                "limitQty must be positive.");
        }

        string? output = source.ToStringOut(filter, limitQty, args);
        Console.WriteLine(output);
        return output ?? string.Empty;
    }

    /// <summary>
    /// Converts a list of ISeries to a fixed-width formatted string and writes it to the console.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="filter">A predicate to filter the elements.</param>
    /// <param name="limitQty">Maximum number of elements to include in the output.</param>
    /// <param name="args">Optional formatting arguments as key-value pairs.</param>
    /// <returns>Fixed-width formatted string representation of the filtered list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="filter"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="limitQty"/> is less than or equal to zero.</exception>
    public static string ToConsole<T>(
        this IEnumerable<T> source,
        Func<T, bool> filter,
        int limitQty,
        params (string key, string value)[] args)
        where T : ISeries
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(filter);

        if (limitQty <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limitQty), limitQty,
                "limitQty must be positive.");
        }

        Dictionary<string, string>? argsDict = args?.Length > 0
            ? args
                .GroupBy(static x => x.key)
                .ToDictionary(static g => g.Key, static g => g.Last().value)
            : null;

        string? output = source.ToStringOut(filter, limitQty, argsDict);
        Console.WriteLine(output);
        return output ?? string.Empty;
    }

    // =======================================
    // STRING OUTPUT
    // =======================================

    /// <summary>
    /// Converts a list of ISeries to a fixed-width formatted string.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="args">Optional overrides for `ToString()` formatter. Key values can be type or property name.</param>
    /// <returns>A fixed-width formatted string representation of the list.</returns>
    /// <remarks>
    /// Examples:
    /// <code>
    /// Dictionary&lt;string, string&gt; args = new()
    /// {
    ///     { "Decimal", "F2" },
    ///     { "DateTime", "MM/dd/yyyy" },
    ///     { "MyPropertyName", "C" }
    /// };
    /// </code>
    /// <para>
    /// The special format token <c>unset</c> (case-insensitive) can be used to bypass
    /// <see cref="IFormattable"/> formatting and instead call the type's default
    /// <c>ToString(null)</c> behavior. For example, specifying <c>{ "MyPropertyName", "unset" }</c>
    /// will cause the library to use the property's natural <c>ToString()</c> output rather than applying
    /// a numeric or date format string.
    /// </para>
    /// </remarks>
    public static string ToStringOut<T>(
        this IEnumerable<T> source, IDictionary<string, string>? args = null)
        where T : ISeries => source.ToList().ToStringOut(0, int.MaxValue, args);

    /// <summary>
    /// Converts a list of ISeries to a fixed-width formatted string.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="limitQty">Maximum number of elements to include in the output.</param>
    /// <param name="args">Optional overrides for `ToString()` formatter. Key values can be type or property name.</param>
    /// <returns>A fixed-width formatted string representation of the list.</returns>
    /// <remarks>
    /// Examples:
    /// <code>
    /// Dictionary&lt;string, string&gt; args = new()
    /// {
    ///     { "Decimal", "F2" },
    ///     { "DateTime", "MM/dd/yyyy" },
    ///     { "MyPropertyName", "C" }
    /// };
    /// </code>
    /// <para>
    /// The special format token <c>unset</c> (case-insensitive) can be used to bypass
    /// <see cref="IFormattable"/> formatting and instead call the type's default
    /// <c>ToString(null)</c> behavior. For example, specifying <c>{ "MyPropertyName", "unset" }</c>
    /// will cause the library to use the property's natural <c>ToString()</c> output rather than applying
    /// a numeric or date format string.
    /// </para>
    /// </remarks>
    public static string ToStringOut<T>(
        this IEnumerable<T> source, int limitQty, IDictionary<string, string>? args = null)
        where T : ISeries
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.ToList().ToStringOut(0, limitQty - 1, args);
    }

    /// <summary>
    /// Converts a list of ISeries to a fixed-width formatted string.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="startIndex">Starting index of the elements to include in the output.</param>
    /// <param name="endIndex">Ending index of the elements to include in the output.</param>
    /// <param name="args">Optional overrides for `ToString()` formatter. Key values can be type or property name.</param>
    /// <returns>A fixed-width formatted string representation of the list.</returns>
    /// <remarks>
    /// Examples:
    /// <code>
    /// Dictionary&lt;string, string&gt; args = new()
    /// {
    ///     { "Decimal", "F2" },
    ///     { "DateTime", "MM/dd/yyyy" },
    ///     { "MyPropertyName", "C" }
    /// };
    /// </code>
    /// </remarks>
    public static string ToStringOut<T>(
        this IEnumerable<T> source, int startIndex, int endIndex, IDictionary<string, string>? args = null)
        where T : ISeries => source.ToList().ToStringOut(startIndex, endIndex, args);

    /// <summary>
    /// Converts a filtered list of ISeries to a fixed-width formatted string.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="filter">A predicate to filter the elements.</param>
    /// <param name="args">Optional overrides for `ToString()` formatter. Key values can be type or property name.</param>
    /// <returns>A fixed-width formatted string representation of the filtered list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="filter"/> is <c>null</c>.</exception>
    public static string ToStringOut<T>(
        this IEnumerable<T> source,
        Func<T, bool> filter,
        IDictionary<string, string>? args = null)
        where T : ISeries
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(filter);

        List<T> sourceList = source.ToList();
        int[] indices = sourceList
            .Select((item, index) => new { item, index })
            .Where(x => filter(x.item))
            .Select(x => x.index)
            .ToArray();

        return ToStringOutPreserveIndices(sourceList, indices, args);
    }

    /// <summary>
    /// Converts a filtered list of ISeries to a fixed-width formatted string.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="filter">A predicate to filter the elements.</param>
    /// <param name="limitQty">Maximum number of elements to include in the output.</param>
    /// <param name="args">Optional overrides for `ToString()` formatter. Key values can be type or property name.</param>
    /// <returns>A fixed-width formatted string representation of the filtered list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="filter"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="limitQty"/> is less than or equal to zero.</exception>
    public static string ToStringOut<T>(
        this IEnumerable<T> source,
        Func<T, bool> filter,
        int limitQty,
        IDictionary<string, string>? args = null)
        where T : ISeries
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(filter);

        if (limitQty <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limitQty), limitQty,
                "limitQty must be positive.");
        }

        List<T> sourceList = source.ToList();
        int[] indices = sourceList
            .Select((item, index) => new { item, index })
            .Where(x => filter(x.item))
            .Take(limitQty)
            .Select(x => x.index)
            .ToArray();

        return ToStringOutPreserveIndices(sourceList, indices, args);
    }

    /// <summary>
    /// Converts a list of ISeries to a fixed-width formatted string.</summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="args">Optional overrides for `ToString()` formatter. Key values can be type or property name.</param>
    /// <returns>A fixed-width formatted string representation of the list.</returns>
    /// <remarks>
    /// Examples:
    /// <code>
    /// Dictionary&lt;string, string&gt; args = new()
    /// {
    ///     { "Decimal", "F2" },
    ///     { "DateTime", "MM/dd/yyyy" },
    ///     { "MyPropertyName", "C" }
    /// };
    /// </code>
    /// </remarks>
    public static string ToStringOut<T>(
        this IReadOnlyList<T> source,
        IDictionary<string, string>? args = null)
        where T : ISeries => source.ToStringOut(0, int.MaxValue, args);

    /// <summary>
    /// Converts a list of ISeries to a fixed-width formatted string.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="limitQty">Maximum number of elements to include in the output.</param>
    /// <param name="args">Optional overrides for `ToString()` formatter. Key values can be type or property name.</param>
    /// <returns>A fixed-width formatted string representation of the list.</returns>
    /// <remarks>
    /// Examples:
    /// <code>
    /// Dictionary&lt;string, string&gt; args = new()
    /// {
    ///     { "Decimal", "F2" },
    ///     { "DateTime", "MM/dd/yyyy" },
    ///     { "MyPropertyName", "C" }
    /// };
    /// </code>
    /// </remarks>
    public static string ToStringOut<T>(
        this IReadOnlyList<T> source,
        int limitQty,
        IDictionary<string, string>? args = null)
        where T : ISeries => source.ToStringOut(0, limitQty - 1, args);

    /// <summary>
    /// Converts a list of ISeries to a fixed-width formatted string.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="source">List of ISeries elements to convert.</param>
    /// <param name="startIndex">Starting index of the elements to include in the output.</param>
    /// <param name="endIndex">Ending index of the elements to include in the output.</param>
    /// <param name="args">Optional overrides for `ToString()` formatter. Key values can be type or property name.</param>
    /// <returns>A fixed-width formatted string representation of the list.</returns>
    /// <remarks>
    /// Examples:
    /// <code>
    /// Dictionary&lt;string, string&gt; args = new()
    /// {
    ///     { "Decimal", "F2" },
    ///     { "DateTime", "MM/dd/yyyy" },
    ///     { "MyPropertyName", "C" }
    /// };
    /// </code>
    /// </remarks>
    public static string ToStringOut<T>(
        this IReadOnlyList<T> source,
        int startIndex,
        int endIndex,
        IDictionary<string, string>? args = null)
        where T : ISeries
    {
        ArgumentNullException.ThrowIfNull(source);

        int endIndexReal = Math.Min(endIndex, source.Count - 1);
        T[] sourceSubset = source.Skip(startIndex).Take(endIndexReal - startIndex + 1).ToArray();

        Dictionary<string, string> formatArgs = defaultArgs
            .Concat(args ?? Enumerable.Empty<KeyValuePair<string, string>>())
            .GroupBy(kvp => kvp.Key.ToUpperInvariant())
            .ToDictionary(g => g.Key, g => g.Last().Value);

        // Get properties of the object
        PropertyInfo[] properties = GetStringOutProperties(typeof(T));

        // Define header values
        string[] headers = IndexHeaderName
            .Concat(properties.Select(p => p.Name))
            .ToArray();

        int columnCount = headers.Length;

        // Set formatting for each column
        string[] formats = new string[columnCount];
        bool[] alignLeft = new bool[columnCount];
        int[] columnWidth = headers.Select(header => header.Length).ToArray();

        formats[0] = "N0";     // index is always an integer
        alignLeft[0] = true;   // index is always left-aligned

        for (int i = 1; i < columnCount; i++)
        {
            PropertyInfo property = properties[i - 1];

            // try by property type
            formats[i] = formatArgs.TryGetValue(
                ColloquialTypeName(property.PropertyType).ToUpperInvariant(),
                    out string? typeFormat)
                ? typeFormat
                : string.Empty;

            // try by property name (overrides type)
            formats[i] = formatArgs.TryGetValue(
                property.Name.ToUpperInvariant(),
                    out string? nameFormat)
                ? nameFormat
                : formats[i];

            // handle auto-detect
            if (formats[i] == "auto")
            {
                formats[i] = AutoFormat(property, sourceSubset);
            }

            // set alignment
            alignLeft[i] = !property.PropertyType.IsNumeric();
        }

        // Compile formatted values
        string[][] dataRows = sourceSubset.Select((item, index) => {
            string[] row = new string[columnCount];

            row[0] = (index + startIndex).ToString(formats[0], invariantCulture);

            for (int i = 1; i < columnCount; i++)
            {
                object? value = properties[i - 1].GetValue(item);
                // Format value (supports special "unset" token to bypass format strings)
                row[i] = FormatValue(value, formats[i], invariantCulture);

                columnWidth[i] = Math.Max(columnWidth[i], row[i].Length);
            }

            return row;
        }).ToArray();

        columnWidth[0] = dataRows.Max(row => row[0].Length);

        // Compile formatted string
        StringBuilder sb = new();

        // Create header line with proper alignment
        sb.AppendJoin("  ",
            headers.Select((header, index) => alignLeft[index]
                ? header.PadRight(columnWidth[index])
                : header.PadLeft(columnWidth[index])
        )).AppendLine();

        // Create separator with column boundaries
        sb.AppendJoin("  ",
            columnWidth.Select(width => new string('-', width))
        ).AppendLine();

        // Create data lines with proper alignment
        foreach (string[] row in dataRows)
        {
            sb.AppendJoin("  ",
                row.Select((value, index) => alignLeft[index]
                    ? value.PadRight(columnWidth[index])
                    : value.PadLeft(columnWidth[index])
            )).AppendLine();
        }

        return sb.ToString();  // includes a trailing newline
    }

    // =======================================
    // HELPER METHODS
    // =======================================

    /// <summary>
    /// Determines the appropriate date precision or decimal places
    /// based on the first 1,000 actual values.
    /// </summary>
    /// <typeparam name="T">Type of elements in the list, which must implement ISeries.</typeparam>
    /// <param name="property">Array of PropertyInfo objects representing the properties of the type.</param>
    /// <param name="list">List of ISeries elements to analyze.</param>
    /// <returns>Format to be used in ToString()</returns>
    private static string AutoFormat<T>(
        PropertyInfo property,
        IReadOnlyList<T> list)
        where T : ISeries
    {
        Type propertyType = property.PropertyType;

        // auto-detect date format from precision
        if (propertyType == typeof(DateOnly))
        {
            return "yyyy-MM-dd";
        }

        else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTimeOffset))
        {
            List<string> dateValues = list
                .Take(1000)
                .Select(item => ((DateTime)property.GetValue(item)!).ToString("o", invariantCulture))
                .ToList();

            bool sameHour = dateValues.Select(d => d.Substring(11, 2)).Distinct().Count() == 1;
            bool sameMinute = dateValues.Select(d => d.Substring(14, 2)).Distinct().Count() == 1;
            bool sameSecond = dateValues.Select(d => d.Substring(17, 2)).Distinct().Count() == 1;

            return sameHour && sameMinute && sameSecond
                 ? "yyyy-MM-dd"
                 : sameSecond
                   ? "yyyy-MM-dd HH:mm"
                   : "yyyy-MM-dd HH:mm:ss";
        }

        // auto-detect decimal places
        else if (propertyType == typeof(decimal))
        {
            int decimalPlaces = list
                .Take(1000)
                .Max(item => ((decimal)property.GetValue(item)!).GetDecimalPlaces());

            return $"F{decimalPlaces}";
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Formats a value using an optional format string and provider.
    /// Treats the literal format string "unset" (case-insensitive) as a request
    /// to bypass IFormattable formatting and call the default ToString(null) behavior.
    /// </summary>
    /// <param name="value">Value to format.</param>
    /// <param name="fmt">Format string or special token.</param>
    /// <param name="provider">Format provider to use for IFormattable values.</param>
    /// <returns>Formatted string or empty string when value is null.</returns>
    private static string FormatValue(object? value, string? fmt, IFormatProvider provider)
    {
        if (string.Equals(fmt, "unset", StringComparison.OrdinalIgnoreCase))
        {
            return value is IFormattable formattable
                ? formattable.ToString(null, provider) ?? string.Empty
                : value?.ToString() ?? string.Empty;
        }

        return value is IFormattable formattableVal
            ? formattableVal.ToString(fmt, provider) ?? string.Empty
            : value?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Returns the colloquial type name for a given type.
    /// </summary>
    /// <param name="type">Type to get the colloquial name for.</param>
    /// <returns>Colloquial type name.</returns>
    public static string ColloquialTypeName(Type? type)
    {
        if (type == null)
        {
            return string.Empty;
        }

        // Handle nullable types - extract the underlying type
        Type actualType = Nullable.GetUnderlyingType(type) ?? type;

        // Return the type's name (C# alias for primitives, or the actual type name)
        return actualType.Name;
    }

    /// <summary>
    /// Formats a list of ISeries items using specific indices, preserving original index numbers.
    /// Reuses core formatting logic by delegating to the main ToStringOut implementation.
    /// </summary>
    /// <typeparam name="T">Type of series items, must implement ISeries.</typeparam>
    /// <param name="source">Source list of series items.</param>
    /// <param name="indices">Array of indices to include in the output.</param>
    /// <param name="args">Optional format arguments for specific properties or types.</param>
    /// <returns>Formatted string representation of the specified items.</returns>
    private static string ToStringOutPreserveIndices<T>(
        IReadOnlyList<T> source,
        int[] indices,
        IDictionary<string, string>? args = null)
        where T : ISeries
    {
        if (indices.Length == 0)
        {
            return string.Empty;
        }

        // Create a wrapper that tracks original indices
        var indexedItems = indices.Select(i => new { Index = i, Item = source[i] }).ToList();

        // Build format arguments (merge with defaults)
        Dictionary<string, string> formatArgs = defaultArgs
            .Concat(args ?? Enumerable.Empty<KeyValuePair<string, string>>())
            .GroupBy(kvp => kvp.Key.ToUpperInvariant())
            .ToDictionary(g => g.Key, g => g.Last().Value);

        // Get properties
        PropertyInfo[] properties = GetStringOutProperties(typeof(T));
        string[] headers = IndexHeaderName.Concat(properties.Select(p => p.Name)).ToArray();

        // Prepare formatting configuration
        string[] formats = new string[headers.Length];
        bool[] alignLeft = new bool[headers.Length];
        int[] columnWidth = headers.Select(h => h.Length).ToArray();

        formats[0] = "N0";
        alignLeft[0] = true;

        for (int i = 1; i < headers.Length; i++)
        {
            PropertyInfo property = properties[i - 1];

            // Property name takes precedence over type
            formats[i] = formatArgs.TryGetValue(property.Name.ToUpperInvariant(), out string? propFormat)
                ? propFormat
                : formatArgs.TryGetValue(ColloquialTypeName(property.PropertyType).ToUpperInvariant(), out string? typeFormat)
                    ? typeFormat
                    : "auto";

            if (formats[i] == "auto")
            {
                formats[i] = AutoFormat(property, source);
            }

            alignLeft[i] = !property.PropertyType.IsNumeric();
        }

        // Format all data rows (using original indices)
        string[][] dataRows = indexedItems.Select(x => {
            string[] row = new string[headers.Length];
            row[0] = x.Index.ToString("N0", invariantCulture);

            for (int i = 1; i < headers.Length; i++)
            {
                object? value = properties[i - 1].GetValue(x.Item);
                // Use centralized formatting helper (supports "unset" token)
                row[i] = FormatValue(value, formats[i], invariantCulture);
            }

            return row;
        }).ToArray();

        // Calculate column widths
        for (int i = 0; i < headers.Length; i++)
        {
            columnWidth[i] = Math.Max(columnWidth[i], dataRows.Max(row => row[i].Length));
        }

        // Build output
        StringBuilder sb = new();
        sb.AppendJoin("  ", headers.Select((h, i) => alignLeft[i] ? h.PadRight(columnWidth[i]) : h.PadLeft(columnWidth[i]))).AppendLine()
          .AppendJoin("  ", columnWidth.Select(w => new string('-', w))).AppendLine();
        foreach (string[] row in dataRows)
        {
            sb.AppendJoin("  ", row.Select((v, i) => alignLeft[i] ? v.PadRight(columnWidth[i]) : v.PadLeft(columnWidth[i]))).AppendLine();
        }

        return sb.ToString();
    }
}
