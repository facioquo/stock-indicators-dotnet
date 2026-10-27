using System.Text.Json;
using Custom.Stock.Indicators;
using FacioQuo.Stock.Indicators;

// USE CUSTOM INDICATORS exactly the same as
// other indicators in the library

// Fetch historical quotes from data provider.
// We're mocking with a simple JSON file import
string json = File.ReadAllText("quotes.data.json");

IReadOnlyList<Bar> bars = JsonSerializer
    .Deserialize<IReadOnlyList<Bar>>(json)
    ?? throw new InvalidDataException("The price-bar file does not contain a JSON collection.");

bars = bars
    .ToSortedList();

// Calculate custom indicators
IReadOnlyList<AtrWmaResult> atrWma = bars.ToAtrWma(10);
IReadOnlyList<ExtendedPivotPointsResult> pivots = bars.ToExtendedPivotPoints();

// Show results
Console.WriteLine("ATR WMA Results ---------------------------");

foreach (AtrWmaResult r in atrWma.Take(30))
{
    // only showing first 30 records for brevity
    Console.WriteLine($"ATR WMA on {r.Timestamp:u} was ${r.AtrWma:N3}");
}

ExtendedPivotPointsResult latest = pivots[pivots.Count - 1];
Console.WriteLine();
Console.WriteLine($"Extended monthly pivots on {latest.Timestamp:d}: S5 {latest.S5:N3}, PP {latest.PP:N3}, R5 {latest.R5:N3}");
