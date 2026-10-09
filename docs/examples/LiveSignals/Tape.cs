using FacioQuo.Stock.Indicators;

// One symbol on the desk: a one-minute stream for live signals, and a daily stream for market context.
// Both are BarHubs with chained indicator hubs; the Desk serializes every write.
sealed class Tape
{
    private readonly Func<DateTime, DateTime> dayOf;

    public Tape(string symbol, Func<DateTime, DateTime> dayOf)
    {
        Symbol = symbol;
        this.dayOf = dayOf;

        // count the hubs as they are built, so the proof line can't drift from the code
        Fast = Hub(Minutes.ToEmaHub(21));
        Slow = Hub(Minutes.ToEmaHub(55));
        Adx = Hub(Minutes.ToAdxHub(14));
        Atr = Hub(Minutes.ToAtrHub(14));

        Sma50 = Hub(Daily.ToSmaHub(50));
        Sma200 = Hub(Daily.ToSmaHub(200));
        DailyAdx = Hub(Daily.ToAdxHub(14));
        DailyAtr = Hub(Daily.ToAtrHub(14));
    }

    public string Symbol { get; }

    // one-minute bars: the live signal recipe
    public BarHub Minutes { get; } = new();
    public EmaHub Fast { get; }
    public EmaHub Slow { get; }
    public AdxHub Adx { get; }
    public AtrHub Atr { get; }
    public List<Signal> Signals { get; } = [];

    // daily bars: trend, strength, and volatility context; the newest bar is today's, still forming
    public BarHub Daily { get; } = new();
    public SmaHub Sma50 { get; }
    public SmaHub Sma200 { get; }
    public AdxHub DailyAdx { get; }
    public AtrHub DailyAtr { get; }

    public int HubCount { get; private set; }

    private T Hub<T>(T hub)
    {
        HubCount++;
        return hub;
    }

    public decimal? Price => Minutes.Results.Count > 0 ? Minutes.Results[^1].Close : Daily.Results.Count > 0 ? Daily.Results[^1].Close : null;

    // A live minute also moves today's daily bar, so the daily context updates intraday.
    // Volume is left as seeded: no daily metric here uses it, and repeated forming updates would double-count it.
    public void AddMinute(Bar bar, bool live)
    {
        // a late closed or revised minute must not move today's close back behind a newer one
        bool late = Minutes.Results.Count > 0 && bar.Timestamp < Minutes.Results[^1].Timestamp;
        Minutes.Add(bar);
        if (!live || late || Daily.Results.Count == 0) { return; }

        IBar today = Daily.Results[^1];
        DateTime day = dayOf(bar.Timestamp);
        if (day < today.Timestamp) { return; }

        Daily.Add(day == today.Timestamp
            ? new Bar(today.Timestamp, today.Open, Math.Max(today.High, bar.High), Math.Min(today.Low, bar.Low), bar.Close, today.Volume)
            : new Bar(day, bar.Open, bar.High, bar.Low, bar.Close, bar.Volume));
    }

    // Signals are evaluated only on closed candles, so they never repaint.
    public Signal? Evaluate(DateTime timestamp)
    {
        IReadOnlyList<IBar> bars = Minutes.Results;
        int i = bars.Count - 1;
        while (i >= 0 && bars[i].Timestamp != timestamp) { i--; }
        if (i < 1) { return null; }

        // a revised closed candle replaces its earlier verdict
        Signals.RemoveAll(s => s.Time == timestamp);

        double? f0 = Fast.Results[i - 1].Ema, f1 = Fast.Results[i].Ema;
        double? s0 = Slow.Results[i - 1].Ema, s1 = Slow.Results[i].Ema;
        double? adx = Adx.Results[i].Adx, atr = Atr.Results[i].Atr;
        if (f0 is null || s0 is null || f1 is null || s1 is null || adx is null || atr is null) { return null; }

        string? side = f0 <= s0 && f1 > s1 ? "BUY" : f0 >= s0 && f1 < s1 ? "SELL" : null;
        if (side is null) { return null; }

        double close = (double)bars[i].Close;
        double stop = side == "BUY" ? close - (1.5 * atr.Value) : close + (1.5 * atr.Value);
        string reason = $"EMA21 crossed {(side == "BUY" ? "above" : "below")} EMA55 · ADX {adx:F0}"
            + (adx < 20 ? " (weak trend, low conviction)" : "")
            + $" · stop {Fmt.Price(stop)} (1.5×ATR)";

        Signal signal = new(timestamp, side, close, reason, adx >= 20);
        Signals.Add(signal);
        return signal;
    }
}

sealed record Signal(DateTime Time, string Side, double Price, string Reason, bool Strong);

static class Fmt
{
    // crypto quotes range from fractions of a cent to six figures
    public static string Price(double value) => value switch
    {
        >= 1000 => value.ToString("N2"),
        >= 1 => value.ToString("F2"),
        _ => value.ToString("G4")
    };

    public static string Price(decimal value) => Price((double)value);

    public static string Pct(double value) => Pct(value, 1);

    public static string Pct(double value, int digits) => $"{(value >= 0 ? "+" : "")}{value.ToString($"F{digits}")}%";
}
