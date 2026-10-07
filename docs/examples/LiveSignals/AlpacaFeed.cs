using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using FacioQuo.Stock.Indicators;

// Alpaca market data, free plan (IEX feed). Needs ALPACA_KEY and ALPACA_SECRET.
// "bars" arrive just after each minute closes and "updatedBars" revise a minute a late trade changed;
// trades in between build the forming candle, which the closed bar then replaces by timestamp.
sealed class AlpacaFeed(Desk desk, IConfiguration config, ILogger<AlpacaFeed> log) : BackgroundService
{
    private readonly string key = config["ALPACA_KEY"]
        ?? throw new InvalidOperationException("Set ALPACA_KEY and ALPACA_SECRET to stream US stocks.");
    private readonly string secret = config["ALPACA_SECRET"]
        ?? throw new InvalidOperationException("Set ALPACA_SECRET to stream US stocks.");

    private DateTime lastClosed;
    private Bar? forming;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try { await Run(ct); }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                log.LogWarning("Feed dropped ({Message}); reconnecting in 5s", ex.Message);
                desk.SetStatus("Reconnecting…");
                await Task.Delay(5000, ct);
            }
        }
    }

    private HttpClient Http()
    {
        HttpClient http = new();
        http.DefaultRequestHeaders.Add("APCA-API-KEY-ID", key);
        http.DefaultRequestHeaders.Add("APCA-API-SECRET-KEY", secret);
        return http;
    }

    private static async Task<string> MarketStatus(HttpClient http, CancellationToken ct)
    {
        using JsonDocument clock = JsonDocument.Parse(
            await http.GetStringAsync("https://paper-api.alpaca.markets/v2/clock", ct));
        if (clock.RootElement.GetProperty("is_open").GetBoolean()) { return "Live · Alpaca IEX feed"; }

        // next_open carries the exchange's own offset, so it formats as New York time
        DateTimeOffset next = DateTimeOffset.Parse(
            clock.RootElement.GetProperty("next_open").GetString()!, CultureInfo.InvariantCulture);
        return $"Market closed · opens {next:ddd HH:mm} ET";
    }

    private async Task Seed(HttpClient http, CancellationToken ct)
    {
        string start = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        string url = "https://data.alpaca.markets/v2/stocks/bars"
            + $"?symbols={Uri.EscapeDataString(desk.Symbol)}&timeframe=1Min&feed=iex&start={start}&limit=1000&sort=desc";
        using JsonDocument doc = JsonDocument.Parse(await http.GetStringAsync(url, ct));

        if (!doc.RootElement.GetProperty("bars").TryGetProperty(desk.Symbol, out JsonElement rows))
        {
            throw new InvalidOperationException($"Alpaca returned no IEX bars for {desk.Symbol}.");
        }

        // newest first, so the 1,000 bars are the most recent; replay them oldest first
        foreach (JsonElement r in rows.EnumerateArray().Reverse()) { OnClosedBar(ToBar(r)); }
        Console.WriteLine($"Seeded {rows.GetArrayLength()} one-minute bars for {desk.Symbol}; streaming live.");
    }

    private async Task Run(CancellationToken ct)
    {
        using HttpClient http = Http();
        if (desk.Bars.Results.Count == 0) { await Seed(http, ct); }

        using ClientWebSocket ws = new();
        await ws.ConnectAsync(new Uri("wss://stream.data.alpaca.markets/v2/iex"), ct);
        await Send(ws, new { action = "auth", key, secret }, ct);
        await Send(ws, new
        {
            action = "subscribe",
            bars = new[] { desk.Symbol },
            updatedBars = new[] { desk.Symbol },
            trades = new[] { desk.Symbol }
        }, ct);
        desk.SetStatus(await MarketStatus(http, ct));

        byte[] buffer = new byte[1 << 16];
        using MemoryStream message = new();
        while (ws.State == WebSocketState.Open)
        {
            // single sequential receive loop: bars reach the hub in arrival order
            WebSocketReceiveResult r = await ws.ReceiveAsync(buffer, ct);
            message.Write(buffer, 0, r.Count);
            if (!r.EndOfMessage) { continue; }

            using JsonDocument doc = JsonDocument.Parse(message.ToArray());
            message.SetLength(0);

            foreach (JsonElement m in doc.RootElement.EnumerateArray())
            {
                switch (m.GetProperty("T").GetString())
                {
                    case "error":
                        throw new InvalidOperationException($"Alpaca stream error: {m}");
                    case "b" or "u":
                        OnClosedBar(ToBar(m));
                        break;
                    case "t":
                        OnTrade(m);
                        break;
                }
            }
        }
    }

    private void OnClosedBar(Bar bar)
    {
        desk.Add(bar);
        desk.Close(bar.Timestamp);
        if (bar.Timestamp > lastClosed) { lastClosed = bar.Timestamp; }
    }

    private void OnTrade(JsonElement trade)
    {
        // nanosecond timestamps; only the minute matters here
        DateTime minute = DateTime.ParseExact(
            trade.GetProperty("t").GetString()![..16], "yyyy-MM-ddTHH:mm",
            CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
        if (minute <= lastClosed) { return; } // that minute's closed bar is authoritative

        decimal price = trade.GetProperty("p").GetDecimal();
        decimal size = trade.GetProperty("s").GetDecimal();

        forming = forming is null || forming.Timestamp != minute
            ? new Bar(minute, price, price, price, price, size)
            : forming with
            {
                High = Math.Max(forming.High, price),
                Low = Math.Min(forming.Low, price),
                Close = price,
                Volume = forming.Volume + size
            };
        desk.Add(forming);
    }

    private static Bar ToBar(JsonElement b) => new(
        Timestamp: b.GetProperty("t").GetDateTime().ToUniversalTime(),
        Open: b.GetProperty("o").GetDecimal(),
        High: b.GetProperty("h").GetDecimal(),
        Low: b.GetProperty("l").GetDecimal(),
        Close: b.GetProperty("c").GetDecimal(),
        Volume: b.GetProperty("v").GetDecimal());

    private static Task Send(ClientWebSocket ws, object payload, CancellationToken ct)
        => ws.SendAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload)), WebSocketMessageType.Text, true, ct);
}
