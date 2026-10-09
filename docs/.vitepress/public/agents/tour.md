# Playbook: show me what it can do

Read [start.md](https://dotnet.stockindicators.dev/agents/start.md) first; its ground rules apply here.

## Finish line

A small console project whose data demos print a result from real market data (the trade-aggregation and catalog demos show API shape only), plus a short "what you could build" list tailored to what the developer told you about themselves. Run the demos yourself when you can execute code, and show their actual output; the provenance, speed, code, and next-prompt items of start's finish line apply. Pick three demos that fit their interest. These demos show mechanics, so they use the test datasets from start.md; the data demos were verified on `msft.csv` with the 3.x release current when this guide was written. When the developer asks about today's market, or anything they will act on, run the same demos on a live feed instead: replace `LoadCsv` with Kraken daily bars for `BTC/USD` (no key), using the `interval=1440` REST call in the [live market desk's Kraken notes](https://dotnet.stockindicators.dev/agents/live-signals.md) and the parsing in [`KrakenFeed.cs`](https://github.com/facioquo/stock-indicators-dotnet/blob/main/docs/examples/LiveSignals/KrakenFeed.cs). Its newest bar is still forming: drop it, or label results on it as provisional. The printed values then differ from the comments below.

Keep the narration to one line per demo: what it proves and why that matters. The output is the persuasion.

## Shared setup

```csharp
using System.Globalization;
using FacioQuo.Stock.Indicators;

const string Data = "https://raw.githubusercontent.com/facioquo/stock-indicators-dotnet/main/tests/Library/TestData/quotes/";
using HttpClient http = new();

async Task<List<Bar>> LoadCsv(string file)
{
    string csv = await http.GetStringAsync(Data + file);
    return csv.Split('\n', StringSplitOptions.RemoveEmptyEntries)
        .Skip(1) // header: date,open,high,low,close,volume
        .Select(line => line.Trim().Split(','))
        .Select(f => new Bar(
            DateTime.Parse(f[0], CultureInfo.InvariantCulture),
            decimal.Parse(f[1], CultureInfo.InvariantCulture),
            decimal.Parse(f[2], CultureInfo.InvariantCulture),
            decimal.Parse(f[3], CultureInfo.InvariantCulture),
            decimal.Parse(f[4], CultureInfo.InvariantCulture),
            decimal.Parse(f[5], CultureInfo.InvariantCulture)))
        .OrderBy(b => b.Timestamp)
        .ToList();
}

List<Bar> msft = await LoadCsv("msft.csv"); // 8,111 real daily bars, 1990–2022
```

## Demos

### Three styles, one answer

The same RSI, calculated as a batch, incrementally, and as a live stream, matches to the last digit. Prototype in batch, ship on a stream, and nothing changes.

```csharp
IReadOnlyList<RsiResult> series = msft.ToRsi(14);

RsiList buffer = new(14);
foreach (Bar b in msft) { buffer.Add(b); }

BarHub hub = new();
RsiHub stream = hub.ToRsiHub(14);
foreach (Bar b in msft) { hub.Add(b); }

Console.WriteLine($"Series {series[^1].Rsi:R} | Buffer {buffer[^1].Rsi:R} | Stream {stream.Results[^1].Rsi:R}");
// Series 43.771854913437274 | Buffer 43.771854913437274 | Stream 43.771854913437274
```

### A late bar corrects itself

Live feeds deliver bars out of order. A stream hub inserts the late bar and recalculates everything downstream, landing exactly on the batch answer.

```csharp
List<Bar> recent = msft.TakeLast(100).ToList();
Bar late = recent[90];

BarHub live = new();
EmaHub ema = live.ToEmaHub(20);
foreach (Bar b in recent.Where(b => b != late)) { live.Add(b); }
double? before = ema.Results[^1].Ema;

live.Add(late); // arrives out of order
Console.WriteLine($"{before:N4} -> {ema.Results[^1].Ema:N4}; batch {recent.ToEma(20)[^1].Ema:N4}");
// 292.0873 -> 292.0800; batch 292.0800
```

### Chain anything

Any indicator with a single value feeds another. RSI of on-balance volume is one line.

```csharp
IReadOnlyList<RsiResult> rsiOfObv = msft.ToObv().ToRsi(14);
```

### Raw trades to candles

Exchanges stream individual trades. An aggregator hub builds OHLCV bars from them in real time, and indicator hubs chain off its output. This demo is a sketch of the shape: skip it unless the developer has a trade feed, and never invent ticks to run it.

```csharp
TradeTickHub ticks = new();
TradeTickAggregatorHub oneMinute = ticks.ToTradeTickAggregatorHub(BarInterval.OneMinute);
EmaHub emaOfTicks = oneMinute.ToEmaHub(20);

// in the feed handler: ticks.Add(new TradeTick(timestamp, price, size, tradeId));
```

### Indicators as data

The catalog describes every indicator, its parameters, and its results, and executes one by ID. That turns the library into a UI indicator picker, a strategy saved as JSON, or a set of tools an AI model can call.

```csharp
int listings = Catalog.Get(Style.Series).Count; // 85 when this guide was written

IReadOnlyList<RsiResult> rsi = Catalog.Get("RSI", Style.Series)!
    .WithParamValue("lookbackPeriods", 14)
    .FromSource((IEnumerable<IBar>)msft)
    .Execute<RsiResult>();
```

### Live, right now

If they have two minutes, run the [live market desk](https://dotnet.stockindicators.dev/agents/live-signals.md): eight live crypto markets with daily regimes, rotation against Bitcoin, explained signals, and a narrated insight feed, no key required.

## What you could build

Close with three or four ideas matched to the developer, each one sentence and grounded in a demo they just saw. For example:

- a signal bot that posts explained alerts to Discord or Slack
- a Blazor dashboard watching a list of symbols through stream hubs
- a nightly job that screens hundreds of symbols in batch and emails the setups
- an MCP server that exposes the catalog to an AI assistant, so it can calculate any indicator on request
- a backtester for their own rules, using the same code that will run live

Offer to build the one they pick.
