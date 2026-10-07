using System.Threading.Channels;

// Live signal desk: real market feed -> BarHub -> chained indicator hubs -> SSE -> browser chart.
//   dotnet run                      BTC/USD from Kraken's public feed, no key
//   dotnet run -- --symbol ETH/USD  any Kraken pair
//   dotnet run -- --symbol MSFT     US stock from Alpaca; needs ALPACA_API_KEY and ALPACA_SECRET_KEY
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
string symbol = builder.Configuration["symbol"] ?? "BTC/USD";

bool crypto = symbol.Contains('/');

builder.Services.AddSingleton(new Desk(symbol) { TimeZone = crypto ? null : "America/New_York" });
if (crypto)
{
    builder.Services.AddHostedService<KrakenFeed>();
}
else
{
    builder.Services.AddHostedService<AlpacaFeed>();
}

WebApplication app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/stream", async (HttpContext ctx, Desk desk, CancellationToken ct) =>
{
    ctx.Response.Headers.ContentType = "text/event-stream";
    ChannelReader<string> updates = desk.Subscribe(ct);
    await foreach (string json in updates.ReadAllAsync(ct))
    {
        await ctx.Response.WriteAsync($"data: {json}\n\n", ct);
        await ctx.Response.Body.FlushAsync(ct);
    }
});

app.Run();
