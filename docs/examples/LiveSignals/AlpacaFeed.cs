using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using FacioQuo.Stock.Indicators;

// Alpaca market data, free plan (IEX feed). Needs ALPACA_KEY and ALPACA_SECRET.
// Daily history (split- and dividend-adjusted) gives market context; one-minute bars drive signals.
// "bars" arrive just after each minute closes and "updatedBars" revise a minute a late trade changed;
// trades in between build the forming candle, which the closed bar then replaces by timestamp.
sealed class AlpacaFeed(Desk desk, IConfiguration config, ILogger<AlpacaFeed> log) : BackgroundService
{
    private readonly string key = Required(config, "ALPACA_KEY");
    private readonly string secret = Required(config, "ALPACA_SECRET");

    private static string Required(IConfiguration config, string name) =>
        config[name] is { } value && !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException($"Set ALPACA_KEY and ALPACA_SECRET to stream US stocks ({name} is empty).");

    private readonly Dictionary<string, DateTime> lastClosed = [];
    private readonly Dictionary<string, Bar> forming = [];

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

    // The clock endpoint is paper-only: a live-account key gets a 401 there but still streams data.
    private async Task<string> MarketStatusOrDefault(HttpClient http, CancellationToken ct)
    {
        try { return await MarketStatus(http, ct); }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            // a cosmetic call must never drop a healthy stream
            log.LogInformation("Market clock unavailable ({Message}); streaming without it", ex.Message);
            return "Live · Alpaca IEX feed";
        }
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

    private static async Task<List<Bar>> History(HttpClient http, string symbol, string timeframe, DateTime start, string sort, CancellationToken ct)
    {
        string url = "https://data.alpaca.markets/v2/stocks/bars"
            + $"?symbols={Uri.EscapeDataString(symbol)}&timeframe={timeframe}&feed=iex&adjustment=all"
            + $"&start={start.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)}&limit=1000&sort={sort}";
        using JsonDocument doc = JsonDocument.Parse(await http.GetStringAsync(url, ct));

        if (!doc.RootElement.GetProperty("bars").TryGetProperty(symbol, out JsonElement rows))
        {
            throw new InvalidOperationException($"Alpaca returned no IEX bars for {symbol}.");
        }
        return [.. rows.EnumerateArray().Select(ToBar).OrderBy(b => b.Timestamp)];
    }

    private async Task Seed(HttpClient http, CancellationToken ct)
    {
        Dictionary<string, (List<Bar> Daily, List<Bar> Minutes)> history = [];
        foreach (Tape tape in desk.Tapes)
        {
            // three years of sessions fit one page; minutes are fetched newest first, so the 1,000 are the most recent
            history[tape.Symbol] = (
                await History(http, tape.Symbol, "1Day", DateTime.UtcNow.AddYears(-3), "asc", ct),
                await History(http, tape.Symbol, "1Min", DateTime.UtcNow.AddDays(-7), "desc", ct));
        }

        // the newest daily bar is the current session's while the market is open
        desk.Replay(() =>
        {
            foreach ((string symbol, (List<Bar> daily, List<Bar> minutes)) in history)
            {
                foreach (Bar bar in daily) { desk.AddDaily(symbol, bar); }
                foreach (Bar bar in minutes) { OnClosedBar(symbol, bar); }
            }
        });
        Console.WriteLine($"Seeded daily and one-minute history for {history.Count} symbols from Alpaca; streaming live.");
    }

    private async Task Run(CancellationToken ct)
    {
        using HttpClient http = Http();
        // every connect re-seeds, so a reconnect backfills the gap; the hub replaces bars by timestamp
        await Seed(http, ct);

        using ClientWebSocket ws = new();
        await ws.ConnectAsync(new Uri("wss://stream.data.alpaca.markets/v2/iex"), ct);
        await Send(ws, new { action = "auth", key, secret }, ct);
        string[] symbols = [.. desk.Tapes.Select(t => t.Symbol)];
        await Send(ws, new { action = "subscribe", bars = symbols, updatedBars = symbols, trades = symbols }, ct);
        desk.SetStatus(await MarketStatusOrDefault(http, ct));
        desk.Seeded();

        // the banner follows the open and the close even when the feed is quiet
        using CancellationTokenSource stop = CancellationTokenSource.CreateLinkedTokenSource(ct);
        Task refresh = RefreshStatus(http, stop.Token);
        try { await Receive(ws, ct); }
        finally
        {
            await stop.CancelAsync();
            await refresh;
        }
    }

    private async Task RefreshStatus(HttpClient http, CancellationToken ct)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMinutes(1));
        try
        {
            while (await timer.WaitForNextTickAsync(ct)) { desk.SetStatus(await MarketStatusOrDefault(http, ct)); }
        }
        catch (OperationCanceledException) { }
    }

    private async Task Receive(ClientWebSocket ws, CancellationToken ct)
    {
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
                        OnClosedBar(m.GetProperty("S").GetString()!, ToBar(m));
                        break;
                    case "t":
                        OnTrade(m);
                        break;
                }
            }
        }
    }

    private void OnClosedBar(string symbol, Bar bar)
    {
        desk.AddMinute(symbol, bar);
        desk.Close(symbol, bar.Timestamp);
        if (bar.Timestamp > lastClosed.GetValueOrDefault(symbol)) { lastClosed[symbol] = bar.Timestamp; }
    }

    private void OnTrade(JsonElement trade)
    {
        string symbol = trade.GetProperty("S").GetString()!;

        // nanosecond timestamps; only the minute matters here
        DateTime minute = DateTime.ParseExact(
            trade.GetProperty("t").GetString()![..16], "yyyy-MM-ddTHH:mm",
            CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
        if (minute <= lastClosed.GetValueOrDefault(symbol)) { return; } // that minute's closed bar is authoritative

        decimal price = trade.GetProperty("p").GetDecimal();
        decimal size = trade.GetProperty("s").GetDecimal();

        Bar bar = !forming.TryGetValue(symbol, out Bar? f) || f.Timestamp != minute
            ? new Bar(minute, price, price, price, price, size)
            : f with
            {
                High = Math.Max(f.High, price),
                Low = Math.Min(f.Low, price),
                Close = price,
                Volume = f.Volume + size
            };
        forming[symbol] = bar;
        desk.AddMinute(symbol, bar);
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
