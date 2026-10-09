// Live market desk: real market feed -> BarHubs -> chained indicator hubs -> insights -> SSE and console.
//   dotnet run                                       8 crypto pairs vs BTC/USD from Kraken's public feed, no key
//   dotnet run -- --symbols ETH/USD,SOL/USD          any Kraken pairs (BTC/USD is added as the benchmark)
//   dotnet run -- --symbols XLK,XLE,XLF,XLV --benchmark SPY   US stocks and ETFs from Alpaca; needs ALPACA_KEY and ALPACA_SECRET
//   dotnet run -- --headless --minutes 3             no browser: narrate to the console, then write out/desk.html, .svg, and .json
bool headless = args.Contains("--headless");
string[] rest = [.. args.Where(a => a != "--headless")];

IConfiguration options = new ConfigurationBuilder().AddCommandLine(rest).Build();
List<string> symbols = (options["symbols"] ?? options["symbol"] ?? "BTC/USD,ETH/USD,SOL/USD,XRP/USD,ADA/USD,DOGE/USD,LINK/USD,AVAX/USD")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Select(s => s.ToUpperInvariant())
    .ToList();

// a slash means a crypto pair from Kraken; otherwise a US stock or ETF from Alpaca
bool crypto = symbols.TrueForAll(s => s.Contains('/'));
if (!crypto && symbols.Exists(s => s.Contains('/')))
{
    throw new ArgumentException("Use either crypto pairs (BTC/USD) or US stocks (MSFT) in one run, not both.");
}
string benchmark = (options["benchmark"] ?? (crypto ? "BTC/USD" : "SPY")).ToUpperInvariant();
symbols = [benchmark, .. symbols.Where(s => s != benchmark).Distinct()];

Desk desk = new(symbols, benchmark, crypto ? "Kraken public API" : "Alpaca IEX feed")
{
    // US sessions start a daily bar at New York midnight; crypto at UTC midnight
    TimeZone = crypto ? null : "America/New_York",
    // a short headless run narrates every minute; the page every five
    PulseEvery = TimeSpan.FromMinutes(headless ? 1 : 5)
};

void AddDesk(IHostApplicationBuilder builder)
{
    builder.Services.AddSingleton(desk);
    if (crypto) { builder.Services.AddHostedService<KrakenFeed>(); }
    else { builder.Services.AddHostedService<AlpacaFeed>(); }
}

if (headless)
{
    HostApplicationBuilder host = Host.CreateApplicationBuilder(rest);
    host.Logging.SetMinimumLevel(LogLevel.Warning);
    AddDesk(host);
    host.Services.AddHostedService<Headless>();
    await host.Build().RunAsync();
    return;
}

WebApplicationBuilder web = WebApplication.CreateBuilder(rest);
AddDesk(web);
WebApplication app = web.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/stream", (Desk d, CancellationToken ct) =>
    TypedResults.ServerSentEvents(d.Subscribe(ct).ReadAllAsync(ct)));
app.MapGet("/tape", (Desk d, string symbol) => TypedResults.Text(d.TapeJson(symbol), "application/json"));
app.MapGet("/snapshot", (Desk d) => TypedResults.Text(Report.Html(d.Snapshot()), "text/html"));

app.Run();
