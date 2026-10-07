using System.Text.Json;
using System.Threading.Channels;
using FacioQuo.Stock.Indicators;

// Owns the hubs. Only the feed thread writes; browser clients get JSON snapshots.
sealed class Desk
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly Lock gate = new();
    private readonly List<Channel<string>> clients = [];
    private readonly List<Signal> signals = [];
    private string status = "Connecting…";

    public Desk(string symbol)
    {
        Symbol = symbol;
        Fast = Bars.ToEmaHub(21);
        Slow = Bars.ToEmaHub(55);
        Adx = Bars.ToAdxHub(14);
        Atr = Bars.ToAtrHub(14);
    }

    public string Symbol { get; }

    // IANA zone the page shows times in; null means the viewer's own
    public string? TimeZone { get; init; }

    public BarHub Bars { get; } = new();
    public EmaHub Fast { get; }
    public EmaHub Slow { get; }
    public AdxHub Adx { get; }
    public AtrHub Atr { get; }

    // A bar with an existing timestamp replaces the cached one, and every chained hub recalculates.
    public void Add(Bar bar)
    {
        lock (gate)
        {
            bool behind = IsBehindNewest(bar.Timestamp);
            Bars.Add(bar);
            Broadcast(Payload(full: behind));
        }
    }

    // Signals are evaluated only on closed candles, so they never repaint.
    public void Close(DateTime timestamp)
    {
        lock (gate)
        {
            Evaluate(timestamp);
            Broadcast(Payload(full: IsBehindNewest(timestamp)));
        }
    }

    public void SetStatus(string text)
    {
        lock (gate)
        {
            status = text;
            Broadcast(Payload(full: false));
        }
    }

    private void Evaluate(DateTime timestamp)
    {
        IReadOnlyList<IBar> bars = Bars.Results;
        int i = bars.Count - 1;
        while (i >= 0 && bars[i].Timestamp != timestamp) { i--; }
        if (i < 1) { return; }

        // a revised closed candle replaces its earlier verdict
        signals.RemoveAll(s => s.Time == timestamp);

        double? f0 = Fast.Results[i - 1].Ema, f1 = Fast.Results[i].Ema;
        double? s0 = Slow.Results[i - 1].Ema, s1 = Slow.Results[i].Ema;
        double? adx = Adx.Results[i].Adx, atr = Atr.Results[i].Atr;
        if (f0 is null || s0 is null || f1 is null || s1 is null || adx is null || atr is null) { return; }

        string? side = f0 <= s0 && f1 > s1 ? "BUY" : f0 >= s0 && f1 < s1 ? "SELL" : null;
        if (side is null) { return; }

        double close = (double)bars[i].Close;
        double stop = side == "BUY" ? close - (1.5 * atr.Value) : close + (1.5 * atr.Value);
        string reason = $"EMA21 crossed {(side == "BUY" ? "above" : "below")} EMA55 · ADX {adx:F0}"
            + (adx < 20 ? " (weak trend, low conviction)" : "")
            + $" · stop {stop:F2} (1.5×ATR)";

        Signal signal = new(timestamp, side, close, reason, adx >= 20);
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

    // The chart accepts updates only to its newest point or later, so a candle older than the newest needs the full series.
    private bool IsBehindNewest(DateTime timestamp) =>
        Bars.Results.Count > 0 && timestamp < Bars.Results[^1].Timestamp;

    // full: every bar for the initial paint or a revised older candle; otherwise the last few
    private string Payload(bool full)
    {
        int from = full ? 0 : Math.Max(0, Bars.Results.Count - 3);
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
        return JsonSerializer.Serialize(
            new { symbol = Symbol, tz = TimeZone, status, full, rows, signals = signals.OrderBy(s => s.Time) }, Json);
    }
}

sealed record Signal(DateTime Time, string Side, double Price, string Reason, bool Strong);
