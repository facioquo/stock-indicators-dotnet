using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using FacioQuo.Stock.Indicators;

// Kraken public API, no key. Crypto trades around the clock.
// The WebSocket "ohlc" snapshot holds only a few candles, so warmup history comes from REST first:
// about two years of daily bars for context and 12 hours of minutes for signals.
// Updates then repeat the forming candle with the same timestamp until the next one begins.
sealed class KrakenFeed(Desk desk, ILogger<KrakenFeed> log) : BackgroundService
{
    private static readonly TimeSpan Pace = TimeSpan.FromSeconds(1);
    private readonly Dictionary<string, DateTime> latest = [];

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
    private void Push(string symbol, Bar bar)
    {
        DateTime last = latest.GetValueOrDefault(symbol);
        if (last != default && bar.Timestamp > last) { desk.Close(symbol, last); }
        if (bar.Timestamp >= last) { latest[symbol] = bar.Timestamp; }
        desk.AddMinute(symbol, bar);
    }

    // row: [time, open, high, low, close, vwap, volume, count], prices as strings
    private static async Task<List<Bar>> History(HttpClient http, string symbol, int interval, CancellationToken ct)
    {
        string url = $"https://api.kraken.com/0/public/OHLC?pair={Uri.EscapeDataString(symbol)}&interval={interval}";
        using JsonDocument doc = JsonDocument.Parse(await http.GetStringAsync(url, ct));
        if (doc.RootElement.GetProperty("error").GetArrayLength() > 0)
        {
            throw new InvalidOperationException($"Kraken rejected {symbol}: {doc.RootElement.GetProperty("error")}");
        }
        JsonElement rows = doc.RootElement.GetProperty("result").EnumerateObject().First(p => p.Name != "last").Value;
        return rows.EnumerateArray().Select(r => new Bar(
            Timestamp: DateTimeOffset.FromUnixTimeSeconds(r[0].GetInt64()).UtcDateTime,
            Open: decimal.Parse(r[1].GetString()!, CultureInfo.InvariantCulture),
            High: decimal.Parse(r[2].GetString()!, CultureInfo.InvariantCulture),
            Low: decimal.Parse(r[3].GetString()!, CultureInfo.InvariantCulture),
            Close: decimal.Parse(r[4].GetString()!, CultureInfo.InvariantCulture),
            Volume: decimal.Parse(r[6].GetString()!, CultureInfo.InvariantCulture))).ToList();
    }

    private async Task Seed(CancellationToken ct)
    {
        using HttpClient http = new();
        Dictionary<string, (List<Bar> Daily, List<Bar> Minutes)> history = [];
        // Kraken asks public clients to stay near one call a second; a reconnect re-seeds, so it paces too
        foreach (Tape tape in desk.Tapes)
        {
            List<Bar> daily = await History(http, tape.Symbol, 1440, ct);
            await Task.Delay(Pace, ct);
            List<Bar> minutes = await History(http, tape.Symbol, 1, ct);
            await Task.Delay(Pace, ct);
            history[tape.Symbol] = (daily, minutes);
        }

        // the newest daily row is today's, still forming
        desk.Replay(() =>
        {
            foreach ((string symbol, (List<Bar> daily, List<Bar> minutes)) in history)
            {
                foreach (Bar bar in daily) { desk.AddDaily(symbol, bar); }
                foreach (Bar bar in minutes) { Push(symbol, bar); }
            }
        });
        Console.WriteLine($"Seeded daily and one-minute history for {history.Count} pairs from Kraken; streaming live.");
    }

    private async Task Run(CancellationToken ct)
    {
        // every connect re-seeds, so a reconnect backfills the gap; the hubs replace bars by timestamp
        await Seed(ct);

        using ClientWebSocket ws = new();
        await ws.ConnectAsync(new Uri("wss://ws.kraken.com/v2"), ct);
        string subscribe = JsonSerializer.Serialize(new
        {
            method = "subscribe",
            @params = new { channel = "ohlc", symbol = desk.Tapes.Select(t => t.Symbol).ToArray(), interval = 1, snapshot = true }
        });
        await ws.SendAsync(Encoding.UTF8.GetBytes(subscribe), WebSocketMessageType.Text, true, ct);
        desk.SetStatus("Live · Kraken public feed");
        desk.Seeded();

        byte[] buffer = new byte[1 << 16];
        using MemoryStream message = new();
        while (ws.State == WebSocketState.Open)
        {
            // single sequential receive loop: bars reach the hubs in arrival order
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

            List<(string Symbol, Bar Bar)> bars = root.GetProperty("data").EnumerateArray()
                .Select(d => (d.GetProperty("symbol").GetString()!, new Bar(
                    Timestamp: d.GetProperty("interval_begin").GetDateTime().ToUniversalTime(),
                    Open: d.GetProperty("open").GetDecimal(),
                    High: d.GetProperty("high").GetDecimal(),
                    Low: d.GetProperty("low").GetDecimal(),
                    Close: d.GetProperty("close").GetDecimal(),
                    Volume: d.GetProperty("volume").GetDecimal())))
                .OrderBy(x => x.Item2.Timestamp)
                .ToList();

            // a snapshot replays history, so it is sent as one payload like the seed
            if (root.TryGetProperty("type", out JsonElement type) && type.GetString() == "snapshot")
            {
                desk.Replay(() => { foreach ((string s, Bar b) in bars) { Push(s, b); } });
            }
            else
            {
                foreach ((string s, Bar b) in bars) { Push(s, b); }
            }
        }
    }
}
