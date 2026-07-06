using System.Text.Json;

namespace Test.Data;

// TEST DATA FILE LOADERS

internal static class Data
{
    private static readonly JsonSerializerOptions JsonOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    internal static IReadOnlyList<TResult> ResultsFromJson<TResult>(string filename)
        where TResult : ISeries
    {
        // Check if baseline file exists
        string filepath = Path.Combine("TestData", "results", filename);

        if (!File.Exists(filepath))
        {
            throw new FileNotFoundException("Test data file not found.", filepath);
        }

        // Load and deserialize file
        string json = File.ReadAllText(filepath);
        List<TResult> results = JsonSerializer.Deserialize<List<TResult>>(json, JsonOptions);

        Assert.IsNotNull(results, $"Failed to deserialize baseline file: {filepath}");

        return results;
    }

    internal static IReadOnlyList<Bar> BarsFromJson(string filename)
    {
        // Check if baseline file exists
        string filepath = Path.Combine("TestData", "quotes", filename);

        if (!File.Exists(filepath))
        {
            throw new FileNotFoundException("Test data file not found.", filepath);
        }

        // Load and deserialize file
        string json = File.ReadAllText(filepath);
        List<Bar> bars = JsonSerializer.Deserialize<List<Bar>>(json, JsonOptions);

        Assert.IsNotNull(bars, $"Failed to deserialize baseline file: {filepath}");

        return bars;
    }

    internal static IReadOnlyList<Bar> BarsFromCsv(string filename, int days = int.MaxValue)
        => File.ReadAllLines(Path.Combine("TestData", "quotes", filename))
            .Skip(1)
            .Select(Utilities.BarFromCsv)
            .OrderByDescending(static x => x.Timestamp)
            .Take(days)
            .ToSortedList();

    // DEFAULT: SnP 500 ~2 years of daily data
    internal static IReadOnlyList<Bar> GetDefault(int days = 502)
        => BarsFromCsv("default.csv", days);

    // RANDOM: gaussian brownaian motion
    internal static IReadOnlyList<Bar> GetRandom(
        int bars = 502,
        BarInterval barInterval = BarInterval.OneMinute,
        bool includeWeekends = true)
        => new RandomGbm(
            bars: bars,
            barInterval: barInterval,
            includeWeekends: includeWeekends);

    // sorted by filename

    // BAD DATA
    internal static IReadOnlyList<Bar> GetBad(int days = 502)
        => BarsFromCsv("bad.csv", days);

    // BITCOIN DATA
    internal static IReadOnlyList<Bar> GetBitcoin(int days = 1246)
        => BarsFromCsv("bitcoin.csv", days);

    // BTCUSD, 69288 records, 15-minute bars
    internal static IReadOnlyList<Bar> GetBtcUsdNan(int bars = 69288)
        => BarsFromCsv("btcusd15x69k.csv", bars);

    // COMPARE DATA ~2 years of TSLA data (matches default time)
    internal static IReadOnlyList<Bar> GetCompare(int days = 502)
        => BarsFromCsv("compare.csv", days);

    // INTRADAY DATA
    internal static IReadOnlyList<Bar> GetIntraday(int days = 1564)
        => BarsFromCsv("intraday.csv", days);

    // LONGEST DATA ~62 years of SnP 500 daily data (15,821)
    internal static IReadOnlyList<Bar> GetLongest()
        => BarsFromCsv("longest.csv");

    // LONGISH DATA ~20 years of SnP 500 daily data
    internal static IReadOnlyList<Bar> GetLongish(int days = 5285)
        => BarsFromCsv("longish.csv", days);

    // MISMATCH DATA is in incorrect sequence
    internal static IReadOnlyList<Bar> GetMismatch()
        => File.ReadAllLines(Path.Combine("TestData", "quotes", "mismatch.csv"))
            .Skip(1)
            .Select(Utilities.BarFromCsv)
            .ToList();  // not sorted

    // MSFT, 30 years, daily
    internal static IReadOnlyList<Bar> GetMsft(int days = 8111)
        => BarsFromCsv("msft.csv", days);

    // PENNY DATA
    internal static IReadOnlyList<Bar> GetPenny()
        => BarsFromCsv("penny.csv");

    // SPX, 30 years, daily
    internal static IReadOnlyList<Bar> GetSpx(int days = 8111)
        => BarsFromCsv("spx.csv", days);

    // TOO BIG DATA
    internal static IReadOnlyList<Bar> GetTooBig(int days = 1246)
        => BarsFromCsv("toobig.csv", days);

    // MAX SIZE DATA
    internal static IReadOnlyList<Bar> GetMax(int days = 502)
        => BarsFromCsv("toobig.csv", days);

    // ZEROS (200)
    internal static IReadOnlyList<Bar> GetZeros(int days = 200)
        => BarsFromCsv("zeros.csv", days);
}
