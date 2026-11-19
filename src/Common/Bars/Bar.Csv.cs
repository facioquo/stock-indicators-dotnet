using System.Globalization;

namespace FacioQuo.Stock.Indicators;

// BAR UTILITIES: CSV IMPORT

public partial record Bar
{
    private static readonly string[] csvColumns
        = ["open", "high", "low", "close", "volume"];

    private static readonly Lazy<HttpClient> sharedHttpClient
        = new(static () => new HttpClient());

    /// <summary>
    /// Load bars from a CSV file with date, open, high, low, close, and volume columns.
    /// </summary>
    /// <remarks>
    /// Values are read with the invariant culture. Timestamps with an offset or Z are
    /// converted to UTC; timestamps without one are kept as written.
    /// A first row that does not start with a date is read as a header, and its
    /// columns must be in that order. Blank lines are skipped.
    /// </remarks>
    /// <param name="path">Path to the CSV file.</param>
    /// <returns>A list of bars, sorted by timestamp.</returns>
    /// <exception cref="ArgumentNullException">
    /// Path cannot be a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Path cannot be empty.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// The file does not exist.
    /// </exception>
    /// <exception cref="DirectoryNotFoundException">
    /// The directory in the path does not exist.
    /// </exception>
    /// <exception cref="FormatException">
    /// The header or a row is not in date, open, high, low, close, volume format.
    /// </exception>
    public static IReadOnlyList<Bar> FromCsv(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return ParseCsv(File.ReadLines(path));
    }

    /// <summary>
    /// Download bars from a CSV file with date, open, high, low, close, and volume columns.
    /// </summary>
    /// <remarks>
    /// Values are read with the invariant culture. Timestamps with an offset or Z are
    /// converted to UTC; timestamps without one are kept as written.
    /// A first row that does not start with a date is read as a header, and its
    /// columns must be in that order. Blank lines are skipped.
    /// </remarks>
    /// <param name="uri">Address of the CSV file.</param>
    /// <param name="httpClient">
    /// Client to download with. When omitted, a client shared across calls is used.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the download.</param>
    /// <returns>A list of bars, sorted by timestamp.</returns>
    /// <exception cref="ArgumentNullException">
    /// Address cannot be a null reference.
    /// </exception>
    /// <exception cref="HttpRequestException">
    /// The download failed or returned a status code that does not indicate success.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// The download was canceled or timed out.
    /// </exception>
    /// <exception cref="FormatException">
    /// The header or a row is not in date, open, high, low, close, volume format.
    /// </exception>
    public static async Task<IReadOnlyList<Bar>> FromCsvAsync(
        Uri uri,
        HttpClient? httpClient = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri);

        string csv = await (httpClient ?? sharedHttpClient.Value)
            .GetStringAsync(uri, cancellationToken)
            .ConfigureAwait(false);

        return ParseCsv(csv.Split('\n'));
    }

    internal static IReadOnlyList<Bar> ParseCsv(IEnumerable<string> lines)
    {
        List<Bar> bars = [];
        bool firstRow = true;
        int lineNumber = 0;

        foreach (string line in lines)
        {
            lineNumber++;

            // a UTF-8 byte order mark can survive decoding on the first line
            string row = line.TrimStart('\uFEFF').Trim();

            if (row.Length == 0)
            {
                continue;
            }

            string[] fields = row.Split(',');

            if (fields.Length != 6)
            {
                throw new FormatException(
                    $"CSV line {lineNumber} has {fields.Length} columns; expected 6 (date, open, high, low, close, volume).");
            }

            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = fields[i].Trim();
            }

            // offsets and Z go to UTC, so the result doesn't depend on the host time zone
            bool isDate = DateTime.TryParse(
                fields[0], CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime timestamp);

            if (firstRow && !isDate)
            {
                firstRow = false;
                ValidateCsvHeader(fields, lineNumber);
                continue;
            }

            firstRow = false;

            if (!isDate)
            {
                throw new FormatException(
                    $"CSV line {lineNumber} has an invalid date '{fields[0]}'.");
            }

            bars.Add(new Bar(
                Timestamp: timestamp,
                Open: ParseCsvValue(fields[1], csvColumns[0], lineNumber),
                High: ParseCsvValue(fields[2], csvColumns[1], lineNumber),
                Low: ParseCsvValue(fields[3], csvColumns[2], lineNumber),
                Close: ParseCsvValue(fields[4], csvColumns[3], lineNumber),
                Volume: ParseCsvValue(fields[5], csvColumns[4], lineNumber)));
        }

        return bars.ToSortedList();
    }

    private static void ValidateCsvHeader(string[] fields, int lineNumber)
    {
        bool isDateColumn
            = fields[0].Equals("date", StringComparison.OrdinalIgnoreCase)
           || fields[0].Equals("timestamp", StringComparison.OrdinalIgnoreCase);

        bool inOrder = isDateColumn
            && fields
                .Skip(1)
                .SequenceEqual(csvColumns, StringComparer.OrdinalIgnoreCase);

        if (!inOrder)
        {
            throw new FormatException(
                $"CSV line {lineNumber} was read as a header because '{fields[0]}' is not a date, "
              + $"but '{string.Join(',', fields)}' is not in date, open, high, low, close, volume order.");
        }
    }

    private static decimal ParseCsvValue(string value, string column, int lineNumber)
        => decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal d)
            ? d
            : throw new FormatException(
                $"CSV line {lineNumber} has an invalid {column} value '{value}'.");
}
