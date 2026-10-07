using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using FacioQuo.Stock.Indicators;

// Kraken public API, no key. Crypto trades around the clock.
// The WebSocket "ohlc" snapshot holds only a few candles, so warmup history comes from REST first;
// updates then repeat the forming candle with the same timestamp until the next one begins.
sealed class KrakenFeed(Desk desk, ILogger<KrakenFeed> log) : BackgroundService
{
    private DateTime latest;

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

    // a newer candle means the previous one has closed
    private void Push(Bar bar)
    {
        if (latest != default && bar.Timestamp > latest) { desk.Close(latest); }
        if (bar.Timestamp >= latest) { latest = bar.Timestamp; }
        desk.Add(bar);
    }

    private async Task Seed(CancellationToken ct)
    {
        using HttpClient http = new();
        string url = $"https://api.kraken.com/0/public/OHLC?pair={Uri.EscapeDataString(desk.Symbol)}&interval=1";
        using JsonDocument doc = JsonDocument.Parse(await http.GetStringAsync(url, ct));
        JsonElement result = doc.RootElement.GetProperty("result");
        JsonElement rows = result.EnumerateObject().First(p => p.Name != "last").Value;

        // row: [time, open, high, low, close, vwap, volume, count], prices as strings
        foreach (JsonElement r in rows.EnumerateArray())
        {
            Push(new Bar(
                Timestamp: DateTimeOffset.FromUnixTimeSeconds(r[0].GetInt64()).UtcDateTime,
                Open: decimal.Parse(r[1].GetString()!, CultureInfo.InvariantCulture),
                High: decimal.Parse(r[2].GetString()!, CultureInfo.InvariantCulture),
                Low: decimal.Parse(r[3].GetString()!, CultureInfo.InvariantCulture),
                Close: decimal.Parse(r[4].GetString()!, CultureInfo.InvariantCulture),
                Volume: decimal.Parse(r[6].GetString()!, CultureInfo.InvariantCulture)));
        }
        Console.WriteLine($"Seeded {rows.GetArrayLength()} one-minute bars for {desk.Symbol}; streaming live.");
    }

    private async Task Run(CancellationToken ct)
    {
        if (desk.Bars.Results.Count == 0) { await Seed(ct); }

        using ClientWebSocket ws = new();
        await ws.ConnectAsync(new Uri("wss://ws.kraken.com/v2"), ct);
        string subscribe = JsonSerializer.Serialize(new
        {
            method = "subscribe",
            @params = new { channel = "ohlc", symbol = new[] { desk.Symbol }, interval = 1, snapshot = true }
        });
        await ws.SendAsync(Encoding.UTF8.GetBytes(subscribe), WebSocketMessageType.Text, true, ct);
        desk.SetStatus("Live · Kraken public feed");

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
            JsonElement root = doc.RootElement;
            if (root.TryGetProperty("success", out JsonElement ok) && !ok.GetBoolean())
            {
                throw new InvalidOperationException($"Kraken rejected the subscription: {root}");
            }
            if (!root.TryGetProperty("channel", out JsonElement ch) || ch.GetString() != "ohlc") { continue; }

            IEnumerable<Bar> bars = root.GetProperty("data").EnumerateArray()
                .Select(d => new Bar(
                    Timestamp: d.GetProperty("interval_begin").GetDateTime().ToUniversalTime(),
                    Open: d.GetProperty("open").GetDecimal(),
                    High: d.GetProperty("high").GetDecimal(),
                    Low: d.GetProperty("low").GetDecimal(),
                    Close: d.GetProperty("close").GetDecimal(),
                    Volume: d.GetProperty("volume").GetDecimal()))
                .OrderBy(b => b.Timestamp);

            foreach (Bar bar in bars) { Push(bar); }
        }
    }
}
