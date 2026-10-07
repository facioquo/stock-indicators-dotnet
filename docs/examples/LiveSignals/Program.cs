using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using FacioQuo.Stock.Indicators;

// Live signal desk: Kraken public OHLC feed (no key) -> BarHub -> chained hubs -> SSE -> browser chart.
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
string symbol = builder.Configuration["symbol"] ?? "BTC/USD"; // dotnet run -- --symbol ETH/USD
builder.Services.AddSingleton(new Desk(symbol));
builder.Services.AddHostedService<KrakenFeed>();
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

// Owns the hubs. Only the feed thread writes; readers get JSON snapshots.
sealed class Desk
{
    private readonly Lock gate = new();
    private readonly List<Channel<string>> clients = [];
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly List<Signal> signals = [];

    public Desk(string symbol)
    {
        Symbol = symbol;
        Fast = Bars.ToEmaHub(21);
        Slow = Bars.ToEmaHub(55);
        Adx = Bars.ToAdxHub(14);
        Atr = Bars.ToAtrHub(14);
    }

    public string Symbol { get; }
    public BarHub Bars { get; } = new();
    public EmaHub Fast { get; }
    public EmaHub Slow { get; }
    public AdxHub Adx { get; }
    public AtrHub Atr { get; }

    // Called for every feed message; a forming candle arrives repeatedly with the same timestamp.
    public void OnBar(Bar bar)
    {
        lock (gate)
        {
            bool newCandle = Bars.Results.Count > 0 && bar.Timestamp > Bars.Results[^1].Timestamp;
            Bars.Add(bar);

            // evaluate signals only when a candle closes, so they never repaint
            if (newCandle) { Evaluate(); }

            Broadcast(Payload(full: false));
        }
    }

    private void Evaluate()
    {
        int i = Bars.Results.Count - 2; // last closed candle
        if (i < 1) { return; }

        double? f0 = Fast.Results[i - 1].Ema, f1 = Fast.Results[i].Ema;
        double? s0 = Slow.Results[i - 1].Ema, s1 = Slow.Results[i].Ema;
        double? adx = Adx.Results[i].Adx, atr = Atr.Results[i].Atr;
        if (f0 is null || s0 is null || f1 is null || s1 is null || adx is null || atr is null) { return; }

        string? side = f0 <= s0 && f1 > s1 ? "BUY" : f0 >= s0 && f1 < s1 ? "SELL" : null;
        if (side is null) { return; }

        double close = (double)Bars.Results[i].Close;
        double stop = side == "BUY" ? close - (1.5 * atr.Value) : close + (1.5 * atr.Value);
        string reason = $"EMA21 crossed {(side == "BUY" ? "above" : "below")} EMA55 · ADX {adx:F0}"
            + (adx < 20 ? " (weak trend, low conviction)" : "")
            + $" · stop {stop:F2} (1.5×ATR)";

        Signal signal = new(Bars.Results[i].Timestamp, side, close, reason, adx >= 20);
        signals.Add(signal);
        Console.WriteLine($"{signal.Time:HH:mm} {Symbol} {side} @ {close:F2} — {reason}");
    }

    public ChannelReader<string> Subscribe(CancellationToken ct)
    {
        Channel<string> channel = Channel.CreateBounded<string>(
            new BoundedChannelOptions(64) { FullMode = BoundedChannelFullMode.DropOldest });
        lock (gate)
        {
            channel.Writer.TryWrite(Payload(full: true));
            clients.Add(channel);
        }
        ct.Register(() => { lock (gate) { clients.Remove(channel); } channel.Writer.TryComplete(); });
        return channel.Reader;
    }

    private void Broadcast(string json)
    {
        foreach (Channel<string> c in clients) { c.Writer.TryWrite(json); }
    }

    // full: every bar for the initial paint; otherwise just the last two (forming + just closed)
    private string Payload(bool full)
    {
        int from = full ? 0 : Math.Max(0, Bars.Results.Count - 2);
        var rows = Enumerable.Range(from, Bars.Results.Count - from).Select(i => new
        {
            t = new DateTimeOffset(Bars.Results[i].Timestamp).ToUnixTimeSeconds(),
            o = Bars.Results[i].Open,
            h = Bars.Results[i].High,
            l = Bars.Results[i].Low,
            c = Bars.Results[i].Close,
            fast = Fast.Results[i].Ema,
            slow = Slow.Results[i].Ema,
            adx = Adx.Results[i].Adx
        });
        return JsonSerializer.Serialize(new { symbol = Symbol, full, rows, signals }, Json);
    }
}

sealed record Signal(DateTime Time, string Side, double Price, string Reason, bool Strong);

// Kraken WebSocket v2, "ohlc" channel: a snapshot of recent candles, then updates to the forming candle.
sealed class KrakenFeed(Desk desk, ILogger<KrakenFeed> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try { await Run(ct); }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                log.LogWarning("Feed dropped ({Message}); reconnecting in 5s", ex.Message);
                await Task.Delay(5000, ct);
            }
        }
    }

    // The WebSocket snapshot holds only a few candles; seed warmup history (720 bars) from REST first.
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
            desk.OnBar(new Bar(
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

            foreach (Bar bar in bars) { desk.OnBar(bar); }
        }
    }
}
