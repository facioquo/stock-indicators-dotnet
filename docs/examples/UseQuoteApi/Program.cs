using Alpaca.Markets;
using FacioQuo.Stock.Indicators;

string symbol = "AAPL";

/************************************************************

 We're using Alpaca SDK for .NET to access their public APIs.

 This approach will vary widely depending on where you are
 getting your aggregate price bars (quote history).

 See https://github.com/facioquo/stock-indicators-dotnet/discussions/579
 for free or inexpensive market data providers and examples.

 The return type of IEnumerable<Bar> can also be List<Bar>
 or ICollection<Bar> or other IEnumerable compatible types.

 ************************************************************/

// get and validate keys, see README.md
string ALPACA_KEY = Environment.GetEnvironmentVariable("ALPACA_KEY");
string ALPACA_SECRET = Environment.GetEnvironmentVariable("ALPACA_SECRET");

if (string.IsNullOrEmpty(ALPACA_KEY))
{
    throw new ArgumentNullException(
        ALPACA_KEY,
        $"API KEY missing, use `setx ALPACA_KEY \"MY-ALPACA-KEY\"` to set.");
}

if (string.IsNullOrEmpty(ALPACA_SECRET))
{
    throw new ArgumentNullException(
        ALPACA_SECRET,
        $"API SECRET missing, use `setx ALPACA_SECRET \"MY-ALPACA-SECRET\"` to set.");
}

// connect to Alpaca REST API
SecretKey secretKey = new(ALPACA_KEY, ALPACA_SECRET);

IAlpacaDataClient client = Environments.Paper.GetAlpacaDataClient(secretKey);

// compose request
// (excludes last 15 minutes for free delayed quotes)
DateTime into = DateTime.Now.Subtract(TimeSpan.FromMinutes(16));
DateTime from = into.Subtract(TimeSpan.FromDays(1000));

HistoricalBarsRequest request = new(symbol, from, into, BarTimeFrame.Minute);

// fetch minute-bar quotes in Alpaca's format
IPage<Alpaca.Markets.IBar> barSet = await client.ListHistoricalBarsAsync(request);

// convert library compatible quotes
List<Bar> bars = barSet
    .Items
    .Select(bar => new Bar(
        Timestamp: bar.TimeUtc,
        Open: bar.Open,
        High: bar.High,
        Low: bar.Low,
        Close: bar.Close,
        Volume: bar.Volume))
    .OrderBy(x => x.Timestamp)
    .ToList();

// calculate 10-period SMA
IEnumerable<SmaResult> results = bars.ToSma(10);

if (results == null || !results.Any())
{
    throw new InvalidOperationException("No indicator results were returned.");
}

// show results
Console.WriteLine($"{symbol} Results ------- (last 10 of {results.Count()}) --");

foreach (SmaResult r in results.TakeLast(10))
{
    // only showing last 10 records for brevity
    Console.WriteLine($"SMA on {r.Timestamp:u} was ${r.Sma:N3}");
}

// analyze results (compare to bar values)
Console.WriteLine();
Console.WriteLine($"{symbol} Analysis --------------------------");

/************************************************************
  Results are usually returned with the same number of
  elements as the provided bar; see individual indicator
  docs for more information.

  As such, converting to List means they can be indexed
  with the same ordinal position.
 ************************************************************/

List<SmaResult> resultsList = results
    .ToList();

for (int i = bars.Count - 25; i < bars.Count; i++)
{
    // only showing ~25 records for brevity

    Bar q = bars[i];
    SmaResult r = resultsList[i];

    bool isBullish = (double)q.Close > r.Sma;

    Console.WriteLine($"SMA on {r.Timestamp:u} was ${r.Sma:N3}"
                    + $" and Bullishness is {isBullish}");
}
