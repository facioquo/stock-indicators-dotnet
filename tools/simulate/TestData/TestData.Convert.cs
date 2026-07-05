using System.Globalization;
using FacioQuo.Stock.Indicators;

namespace Test.Data;

// TEST DATA CONVERTER UTILITIES

internal static class Utilities
{
    private static readonly CultureInfo EnglishCulture = new("en-US", false);

    // importer / parser
    internal static Bar BarFromCsv(string csvLine)
    {
        if (string.IsNullOrEmpty(csvLine))
        {
            throw new InvalidDataException("CSV line was empty");
        }

        string[] csv = csvLine.Split(',');

        return new(
            Timestamp: DateTime.TryParse(csv[0], EnglishCulture, out DateTime d) ? d : default,
            Open: csv[1].ToDecimalDefault(),
            High: csv[2].ToDecimalDefault(),
            Low: csv[3].ToDecimalDefault(),
            Close: csv[4].ToDecimalDefault(),
            Volume: csv[5].ToDecimalDefault()
        );
    }

    internal static decimal ToDecimal(this string value)
        => decimal.TryParse(value, out decimal d) ? d
            : throw new NotFiniteNumberException(
                $"Cannot convert `{value}`,  it is not a number.");

    internal static decimal ToDecimalDefault(this string value)
        => decimal.TryParse(value, out decimal d) ? d : default;

    internal static decimal? ToDecimalNull(this string value)
        => decimal.TryParse(value, out decimal d) ? d : null;

    internal static double? ToDoubleNull(this string value)
        => double.TryParse(value, out double d) ? d : null;
}
