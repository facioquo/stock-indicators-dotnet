using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Threading.Channels;
using FacioQuo.Stock.Indicators;

// Owns every tape. Only the feed thread writes; browser clients and the console get insights and JSON snapshots.
sealed class Desk : IDisposable
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly TimeSpan Quiet = TimeSpan.FromMinutes(15);

    private readonly Lock gate = new();
    private readonly List<Channel<string>> clients = [];
    private readonly Dictionary<string, Tape> tapes;
    private readonly List<Insight> insights = [];
    private readonly Dictionary<string, Regime> regimes = [];
    private readonly Dictionary<string, string> states = [];
    private readonly Dictionary<string, DateTime> spoken = [];
    private readonly TaskCompletionSource ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Stopwatch compute = new();
    private readonly Timer flush;
    private TimeZoneInfo? zone;
    private string status = "Connecting…";
    private string? leader;
    private DateTime lastPulse;
    private long barsIn;
    private bool replaying;
    private bool dirty;

    public Desk(IReadOnlyList<string> symbols, string benchmark, string provider)
    {
        Benchmark = benchmark;
        Provider = provider;
        tapes = symbols.ToDictionary(s => s, s => new Tape(s, DayOf));
        // the basket view refreshes at most once a second, however fast trades print
        flush = new Timer(_ => Flush(), null, 1000, 1000);
    }

    public string Benchmark { get; }

    // how often to narrate a market pulse across the basket
    public TimeSpan PulseEvery { get; init; } = TimeSpan.FromMinutes(5);
    public string Provider { get; }
    public IEnumerable<Tape> Tapes => tapes.Values;
    public Task Ready => ready.Task;
    // the NuGet version, as restored; the assembly's own version attributes don't carry it
    public static string PackageVersion { get; } = RestoredVersion();

    private static string RestoredVersion()
    {
        string deps = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetEntryAssembly()?.GetName().Name}.deps.json");
        if (!File.Exists(deps)) { return "unknown"; }
        using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(deps));
        return doc.RootElement.GetProperty("libraries").EnumerateObject()
            .Select(p => p.Name.Split('/'))
            .FirstOrDefault(n => n[0] == typeof(BarHub).Assembly.GetName().Name)?[1] ?? "unknown";
    }

    // IANA zone the page shows times in, and whose midnight starts a daily bar; null means UTC and the viewer's own clock
    public string? TimeZone
    {
        get => zone?.Id;
        init => zone = value is null ? null : TimeZoneInfo.FindSystemTimeZoneById(value);
    }

    // daily bars a week: crypto trades every day, US markets on weekdays
    public int BarsPerWeek => zone is null ? 7 : 5;

    public DateTime DayOf(DateTime utc) => zone is null
        ? utc.Date
        : TimeZoneInfo.ConvertTimeToUtc(TimeZoneInfo.ConvertTimeFromUtc(utc, zone).Date, zone);

    // Feeds replay history on every connect: broadcast one full payload when it ends, not one per replayed bar.
    public void Replay(Action feed)
    {
        lock (gate)
        {
            replaying = true;
            compute.Start();
            try { feed(); }
            finally
            {
                compute.Stop();
                replaying = false;
            }
            Refresh();
            Broadcast(DeskPayload());
            foreach (Tape t in tapes.Values) { Broadcast(TapePayload(t, full: true)); }
        }
    }

    public void AddDaily(string symbol, Bar bar)
    {
        lock (gate)
        {
            if (!tapes.TryGetValue(symbol, out Tape? tape)) { return; }
            tape.Daily.Add(bar);
            barsIn++;
        }
    }

    // A bar with an existing timestamp replaces the cached one, and every chained hub recalculates.
    public void AddMinute(string symbol, Bar bar)
    {
        lock (gate)
        {
            if (!tapes.TryGetValue(symbol, out Tape? tape)) { return; }
            bool behind = !replaying && tape.Minutes.Results.Count > 0 && bar.Timestamp < tape.Minutes.Results[^1].Timestamp;
            tape.AddMinute(bar, live: !replaying);
            barsIn++;
            if (!replaying)
            {
                Broadcast(TapePayload(tape, full: behind));
                dirty = true;
            }
        }
    }

    // A closed minute: evaluate the signal recipe, then say what changed in the daily context.
    public void Close(string symbol, DateTime timestamp)
    {
        lock (gate)
        {
            if (!tapes.TryGetValue(symbol, out Tape? tape)) { return; }
            Signal? signal = tape.Evaluate(timestamp);
            if (replaying || !ready.Task.IsCompleted) { return; }

            Refresh();
            if (!regimes.TryGetValue(symbol, out Regime? r)) { return; }
            if (signal is not null)
            {
                bool with = (signal.Side == "BUY" && r.Trend == "Uptrend") || (signal.Side == "SELL" && r.Trend == "Downtrend");
                bool against = (signal.Side == "BUY" && r.Trend == "Downtrend") || (signal.Side == "SELL" && r.Trend == "Uptrend");
                string context = with ? $"with the daily {r.Trend.ToLowerInvariant()}"
                    : against ? $"against the daily {r.Trend.ToLowerInvariant()} ({Fmt.Pct(r.VsSma200 ?? 0)} vs SMA200): counter-trend, lower conviction"
                    : "while the daily trend is unresolved";
                Say(symbol, "signal", $"1-minute {signal.Side} at {Fmt.Price(signal.Price)}: {signal.Reason}; {context}.", force: true);
            }
            Burst(tape, timestamp);
            Watch();
            Broadcast(TapePayload(tape, full: tape.Minutes.Results.Count > 0 && timestamp < tape.Minutes.Results[^1].Timestamp));
            dirty = true;
        }
    }

    // History is loaded for every symbol: brief once, then watch for changes.
    public void Seeded()
    {
        lock (gate)
        {
            if (ready.Task.IsCompleted) { return; }
            Refresh();

            Tape bench = tapes[Benchmark];
            List<double?> batch = [.. bench.Daily.Results.ToSma(200).Select(r => r.Sma)];
            List<double?> stream = [.. bench.Sma200.Results.Select(r => r.Sma)];
            int compared = batch.Count(v => v is not null);
            string parity = compared == 0
                ? $"{Benchmark} has too little daily history for SMA(200), so stream and batch results were not compared."
                : batch.SequenceEqual(stream)
                    ? $"The stream hub's SMA(200) on {Benchmark} matches the batch calculation exactly, across {compared:N0} values."
                    : "Stream and batch SMA(200) differ; please report this.";
            Say(null, "proof",
                $"Streamed {barsIn:N0} real bars from {Provider} through {tapes.Values.Sum(t => t.HubCount)} chained indicator hubs "
                + $"({tapes.Count} symbols) in {compute.Elapsed.TotalMilliseconds:N0} ms. {parity}");

            foreach ((string? symbol, string kind, string text) in Analysis.Brief([.. regimes.Values], Benchmark))
            {
                Say(symbol, kind, text);
            }
            Watch(quietly: true);
            lastPulse = DateTime.UtcNow;
            ready.SetResult();
            Broadcast(DeskPayload());
        }
    }

    public void SetStatus(string text)
    {
        lock (gate)
        {
            status = text;
            dirty = true;
        }
    }

    private void Refresh()
    {
        Tape bench = tapes[Benchmark];
        foreach (Tape t in tapes.Values.Where(t => t.Daily.Results.Count > 0))
        {
            regimes[t.Symbol] = Analysis.Evaluate(t, bench.Daily.Results.Count > 0 ? bench : null, BarsPerWeek);
        }
    }

    // State changes on today's forming daily bar are provisional until it closes.
    private void Watch(bool quietly = false)
    {
        foreach (Regime r in regimes.Values)
        {
            Check(r.Symbol, "trend", r.Trend, quietly,
                $"now in a daily {r.Trend.ToLowerInvariant()}: {Fmt.Pct(r.VsSma200 ?? 0)} vs its 200-day SMA (provisional until today's daily close).");
            if (r.Quadrant is not null)
            {
                RotationPoint p = r.Tail[^1];
                Check(r.Symbol, "rotation", r.Quadrant, quietly,
                    $"moved into the {r.Quadrant.ToLowerInvariant()} quadrant against {Benchmark} (RS-ratio {p.Ratio:F1}, RS-momentum {p.Momentum:F1}; provisional).");
            }
            Check(r.Symbol, "volatility", r.Volatility, quietly,
                $"volatility is now {r.Volatility}: ATR {r.Atrp:F1}% of price, {Analysis.Ordinal(r.VolPercentile ?? 0)} percentile of its past year.");
        }

        Regime? top = regimes.Values.Where(r => r.DayChange is not null).MaxBy(r => r.DayChange);
        if (top is not null && top.Symbol != leader)
        {
            string? was = leader;
            leader = top.Symbol;
            if (!quietly && was is not null && regimes.TryGetValue(was, out Regime? prior))
            {
                Say(top.Symbol, "leader",
                    $"takes today's lead at {Fmt.Pct(top.DayChange!.Value)}, passing {was} ({Fmt.Pct(prior.DayChange ?? 0)}).");
            }
        }
    }

    // a closed minute far wider than its recent average range
    private void Burst(Tape tape, DateTime timestamp)
    {
        int i = tape.Minutes.Results.Count - 1;
        while (i >= 0 && tape.Minutes.Results[i].Timestamp != timestamp) { i--; }
        if (i < 1 || tape.Atr.Results[i - 1].Atr is not double atr || atr == 0) { return; }

        IBar bar = tape.Minutes.Results[i];
        double range = (double)(bar.High - bar.Low);
        if (range < 3 * atr) { return; }
        Say(tape.Symbol, "burst",
            $"printed a {range / (double)bar.Close * 100:F2}% one-minute range, {range / atr:F1}× its 14-minute ATR, closing "
            + $"{(bar.Close >= bar.Open ? "up" : "down")} at {Fmt.Price(bar.Close)}: a volatility burst.");
    }

    // Every few minutes: who moved most over the window, and how the benchmark did.
    private void Pulse()
    {
        DateTime now = DateTime.UtcNow;
        if (!ready.Task.IsCompleted || now - lastPulse < PulseEvery) { return; }
        DateTime since = lastPulse;
        lastPulse = now;

        List<(string Symbol, double Change)> moves = [];
        foreach (Tape t in tapes.Values)
        {
            IReadOnlyList<IBar> bars = t.Minutes.Results;
            IBar? start = bars.LastOrDefault(b => b.Timestamp <= since.AddMinutes(-1));
            if (start is null || start.Close == 0) { continue; }
            moves.Add((t.Symbol, ((double)(bars[^1].Close / start.Close) - 1) * 100));
        }
        if (moves.Count < 2) { return; }

        (string top, double up) = moves.MaxBy(m => m.Change);
        (string bottom, double down) = moves.MinBy(m => m.Change);
        // a flat market is not news
        if (up - down < 0.2) { return; }

        double bench = moves.FirstOrDefault(m => m.Symbol == Benchmark).Change;
        string window = PulseEvery.TotalMinutes < 2 ? "Past minute" : $"Past {PulseEvery.TotalMinutes:0} minutes";
        string benchNote = top == Benchmark || bottom == Benchmark ? "" : $"; {Benchmark} {Fmt.Pct(bench, 2)}";
        Say(null, "pulse",
            $"{window}: {top} {Fmt.Pct(up, 2)} leads, {bottom} {Fmt.Pct(down, 2)} lags{benchNote}. "
            + $"{moves.Count(m => m.Change > 0)} of {moves.Count} up.");
    }

    private void Check(string symbol, string kind, string state, bool quietly, string text)
    {
        string key = $"{symbol}|{kind}";
        if (states.TryGetValue(key, out string? was) && was == state) { return; }
        // record the state only once it has been said, so a change held back by the quiet period is retried
        if (quietly || was is null || Say(symbol, kind, text)) { states[key] = state; }
    }

    // One voice: every insight goes to the console, the page, and the snapshot. A flickering state is said at most once per quiet period;
    // the briefing doesn't start that period. Returns whether it was said.
    private bool Say(string? symbol, string kind, string text, bool force = false)
    {
        DateTime now = DateTime.UtcNow;
        string key = $"{symbol}|{kind}";
        bool live = ready.Task.IsCompleted;
        if (!force && symbol is not null && live && spoken.TryGetValue(key, out DateTime last) && now - last < Quiet) { return false; }
        if (live) { spoken[key] = now; }

        string line = symbol is null || text.StartsWith(symbol, StringComparison.Ordinal) ? text : $"{symbol} {text}";
        insights.Add(new Insight(now, symbol, kind, line, Live: live));
        Console.WriteLine($"{now:HH:mm:ss}Z  {line}");
        dirty = true;
        return true;
    }

    private void Flush()
    {
        lock (gate)
        {
            if (replaying) { return; }
            Pulse();
            if (!dirty) { return; }
            dirty = false;
            Refresh();
            Broadcast(DeskPayload());
        }
    }

    public DeskSnapshot Snapshot()
    {
        lock (gate)
        {
            Refresh();
            return new DeskSnapshot(
                DateTime.UtcNow, Provider, Benchmark, PackageVersion, status,
                [.. tapes.Keys.Where(regimes.ContainsKey).Select(s => regimes[s])],
                [.. insights],
                tapes.Values.Sum(t => t.Daily.Results.Count),
                tapes.Values.Sum(t => t.Minutes.Results.Count));
        }
    }

    public ChannelReader<string> Subscribe(CancellationToken ct)
    {
        Channel<string> channel = Channel.CreateBounded<string>(
            new BoundedChannelOptions(256) { FullMode = BoundedChannelFullMode.DropOldest });
        lock (gate)
        {
            channel.Writer.TryWrite(DeskPayload());
            foreach (Tape t in tapes.Values) { channel.Writer.TryWrite(TapePayload(t, full: true)); }
            clients.Add(channel);
        }
        ct.Register(() => { lock (gate) { clients.Remove(channel); } channel.Writer.TryComplete(); });
        return channel.Reader;
    }

    private void Broadcast(string json)
    {
        foreach (Channel<string> c in clients) { c.Writer.TryWrite(json); }
    }

    private string DeskPayload()
    {
        DeskSnapshot s = new(
            DateTime.UtcNow, Provider, Benchmark, PackageVersion, status,
            [.. tapes.Keys.Where(regimes.ContainsKey).Select(k => regimes[k])], [.. insights.Where(i => !i.Live), .. insights.Where(i => i.Live).TakeLast(60)],
            tapes.Values.Sum(t => t.Daily.Results.Count), tapes.Values.Sum(t => t.Minutes.Results.Count));
        return JsonSerializer.Serialize(new { type = "desk", tz = TimeZone, desk = s, quadrant = Report.Quadrant(s) }, Json);
    }

    // full: every bar for the initial paint or a revised older candle; otherwise the last few
    private string TapePayload(Tape tape, bool full)
    {
        IReadOnlyList<IBar> bars = tape.Minutes.Results;
        int from = full ? 0 : Math.Max(0, bars.Count - 3);
        var rows = Enumerable.Range(from, bars.Count - from).Select(i => new
        {
            t = new DateTimeOffset(bars[i].Timestamp).ToUnixTimeSeconds(),
            o = bars[i].Open,
            h = bars[i].High,
            l = bars[i].Low,
            c = bars[i].Close,
            fast = tape.Fast.Results[i].Ema,
            slow = tape.Slow.Results[i].Ema
        });
        return JsonSerializer.Serialize(
            new { type = "tape", symbol = tape.Symbol, full, rows, signals = tape.Signals.OrderBy(s => s.Time) }, Json);
    }

    public void Dispose() => flush.Dispose();
}

// Live is false for the opening briefing and true for what happens after it.
sealed record Insight(DateTime Time, string? Symbol, string Kind, string Text, bool Live);

sealed record DeskSnapshot(
    DateTime AsOf,
    string Provider,
    string Benchmark,
    string PackageVersion,
    string Status,
    IReadOnlyList<Regime> Regimes,
    IReadOnlyList<Insight> Insights,
    int DailyBars,
    int MinuteBars);
