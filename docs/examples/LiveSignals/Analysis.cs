using FacioQuo.Stock.Indicators;

// Daily market context for one symbol: absolute regime (trend, strength, volatility, drawdown)
// and relative rotation against the benchmark. Recalculated from the hubs' current results.
sealed record Regime(
    string Symbol,
    DateTime AsOf,
    double Price,
    double? DayChange,
    string Trend,
    double? VsSma200,
    double? Adx,
    string Strength,
    double? VolPercentile,
    double? Atrp,
    string Volatility,
    double? Drawdown,
    string? Quadrant,
    IReadOnlyList<RotationPoint> Tail,
    Signal? LastSignal)
{
    public string Label => $"{Trend} · {Strength} · {Volatility} volatility";
}

// RS-ratio: the symbol/benchmark price ratio against its own 50-day average (100 = in line).
// RS-momentum: the 10-day rate of change of RS-ratio (100 = flat). A simplified relative-rotation view.
sealed record RotationPoint(DateTime Time, double Ratio, double Momentum);

static class Analysis
{
    private const int Year = 365;
    private const int TailDays = 20;
    private const int TailStep = 5;

    public static Regime Evaluate(Tape tape, Tape? benchmark)
    {
        IReadOnlyList<IBar> daily = tape.Daily.Results;
        IBar last = daily[^1];
        double price = (double)last.Close;
        double? dayChange = daily.Count > 1 ? ((price / (double)daily[^2].Close) - 1) * 100 : null;

        double? sma50 = tape.Sma50.Results[^1].Sma;
        double? sma200 = tape.Sma200.Results[^1].Sma;
        string trend = sma50 is null || sma200 is null ? "Warming up"
            : price > sma200 && sma50 > sma200 ? "Uptrend"
            : price < sma200 && sma50 < sma200 ? "Downtrend"
            : "Transition";

        // ADX says how strong the directional move is; +DI against -DI says which way it points
        AdxResult dmi = tape.DailyAdx.Results[^1];
        double? adx = dmi.Adx;
        string side = dmi.Pdi >= dmi.Mdi ? "buying" : "selling";
        string strength = adx switch { null => "n/a", >= 25 => $"strong {side}", >= 20 => $"moderate {side}", _ => "weak trend" };

        // today's ATR as a percent of price, ranked against its own past year
        List<double> atrp = tape.DailyAtr.Results.TakeLast(Year).Where(r => r.Atrp is not null).Select(r => r.Atrp!.Value).ToList();
        double? volPct = atrp.Count > 20 ? 100.0 * atrp.Count(v => v <= atrp[^1]) / atrp.Count : null;
        string volatility = volPct switch { null => "n/a", >= 80 => "stressed", <= 20 => "calm", _ => "normal" };

        double high = daily.TakeLast(Year).Max(b => (double)b.High);

        IReadOnlyList<RotationPoint> rotation = benchmark is null || benchmark == tape ? [] : Rotation(tape, benchmark);
        IReadOnlyList<RotationPoint> tail = Tail(rotation);

        return new Regime(
            tape.Symbol, last.Timestamp, price, dayChange, trend,
            sma200 is null ? null : ((price / sma200.Value) - 1) * 100,
            adx, strength, volPct, atrp.Count > 0 ? atrp[^1] : null, volatility,
            ((price / high) - 1) * 100,
            tail.Count > 0 ? QuadrantOf(tail[^1]) : null,
            tail,
            tape.Signals.Count > 0 ? tape.Signals[^1] : null);
    }

    public static string QuadrantOf(RotationPoint p) => (p.Ratio >= 100, p.Momentum >= 100) switch
    {
        (true, true) => "Leading",
        (true, false) => "Weakening",
        (false, false) => "Lagging",
        (false, true) => "Improving"
    };

    // Align by timestamp, not position: two markets can have different calendars and gaps.
    private static List<RotationPoint> Rotation(Tape tape, Tape benchmark)
    {
        Dictionary<DateTime, decimal> bench = benchmark.Daily.Results.ToDictionary(b => b.Timestamp, b => b.Close);
        List<TimeValue> ratio = tape.Daily.Results
            .Where(b => bench.TryGetValue(b.Timestamp, out decimal c) && c != 0)
            .Select(b => new TimeValue(b.Timestamp, (double)(b.Close / bench[b.Timestamp])))
            .ToList();
        if (ratio.Count < 80) { return []; }

        // chained: EMA(5) smooths the ratio, then SMA(50) of that EMA is its baseline
        IReadOnlyList<EmaResult> smooth = ratio.ToEma(5);
        IReadOnlyList<SmaResult> baseline = smooth.ToSma(50);

        List<TimeValue> rsRatio = [];
        for (int i = 0; i < ratio.Count; i++)
        {
            if (smooth[i].Ema is double e && baseline[i].Sma is double s && s != 0)
            {
                rsRatio.Add(new TimeValue(ratio[i].Timestamp, 100 * e / s));
            }
        }

        IReadOnlyList<RocResult> momentum = rsRatio.ToRoc(10);
        List<RotationPoint> points = [];
        for (int i = 0; i < rsRatio.Count; i++)
        {
            if (momentum[i].Roc is double roc)
            {
                points.Add(new RotationPoint(rsRatio[i].Timestamp, rsRatio[i].Value, 100 + roc));
            }
        }
        return points;
    }

    // the last four weeks, one point a week, ending today
    private static List<RotationPoint> Tail(IReadOnlyList<RotationPoint> points)
    {
        List<RotationPoint> tail = [];
        for (int back = Math.Min(TailDays, points.Count - 1); back >= 0; back -= TailStep)
        {
            tail.Add(points[points.Count - 1 - back]);
        }
        return tail;
    }

    // The opening briefing: what the numbers say across the basket, most useful first.
    public static List<(string? Symbol, string Kind, string Text)> Brief(IReadOnlyList<Regime> all, string benchmark)
    {
        List<(string?, string, string)> notes = [];
        List<Regime> others = all.Where(r => r.Symbol != benchmark).ToList();

        int up = all.Count(r => r.Trend == "Uptrend"), down = all.Count(r => r.Trend == "Downtrend");
        string upNames = up is > 0 and <= 3 ? $" ({Names(all.Where(r => r.Trend == "Uptrend"))})" : "";
        notes.Add((null, "breadth",
            $"Breadth: {up} of {all.Count} symbols are in daily uptrends{upNames} and {down} in downtrends, judged by price and the 50-day SMA against the 200-day SMA."));

        List<Regime> leading = others.Where(r => r.Quadrant == "Leading").ToList();
        List<Regime> lagging = others.Where(r => r.Quadrant == "Lagging").ToList();
        if (others.Any(r => r.Quadrant is not null))
        {
            notes.Add((null, "rotation",
                $"Against {benchmark}: leading {(leading.Count > 0 ? Names(leading) : "none")}; lagging {(lagging.Count > 0 ? Names(lagging) : "none")}."));
        }

        // the biggest rotation over the tail that changed quadrant
        Regime? mover = others
            .Where(r => r.Tail.Count > 1 && QuadrantOf(r.Tail[0]) != r.Quadrant)
            .MaxBy(r => Distance(r.Tail[0], r.Tail[^1]));
        if (mover is not null)
        {
            RotationPoint a = mover.Tail[0], b = mover.Tail[^1];
            notes.Add((mover.Symbol, "rotation",
                $"{mover.Symbol} rotated from {QuadrantOf(a).ToLowerInvariant()} to {mover.Quadrant!.ToLowerInvariant()} against {benchmark} over four weeks "
                + $"(RS-ratio {a.Ratio:F1} → {b.Ratio:F1}, RS-momentum {a.Momentum:F1} → {b.Momentum:F1})."));
        }

        // relative strength is not absolute strength
        Regime? falsePositive = others.Where(r => r.Quadrant == "Leading" && r.Trend == "Downtrend").MinBy(r => r.VsSma200);
        if (falsePositive is not null)
        {
            notes.Add((falsePositive.Symbol, "divergence",
                $"{falsePositive.Symbol} leads {benchmark} on relative strength but is {Math.Abs(falsePositive.VsSma200 ?? 0):F1}% below its own 200-day SMA: "
                + "it is falling more slowly than the benchmark, not rising."));
        }

        // a long-term uptrend whose current directional move is down
        List<Regime> pressured = [.. all.Where(r => r.Trend == "Uptrend" && r.Strength == "strong selling")];
        if (pressured.Count > 0)
        {
            notes.Add((pressured.Count == 1 ? pressured[0].Symbol : null, "pressure",
                $"{(pressured.Count == 1 ? pressured[0].Symbol + " is" : $"{pressured.Count} of {up} uptrends ({Names(pressured)}) are")} under strong selling pressure: "
                + "ADX(14) is at least 25 with −DI above +DI, so recent daily moves have run against a 200-day trend that is still up "
                + $"(median move today so far: {Fmt.Pct(Median(pressured.Select(r => r.DayChange ?? 0)))})."));
        }

        Regime? weakTrend = all.Where(r => r.Trend == "Uptrend" && r.Adx < 20).MinBy(r => r.Adx);
        if (weakTrend is not null)
        {
            notes.Add((weakTrend.Symbol, "strength",
                $"{weakTrend.Symbol} is above its 200-day SMA, but ADX(14) is only {weakTrend.Adx:F0}: the uptrend has little directional force right now."));
        }

        Regime? stressed = all.Where(r => r.VolPercentile >= 80).MaxBy(r => r.VolPercentile);
        Regime? calm = all.Where(r => r.VolPercentile <= 20).MinBy(r => r.VolPercentile);
        foreach (Regime r in new[] { stressed, calm }.OfType<Regime>())
        {
            notes.Add((r.Symbol, "volatility",
                $"{r.Symbol} daily ATR is {r.Atrp:F1}% of price, the {Ordinal(r.VolPercentile ?? 0)} percentile of its past year: {r.Volatility}."));
        }

        // an uptrend far below its high is a recovery, not a bull market
        List<double> drawdowns = [.. all.Where(r => r.Trend == "Uptrend" && r.Drawdown is not null).Select(r => r.Drawdown!.Value).Order()];
        if (up * 2 > all.Count && drawdowns.Count > 0 && drawdowns[drawdowns.Count / 2] < -30)
        {
            notes.Add((null, "drawdown",
                $"These uptrends are recoveries: the median one is still {Math.Abs(drawdowns[drawdowns.Count / 2]):F0}% below its one-year high, "
                + "so price has turned up from deep lows without reclaiming last year's levels."));
        }

        Regime? deepest = all.Where(r => r.Drawdown < -20).MinBy(r => r.Drawdown);
        if (deepest is not null)
        {
            notes.Add((deepest.Symbol, "drawdown", $"{deepest.Symbol} trades {Math.Abs(deepest.Drawdown!.Value):F0}% below its one-year high."));
        }

        return notes;
    }

    private static double Median(IEnumerable<double> values)
    {
        List<double> v = [.. values.Order()];
        return v.Count == 0 ? 0 : v[v.Count / 2];
    }

    private static double Distance(RotationPoint a, RotationPoint b) => Math.Sqrt(Math.Pow(b.Ratio - a.Ratio, 2) + Math.Pow(b.Momentum - a.Momentum, 2));

    private static string Names(IEnumerable<Regime> regimes) => string.Join(", ", regimes.Select(r => r.Symbol));

    public static string Ordinal(double value)
    {
        int n = (int)Math.Round(value);
        string suffix = (n % 100) is >= 11 and <= 13 ? "th" : (n % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" };
        return $"{n}{suffix}";
    }
}
