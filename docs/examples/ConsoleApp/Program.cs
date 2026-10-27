using System.Globalization;
using System.Text.Json;
using FacioQuo.Stock.Indicators;

/* BASIC CONSOLE APP | SIMPLE MOVING AVERAGE */

// Fetch historical quotes from data provider.
// We're mocking with a simple JSON file import
string json = File.ReadAllText("quotes.data.json");

IReadOnlyCollection<Bar> importedBars = JsonSerializer
    .Deserialize<IReadOnlyCollection<Bar>>(json)
    ?? throw new InvalidDataException("The price-bar file does not contain a JSON collection.");

IReadOnlyList<Bar> bars = importedBars
    .ToSortedList();

// Calculate 10-period SMA
IEnumerable<SmaResult> results = bars.ToSma(10);

// show results
Console.WriteLine("SMA Results ---------------------------");

foreach (SmaResult r in results.TakeLast(10))
{
    // only showing last 10 records for brevity
    Console.WriteLine($"SMA on {r.Timestamp:u} was ${r.Sma:N3}");
}

// optionally, you can lookup individual values by date
DateTime lookupDate = DateTime
    .Parse("2021-08-12T17:08:17.9746795+02:00", CultureInfo.InvariantCulture);

double? specificSma = results.First(x => x.Timestamp == lookupDate).Sma;

Console.WriteLine();
Console.WriteLine("SMA on Specific Date ------------------");
Console.WriteLine($"SMA on {lookupDate:u} was ${specificSma:N3}");

// analyze results (compare to quote values)
Console.WriteLine();
Console.WriteLine("SMA Analysis --------------------------");

/************************************************************
  Results are usually returned with the same number of
  elements as the provided price bars; see individual indicator
  docs for more information.

  As such, converting to List means they can be indexed
  with the same ordinal position.
 ************************************************************/

List<SmaResult> resultsList
    = results.ToList();

for (int i = bars.Count - 25; i < bars.Count; i++)
{
    // only showing ~25 records for brevity

    Bar b = bars[i];
    SmaResult r = resultsList[i];

    bool isBullish = (double)b.Close > r.Sma;

    Console.WriteLine($"SMA on {r.Timestamp:u} was ${r.Sma:N3}"
                    + $" and Bullishness is {isBullish}");
}
