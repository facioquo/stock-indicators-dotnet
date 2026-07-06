using FacioQuo.Stock.Indicators;

namespace Test.Data;

// TEST BAR GETTERs

internal static class Data
{
    // sorted by filename

    // DEFAULT: SnP 500 ~2 years of daily data
    internal static IReadOnlyList<Bar> GetDefault(int days = 502)
        => File.ReadAllLines("TestData/quotes/default.csv")
            .Skip(1)
            .Select(Utilities.BarFromCsv)
            .OrderBy(static x => x.Timestamp)
            .Take(days)
            .ToList();

    // COMPARE DATA ~2 years of TSLA data (matches default time)
    internal static IReadOnlyList<Bar> GetCompare(int days = 502)
        => File.ReadAllLines("TestData/quotes/compare.csv")
            .Skip(1)
            .Select(Utilities.BarFromCsv)
            .OrderBy(static x => x.Timestamp)
            .Take(days)
            .ToList();

    // INTRADAY DATA
    internal static IReadOnlyList<Bar> GetIntraday(int days = 1564)
        => File.ReadAllLines("TestData/quotes/intraday.csv")
            .Skip(1)
            .Select(Utilities.BarFromCsv)
            .OrderBy(static x => x.Timestamp)
            .Take(days)
            .ToList();

    // LONGEST DATA ~62 years of SnP 500 daily data
    internal static IReadOnlyList<Bar> GetLongest()
        => File.ReadAllLines("TestData/quotes/longest.csv")
            .Skip(1)
            .Select(Utilities.BarFromCsv)
            .OrderBy(static x => x.Timestamp)
            .ToList();

    // LONGISH DATA ~20 years of SnP 500 daily data
    internal static IReadOnlyList<Bar> GetLongish(int days = 5285)
        => File.ReadAllLines("TestData/quotes/longish.csv")
            .Skip(1)
            .Select(Utilities.BarFromCsv)
            .OrderBy(static x => x.Timestamp)
            .Take(days)
            .ToList();
}
